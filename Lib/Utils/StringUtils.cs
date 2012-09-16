using System;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Globalization;

namespace NDiffStatLib.Utils
{
	public static class StringUtils
	{
		const string alphabet_upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
		const string alphabet_lower = "abcdefghijklmnopqrstuvwxyz";
		const string STARTING_WORD_REGEX = @"[^-'\w][-'\w]";
		const string ENDING_WORD_REGEX = @"[-'\w][^-'\w]";
		
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

		public static string XPathToLower( string s )
		{
			return "translate(" + s + ", '" + alphabet_upper + "' ,'" + alphabet_lower + "')"; 
		}

		/// <summary>
		/// Encode la chaîne pour pouvoir l'inserer dans une requête sql
		/// </summary>
		/// <param name="s">Chaine à encoder</param>
		/// <param name="defaultValue">Chaîne à retourer lorsque le paramètre d'entrée est null</param>
		/// <returns>Chaine encodée</returns>
		public static string SqlEncode( this string s, string defaultValue=null )
		{
			if (s == null) return defaultValue;
			return "'" + s.Replace("'", "''") + "'";
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

		public static string SchrinkSpaces(string text, bool trim=true) {
			// remove double spaces
			StringBuilder spaceBuffer = new StringBuilder();
			StringBuilder sb = new StringBuilder();
			foreach (char c in text) {
				if (!c.IsSpaceOrTab()) {
					if (spaceBuffer.Length > 0) {
						if (c.In('\r', '\n')) {
							// enlever les espaces /tabulations en fin de ligne
							spaceBuffer.Clear();
						} else {
							string spaces = spaceBuffer.ToString();
							if (spaces.Contains("\t")) {
								sb.Append(spaces.Replace(" ", "")); // plusieurs espaces / tabulations mélangés soit réduits aux seules tabulations
							} else {
								sb.Append(' '); // si on a que des espaces, on réduit à un seul espace
							}
						}
						spaceBuffer.Clear();
					}
					sb.Append(c);
				} else {
					spaceBuffer.Append(c);
				}
			}
			string remainingSpaces = spaceBuffer.ToString();
			if (remainingSpaces.Contains("\t")) {
				sb.Append(remainingSpaces.Replace(" ", "")); // plusieurs espaces / tabulations mélangés sont réduits aux seules tabulations
			} else {
				sb.Append(' '); // si on a que des espaces, on réduit à un seul espace
			}
			if (trim && sb.Length > 0) {
				if (sb[0].IsSpaceOrTab()) sb.Remove(0, 1);
				if (sb[sb.Length-1].IsSpaceOrTab()) sb.Remove(sb.Length-1, 1); 
			}
			return sb.ToString();
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
		/// Retourne la position du 1er mot dans la chaîne (caractère "non mot" suivi d'un caractère "mot")
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public static int GetFirstWordBeginingPos( string s, int startIndex, int length )
		{
			Regex wordReg = new Regex(STARTING_WORD_REGEX, RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
			Match match = wordReg.Match(s, startIndex, length);
			if (match.Success) {
				return match.Index + 1;
			} else {
				return -1;
			}
		}

		/// <summary>
		/// Retourne la position de fin du 1er mot dans la chaîne (caractère "non mot" suivi d'un caractère "mot")
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public static int GetFirstWordEndPos( string s, int startIndex, int length )
		{
			Regex wordReg = new Regex(ENDING_WORD_REGEX, RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
			Match match = wordReg.Match(s, startIndex, length);
			if (match.Success) {
				return match.Index + 1;
			} else {
				return -1;
			}
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
	}
}
