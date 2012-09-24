﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NDiffStatLib.Utils;
using System.Diagnostics;
using NDiffStatLib.DiffParsers;

namespace NDiffStatLib
{
	public class DiffStat
	{
		/// <summary>
		/// Default width limit for text output
		/// </summary>
		public const int DEFAULT_MAX_WIDTH = 80;

		public readonly DiffStatOptions options;
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
		
		private Dictionary<string, StatsCounter> fileStats = new Dictionary<string, StatsCounter>();

		/// <summary>
		/// Constructs a new objet DiffStat and parse the contents of the TextReader
		/// </summary>
		public DiffStat( TextReader lines, DiffStatOptions options )
		{
			this.options = options;
			ParseDiff(lines);
		}

		private void ParseDiff( TextReader lines )
		{
			CustomTextReader reader = new CustomTextReader(lines);
			FileDiffWithCounterFactory factory = new FileDiffWithCounterFactory(options.merge_opt);
			DiffParser diffParser = GetDiffParser(reader, factory);
			foreach (FileDiff fileDiff in diffParser.parse()) {
				string fileName = !fileDiff.newFile.IsNullOrEmpty() ? fileDiff.newFile : fileDiff.origFile;
				StatsCounter counter = ((FileDiffWithCounter)fileDiff).statsCounter;
				counter.ClearTempStats();
				AddStats(fileName, counter);
			}
		}

		private DiffParser GetDiffParser(CustomTextReader reader, FileDiffWithCounterFactory factory)
		{
			string firstLine = reader.NextLine;
			// very basic test to find diff format
			if (firstLine == null) {
				throw new DiffParserError("Diff is empty", 0);
			} else if (firstLine.StartsWith("Index: ")) {
				return new SvnDiffParser(reader, factory);
			} else if (firstLine.StartsWith("# ") || firstLine.StartsWith("diff ")) {
				return new HgDiffParser(reader, factory);
			} else {
				return new DiffParser(reader, factory);
			}
		}

		private void AddStats(string fileName, StatsCounter fileStat) {
			StatsCounter existingFileStat;
			if (fileStats.TryGetValue(fileName, out existingFileStat)) {
				existingFileStat.SumWith(fileStat);
			} else {
				fileStats.Add(fileName, fileStat);
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
			if (this.total_adds == 0 && this.total_removes == 0 && this.total_modifs == 0) return "0 files changes";

			// Work out widths
			int maxChangeCountWidth = CalcUtils.Max(maxtotal.ToString().Length, 5);
			int separatorsCharsCount = 4; // one space at the begining of line, two chars after filename (" |"), one space after change count column
			int graphWidth = DEFAULT_MAX_WIDTH - maxChangeCountWidth - longuestNameLength - separatorsCharsCount;

			// The graph width can be <= 0 if there is a modified file with a
			// filename longer than DEFAULT_MAX_WIDTH. Use a minimum of 10.
			if (graphWidth < 10) graphWidth = 10;

			double histogramScale = Math.Min((double)graphWidth / maxtotal, 1d);

			StringBuilder output = new StringBuilder();
			int modifiedFilesCount = 0;
			foreach (var fileStat in this.fileStats.OrderBy(fs => fs.Key, StringComparer.Ordinal).Where(fs => fs.Value.total > 0)) {
				string formatStr = " {0,-" + longuestNameLength + "} |";
				output.AppendFormat(formatStr, fileStat.Key);
				OutputArrayData(fileStat.Key, fileStat.Value, output, maxChangeCountWidth);
				OutputHistogram(fileStat.Key, fileStat.Value, output, histogramScale);
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

		public void OutputArrayData( string fileName, StatsCounter fileStat, StringBuilder output, int numberWidth )
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

		public void OutputHistogram( string fileName, StatsCounter fileStat, StringBuilder output, double scale )
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
	
}
