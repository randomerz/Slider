using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace MoreMountains.Tools
{
	/// <summary>
	/// A simple class that lets you lock the current inspector by pressing ctrl (or cmd) + L
	/// Pressing the same shortcut again unlocks the 
	/// </summary>
	public class MMLockInspector : MonoBehaviour
	{
		[MenuItem("Tools/More Mountains/Lock Inspector %l")]
		static public void LockInspector()
		{
			Type inspectorType = typeof(Editor).Assembly.GetType("UnityEditor.InspectorWindow");
			EditorWindow inspectorWindow = EditorWindow.GetWindow(inspectorType);

			PropertyInfo isLockedPropertyInfo = inspectorType.GetProperty("isLocked", BindingFlags.Public | BindingFlags.Instance);
			bool state = (bool)isLockedPropertyInfo.GetGetMethod().Invoke(inspectorWindow, new object[] { });

			isLockedPropertyInfo.GetSetMethod().Invoke(inspectorWindow, new object[] { !state });
		}
	}
}