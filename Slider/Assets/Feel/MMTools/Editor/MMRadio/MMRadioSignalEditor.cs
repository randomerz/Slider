using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MoreMountains.Tools
{
	[CustomEditor(typeof(MMRadioSignal), true)]
	[CanEditMultipleObjects]
	public class MMRadioSignalEditor : Editor
	{
		protected MMRadioSignal _radioSignal;

		protected float _inspectorWidth;
        
		protected SerializedProperty _duration;
		protected SerializedProperty _currentLevel;

		public override bool RequiresConstantRepaint()
		{
			return true;
		}

		protected virtual void OnEnable()
		{
			_radioSignal = target as MMRadioSignal;
			_duration = serializedObject.FindProperty("Duration");
			_currentLevel = serializedObject.FindProperty("CurrentLevel");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			_inspectorWidth = EditorGUIUtility.currentViewWidth - 24;

			DrawProperties();

			serializedObject.ApplyModifiedProperties();
		}

		protected virtual void DrawProperties()
		{
			DrawPropertiesExcluding(serializedObject, "AnimatedPreview", "CurrentLevel");
		}

        
	}
}