using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NDiffStatLib.Utils;

namespace NDiffStatLib.DiffParsers
{
	public class Revision : IEquatable<Revision>
	{
		public const string HEAD = "HEAD";
		public const string UNKNOWN = "UNKNOWN";
		public const string PRE_CREATION = "PRE_CREATION";

		public readonly int? revNumber;
		public readonly string rev_str;

		public Revision( string rev_str )
		{
			if (StringUtils.IsNaturalNumberString(rev_str)) {
				revNumber = int.Parse(rev_str);
			} else {
				if (!rev_str.In(HEAD, UNKNOWN, PRE_CREATION)) {
					throw new ArgumentNullException("Invalid argument for Revision constructor : '" + rev_str + "'");
				}
			}
			this.rev_str = rev_str;
		}

		public Revision( int rev_num )
		{
			revNumber = rev_num;
			rev_str = rev_num.ToString();
		}

		#region IEquatable<Revision> Membres
		public bool Equals( Revision other )
		{
			if (other == null) return false;
			return this.rev_str == other.rev_str;
		}
		#endregion
	}
}
