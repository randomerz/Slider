using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace MoreMountains.Tools
{
	[InitializeOnLoad]
	public static class MMMonoBehaviourDrawerHelper
	{
		public static void DrawButton(this Editor editor, MethodInfo methodInfo)
		{
			if (GUILayout.Button(methodInfo.Name))
			{
				methodInfo.Invoke(editor.target, null);
			}
		}

		public static void DrawVerticalLayout(this Editor editor, Action action, GUIStyle style)
		{
			EditorGUILayout.BeginVertical(style);
			action();
			EditorGUILayout.EndVertical();
		}
	}
}