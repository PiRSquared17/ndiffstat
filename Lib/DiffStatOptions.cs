using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NDiffStatLib
{
	[Flags]
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
		public bool merge_opt = false;
		/// <summary>
		/// Text output format
		/// - FormatOption.normal (default)	: displays only total changes per file
		/// - FormatOption.detailed			: display all counters (lines added, removed, etc.)
		///									  for each file
		/// </summary>
		public FormatOption format_opt = FormatOption.normal;
		/// <summary>
		/// regex pattern which allows to exclude specifics files from diff
		/// </summary>
		public string excluded_files_pattern;

	}
}
