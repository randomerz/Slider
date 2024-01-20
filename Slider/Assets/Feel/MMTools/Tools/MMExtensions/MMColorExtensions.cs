using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace MoreMountains.Tools
{
	/// <summary>
	/// Color extensions
	/// </summary>
	public static class MMColorExtensions
	{
		/// <summary>
		/// Adds all parts of the color and returns a float
		/// </summary>
		/// <param name="color"></param>
		/// <returns></returns>
		public static float Sum(this Color color)
		{
			return color.r + color.g + color.b + color.a;
		}

		/// <summary>
		/// Returns a mean value between r, g and b
		/// </summary>
		/// <param name="color"></param>
		/// <returns></returns>
		public static float MeanRGB(this Color color)
		{
			return (color.r + color.g + color.b) / 3f;
		}

		/// <summary>
		/// Computes the color's luminance value
		/// </summary>
		/// <param name="color"></param>
		/// <returns></returns>
		public static float Luminance(this Color color)
		{
			return 0.2126f * color.r + 0.7152f * color.g + 0.0722f * color.b;
		}
	}
}