using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NDiffStatLib
{
	public enum FormatOption
	{
		normal = 1,
		detailed = 4
	}

	public class DiffStatOptions
	{
		/// <summary>
		/// count consecutives inserted/deleted lines as modified-lines (-m)
		/// </summary>
		public bool merge_opt { get; set; }
		/// <summary>
		/// Text output format
		/// - FormatOption.normal (default)	: displays only total changes per file
		/// - FormatOption.detailed			: display all counters (lines added, removed, etc.)
		///									  for each file
		/// </summary>
		public FormatOption format_opt { get; set; }
		/// <summary>
		/// wildcard pattern which allows to include only specifics files/folders in stats
		/// <example>
		/// included_files_pattern = { "**/foo/*" }
		/// excluded_files_pattern = { "**/*.txt" }
		/// --> takes only files contained in a directory named "foo", whose extension is not .txt
		/// </example>
		/// </summary>
		private readonly List<string> included_files_pattern;
		/// <summary>
		/// wildcard pattern which allows to exclude specifics files/folders from stats
		/// the pattern is matched against the file path relative to the root folder of the diff
		/// </summary>
		public readonly List<string> excluded_files_pattern;

		public DiffStatOptions()
		{
			this.merge_opt = false;
			this.format_opt = FormatOption.normal;
			this.excluded_files_pattern = new List<string>();
			this.included_files_pattern = new List<string>();
		}

	}
}
