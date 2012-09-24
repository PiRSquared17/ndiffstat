using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using NDiffStatLib.Utils;
using System.Text.RegularExpressions;
using System.IO;
using System.Diagnostics;

namespace NDiffStatLib.DiffParsers
{
	/// <summary>
	/// This class is able to extract Mercurial changeset ids, &&
	/// replaces /dev/null with a useful name
	/// Tranlated from Review Board (Python)
	/// </summary>
	public class HgDiffParser : DiffParser
	{
		public string newChangesetId = null;
		public string origChangesetId = null;
		public bool isGitDiff = false;

		public HgDiffParser( TextReader text, FileDiffFactory fileDiffFactory ) : base(text, fileDiffFactory) { }

		public HgDiffParser( CustomTextReader reader, FileDiffFactory fileDiffFactory ) : base(reader, fileDiffFactory) { }

		protected override void parse_special_header( CustomTextReader reader, NameValueCollection info, StringBuilder sbHeader )
		{
			string[] diffLine = reader.CurrentLine.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

			// git style diffs are supported as long as the node ID && parent ID
			// are present in the patch header
			if (reader.CurrentLine.StartsWith("# Node ID") && diffLine.Length == 4) {
				this.newChangesetId = diffLine[3];
			} else if (reader.CurrentLine.StartsWith("# Parent") && diffLine.Length == 3) {
				this.origChangesetId = diffLine[2];
			} else if (reader.CurrentLine.StartsWith("diff -r")) {
				// diff between two revisions are in the following form:
				//  "diff -r abcdef123456 -r 123456abcdef filename"
				// diff between a revision && the working copy are like:
				//  "diff -r abcdef123456 filename"
				this.isGitDiff = false;
				try {
					// ordinary hg diffs don't record renames, so
					// new file always == old file
					bool isCommitted = diffLine.Length > 4 && diffLine[3] == "-r";
					int nameStartIndex;
					if (isCommitted) {
						nameStartIndex = 5;
						info["newInfo"] = diffLine[4];
					} else {
						nameStartIndex = 3;
						info["newInfo"] = "Uncommitted";
					}
					info["newFile"] = (info["origFile"] = string.Join(" ", diffLine, nameStartIndex, diffLine.Length - nameStartIndex));
					info["origInfo"] = diffLine[2];
					info["origChangesetId"] = diffLine[2];
				} catch (Exception ex) {
					throw new DiffParserError("The diff file is missing revision information", reader.CurrentLineIndex, ex);
				}
				sbHeader.AppendLine(reader.CurrentLine);
				reader.MoveFoward();

			} else if (reader.CurrentLine.StartsWith("diff --git") && 
            !this.origChangesetId.IsNullOrEmpty()) {
				// diff is in the following form:
				//  "diff --git a/origfilename b/newfilename"
				// possibly followed by:
				//  "{copy|rename} from origfilename"
				//  "{copy|rename} from newfilename"
				this.isGitDiff = true;
				info["origInfo"] = info["origChangesetId"] = this.origChangesetId;
				if (this.newChangesetId.IsNullOrEmpty()) {
					info["newInfo"] = "Uncommitted";
				} else {
					info["newInfo"] = this.newChangesetId;
				}
				Match lineMatch = Regex.Match(
					reader.CurrentLine,
					@" a/(.*?) b/(.*?)( (copy|rename) from .*)?$"
				);
				info["origFile"] = lineMatch.Groups[1].Value;
				info["newFile"] = lineMatch.Groups[2].Value;
				sbHeader.AppendLine(reader.CurrentLine);
				reader.MoveFoward();
			}
		}

		protected override void parse_diff_header( CustomTextReader reader, NameValueCollection info, StringBuilder sbHeader )
		{
			if (!this.isGitDiff) {
				if (reader.CurrentLine != null && reader.CurrentLine.StartsWith("Binary file ")) {
					info["binary"] = "True";
					sbHeader.AppendLine(reader.CurrentLine);
					reader.MoveFoward();
				}
				if (this._check_file_diff_start(reader, info)) {
					// move two lines foward
					sbHeader.AppendLine(reader.CurrentLine);
					reader.MoveFoward();
					sbHeader.AppendLine(reader.CurrentLine);
					reader.MoveFoward();
				}

			} else {
				bool canMoveFoward = true;
				while (canMoveFoward) {
					if (this._check_file_diff_start(reader, info)) {
						this.isGitDiff = false;
						// move two lines foward
						sbHeader.AppendLine(reader.CurrentLine);
						reader.MoveFoward();
						sbHeader.AppendLine(reader.CurrentLine);
						reader.MoveFoward();
						return;
					}
					string line = reader.CurrentLine;
					if (line.StartsWith("Binary file") || line.StartsWith("GIT binary")) {
						info["binary"] = "True";
						sbHeader.AppendLine(reader.CurrentLine);
						canMoveFoward = reader.MoveFoward();
					} else if (line.StartsWith("copy") || line.StartsWith("rename") 
						|| line.StartsWith("new") || line.StartsWith("old") 
						|| line.StartsWith("deleted") || line.StartsWith("index")) {
						// Not interested, just pass over this one
						sbHeader.AppendLine(reader.CurrentLine);
						canMoveFoward = reader.MoveFoward();
					} else {
						break;
					}
				}
			}
		}

		private bool _check_file_diff_start( CustomTextReader reader, NameValueCollection info )
		{
			if (reader.NextLine != null 
				&& reader.CurrentLine.StartsWith("--- ") 
				&& reader.NextLine.StartsWith("+++ ")) {
				// check if we're a new file
				string[] tab = reader.CurrentLine.Split();
				if (tab[1] == "/dev/null") {
					info["origInfo"] = Revision.PRE_CREATION;
				}
				return true;
			} else {
				return false;
			}
		}

		public static RevisionParseResult parse_diff_revision( string file_str, string revision_str )
		{
			string revision = revision_str;
			if (file_str == "/dev/null") {
				revision = Revision.PRE_CREATION;
			}
			if (revision_str.IsNullOrEmpty()) {
				revision = Revision.UNKNOWN;
			}
			return new RevisionParseResult(file_str, revision);
		}
	}
}
