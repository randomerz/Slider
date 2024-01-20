using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MoreMountains.Tools
{
	static class MMMonoBehaviourDrawerStyle
	{
		public static GUIStyle ContainerStyle;
		public static GUIStyle BoxChildStyle;
		public static GUIStyle GroupStyle;
		public static GUIStyle TextStyle;

		public static bool IsProSkin = EditorGUIUtility.isProSkin;
		public static Texture2D GroupClosedTriangle = Resources.Load<Texture2D>("IN foldout focus-6510");
		public static Texture2D GroupOpenTriangle = Resources.Load<Texture2D>("IN foldout focus on-5718");
		public static Texture2D NoTexture = new Texture2D(0, 0);

		static MMMonoBehaviourDrawerStyle()
		{
			// TEXT STYLE --------------------------------------------------------------------------------------------------------------

			TextStyle = new GUIStyle(EditorStyles.largeLabel);
			TextStyle.richText = true;
			TextStyle.contentOffset = new Vector2(0, 5);
            
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
			ContainerStyle.padding = new RectOffset(20, 0, 10, 10);

			// BOX CHILD STYLE --------------------------------------------------------------------------------------------------------------

			BoxChildStyle = new GUIStyle(GUI.skin.box);
			/*BoxChildStyle.active.background = GroupClosedTriangle;
			BoxChildStyle.focused.background = GroupClosedTriangle;
			BoxChildStyle.onActive.background = GroupOpenTriangle;
			BoxChildStyle.onFocused.background = GroupOpenTriangle;*/
			BoxChildStyle.padding = new RectOffset(0, 0, 0, 0);
			BoxChildStyle.margin = new RectOffset(0, 0, 0, 0);
			BoxChildStyle.normal.background = NoTexture;

			// FOLDOUT STYLE --------------------------------------------------------------------------------------------------------------

			/*EditorStyles.foldout.active.background = GroupClosedTriangle;
			EditorStyles.foldout.focused.background = GroupClosedTriangle;
			EditorStyles.foldout.hover.background = GroupClosedTriangle;

			EditorStyles.foldout.onActive.background = GroupOpenTriangle;
			EditorStyles.foldout.onFocused.background = GroupOpenTriangle;
			EditorStyles.foldout.onHover.background = GroupOpenTriangle;

			//EditorStyles.foldout.overflow = new RectOffset(100, 0, 0, 0);
			EditorStyles.foldout.padding = new RectOffset(0, 0, 0, 0);
			EditorStyles.foldout.overflow = new RectOffset(0, 0, 0, 0);
			EditorStyles.foldout.*/
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