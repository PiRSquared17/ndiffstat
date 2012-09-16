using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace NDiffStatLib.Utils
{
	public static class MiscUtils {
		
		/// <summary>
		/// Renvoie si la valeur est dans le tableau
		/// </summary>
		/// <typeparam name="T">Type de la valeur</typeparam>
		/// <param name="value">Valeur à rechercher</param>
		/// <param name="objects">Liste de valeurs dans lesquelles la recherche s'applique</param>
		/// <returns></returns>
		public static bool In<T>( this T value, params T[] objects )
		{
			return Array.IndexOf<T>(objects, value) > -1;
		}
	}
}
