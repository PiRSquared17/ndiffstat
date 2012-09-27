using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NDiffStatLib.Utils;

namespace NDiffStatLib.DiffParsers
{
	/// <summary>
	/// Parses diff files into fragments, taking into account special fields
	/// present in certain types of diffs.
	/// Tranlated from Review Board (Python) branch master, changeset 839fc69 2012-09-06 20:26:59
	/// </summary>
	public class DiffParser
	{
		private readonly string INDEX_SEP = string.Join("", Enumerable.Repeat('=', 67));
		private readonly CustomTextReader reader;
		private readonly FileDiffFactory fileDiffFactory;

		public DiffParser( TextReader data, FileDiffFactory fileDiffFactory ) : this(new CustomTextReader(data), fileDiffFactory) {}

		public DiffParser( CustomTextReader reader, FileDiffFactory fileDiffFactory )
		{
			this.reader = reader;
			this.fileDiffFactory = fileDiffFactory;
		}

		/// <summary>
		/// Parses the diff, returning a list of File objects representing each
		/// file in the diff.
		/// </summary>
		public IEnumerable<FileDiff> parse()
		{
			Debug.WriteLine(string.Format("{0}.parse: Beginning parse of diff", this.GetType().Name));

			FileDiff currentFile = null;
			// Go through each line in the diff, looking for diff headers.
			while (reader.MoveFoward()) {
				FileDiff new_file = this.parse_change_header(reader);
				if (new_file != null) {
					// This line is the start of a new file diff.
					if (currentFile != null) yield return currentFile;
					currentFile = new_file;
				} else {
					if (currentFile != null) {
						currentFile.WriteLine(reader.CurrentLine, isHeader: false);
					}
				}
			}
			if (currentFile != null) yield return currentFile;
			Debug.WriteLine(string.Format("{0}.parse: Finished parsing diff.", this.GetType().Name));
		}

		///<summary>
		///Parses part of the diff beginning at the specified line number, trying
		///to find a diff header.
		///<summary>
		private FileDiff parse_change_header( CustomTextReader reader )
		{
			NameValueCollection info = new NameValueCollection();
			FileDiff fileDiff = null;
			int start = reader.CurrentLineIndex;
			StringBuilder sbHeader = new StringBuilder();
			this.parse_special_header(reader, info, sbHeader);
			this.parse_diff_header(reader, info, sbHeader);

			// If we have enough information to represent a header, build the
			// file to return.
			if (info.AllKeys.Contains("origFile") && info.AllKeys.Contains("newFile") && 
			   info.AllKeys.Contains("origInfo") && info.AllKeys.Contains("newInfo")) {
				fileDiff = fileDiffFactory.Create();
				if (info["binary"] != null) fileDiff.binary = bool.Parse(info["binary"]);
				if (info["deleted"] != null) fileDiff.deleted  = bool.Parse(info["deleted"]);
				fileDiff.origFile = info["origFile"];
				fileDiff.newFile  = info["newFile"];
				fileDiff.origInfo = info["origInfo"];
				fileDiff.newInfo  = info["newInfo"];
				fileDiff.origChangesetId = info["origChangesetId"];

				// The header is part of the diff, so make sure it gets in the
				// diff content. But only the parts that patch will understand.
				string[] headerLines = new StringReader(sbHeader.ToString()).ReadAsCollection().ToArray();
				for (int i=0 ; i<headerLines.Length ; i++) {
					string line = headerLines[i];
					if (line.StartsWith("--- ") 
						|| line.StartsWith("+++ ") 
						|| line.StartsWith("RCS file: ") 
						|| line.StartsWith("retrieving revision ") 
						|| line.StartsWith("diff ") || (i > start && line == this.INDEX_SEP && headerLines[i - 1].StartsWith("Index: ")) 
					    ||(i + 1 < headerLines.Length && line.StartsWith("Index: ") && headerLines[i + 1] == this.INDEX_SEP)) {

						// This is a valid part of a diff header. Add it.
						fileDiff.WriteLine(line, isHeader: true);
					}
				}
			}
			return fileDiff;
		}

		/// <summary>
		/// Parses part of a diff beginning at the specified line number, trying
		/// to find a special diff header. This usually occurs before the standard
		/// diff header.
		/// 
		/// The line number returned is the line after the special header,
		/// which can be multiple lines long.
		/// </summary>
		/// <param name="line_enum"></param>
		/// <param name="info"></param>
		protected virtual void parse_special_header( CustomTextReader reader, NameValueCollection info, StringBuilder sbHeader )
		{
			if (reader.NextLine != null && reader.CurrentLine.StartsWith("Index: ") && reader.NextLine == this.INDEX_SEP) {
				string[] special_header = new string[2];
				// This is an Index: header, which is common in CVS && Subversion,
				// amongst other systems.
				try {
					int firstSpacePos = reader.CurrentLine.IndexOf(" ");
					info["index"] = reader.CurrentLine.Substring(firstSpacePos + 1);
				} catch (Exception ex) {
					throw new DiffParserError("Malformed Index line {0}", reader.CurrentLineIndex, ex);
				}
				// move reader two steps foward
				sbHeader.AppendLine(reader.CurrentLine);
				reader.MoveFoward();
				sbHeader.AppendLine(reader.CurrentLine);
				reader.MoveFoward();
			}
		}

		/// <summary>
		/// Parses part of a diff beginning at the specified line number, trying
		/// to find a standard diff header.
		/// 
		/// The line number returned is the line after the special header,
		/// which can be multiple lines long.
		/// </summary>
		protected virtual void parse_diff_header( CustomTextReader reader, NameValueCollection info, StringBuilder sbHeader )
		{

			if (reader.NextLine != null && (
				reader.CurrentLine.StartsWith("--- ") && reader.NextLine.StartsWith("+++ ") 
			   || reader.CurrentLine.StartsWith("*** ") && reader.NextLine.StartsWith("--- ") && !reader.CurrentLine.EndsWith(" ****"))) {
				// This is a unified || context diff header. Parse the
				// file && extra info.
				try {
					string[] diff_header = new string[2];
					Tuple<string, string> origFileInfos = this.parse_filename_header(reader.CurrentLine.Substring(4), reader.CurrentLineIndex);
					info["origFile"] = origFileInfos.Item1;
					info["origInfo"] = origFileInfos.Item2;
					sbHeader.AppendLine(reader.CurrentLine);
					reader.MoveFoward();

					Tuple<string, string> newFileInfos = this.parse_filename_header(reader.CurrentLine.Substring(4), reader.CurrentLineIndex);
					info["newFile"] = newFileInfos.Item1;
					info["newInfo"] = newFileInfos.Item2;
					sbHeader.AppendLine(reader.CurrentLine);
					reader.MoveFoward();

				} catch (Exception ex) {
					throw new DiffParserError("The diff file is missing revision information", reader.CurrentLineIndex, ex);
				}
			}
		}

		/// <summary>
		/// Returns (file, info)
		/// </summary>
		private Tuple<string, string> parse_filename_header( string s, int currentLineIndex ) {
			int tabIndex = s.IndexOf('\t');
			if (tabIndex != -1) {
				// There's a \t separating the filename && info. This is the
				// best case scenario, since it allows for filenames with spaces
				// without much work.
				return new Tuple<string,string>(s.Substring(0, tabIndex), s.Substring(tabIndex + 1));
			}
			// There's spaces being used to separate the filename && info.
			// This is technically wrong, so all we can do is assume that
			// 1) the filename won't have multiple consecutive spaces, &&
			// 2) there's at least 2 spaces separating the filename && info.
			if (s.Contains("  ")) {
				string[] tab = Regex.Split(s, "  +");
				return new Tuple<string,string>(tab[0], string.Join("", tab, 1, tab.Length - 1));
			}
			throw new DiffParserError("No valid separator after the filename was found in the diff header", currentLineIndex);
		}

		/// <summary>
		/// Returns a raw diff as a string.
		/// </summary>
		/// <param name="diffset">diffset object</param>
		/// <returns>The returned diff as composed of all FileDiffs in the provided diffset</returns>
		private string raw_diff( object diffset )
		{
			//return ''.Join([filediff.diff for filediff in diffset.files.all()]);
			throw new NotImplementedException();
		}

	}

	public class DiffParserError : Exception
	{
		public int linenum;

		public DiffParserError( string msg, int linenum, Exception innerException ) : base(msg, innerException)
		{
			this.linenum = linenum;
		}

		public DiffParserError( string msg, int linenum ) : base(msg)
		{
			this.linenum = linenum;
		}

		protected DiffParserError( string msg, params string[] args ) : base(string.Format(msg, args)) { }
	}
}
