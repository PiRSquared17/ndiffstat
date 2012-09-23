using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace NDiffStatLib.DiffParsers
{
	public class FileDiff : TextWriter
	{
		public string origFile;
		public string newFile;
		public string origInfo;
		public string newInfo;
		public string origChangesetId;
		private StringBuilder _data;
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
			this._data = new StringBuilder();
			this.binary = false;
			this.deleted = false;
			this.moved = false;
		}

		public override void Write( string text )
		{
			_data.Append(text);
		}

		public override void WriteLine( string text )
		{
			_data.AppendLine(text);
		}

		public override void WriteLine( string text, params object[] args )
		{
			_data.AppendFormat(text + "\r\n", args);
		}

		public string GetData()
		{
			return _data.ToString();
		}

		public override Encoding Encoding
		{
			get { throw new NotImplementedException(); }
		}
	}
}
