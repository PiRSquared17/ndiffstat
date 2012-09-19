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
				// integer revision number
				// Subversion revisions for example
				revNumber = int.Parse(rev_str);
			} else if (StringUtils.IsAlphaNumString(rev_str)) { 
				// alpha num value (changeset id)
				// ok, that's fine
			} else if (!rev_str.In(HEAD, UNKNOWN, PRE_CREATION)) {
				throw new ArgumentNullException("Invalid argument for Revision constructor : '" + rev_str + "'");
			}
			this.rev_str = rev_str;
		}

		public Revision( int rev_num )
		{
			revNumber = rev_num;
			rev_str = rev_num.ToString();
		}

		public override bool Equals( object obj )
		{
			if (obj == null) return false;
			Revision rev = obj as Revision;
			if (rev == null) return false;
			return this.Equals(rev);
		}

		public override int GetHashCode()
		{
			return rev_str.GetHashCode();
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
