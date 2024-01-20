using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;

namespace MoreMountains.Tools
{
	[CustomPropertyDrawer(typeof(MMTweenType))]
	public class MMTweenTypeDrawer : PropertyDrawer
	{
		protected const int _lineHeight = 20; 
		protected const int _lineMargin = 2;

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return _lineHeight * 2 + _lineMargin;
		}

		#if  UNITY_EDITOR
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var type = property.FindPropertyRelative("MMTweenDefinitionType");
            
			EditorGUI.BeginProperty(position, label, property);

			position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            
			var definitionTypeRect = new Rect(position.x, position.y, position.width, _lineHeight);
			var curveRect = new Rect(position.x, position.y + _lineHeight + _lineMargin, position.width, _lineHeight);

			EditorGUI.PropertyField(definitionTypeRect, property.FindPropertyRelative("MMTweenDefinitionType"), GUIContent.none);
			if (type.enumValueIndex == 0)
			{
				EditorGUI.PropertyField(curveRect, property.FindPropertyRelative("MMTweenCurve"), GUIContent.none);
			}
			if (type.enumValueIndex == 1)
			{
				EditorGUI.PropertyField(curveRect, property.FindPropertyRelative("Curve"), GUIContent.none);
			}
            
			EditorGUI.EndProperty();
		}
		#endif
	}
}