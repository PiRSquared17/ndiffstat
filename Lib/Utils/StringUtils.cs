using System;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Globalization;
using System.IO;

namespace NDiffStatLib.Utils
{
	public static class StringUtils
	{
		/// <summary>
		/// Extrait la sous-chaîne formée des n premiers caractères d'une chaîne donnée.
		/// </summary>
		/// <param name="text">chaîne d'entrée</param>
		/// <param name="maxLength">longueur de la sous-chaîne à extraire</param>
		/// <returns></returns>
		public static string TruncateText( string text, int maxLength )
		{
			return TruncateText(text, 0, maxLength);
		}

		/// <summary>
		/// Extrait la sous-chaîne formée des n premiers caractères d'une chaîne donnée.
		/// </summary>
		/// <param name="text">chaîne d'entrée</param>
		/// <param name="maxLength">longueur de la sous-chaîne à extraire</param>
		/// <returns></returns>
		public static string TruncateText( string text, int startIndex, int maxLength )
		{
			if (text == null) return null;
			if (startIndex + maxLength >= text.Length) {
				return text.Substring(startIndex);
			} else {
				return text.Substring(startIndex, maxLength);
			}
		}

		public static string AsString<T>( this T? input ) where T:struct 
		{
			if (input.HasValue) return input.ToString();
			else return "null";
		}

		/// <summary>
		/// Renvoie l'index du 1er caractère dans une chaîne qui remplit une condition donnée.
		/// Valeur de retour par défaut : -1;
		/// </summary>
		public static int FirstIndexOf( this string s, int startIndex, Func<char, bool> predicate )
		{
			for (int i=startIndex ; i<s.Length ; i++) {
				if (predicate(s[i])) return i;
			}
			return -1;
		}

		/// <summary>
		/// Returns true if text is null or empty string
		/// (same as System.String.IsNullOrEmpty(...))
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		public static bool IsNullOrEmpty( this string text )
		{
			if (text != null) {
				return (text.Length == 0);
			}
			return true;
		}

		/// <summary>
		/// Returns true if text is null or contains only white spaces
		/// (same as System.String.IsNullOrWhiteSpace(...))
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		public static bool IsNullOrWhiteSpace( this string text )
		{
			if (text != null) {
				for (int i = 0 ; i < text.Length ; i++) {
					if (!char.IsWhiteSpace(text[i])) {
						return false;
					}
				}
			}
			return true;
		}

		public static string ReplaceChars( string text, string charsToReplace, char? replacement )
		{
			int index = -1;
			StringBuilder sb = new StringBuilder();
			while (++index < text.Length) {
				if (charsToReplace.IndexOf(text[index]) != -1) {
					if (replacement.HasValue) {
						sb.Append(replacement);
					}
				} else {
					sb.Append(text[index]);
				}
			}
			return sb.ToString();
		}

		public static bool IsSpaceOrTab( this char c )
		{
			return (c == ' ') || (c ==  '\t');
		}

		/// <summary>
		/// Supprime les accents d'une chaîne de caractères donnée
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public static string RemoveDiacritics( string s )
		{
			string normalized = s.Normalize(NormalizationForm.FormD);
			StringBuilder sb = new StringBuilder();

			for (int ich = 0 ; ich < normalized.Length ; ich++) {
				UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(normalized[ich]);
				if (uc != UnicodeCategory.NonSpacingMark) {
					sb.Append(normalized[ich]);
				}
			}

			return (sb.ToString().Normalize(NormalizationForm.FormC));
		}

		public static IEnumerable<string> ReadAsCollection( this TextReader reader )
		{
			string currentLine;
			while ((currentLine = reader.ReadLine()) != null) {
				yield return currentLine;
			}
		}

		/// <summary>
		///	Check whether a string is a representation of a natural number.
		///	Returns false for null/empty string
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public static bool IsNaturalNumberString( string s )
		{
			if (s.IsNullOrEmpty()) {
				return false;
			} else {
				for (int i = 0 ; i < s.Length ; i++) {
					if (!Char.IsDigit(s[i])) return false;
				}
				return true;
			}
		}

		/// <summary>
		///	Check whether a string is a representation of a natural number.
		///	Returns false for null/empty string
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public static bool IsAlphaNumString( string s )
		{
			if (s.IsNullOrEmpty()) {
				return false;
			} else {
				for (int i = 0 ; i < s.Length ; i++) {
					if (!char.IsDigit(s[i]) && !((int)char.ToLower(s[i])).IsBetween((int)'a', (int)'f')) return false;
				}
				return true;
			}
		}

		/// <summary>
		/// Tests if two string regions are equal.
		/// 
		/// A substring of this <c>String</c> object is compared to a substring
		/// of the argument other. The result is true if these substrings
		/// represent identical character sequences. The substring of this
		/// <c>String</c> object to be compared begins at index <c>toffset</c>
		/// and has length <c>len</c>. The substring of other to be compared
		/// begins at index <c>ooffset</c> and has length <c>len</c>. The
		/// result is <c>false</c> if and only if at least one of the following
		/// is true:
		/// * <c>toffset</c> is negative.
		/// * <c>ooffset</c> is negative.
		/// * <c>toffset+len</c> is greater than the length of this <c>String</c> object.
		/// * <c>ooffset+len</c> is greater than the length of the other argument.
		/// * There is some nonnegative integer <i>k</i> less than <c>len</c> such that:
		/// <code>s.CharAt(toffset+<i>k</i>) != other.CharAt(ooffset+<i>k</i>)</code>
		/// </summary>
		/// <param name="s">reference string</param>
		/// <param name="toffset">the starting offset of the subregion in reference string</param>
		/// <param name="other">the string argument</param>
		/// <param name="ooffset">the starting offset of the subregion in the string argument</param>
		/// <param name="len">the number of characters to compare</param>
		/// <returns></returns>
		public static bool RegionMatches( this string s, int toffset, String other, int ooffset, int len, bool ignoreCase )
		{
			// Note: toffset, ooffset, or len might be near -1>>>1.
			if ((ooffset < 0) || (toffset < 0) || (toffset > (long)s.Length - len)
            || (ooffset > (long)other.Length - len)) {
				return false;
			}
			while (len-- > 0) {
				char c1 = s[toffset++];
				char c2 = other[ooffset++];
				if (c1 == c2) continue;
				if (ignoreCase) {
					// If characters don't match but case may be ignored,
					// try converting both characters to uppercase.
					// If the results match, then the comparison scan should continue.
					if (char.ToUpper(c1) == char.ToUpper(c2)) {
						continue;
					}
				}
				return false;
			}
			return true;
		}
	}
}
