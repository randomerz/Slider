using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
	/// <summary>
	/// Array extensions
	/// </summary>
	public static class MMArrayExtensions
	{
		/// <summary>
		/// Returns a random value inside the array
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="array"></param>
		/// <returns></returns>
		public static T MMRandomValue<T>(this T[] array)
		{
			int newIndex = Random.Range(0, array.Length);
			return array[newIndex];
		}   
        
		/// <summary>
		/// Shuffles an array
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="array"></param>
		/// <returns></returns>
		public static T[] MMShuffle<T>(this T[] array)
		{
			// Fisher Yates shuffle algorithm, see https://en.wikipedia.org/wiki/Fisher%E2%80%93Yates_shuffle
			for (int t = 0; t < array.Length; t++)
			{
				T tmp = array[t];
				int randomIndex = Random.Range(t, array.Length);
				array[t] = array[randomIndex];
				array[randomIndex] = tmp;
			}
			return array;
		}
	}
}