using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace NDiffStatLib.DiffParsers
{
	public abstract class FileDiffFactory
	{
		public abstract FileDiff Create();
	}

	public class MockFileDiffFactory : FileDiffFactory
	{
		public override FileDiff Create()
		{
			return new MockFileDiff();
		}
	}

	public class FileDiffWithCounterFactory : FileDiffFactory
	{
		private readonly bool merge_opt;
		
		public FileDiffWithCounterFactory( bool merge_opt ) : base()
		{
			this.merge_opt = merge_opt;
		}
		
		public override FileDiff Create()
		{
			return new FileDiffWithCounter(this.merge_opt);
		}
	}

	public abstract class FileDiff
	{
		public string origFile;
		public string newFile;
		public string origInfo;
		public string newInfo;
		public string origChangesetId;
		public bool binary;
		public bool deleted;
		public bool moved;

		public FileDiff()
		{
			this.origFile = null;
			this.newFile = null;
			this.origInfo = null;
			this.newInfo = null;
			this.origChangesetId = null;
			this.binary = false;
			this.deleted = false;
			this.moved = false;
		}

		public abstract void WriteLine( string text, bool isHeader );
	}

	public class MockFileDiff : FileDiff
	{
		public MockFileDiff() : base() { }

		public override void WriteLine( string text, bool isHeader ) { }
	}

	public class FileDiffWithCounter : FileDiff
	{
		public readonly StatsCounter statsCounter;

		public FileDiffWithCounter( bool merge_opt ) : base()
		{
			this.statsCounter = new StatsCounter(merge_opt);
		}

		public override void WriteLine( string text, bool isHeader ) {

			if (isHeader) {
				return;
			}
			if (text.StartsWith("+") && !text.StartsWith("+++ ")) {
				statsCounter.LineFound(LinesType.added);
			} else if (text.StartsWith("-") && !text.StartsWith("--- ")) {
				statsCounter.LineFound(LinesType.removed);
			} else if (text.StartsWith("\\ ")) {
				statsCounter.LineFound(LinesType.escaped);
			} else {
				statsCounter.LineFound(LinesType.others);
			}
		}
	}
}
