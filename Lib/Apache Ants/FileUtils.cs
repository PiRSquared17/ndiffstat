using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDiffStatLib.ApacheAnt
{
	public static class FileUtils
	{
		public static readonly char[] DIR_SEPARATORS = new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };

		/// <summary>
		/// Verifies that the specified filename represents an absolute path.
		/// Differs from new java.io.File("filename").isAbsolute() in that a path
		/// beginning with a double file separator--signifying a Windows UNC--must
		/// at minimum match "\\a\b" to be considered an absolute path.
		/// </summary>
		/// <param name="filename">the filename to be checked.</param>
		/// <returns>true if the filename represents an absolute path.</returns>
		public static bool isAbsolutePath( string filename )
		{
			int len = filename.Length;
			if (len == 0) {
				return false;
			}
			char sep = Path.DirectorySeparatorChar;
			filename = filename.Replace('/', sep).Replace('\\', sep);
			char c = filename[0];
			if (c == sep) {
				// CheckStyle:MagicNumber OFF
				if (!(len > 4 && filename[1] == sep)) {
					return false;
				}
				// CheckStyle:MagicNumber ON
				int nextsep = filename.IndexOf(sep, 2);
				return nextsep > 2 && nextsep + 1 < len;
			}
			int colon = filename.IndexOf(':');
			return (char.IsLetter(c) && colon == 1
                && filename.Length > 2 && filename[2] == sep);
		}

		/// <summary>
		/// Dissect the specified absolute path.
		/// </summary>
		/// <param name="path">path the path to dissect.</param>
		/// <returns>string[] {root, remaining path}</returns>
		public static string[] dissect( string path )
		{
			char sep = Path.DirectorySeparatorChar;
			path = path.Replace('/', sep).Replace('\\', sep);
			
			// make sure we are dealing with an absolute path
			if (!isAbsolutePath(path)) {
				throw new Exception(path + " is not an absolute path");
			}
			String root = null;
			int colon = path.IndexOf(':');
			if (colon > 0) {

				int next = colon + 1;
				root = path.Substring(0, next);
				root += sep;
				//remove the initial separator; the root has it.
				next = (path[next] == sep) ? next + 1 : next;

				StringBuilder sbPath = new StringBuilder();
				// Eliminate consecutive slashes after the drive spec:
				for (int i = next ; i < path.Length ; i++) {
					if (path[i] != sep || path[i - 1] != sep) {
						sbPath.Append(path[i]);
					}
				}
				path = sbPath.ToString();
			} else if (path.Length > 1 && path[1] == sep) {
				// UNC drive
				int nextsep = path.IndexOf(sep, 2);
				nextsep = path.IndexOf(sep, nextsep + 1);
				root = (nextsep > 2) ? path.Substring(0, nextsep + 1) : path;
				path = path.Substring(root.Length);
			} else {
				root = Path.DirectorySeparatorChar.ToString();
				path = path.Substring(1);
			}
			return new String[] { root, path };
		}

		public static bool IsDirectorySeparator( char c ) {
			foreach (char separator in DIR_SEPARATORS) {
				if (c == separator) return true;
			}
			return false;
		}
	}
}
