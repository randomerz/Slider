using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using UnityEngine.EventSystems;

namespace MoreMountains.Tools
{
	public static class MMGUI 
	{
		/// <summary>
		/// Sets the size of a rect transform to the specified one
		/// </summary>
		/// <param name="rectTransform"></param>
		/// <param name="newSize"></param>
		public static void SetSize(RectTransform rectTransform, Vector2 newSize) 
		{
			Vector2 currSize = rectTransform.rect.size;
			Vector2 sizeDiff = newSize - currSize;
			rectTransform.offsetMin = rectTransform.offsetMin - new Vector2(sizeDiff.x * rectTransform.pivot.x, sizeDiff.y * rectTransform.pivot.y);
			rectTransform.offsetMax = rectTransform.offsetMax + new Vector2(sizeDiff.x * (1.0f - rectTransform.pivot.x), sizeDiff.y * (1.0f - rectTransform.pivot.y));
		}
		
		/// <summary>
		/// Returns true if the pointer or first touch is blocked by UI
		/// </summary>
		/// <returns></returns>
		public static bool PointOrTouchBlockedByUI()
		{

			if (EventSystem.current.IsPointerOverGameObject())
			{
				return true;
			}
			
			if (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began )
			{
				if (EventSystem.current.IsPointerOverGameObject(Input.touches[0].fingerId))
				{
					return true;
				}
			}

			return false;
		}
		
		/// <summary>
		/// Creates a texture of the specified size and color
		/// </summary>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="color"></param>
		/// <returns></returns>
		public static Texture2D MakeTex(int width, int height, Color color)
		{
			Color[] pixelColors = new Color[width * height];

			for (int i = 0; i < pixelColors.Length; i++)
			{
				pixelColors[i] = color;
			}

			Texture2D newTexture = new Texture2D(width, height);
			newTexture.SetPixels(pixelColors);
			newTexture.Apply();
 
			return newTexture;
		}
	}
}