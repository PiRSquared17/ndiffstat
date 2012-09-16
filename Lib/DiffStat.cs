using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NDiffStatLib.Utils;
using System.Diagnostics;

namespace NDiffStatLib
{
	public enum DiffFormat
	{
		unknown,
		/// <summary>
		/// Format de diff tels que générés par la commande "hg diff" dans Mercurial
		/// </summary>
		hgDiff,
		/// <summary>
		/// Format de diff généré par Subversion (svn diff)
		/// </summary>
		svnDiff
	}; 
	
	public class DiffStat
	{
		/// <summary>
		/// Default width limit for text output
		/// </summary>
		public const int DEFAULT_MAX_WIDTH = 80;

		private readonly DiffStatOptions options;
		private DiffFormat diffFormat;
		public int longuestNameLength
		{
			get
			{
				if (fileStats.Any()) { return fileStats.Keys.Max(fileName => fileName.Length); } 
				else { return 0; }
			}
		}
		public int maxtotal
		{
			get
			{
				if (fileStats.Any()) { return fileStats.Values.Max(fileStat => fileStat.total);	} 
				else { return 0; }
			}
		}
		public int total_adds
		{
			get { return fileStats.Values.Sum(fileStat => fileStat.adds); }
		}
		public int total_removes
		{
			get { return fileStats.Values.Sum(fileStat => fileStat.removes); }
		}
		public int total_modifs
		{
			get { return fileStats.Values.Sum(fileStat => fileStat.modifs); }
		}
		
		private Dictionary<string, FileStat> fileStats = new Dictionary<string, FileStat>();

		/// <summary>
		/// Constructs a new objet DiffStat and parse the contents of the TextReader
		/// </summary>
		public DiffStat( TextReader lines, DiffStatOptions options )
		{
			this.options = options;
			Parse(lines);
		}

		private void Parse( TextReader lines )
		{	
			RegexOptions options = RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.ExplicitCapture;
			// Hg diff format
			// e.g. --- a/path_to_file	Fri Sep 07 18:14:51 2012 +0200
			Regex regHgDiff = new Regex(@"^(?<prefix>---|\+\+\+) ([ab]/)?(?<fileName>.+?)\t(.+)$", options);
			// Svn diff format
			// e.g. --- path_to_file		(revision xxx)
			Regex regSvnDiff = new Regex(@"^(?<prefix>---|\+\+\+) (?<fileName>.+?)\t\(revision (?<revision>\d+)\)$", options);

			FileStat currentFS = null;
			string line;
			int lineCount = 0;
			while ((line = lines.ReadLine()) != null) {
				lineCount++;
				bool line_added = line.StartsWith("+") && !line.StartsWith("+++ ");
				bool line_deleted = line.StartsWith("-") && !line.StartsWith("--- ");
				if ((line_added || line_deleted) && currentFS == null) {
					// if whe find a line begining with '+' or '-' and we don't know witch file its refers to, something turned wrong
					throw new Exception(string.Format("Error : fileName couldn't be parsed in lines 0-{0}", lineCount));
				} else if (line_added) {
					currentFS.Accumulate(adds: 1);
				} else if (line_deleted) {
					currentFS.Accumulate(removes: 1);
				} else {
					if (currentFS != null) {
						// here we compute modified lines (if required) as : min ( adds, removes )
						currentFS.FlushAccumulator(this.options.merge_opt);
					}
					if (line.StartsWith("+++ ") || line.StartsWith("--- ")) {
						if (this.diffFormat == DiffFormat.unknown || this.diffFormat == DiffFormat.hgDiff) {
							Match match = regHgDiff.Match(line);
							if (match.Success) {
								this.diffFormat = DiffFormat.hgDiff;
								if (match.Groups["prefix"].Value == "---") {
									AddStats(currentFS);
									currentFS = new FileStat(match.Groups["fileName"].Value);
								} else if (currentFS != null && currentFS.fileName == "/dev/null" && match.Groups["prefix"].Value == "+++") {
									// cas d'un nouveau fichier. Dans ce cas a lu "/dev/null" comme nom du fichier lors du parsing de la ligne "--- "
									// --> on recrée un nouvel objet FileStat avec le bon nom de fichier
									currentFS = new FileStat(match.Groups["fileName"].Value);
								}
							}
						}
						if (this.diffFormat == DiffFormat.unknown || this.diffFormat == DiffFormat.svnDiff) {
							Match match = regSvnDiff.Match(line);
							if (match.Success) {
								this.diffFormat = DiffFormat.svnDiff;
								if (currentFS == null && match.Groups["prefix"].Value == "---") {
									AddStats(currentFS);
									currentFS = new FileStat(match.Groups["fileName"].Value);
								}
							}
						}
						if (line.StartsWith("--- ") && currentFS == null) {
							throw new Exception(string.Format("unrecognized line pattern (line no. {0}) :\r\n{1}", lineCount, line));
						}
					}
				}
			}
			if (currentFS != null) {
				currentFS.FlushAccumulator(this.options.merge_opt);
				AddStats(currentFS);
			}
		} 

		private void AddStats(FileStat fileStat) {
			if (fileStat == null) return;
			FileStat existingFileStat;
			if (fileStats.TryGetValue(fileStat.fileName, out existingFileStat)) {
				existingFileStat.SumWith(fileStat);
			} else {
				fileStats.Add(fileStat.fileName, fileStat);
			}
		}

		public override string ToString()
		{
			int total_adds = this.total_adds;
			int total_removes = this.total_removes;
			int total_modifs = this.total_modifs;
			if (!this.options.merge_opt) Debug.Assert(total_modifs == 0);
			int maxtotal = this.maxtotal;
			int longuestNameLength = this.longuestNameLength;
			if (this.total_adds == 0 && this.total_removes == 0) return "0 files changes";

			// Work out widths
			int maxChangeCountWidth = CalcUtils.Max(maxtotal.ToString().Length, 5);
			int separatorsCharsCount = 4; // one space at the begining of line, two after filename (" |"), one space after change count column
			int graphWidth = DEFAULT_MAX_WIDTH - maxChangeCountWidth - longuestNameLength - separatorsCharsCount;

			// The graph width can be <= 0 if there is a modified file with a
			// filename longer than DEFAULT_MAX_WIDTH. Use a minimum of 10.
			if (graphWidth < 10) graphWidth = 10;

			double histogramScale = Math.Min((double)graphWidth / maxtotal, 1d);

			StringBuilder output = new StringBuilder();

			int modifiedFilesCount = 0;
			foreach (FileStat fileStat in this.fileStats.Values.Where(fs => fs.total > 0).OrderBy(fs => fs.fileName, StringComparer.Ordinal)) {
				string formatStr = " {0,-" + longuestNameLength + "} |";
				output.AppendFormat(formatStr, fileStat.fileName);
				OutputArrayData(fileStat, output, maxChangeCountWidth);
				OutputHistogram(fileStat, output, histogramScale);
				output.AppendLine();
				modifiedFilesCount++;
			}

			output.AppendFormat(" {0} {1} changed", modifiedFilesCount, modifiedFilesCount > 1 ? "files" : "file");
			if (total_adds > 0) {
				output.AppendFormat(", {0} {1}(+)", total_adds, total_adds > 1 ? "insertions" : "insertion");
			}
			if (total_removes > 0) {
				output.AppendFormat(", {0} {1}(-)", total_removes, total_removes > 1 ? "deletions" : "deletion");
			}
			if (total_modifs > 0) {
				output.AppendFormat(", {0} {1}(!)", total_modifs, total_modifs > 1 ? "modifications" : "modification");
			}

			return output.ToString();
		}

		public void OutputArrayData( FileStat fileStat, StringBuilder output, int numberWidth )
		{
			string strFormat = "{0," + numberWidth + "} ";
			output.AppendFormat(strFormat, fileStat.total);
			if (this.options.format_opt.HasFlag(FormatOption.verbose)) {
				output.AppendFormat(strFormat, fileStat.adds);
				output.AppendFormat(strFormat, fileStat.removes);
				if (this.options.merge_opt) {
					output.AppendFormat(strFormat, fileStat.modifs);
				}
			}
		}

		public void OutputHistogram( FileStat fileStat, StringBuilder output, double scale )
		{	
			// since we could only display an integer number of symbols {+,-,!} the bar could be visibly shorter than we expect
			// to avoid too much difference, we report the error (delta) resulting of the "floor" operation on subsequent steps
			// e.g if scale is 0.1 and adds = 12, whe display one '+' symbol ((int)0.1*12) and set 0.2 as delta (0.1*12 - (int)0.1*12)
			double product;
			int charsCount;
			double delta = 0;
			if (fileStat.adds > 0) {
				product = scale * fileStat.adds;
				charsCount = (int)product;
				output.Append(string.Join("", Enumerable.Repeat('+', charsCount)));
				delta = product - charsCount; // always >= 0 since product >= (int)product 
			}
			if (fileStat.removes > 0) {
				product = scale * (fileStat.removes + delta);
				charsCount = (int)product;
				output.Append(string.Join("", Enumerable.Repeat('-', charsCount)));
				delta = product - charsCount;
			}
			if (this.options.merge_opt) {
				product = scale * (fileStat.modifs + delta);
				charsCount = (int)product;
				output.Append(string.Join("", Enumerable.Repeat('!', charsCount)));
			}
		}
	}

	public class FileStat
	{
		public string fileName { get; private set; }
		public int adds { get; private set; }
		public int removes { get; private set; }
		public int modifs { get; private set; }
		/// <summary>
		/// Object witch serves as buffer for added / removed lines counts
		/// This allow to compute modified lines (if required) as
		/// min ( adds, removes )	where adds / removes are values stored in the buffer
		/// </summary>
		private ChunkStat accumulator;
		public int total {
			get { return adds + removes + modifs; }
		}

		public FileStat(string fileName) : this(fileName, 0, 0, 0) {}

		public FileStat(string fileName, int adds, int removes, int modifs) {
			this.fileName = fileName;
			this.adds = adds;
			this.removes = removes;
			this.modifs = modifs;
			this.accumulator = new ChunkStat();
		}

		public void SumWith(FileStat other) {
			if (this.fileName != other.fileName) {
				throw new Exception("FileStat.Add cannot add files with differents name");
			}
			this.adds += other.adds;
			this.removes += other.removes;
			this.modifs += other.modifs;
		}
		public void Accumulate( int adds=0, int removes=0 )
		{
			this.accumulator.adds += adds;
			this.accumulator.removes += removes;
		}
		public void FlushAccumulator( bool merge_opt )
		{
			if (this.accumulator.adds == 0 && this.accumulator.removes == 0) return;
			int modifs = merge_opt ? CalcUtils.Min(this.accumulator.adds, this.accumulator.removes) : 0;
			this.adds += (this.accumulator.adds - modifs);
			this.removes += (this.accumulator.removes - modifs);
			this.modifs += modifs;
			this.accumulator.Reset();
		}
	}

	internal class ChunkStat
	{
		public int adds;
		public int removes;

		public ChunkStat()
		{
			Reset();
		}

		public void Reset()
		{
			adds = 0;
			removes = 0;
		}

	}

	
}
