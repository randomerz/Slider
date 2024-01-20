using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System;
using System.Linq;

namespace MoreMountains.Tools
{
	[CustomPropertyDrawer(typeof(MMPropertyReceiver), true)]
	[CanEditMultipleObjects]
	public class MMPropertyReceiverDrawer : MMPropertyPickerDrawer
	{
		protected Color _mmYellow = new Color(1f, 0.7686275f, 0f);
		protected Color _mmRed = MMColors.Orangered;

		protected override void FillAuthorizedTypes(PropertyPickerViewData viewData)
		{
			viewData._authorizedTypes = new Type[]
			{
				typeof(String),
				typeof(float),
				typeof(Vector2),
				typeof(Vector3),
				typeof(Vector4),
				typeof(Quaternion),
				typeof(int),
				typeof(bool),
				typeof(Color)
			};
		}

		/// <summary>
		/// Defines the height of the drawer
		/// </summary>
		/// <param name="property"></param>
		/// <param name="label"></param>
		/// <returns></returns>
		public override float AdditionalHeight(PropertyPickerViewData viewData)
		{
			int additionalLines = 0;
			if (viewData._TargetObject != null)
			{
				if ((viewData._selectedPropertyIndex != 0) && (viewData._propertyType != null))
				{
					additionalLines = 1;
					if (viewData._propertyType == typeof(bool))
					{
						additionalLines = 3;
					}
					if (viewData._propertyType == typeof(Color))
					{
						additionalLines = 3;
					}
					if (viewData._propertyType == typeof(string))
					{
						additionalLines = 3;
					}
					if (viewData._propertyType == typeof(float))
					{
						additionalLines = 3;
					}
					if (viewData._propertyType == typeof(int))
					{
						additionalLines = 3;
					}
					if (viewData._propertyType == typeof(Vector2))
					{
						additionalLines = 5;
					}
					if (viewData._propertyType == typeof(Vector3))
					{
						additionalLines = 6;
					}
					if (viewData._propertyType == typeof(Vector4))
					{
						additionalLines = 15;
					}
					if (viewData._propertyType == typeof(Quaternion))
					{
						additionalLines = 6;
					}
				}

				if (Application.isPlaying)
				{
					additionalLines += 1;
				}
			}

			viewData._numberOfLines = viewData._numberOfLines + additionalLines;
            
			return PropertyPickerViewData._lineHeight * additionalLines + PropertyPickerViewData._lineMargin * additionalLines - 1;
		}

		/// <summary>
		/// Draws the inspector
		/// </summary>
		/// <param name="position"></param>
		/// <param name="property"></param>
		/// <param name="label"></param>
		protected override void DisplayAdditionalProperties(Rect position, SerializedProperty property, GUIContent label, PropertyPickerViewData viewData)
		{
			float lineHeight = PropertyPickerViewData._lineHeight;
			float lineMargin = PropertyPickerViewData._lineMargin;
			
			Rect additional1Rect = new Rect(position.x, position.y + (lineHeight + lineMargin) * 4, position.width, lineHeight);
			Rect additional2Rect = new Rect(position.x, position.y + (lineHeight + lineMargin) * 5, position.width, lineHeight);
			Rect additional3Rect = new Rect(position.x, position.y + (lineHeight + lineMargin) * 6, position.width, lineHeight);
			Rect additional4Rect = new Rect(position.x, position.y + (lineHeight + lineMargin) * 7, position.width, lineHeight);
			Rect additional5Rect = new Rect(position.x, position.y + (lineHeight + lineMargin) * 8, position.width, lineHeight);
			Rect additional6Rect = new Rect(position.x, position.y + (lineHeight + lineMargin) * 9, position.width, lineHeight);
			Rect additional7Rect = new Rect(position.x, position.y + (lineHeight + lineMargin) * 10, position.width, lineHeight);
			Rect additional8Rect = new Rect(position.x, position.y + (lineHeight + lineMargin) * 11, position.width, lineHeight);
			Rect additional9Rect = new Rect(position.x, position.y + (lineHeight + lineMargin) * 12, position.width, lineHeight);
			Rect additional10Rect = new Rect(position.x, position.y + (lineHeight + lineMargin) * 13, position.width, lineHeight);

			// displays the related properties
			if ((viewData._selectedPropertyIndex != 0) && (viewData._propertyType != null))
			{
				if ( (viewData._propertyType != typeof(bool)) && (viewData._propertyType != typeof(string)) )
				{
					EditorGUI.PropertyField(additional1Rect, property.FindPropertyRelative("RelativeValue"), new GUIContent("Relative Value"), true);
				}                

				if (viewData._propertyType == typeof(string))
				{
					EditorGUI.PropertyField(additional1Rect, property.FindPropertyRelative("StringRemapZero"), new GUIContent("Remap Zero"), true);
					EditorGUI.PropertyField(additional2Rect, property.FindPropertyRelative("StringRemapOne"), new GUIContent("Remap One"), true);
					EditorGUI.PropertyField(additional3Rect, property.FindPropertyRelative("Threshold"), new GUIContent("Zero/One Threshold"), true);
				}

				if (viewData._propertyType == typeof(bool))
				{
					EditorGUI.PropertyField(additional1Rect, property.FindPropertyRelative("BoolRemapZero"), new GUIContent("Remap Zero"), true);
					EditorGUI.PropertyField(additional2Rect, property.FindPropertyRelative("BoolRemapOne"), new GUIContent("Remap One"), true);
					EditorGUI.PropertyField(additional3Rect, property.FindPropertyRelative("Threshold"), new GUIContent("True/False Threshold"), true);
				}

				if (viewData._propertyType == typeof(float))
				{
					EditorGUI.PropertyField(additional2Rect, property.FindPropertyRelative("FloatRemapZero"), new GUIContent("Remap Zero"), true);
					EditorGUI.PropertyField(additional3Rect, property.FindPropertyRelative("FloatRemapOne"), new GUIContent("Remap One"), true);
				}

				if (viewData._propertyType == typeof(Vector2))
				{
					EditorGUI.PropertyField(additional2Rect, property.FindPropertyRelative("ModifyX"), new GUIContent("Modify x"), true);
					EditorGUI.PropertyField(additional3Rect, property.FindPropertyRelative("ModifyY"), new GUIContent("Modify y"), true);
					EditorGUI.PropertyField(additional4Rect, property.FindPropertyRelative("Vector2RemapZero"), new GUIContent("Remap Zero"), true);
					EditorGUI.PropertyField(additional5Rect, property.FindPropertyRelative("Vector2RemapOne"), new GUIContent("Remap One"), true);
				}

				if (viewData._propertyType == typeof(Vector3))
				{
					EditorGUI.PropertyField(additional2Rect, property.FindPropertyRelative("ModifyX"), new GUIContent("Modify x"), true);
					EditorGUI.PropertyField(additional3Rect, property.FindPropertyRelative("ModifyY"), new GUIContent("Modify y"), true);
					EditorGUI.PropertyField(additional4Rect, property.FindPropertyRelative("ModifyZ"), new GUIContent("Modify z"), true);
					EditorGUI.PropertyField(additional5Rect, property.FindPropertyRelative("Vector3RemapZero"), new GUIContent("Remap Zero"), true);
					EditorGUI.PropertyField(additional6Rect, property.FindPropertyRelative("Vector3RemapOne"), new GUIContent("Remap One"), true);
				}

				if (viewData._propertyType == typeof(Vector4))
				{
					Rect additionalVector47Rect = new Rect(position.x, position.y + (lineHeight + lineMargin) * 9, position.width, lineHeight * 5);
					Rect additionalVector48Rect = new Rect(position.x, position.y + (lineHeight + lineMargin) * 10 + lineHeight * 4, position.width, lineHeight * 5);

					EditorGUI.PropertyField(additional2Rect, property.FindPropertyRelative("ModifyX"), new GUIContent("Modify x"), true);
					EditorGUI.PropertyField(additional3Rect, property.FindPropertyRelative("ModifyY"), new GUIContent("Modify y"), true);
					EditorGUI.PropertyField(additional4Rect, property.FindPropertyRelative("ModifyZ"), new GUIContent("Modify z"), true);
					EditorGUI.PropertyField(additional5Rect, property.FindPropertyRelative("ModifyW"), new GUIContent("Modify z"), true);
					EditorGUI.PropertyField(additionalVector47Rect, property.FindPropertyRelative("Vector4RemapZero"), new GUIContent("Remap Zero"), true);
					EditorGUI.PropertyField(additionalVector48Rect, property.FindPropertyRelative("Vector4RemapOne"), new GUIContent("Remap One"), true);
				}

				if (viewData._propertyType == typeof(Quaternion))
				{
					EditorGUI.PropertyField(additional2Rect, property.FindPropertyRelative("ModifyX"), new GUIContent("Modify x"), true);
					EditorGUI.PropertyField(additional3Rect, property.FindPropertyRelative("ModifyY"), new GUIContent("Modify y"), true);
					EditorGUI.PropertyField(additional4Rect, property.FindPropertyRelative("ModifyZ"), new GUIContent("Modify z"), true);
					EditorGUI.PropertyField(additional5Rect, property.FindPropertyRelative("QuaternionRemapZero"), new GUIContent("Remap Zero"), true);
					EditorGUI.PropertyField(additional6Rect, property.FindPropertyRelative("QuaternionRemapOne"), new GUIContent("Remap One"), true);
				}

				if (viewData._propertyType == typeof(int))
				{
					EditorGUI.PropertyField(additional2Rect, property.FindPropertyRelative("IntRemapZero"), new GUIContent("Remap Zero"), true);
					EditorGUI.PropertyField(additional3Rect, property.FindPropertyRelative("IntRemapOne"), new GUIContent("Remap One"), true);
				}

				if (viewData._propertyType == typeof(Color))
				{
					EditorGUI.PropertyField(additional2Rect, property.FindPropertyRelative("ColorRemapZero"), new GUIContent("Remap Zero"), true);
					EditorGUI.PropertyField(additional3Rect, property.FindPropertyRelative("ColorRemapOne"), new GUIContent("Remap One"), true);
				}
			}

			if ((viewData._TargetObject != null) && (viewData._selectedPropertyIndex != 0) && (viewData._propertyType != null) && (Application.isPlaying))
			{
				// if the application is playing, we display a progress bar

				float level = property.FindPropertyRelative("Level").floatValue;
				DrawLevelProgressBar(position, level, _mmYellow, _mmRed, viewData);
			}
		}

	}
}