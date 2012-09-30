using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NDiffStatLib.Utils;

namespace NDiffStatLib.ApacheCommons.IO
{
	//
	// This class is mainly a translation in .NET of WildcardFileFilter class (Apache Common IO Project)
	//
	/// <summary>
	/// Provides methods to check if a string matches a given WildCard
	/// </summary>
	public class Wildcards
	{
		/// <summary>
		/// Checks a filename to see if it matches the specified wildcard matcher
		/// allowing control over case-sensitivity.
		/// 
		/// The wildcard matcher uses the characters '?' and '*' to represent a
		/// single or multiple (zero or more) wildcard characters.
		/// N.B. the sequence "*?" does not work properly at present in match strings.
		/// </summary>
		/// <param name="filename">the filename to match on</param>
		/// <param name="wildcardMatcher">the wildcard string to match against</param>
		/// <param name="ignoreCase">use case-insensitive string comparison</param>
		/// <returns>true if the filename matches the wilcard string</returns>
		public static bool WildcardMatch( string filename, string wildcardMatcher, bool ignoreCase)
		{
			StringComparison strCompare = ignoreCase ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture;
			
			if (filename == null && wildcardMatcher == null) {
				return true;
			}
			if (filename == null || wildcardMatcher == null) {
				return false;
			}

			List<string> wcs = SplitOnTokens(wildcardMatcher);
			bool anyChars = false;
			int textIdx = 0;
			int wcsIdx = 0;
			Stack<int[]> backtrack = new Stack<int[]>();

			// loop around a backtrack stack, to handle complex * matching
			do {
				if (backtrack.Count > 0) {
					int[] array = backtrack.Pop();
					wcsIdx = array[0];
					textIdx = array[1];
					anyChars = true;
				}

				// loop whilst tokens and text left to process
				while (wcsIdx < wcs.Count) {

					if (wcs[wcsIdx].Equals("?")) {
						// ? so move to next text char
						textIdx++;
						if (textIdx > filename.Length) {
							break;
						}
						anyChars = false;

					} else if (wcs[wcsIdx].Equals("*")) {
						// set any chars status
						anyChars = true;
						if (wcsIdx == wcs.Count - 1) {
							textIdx = filename.Length;
						}

					} else {
						// matching text token
						if (anyChars) {
							// any chars then try to locate text token
							textIdx = filename.IndexOf(wcs[wcsIdx], textIdx, strCompare);
							if (textIdx == -1) {
								// token not found
								break;
							}
							int repeat = filename.IndexOf(wcs[wcsIdx], textIdx + 1, strCompare);
							if (repeat >= 0) {
								backtrack.Push(new int[] { wcsIdx, repeat });
							}
						} else {
							// matching from current position
							if (!filename.RegionMatches(textIdx, wcs[wcsIdx], 0, wcs[wcsIdx].Length, ignoreCase)) {
								// couldnt match token
								break;
							}
						}

						// matched text token, move text index to end of matched token
						textIdx += wcs[wcsIdx].Length;
						anyChars = false;
					}

					wcsIdx++;
				}

				// full match
				if (wcsIdx == wcs.Count && textIdx == filename.Length) {
					return true;
				}

			} while (backtrack.Count > 0);

			return false;
		}

		/// <summary>
		/// Multi-wildcards version of WildcardMatch
		/// Returns true if fileName match any of the suppied wildcards
		/// </summary>
		public static bool WildcardMatchAny( string filename, IEnumerable<string> wildcardMatcher, bool ignoreCase )
		{
			foreach (string pattern in wildcardMatcher) {
				if (WildcardMatch(filename, pattern, ignoreCase)) {
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Splits a string into a number of tokens.
		/// The text is split by '?' and '*'.
		/// Where multiple '*' occur consecutively they are collapsed into a single '*'.
		/// </summary>
		/// <param name="text">the text to split</param>
		/// <returns>the array of tokens, never null</returns>
		static List<string> SplitOnTokens( string text )
		{
			// used by wildcardMatch
			// package level so a unit test may run on this

			if (text.IndexOf('?') == -1 && text.IndexOf('*') == -1) {
				return new List<string>() { text };
			}

			List<string> list = new List<string>();
			StringBuilder buffer = new StringBuilder();
			for (int i = 0 ; i < text.Length ; i++) {
				if (text[i] == '?' || text[i] == '*') {
					if (buffer.Length != 0) {
						list.Add(buffer.ToString());
						buffer.Clear();
					}
					if (text[i] == '?') {
						list.Add("?");
					} else if (list.Count == 0 ||
                        i > 0 && list[list.Count - 1].Equals("*") == false) {
						list.Add("*");
					}
				} else {
					buffer.Append(text[i]);
				}
			}
			if (buffer.Length != 0) {
				list.Add(buffer.ToString());
			}

			return list;
		}
	}
}
