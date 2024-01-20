using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;

namespace MoreMountains.Tools
{
	/// <summary>
	/// String helpers
	/// </summary>
	public static class MMString 
	{
		/// <summary>
		/// Uppercases the first letter of the parameter string
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public static string UppercaseFirst(string s)
		{
			if (string.IsNullOrEmpty(s))
			{
				return string.Empty;
			}
			return char.ToUpper(s[0]) + s.Substring(1);
		}
        
		/// <summary>
		/// Returns the length of a rich text, excluding its tags
		/// </summary>
		/// <param name="richText"></param>
		/// <returns></returns>
		public static int RichTextLength(string richText)
		{
			int richTextLength = 0;
			bool insideTag = false;

			richText = richText.Replace("<br>", "-");
	        
			foreach (char character in richText)
			{
				if (character == '<')
				{
					insideTag = true;
					continue;
				}
				else if (character == '>')
				{
					insideTag = false;
				}
				else if (!insideTag)
				{
					richTextLength++;
				}
			}
 
			return richTextLength;
		}
        
		/// <summary>
		/// Elegantly uppercases the first letter of every word in a string
		/// </summary>
		/// <param name="title"></param>
		/// <returns></returns>
		public static string ToTitleCase(this string title)
		{
			return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(title.ToLower()); 
		}
        
		/// <summary>
		/// Removes extra spaces in a string
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public static string RemoveExtraSpaces(this string s)
		{
			return Regex.Replace(s, @"\s+", " ");
		}
	}
}