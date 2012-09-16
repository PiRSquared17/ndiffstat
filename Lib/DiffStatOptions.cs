using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NDiffStatLib
{
	[Flags]
	public enum FormatOption
	{
		concise = 0,
		normal = 1,
		filled = 2,
		verbose = 4
	}

	public class DiffStatOptions
	{
		/// <summary>
		/// merge insert/delete data in chunks as modified-lines (-m)
		/// </summary>
		public bool merge_opt = false;
		/// <summary>
		/// format (0=concise, 1=normal, 2=filled, 4=values) (-f)
		/// </summary>
		public FormatOption format_opt = FormatOption.normal;
	}
}
