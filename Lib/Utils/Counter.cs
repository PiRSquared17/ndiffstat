using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDiffStatLib.Utils
{
	public class Counter
	{
		public int Value { get; private set; }
		public Counter()
		{
			this.Value = 0;
		}

		public void Reset()
		{
			this.Value = 0;
		}

		public void Increment()
		{
			this.Value++;
		}
	}
}
