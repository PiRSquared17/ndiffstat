using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NDiffStatLib.Utils
{
	public static class CalcUtils
	{
		/// <summary>
		/// Retourne le maximum d'une liste d'objets implémentant l'interface IComparable
		/// </summary>
		/// <typeparam name="T">Type d'objets à traiter</typeparam>
		/// <param name="args">objets à comparer</param>
		/// <returns></returns>
		public static T Max<T>( params T[] args ) where T : IComparable
		{
			T max = args[0];
			for (int i = 1 ; i < args.Length ; i++) {
				if (args[i].CompareTo(max) > 0) {
					max = args[i];
				}
			}
			return max;
		}

		/// <summary>
		/// Retourne le minimum d'une liste d'objets implémentant l'interface IComparable
		/// </summary>
		/// <typeparam name="T">Type d'objets à traiter</typeparam>
		/// <param name="args">objets à comparer</param>
		/// <returns></returns>
		public static T Min<T>( params T[] args ) where T : IComparable
		{
			T min = args[0];
			for (int i = 1 ; i < args.Length ; i++) {
				if (args[i].CompareTo(min) < 0) {
					min = args[i];
				}
			}
			return min;
		}
	}
}
