using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NDiffStatLib.Utils;

namespace NDiffStatLib
{
	public enum LinesType {
		added,
		removed,
		others
	}
	
	public class StatsCounter
	{
		private readonly bool merge_opt;
		public int adds { get; private set; }
		public int removes { get; private set; }
		public int modifs { get; private set; }
		/// <summary>
		/// Object witch serves as buffer for added / removed lines counts
		/// This allow to compute modified lines (if required) as
		/// min ( adds, removes )	where adds / removes
		/// for each chunk consisting of consecutives added/removed lines.
		/// </summary>
		private ChunkStat tempStats;
		public int total
		{
			get { return adds + removes + modifs; }
		}

		public StatsCounter( bool merge_opt ) : this(merge_opt, 0, 0, 0) { }

		public StatsCounter( bool merge_opt, int adds, int removes, int modifs )
		{
			this.merge_opt = merge_opt;
			this.adds = adds;
			this.removes = removes;
			this.modifs = modifs;
			this.tempStats = new ChunkStat();
		}
		public void LineFound( LinesType type )
		{
			switch (type) {
				case LinesType.added:
					this.tempStats.adds++;
					break;
				case LinesType.removed:
					this.tempStats.removes++;
					break;
				case LinesType.others:
					ClearTempStats();
					break;
				default:
					throw new ArgumentException(string.Format("Unknown type {0}", type));
			}
		}

		/// <summary>
		/// Add the corresponding counters of other in current object
		/// </summary>
		/// <param name="other"></param>
		public void SumWith( StatsCounter other )
		{
			this.adds += other.adds;
			this.removes += other.removes;
			this.modifs += other.modifs;
		}

		/// <summary>
		/// Calculates the number of lines modified from tempStats counters (if required)
		/// The number of modified lines is computed as min ( adds, removes )
		/// Add tempsStats counters to the current object and reset tempsStats counter
		/// </summary>
		/// <param name="merge_opt">true if we should compute the number of modified lines</param>
		public void ClearTempStats()
		{
			if (this.tempStats.adds == 0 && this.tempStats.removes == 0) return;
			int modifs = this.merge_opt ? CalcUtils.Min(this.tempStats.adds, this.tempStats.removes) : 0;
			this.adds += (this.tempStats.adds - modifs);
			this.removes += (this.tempStats.removes - modifs);
			this.modifs += modifs;
			this.tempStats.Reset();
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
