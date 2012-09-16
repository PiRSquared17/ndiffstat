using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace NDiffStatLib.Utils
{
	public static class FileUtils
	{
		public static FileStream GetReadonlyStream( string path )
		{
			return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
		}
	}
}
