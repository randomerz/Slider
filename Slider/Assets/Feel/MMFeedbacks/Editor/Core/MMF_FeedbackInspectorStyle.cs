using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
	static class MMF_FeedbackInspectorStyle
	{
		public static GUIStyle ContainerStyle;
		public static GUIStyle BoxChildStyle;
		public static GUIStyle GroupStyle;
		public static GUIStyle TextStyle;

		public static bool IsProSkin = EditorGUIUtility.isProSkin;
		public static Texture2D GroupClosedTriangle = Resources.Load<Texture2D>("IN foldout focus-6510");
		public static Texture2D GroupOpenTriangle = Resources.Load<Texture2D>("IN foldout focus on-5718");
		public static Texture2D NoTexture = new Texture2D(0, 0);

		static MMF_FeedbackInspectorStyle()
		{
			// TEXT STYLE --------------------------------------------------------------------------------------------------------------
            
			TextStyle = new GUIStyle(EditorStyles.largeLabel);
			TextStyle.richText = true;
			TextStyle.contentOffset = new Vector2(0, 25);
            
			//TextStyle.font = Font.CreateDynamicFontFromOSFont(new[] { "Terminus (TTF) for Windows", "Calibri" }, 14);

			// GROUP STYLE --------------------------------------------------------------------------------------------------------------

			GroupStyle = new GUIStyle(EditorStyles.foldout);
            
			GroupStyle.active.background = GroupClosedTriangle;
			GroupStyle.focused.background = GroupClosedTriangle;
			GroupStyle.hover.background = GroupClosedTriangle;
			GroupStyle.onActive.background = GroupOpenTriangle;
			GroupStyle.onFocused.background = GroupOpenTriangle;
			GroupStyle.onHover.background = GroupOpenTriangle; 
            
			GroupStyle.fontStyle = FontStyle.Bold;

			GroupStyle.overflow = new RectOffset(100, 0, 0, 0);
			GroupStyle.padding = new RectOffset(20, 0, 0, 0);

			// CONTAINER STYLE --------------------------------------------------------------------------------------------------------------

			ContainerStyle = new GUIStyle(GUI.skin.box);
			ContainerStyle.padding = new RectOffset(20, 0, 0, 0);

			// BOX CHILD STYLE --------------------------------------------------------------------------------------------------------------

			BoxChildStyle = new GUIStyle(GUI.skin.box);
			BoxChildStyle.padding = new RectOffset(0, 0, 0, 0);
			BoxChildStyle.margin = new RectOffset(0, 0, 0, 0);
			BoxChildStyle.normal.background = NoTexture;

		}

		static Texture2D MakeTex(int width, int height, Color col)
		{
			Color[] pix = new Color[width * height];
			for (int i = 0; i < pix.Length; ++i)
			{
				pix[i] = col;
			}
			Texture2D result = new Texture2D(width, height);
			result.SetPixels(pix);
			result.Apply();
			return result;
		}

	}
}