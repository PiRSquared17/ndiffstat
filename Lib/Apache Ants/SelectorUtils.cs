using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDiffStatLib.ApacheAnt
{
	/// <summary>
	/// This is a utility class used by selectors and DirectoryScanner. The
	/// functionality more properly belongs just to selectors, but unfortunately
	/// DirectoryScanner exposed these as protected methods. Thus we have to
	/// support any subclasses of DirectoryScanner that may access these methods.
	/// This is a Singleton.
	/// </summary>
	public sealed class SelectorUtils
	{
		/// <summary>
		/// The pattern that matches an arbitrary number of directories.
		/// @since Ant 1.8.0
		/// </summary>
		public const string DEEP_TREE_MATCH = "**";
		private static readonly SelectorUtils instance = new SelectorUtils();

		/// <summary>
		/// Private Constructor
		/// </summary>
		private SelectorUtils() {}

		/// <summary>
		/// Retrieves the instance of the Singleton.
		/// </summary>
		/// <returns>singleton instance</returns>
		public static SelectorUtils getInstance()
		{
			return instance;
		}

		/// <summary>
		/// Tests whether or not a given path matches the start of a given
		/// pattern up to the first "**".
		/// This is not a general purpose test and should only be used if you
		/// can live with false positives. For example, <code>pattern=**\a</code>
		/// and <code>str=b</code> will yield <code>true</code>.
		/// </summary>
		/// <param name="pattern">The pattern to match against. Must not be <code>null</code>.</param>
		/// <param name="str">The path to match, as a string. Must not be <code>null</code>.</param>
		/// <returns>whether or not a given path matches the start of a given pattern up to the first "**".</returns>
		public static bool matchPatternStart( string pattern, string str )
		{
			return matchPatternStart(pattern, str, true);
		}

		/// <summary>
		/// Tests whether or not a given path matches the start of a given
		/// pattern up to the first "**".
		/// This is not a general purpose test and should only be used if you
		/// can live with false positives. For example, <code>pattern=**\a</code>
		/// and <code>str=b</code> will yield <code>true</code>.
		/// </summary>
		/// <param name="pattern">The pattern to match against. Must not be <code>null</code>.</param>
		/// <param name="str">The path to match, as a string. Must not be <code>null</code>.</param>
		/// <param name="isCaseSensitive">Whether or not matching should be performed case sensitively.</param>
		/// <returns>whether or not a given path matches the start of a given pattern up to the first "**".</returns>
		public static bool matchPatternStart( string pattern, string str, bool isCaseSensitive )
		{
			// When str starts with a directory separator, pattern has to start with a
			// File.separator.
			// When pattern starts with a directory separator, str has to start with a
			// File.separator.
			if ((str.Length > 0 && FileUtils.IsDirectorySeparator(str[0])) != (pattern.Length > 0 && FileUtils.IsDirectorySeparator(pattern[0]))) {
				return false;
			}

			string[] patDirs = tokenizePathAsArray(pattern);
			string[] strDirs = tokenizePathAsArray(str);
			return matchPatternStart(patDirs, strDirs, isCaseSensitive);
		}


		/// <summary>
		/// Tests whether or not a given path matches the start of a given
		/// pattern up to the first "**".
		/// This is not a general purpose test and should only be used if you
		/// can live with false positives. For example, <code>pattern=**\a</code>
		/// and <code>str=b</code> will yield <code>true</code>.
		/// </summary>
		/// <param name="patDirs">The tokenized pattern to match against. Must not be <code>null</code>.</param>
		/// <param name="strDirs">The tokenized path to match. Must not be <code>null</code>.</param>
		/// <param name="isCaseSensitive">Whether or not matching should be performed case sensitively.</param>
		/// <returns>whether or not a given path matches the start of a given pattern up to the first "**".</returns>
		static bool matchPatternStart( string[] patDirs, string[] strDirs, bool isCaseSensitive )
		{
			int patIdxStart = 0;
			int patIdxEnd = patDirs.Length - 1;
			int strIdxStart = 0;
			int strIdxEnd = strDirs.Length - 1;

			// up to first '**'
			while (patIdxStart <= patIdxEnd && strIdxStart <= strIdxEnd) {
				string patDir = patDirs[patIdxStart];
				if (patDir.Equals(DEEP_TREE_MATCH)) {
					break;
				}
				if (!match(patDir, strDirs[strIdxStart], isCaseSensitive)) {
					return false;
				}
				patIdxStart++;
				strIdxStart++;
			}

			// CheckStyle:SimplifyBooleanReturnCheck OFF
			// Check turned off as the code needs the comments for the various
			// code paths.
			if (strIdxStart > strIdxEnd) {
				// string is exhausted
				return true;
			} else if (patIdxStart > patIdxEnd) {
				// string not exhausted, but pattern is. Failure.
				return false;
			} else {
				// pattern now holds ** while string is not exhausted
				// this will generate false positives but we can live with that.
				return true;
			}
		}

		/// <summary>
		/// Tests whether or not a given path matches a given pattern.
		/// If you need to call this method multiple times with the same
		/// pattern you should rather use TokenizedPattern
		/// <see cref="TokenizedPattern"/>
		/// </summary>
		/// <param name="pattern">The pattern to match against. Must not be <code>null</code>.</param>
		/// <param name="str">The path to match, as a string. Must not be <code>null</code>.</param>
		/// <param name="isCaseSensitive">Whether or not matching should be performed case sensitively.</param>
		/// <returns><code>true</code> if the pattern matches against the string, or <code>false</code> otherwise.</returns>
		public static bool matchPath( string pattern, string str, bool isCaseSensitive=true )
		{
			string[] patDirs = tokenizePathAsArray(pattern);
			return matchPath(patDirs, tokenizePathAsArray(str), isCaseSensitive);
		}

		/// <summary>
		/// Multi-patterns version of matchPath
		/// Returns true if the input string match any of the patterns
		/// </summary>
		public static bool matchPath( IEnumerable<string> patterns, string str, bool isCaseSensitive=true )
		{
			foreach (string pattern in patterns) {
				if (matchPath(pattern, str, isCaseSensitive)) {
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Core implementation of matchPath.  It is isolated so that it
		/// can be called from TokenizedPattern.
		/// </summary>
		static bool matchPath( string[] tokenizedPattern, string[] strDirs,
								 bool isCaseSensitive )
		{
			int patIdxStart = 0;
			int patIdxEnd = tokenizedPattern.Length - 1;
			int strIdxStart = 0;
			int strIdxEnd = strDirs.Length - 1;

			// up to first '**'
			while (patIdxStart <= patIdxEnd && strIdxStart <= strIdxEnd) {
				string patDir = tokenizedPattern[patIdxStart];
				if (patDir.Equals(DEEP_TREE_MATCH)) {
					break;
				}
				if (!match(patDir, strDirs[strIdxStart], isCaseSensitive)) {
					return false;
				}
				patIdxStart++;
				strIdxStart++;
			}
			if (strIdxStart > strIdxEnd) {
				// string is exhausted
				for (int i = patIdxStart ; i <= patIdxEnd ; i++) {
					if (!tokenizedPattern[i].Equals(DEEP_TREE_MATCH)) {
						return false;
					}
				}
				return true;
			} else {
				if (patIdxStart > patIdxEnd) {
					// string not exhausted, but pattern is. Failure.
					return false;
				}
			}

			// up to last '**'
			while (patIdxStart <= patIdxEnd && strIdxStart <= strIdxEnd) {
				string patDir = tokenizedPattern[patIdxEnd];
				if (patDir.Equals(DEEP_TREE_MATCH)) {
					break;
				}
				if (!match(patDir, strDirs[strIdxEnd], isCaseSensitive)) {
					return false;
				}
				patIdxEnd--;
				strIdxEnd--;
			}
			if (strIdxStart > strIdxEnd) {
				// string is exhausted
				for (int i = patIdxStart ; i <= patIdxEnd ; i++) {
					if (!tokenizedPattern[i].Equals(DEEP_TREE_MATCH)) {
						return false;
					}
				}
				return true;
			}

			while (patIdxStart != patIdxEnd && strIdxStart <= strIdxEnd) {
				int patIdxTmp = -1;
				for (int i = patIdxStart + 1 ; i <= patIdxEnd ; i++) {
					if (tokenizedPattern[i].Equals(DEEP_TREE_MATCH)) {
						patIdxTmp = i;
						break;
					}
				}
				if (patIdxTmp == patIdxStart + 1) {
					// '**/**' situation, so skip one
					patIdxStart++;
					continue;
				}
				// Find the pattern between padIdxStart & padIdxTmp in str between
				// strIdxStart & strIdxEnd
				int patLength = (patIdxTmp - patIdxStart - 1);
				int strLength = (strIdxEnd - strIdxStart + 1);
				int foundIdx = -1;
				bool skipCurrentStrLoop = false;
				for (int i = 0 ; i <= strLength - patLength ; i++) {
					for (int j = 0 ; j < patLength ; j++) {
						string subPat = tokenizedPattern[patIdxStart + j + 1];
						string subStr = strDirs[strIdxStart + i + j];
						if (!match(subPat, subStr, isCaseSensitive)) {
							skipCurrentStrLoop = true;
							break;
						}
					}
					if (skipCurrentStrLoop) {
						skipCurrentStrLoop = false;
						continue;
					}
					foundIdx = strIdxStart + i;
					break;
				}

				if (foundIdx == -1) {
					return false;
				}

				patIdxStart = patIdxTmp;
				strIdxStart = foundIdx + patLength;
			}

			for (int i = patIdxStart ; i <= patIdxEnd ; i++) {
				if (!tokenizedPattern[i].Equals(DEEP_TREE_MATCH)) {
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Tests whether or not a string matches against a pattern.
		/// The pattern may contain two special characters:
		/// '*' means zero or more characters
		/// '?' means one and only one character
		/// </summary>
		/// <param name="pattern">The pattern to match against. Must not be <code>null</code>.</param>
		/// <param name="str">    The string which must be matched against the pattern. Must not be <code>null</code>.</param>
		///
		/// <returns><code>true</code> if the string matches against the pattern,
		///         or <code>false</code> otherwise.</returns>
		public static bool match( string pattern, string str )
		{
			return match(pattern, str, true);
		}

		/// <summary>
		/// Tests whether or not a string matches against a pattern.
		/// The pattern may contain two special characters:
		/// '*' means zero or more characters
		/// '?' means one and only one character
		/// </summary>
		/// <param name="pattern">
		///		The pattern to match against. Must not be <code>null</code>.
		/// </param>
		/// <param name="str">
		///		The string which must be matched against the pattern. 
		///		Must not be <code>null</code>.
		///	</param>
		/// <param name="caseSensitive">
		///		Whether or not matching should be performed case sensitively.
		///	</param>
		/// <returns>
		///		<code>true</code> if the string matches against the pattern,
		///     or <code>false</code> otherwise.
		/// </returns>
		public static bool match( string pattern, string str, bool caseSensitive )
		{
			int patIdxStart = 0;
			int patIdxEnd = pattern.Length - 1;
			int strIdxStart = 0;
			int strIdxEnd = str.Length - 1;
			char ch;

			bool containsStar = false;
			for (int i = 0 ; i < pattern.Length ; i++) {
				if (pattern[i] == '*') {
					containsStar = true;
					break;
				}
			}

			if (!containsStar) {
				// No '*'s, so we make a shortcut
				if (patIdxEnd != strIdxEnd) {
					return false; // Pattern and string do not have the same size
				}
				for (int i = 0 ; i <= patIdxEnd ; i++) {
					ch = pattern[i];
					if (ch != '?') {
						if (different(caseSensitive, ch, str[i])) {
							return false; // Character mismatch
						}
					}
				}
				return true; // string matches against pattern
			}

			if (patIdxEnd == 0) {
				return true; // Pattern contains only '*', which matches anything
			}

			// Process characters before first star
			while (true) {
				ch = pattern[patIdxStart];
				if (ch == '*' || strIdxStart > strIdxEnd) {
					break;
				}
				if (ch != '?') {
					if (different(caseSensitive, ch, str[strIdxStart])) {
						return false; // Character mismatch
					}
				}
				patIdxStart++;
				strIdxStart++;
			}
			if (strIdxStart > strIdxEnd) {
				// All characters in the string are used. Check if only '*'s are
				// left in the pattern. If so, we succeeded. Otherwise failure.
				return allStars(pattern, patIdxStart, patIdxEnd);
			}

			// Process characters after last star
			while (true) {
				ch = pattern[patIdxEnd];
				if (ch == '*' || strIdxStart > strIdxEnd) {
					break;
				}
				if (ch != '?') {
					if (different(caseSensitive, ch, str[strIdxEnd])) {
						return false; // Character mismatch
					}
				}
				patIdxEnd--;
				strIdxEnd--;
			}
			if (strIdxStart > strIdxEnd) {
				// All characters in the string are used. Check if only '*'s are
				// left in the pattern. If so, we succeeded. Otherwise failure.
				return allStars(pattern, patIdxStart, patIdxEnd);
			}

			// process pattern between stars. padIdxStart and patIdxEnd point
			// always to a '*'.
			while (patIdxStart != patIdxEnd && strIdxStart <= strIdxEnd) {
				int patIdxTmp = -1;
				for (int i = patIdxStart + 1 ; i <= patIdxEnd ; i++) {
					if (pattern[i] == '*') {
						patIdxTmp = i;
						break;
					}
				}
				if (patIdxTmp == patIdxStart + 1) {
					// Two stars next to each other, skip the first one.
					patIdxStart++;
					continue;
				}
				// Find the pattern between padIdxStart & padIdxTmp in str between
				// strIdxStart & strIdxEnd
				int patLength = (patIdxTmp - patIdxStart - 1);
				int strLength = (strIdxEnd - strIdxStart + 1);
				int foundIdx = -1;
				bool skipCurrentStrLoop = false;
				for (int i = 0 ; i <= strLength - patLength ; i++) {
					for (int j = 0 ; j < patLength ; j++) {
						ch = pattern[patIdxStart + j + 1];
						if (ch != '?') {
							if (different(caseSensitive, ch, str[strIdxStart + i + j])) {
								skipCurrentStrLoop = true;
								break;
							}
						}
					}
					if (skipCurrentStrLoop) {
						skipCurrentStrLoop = false;
						continue;
					}
					foundIdx = strIdxStart + i;
					break;
				}

				if (foundIdx == -1) {
					return false;
				}

				patIdxStart = patIdxTmp;
				strIdxStart = foundIdx + patLength;
			}

			// All characters in the string are used. Check if only '*'s are left
			// in the pattern. If so, we succeeded. Otherwise failure.
			return allStars(pattern, patIdxStart, patIdxEnd);
		}

		private static bool allStars( string pattern, int start, int end )
		{
			for (int i = start ; i <= end ; ++i) {
				if (pattern[i] != '*') {
					return false;
				}
			}
			return true;
		}

		private static bool different(
			bool caseSensitive, char ch, char other )
		{
			return caseSensitive
            ? ch != other
            : char.ToUpper(ch) != char.ToUpper(other);
		}

		/// <summary>
		/// Same as {@link #tokenizePath tokenizePath} but hopefully faster.
		/// </summary>
		static string[] tokenizePathAsArray( string path )
		{
			string root = null;
			if (FileUtils.isAbsolutePath(path)) {
				string[] s = FileUtils.dissect(path);
				root = s[0];
				path = s[1];
			}
			int start = 0;
			int len = path.Length;
			int count = 0;
			for (int pos = 0 ; pos < len ; pos++) {
				if (FileUtils.IsDirectorySeparator(path[pos])) {
					if (pos != start) {
						count++;
					}
					start = pos + 1;
				}
			}
			if (len != start) {
				count++;
			}
			string[] l = new string[count + ((root == null) ? 0 : 1)];

			if (root != null) {
				l[0] = root;
				count = 1;
			} else {
				count = 0;
			}
			start = 0;
			for (int pos = 0 ; pos < len ; pos++) {
				if (FileUtils.IsDirectorySeparator(path[pos])) {
					if (pos != start) {
						string tok = path.Substring(start, pos-start);
						l[count++] = tok;
					}
					start = pos + 1;
				}
			}
			if (len != start) {
				string tok = path.Substring(start);
				l[count/*++*/] = tok;
			}
			return l;
		}

		/// <summary>
		/// Returns dependency information on these two files. If src has been
		/// modified later than target, it returns true. If target doesn't exist,
		/// it likewise returns true. Otherwise, target is newer than src and
		/// is not out of date, thus the method returns false. It also returns
		/// false if the src file doesn't even exist, since how could the
		/// target then be out of date.
		/// </summary>
		/// <param name="src">the original file
		/// <param name="target">the file being compared against
		/// <param name="granularity">the amount in seconds of slack we will give in
		///        determining out of dateness
		/// <returns>whether the target is out of date</returns>
		public static bool isOutOfDate( FileInfo src, FileInfo target, int granularity )
		{
			if (!src.Exists) {
				return false;
			}
			if (!target.Exists) {
				return true;
			}
			if ((src.LastWriteTime.AddSeconds(-granularity)) > target.LastWriteTime) {
				return true;
			}
			return false;
		}

		/// <summary>
		/// Tests if a string contains stars or question marks
		/// </summary>
		/// <param name="input">a string which one wants to test for containing wildcard
		/// <returns>true if the string contains at least a star or a question mark</returns>
		public static bool hasWildcards( string input )
		{
			return (input.IndexOf('*') != -1 || input.IndexOf('?') != -1);
		}
	}
}
