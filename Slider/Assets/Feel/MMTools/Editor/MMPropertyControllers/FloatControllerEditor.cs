using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MoreMountains.Tools
{
	/// <summary>
	/// Custom editor for the FloatController, conditional hiding and dropdown fill
	/// </summary>
	[CustomEditor(typeof(FloatController), true)]
	[CanEditMultipleObjects]    
	public class FloatControllerEditor : Editor
	{
		protected SerializedProperty _TargetObject;

		protected SerializedProperty _Curve;
		protected SerializedProperty _MinValue;
		protected SerializedProperty _MaxValue;
		protected SerializedProperty _Duration;
		protected SerializedProperty _PingPongPauseDuration;

		protected SerializedProperty _Amplitude;
		protected SerializedProperty _Frequency;
		protected SerializedProperty _Shift;
		protected SerializedProperty _RemapNoiseValues;
		protected SerializedProperty _RemapNoiseZero;
		protected SerializedProperty _RemapNoiseOne;

		protected SerializedProperty _OneTimeDuration;
		protected SerializedProperty _OneTimeAmplitude;
		protected SerializedProperty _OneTimeRemapMin;
		protected SerializedProperty _OneTimeRemapMax;
		protected SerializedProperty _OneTimeCurve;
		protected SerializedProperty _DisableAfterOneTime;
		protected SerializedProperty _DisableGameObjectAfterOneTime;
		protected SerializedProperty _OneTimeButton;

		protected SerializedProperty _DrivenLevel;

		protected SerializedProperty _ToDestinationValue;
		protected SerializedProperty _ToDestinationDuration;
		protected SerializedProperty _ToDestinationCurve;
		protected SerializedProperty _DisableAfterToDestination;
		protected SerializedProperty _ToDestinationButton;

		protected SerializedProperty _InitialValue;
		protected SerializedProperty _CurrentValue;
		protected SerializedProperty _CurrentValueNormalized;

		protected SerializedProperty _ChoiceIndex;
		protected SerializedProperty _PropertyName;

		protected SerializedProperty _AudioAnalyzer;
		protected SerializedProperty _BeatID;
		protected SerializedProperty _AudioAnalyzerMode;
		protected SerializedProperty _NormalizedLevelID;
		protected SerializedProperty _AudioAnalyzerMultiplier;

		public override bool RequiresConstantRepaint()
		{
			return true;
		}
        
		/// <summary>
		/// On enable, grabs our serialized properties
		/// </summary>
		protected virtual void OnEnable()
		{
			FloatController myTarget = (FloatController)target;

			_TargetObject = serializedObject.FindProperty("TargetObject");

			_Curve = serializedObject.FindProperty("Curve");
			_MinValue = serializedObject.FindProperty("MinValue");
			_MaxValue = serializedObject.FindProperty("MaxValue");
			_Duration = serializedObject.FindProperty("Duration");
			_PingPongPauseDuration = serializedObject.FindProperty("PingPongPauseDuration");

			_Amplitude = serializedObject.FindProperty("Amplitude");
			_Frequency = serializedObject.FindProperty("Frequency");
			_Shift = serializedObject.FindProperty("Shift");
			_RemapNoiseValues = serializedObject.FindProperty("RemapNoiseValues");
			_RemapNoiseZero = serializedObject.FindProperty("RemapNoiseZero");
			_RemapNoiseOne = serializedObject.FindProperty("RemapNoiseOne");

			_OneTimeDuration = serializedObject.FindProperty("OneTimeDuration");
			_OneTimeAmplitude = serializedObject.FindProperty("OneTimeAmplitude");
			_OneTimeRemapMin = serializedObject.FindProperty("OneTimeRemapMin");
			_OneTimeRemapMax = serializedObject.FindProperty("OneTimeRemapMax");
			_OneTimeCurve = serializedObject.FindProperty("OneTimeCurve");
			_DisableAfterOneTime = serializedObject.FindProperty("DisableAfterOneTime");
			_DisableGameObjectAfterOneTime = serializedObject.FindProperty("DisableGameObjectAfterOneTime");
			_OneTimeButton = serializedObject.FindProperty("OneTimeButton");

			_DrivenLevel = serializedObject.FindProperty("DrivenLevel");

			_ToDestinationValue = serializedObject.FindProperty("ToDestinationValue");
			_ToDestinationDuration = serializedObject.FindProperty("ToDestinationDuration");
			_ToDestinationCurve = serializedObject.FindProperty("ToDestinationCurve");
			_DisableAfterToDestination = serializedObject.FindProperty("DisableAfterToDestination");
			_ToDestinationButton = serializedObject.FindProperty("ToDestinationButton");

			_InitialValue = serializedObject.FindProperty("InitialValue");
			_CurrentValue = serializedObject.FindProperty("CurrentValue");
			_CurrentValueNormalized = serializedObject.FindProperty("CurrentValueNormalized");
            
			_ChoiceIndex = serializedObject.FindProperty("ChoiceIndex");
			_PropertyName = serializedObject.FindProperty("PropertyName");

			_AudioAnalyzer = serializedObject.FindProperty("AudioAnalyzer");
			_AudioAnalyzerMode = serializedObject.FindProperty("AudioAnalyzerMode");
			_NormalizedLevelID = serializedObject.FindProperty("NormalizedLevelID");
			_BeatID = serializedObject.FindProperty("BeatID");
			_AudioAnalyzerMultiplier = serializedObject.FindProperty("AudioAnalyzerMultiplier");

			VerifyChosenIndex();
			AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;

		}

		protected virtual void OnDisable()
		{
			//BindPropertyName();
			AssemblyReloadEvents.afterAssemblyReload -= OnAfterAssemblyReload;
		}

		protected virtual void BindPropertyName()
		{
			FloatController myTarget = (FloatController)target;

			if (myTarget.ChoiceIndex > myTarget.AttributeNames.Length - 1)
			{
				_ChoiceIndex.intValue = 0;
				_PropertyName.stringValue = FloatController._undefinedString;
			}
			else
			{
				_PropertyName.stringValue = myTarget.AttributeNames[myTarget.ChoiceIndex];
				serializedObject.ApplyModifiedProperties();
			}
		}

		protected virtual void VerifyChosenIndex()
		{
			FloatController myTarget = (FloatController)target;

			// determine choice index
			int index = 0;
			bool found = false;
			foreach (string attName in myTarget.AttributeNames)
			{
				if (attName == myTarget.PropertyName)
				{
					_ChoiceIndex.intValue = index;
					found = true;
				}
				index++;
			}
			if (!found)
			{
				_ChoiceIndex.intValue = 0;
				_PropertyName.stringValue = FloatController._undefinedString;
			}
			serializedObject.ApplyModifiedProperties();
		}

		protected virtual void OnAfterAssemblyReload()
		{
			FloatController myTarget = (FloatController)target;
			myTarget.FillDropDownList();
			VerifyChosenIndex();
			serializedObject.ApplyModifiedProperties();
		}
        
		/// <summary>
		/// Draws a custom conditional inspector
		/// </summary>
		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			Undo.RecordObject(target, "Modified FloatController");

			FloatController myTarget = (FloatController)target;

			EditorGUILayout.PropertyField(_TargetObject);
			if (myTarget.AttributeNames != null)
			{
				if (myTarget.AttributeNames.Length > 0)
				{
					// draws a dropdown with all our properties
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.PrefixLabel("Property");

					_ChoiceIndex.intValue = EditorGUILayout.Popup(myTarget.ChoiceIndex, myTarget.AttributeNames);
					BindPropertyName();
					EditorGUILayout.EndHorizontal();

					Editor.DrawPropertiesExcluding(serializedObject, new string[] { "m_Script", "TargetObject", "Curve", "MinValue", "MaxValue", "Duration", "Amplitude",
						"RemapNoiseValues","RemapNoiseZero","RemapNoiseOne",
						"Frequency", "Shift", "InitialValue", "CurrentValue", "CurrentValueNormalized", "PingPongPauseDuration",
						"OneTimeDuration", "OneTimeAmplitude", "OneTimeRemapMin", "OneTimeRemapMax",
						"OneTimeCurve", "OneTimeButton", "DisableAfterOneTime", "DisableGameObjectAfterOneTime",
						"AudioAnalyzer","AudioAnalyzerMode", "BeatID", "NormalizedLevelID", "AudioAnalyzerMultiplier", 
						"DisableAfterToDestination", "DrivenLevel",
						"ToDestinationDuration", "ToDestinationValue", "ToDestinationCurve", "ToDestinationButton"});

					if (myTarget.ControlMode == FloatController.ControlModes.PingPong)
					{
						EditorGUILayout.PropertyField(_Curve);
						EditorGUILayout.PropertyField(_MinValue);
						EditorGUILayout.PropertyField(_MaxValue);
						EditorGUILayout.PropertyField(_Duration);
						EditorGUILayout.PropertyField(_PingPongPauseDuration);
					}
					else if (myTarget.ControlMode == FloatController.ControlModes.Random)
					{
						EditorGUILayout.PropertyField(_Amplitude);
						EditorGUILayout.PropertyField(_Frequency);
						EditorGUILayout.PropertyField(_Shift);
						EditorGUILayout.PropertyField(_RemapNoiseValues);
						EditorGUILayout.PropertyField(_RemapNoiseZero);
						EditorGUILayout.PropertyField(_RemapNoiseOne);
					}
					else if (myTarget.ControlMode == FloatController.ControlModes.Driven)
					{
						EditorGUILayout.PropertyField(_DrivenLevel);
					}
					else if (myTarget.ControlMode == FloatController.ControlModes.OneTime)
					{
						EditorGUILayout.PropertyField(_OneTimeDuration);
						EditorGUILayout.PropertyField(_OneTimeAmplitude);
						EditorGUILayout.PropertyField(_OneTimeRemapMin);
						EditorGUILayout.PropertyField(_OneTimeRemapMax);
						EditorGUILayout.PropertyField(_OneTimeCurve);
						EditorGUILayout.PropertyField(_DisableAfterOneTime);
						EditorGUILayout.PropertyField(_DisableGameObjectAfterOneTime);
						EditorGUILayout.PropertyField(_OneTimeButton);
					}
					else if (myTarget.ControlMode == FloatController.ControlModes.AudioAnalyzer)
					{
						EditorGUILayout.PropertyField(_AudioAnalyzer);
						EditorGUILayout.PropertyField(_AudioAnalyzerMode);
						if (myTarget.AudioAnalyzerMode == FloatController.AudioAnalyzerModes.Beat)
						{
							EditorGUILayout.PropertyField(_BeatID);    
						}
						else
						{
							EditorGUILayout.PropertyField(_NormalizedLevelID);    
						}
						EditorGUILayout.PropertyField(_AudioAnalyzerMultiplier);
					}
					else if (myTarget.ControlMode == FloatController.ControlModes.ToDestination)
					{
						EditorGUILayout.PropertyField(_ToDestinationDuration);
						EditorGUILayout.PropertyField(_ToDestinationValue);
						EditorGUILayout.PropertyField(_ToDestinationCurve);
						EditorGUILayout.PropertyField(_DisableAfterToDestination);
						EditorGUILayout.PropertyField(_ToDestinationButton);
					}

					EditorGUILayout.PropertyField(_InitialValue);
					EditorGUILayout.PropertyField(_CurrentValue);
					EditorGUILayout.PropertyField(_CurrentValueNormalized);
				}
			}
            

			serializedObject.ApplyModifiedProperties();

			if (Application.isPlaying)
			{
				_barRect = EditorGUILayout.GetControlRect();
				DrawLevelProgressBar(_barRect, myTarget.CurrentValueNormalized, _mmYellow, _mmRed);    
			}
		}
        
		protected Rect _barRect;
		protected Color _mmYellow = new Color(1f, 0.7686275f, 0f);
		protected Color _mmRed = MMColors.Orangered;
		protected const int _lineHeight = 20;
		protected const int _lineMargin = 2;
		protected const int _numberOfLines = 1;
		protected Color _progressBarBackground = new Color(0, 0, 0, 0.5f);
        
		protected virtual void DrawLevelProgressBar(Rect position, float level, Color frontColor, Color negativeColor)
		{
			Rect levelLabelRect = new Rect(position.x, position.y + (_lineHeight + _lineMargin) * (_numberOfLines - 1), position.width, _lineHeight);
			Rect levelValueRect = new Rect(position.x - 15 + EditorGUIUtility.labelWidth + 4, position.y + (_lineHeight + _lineMargin) * (_numberOfLines - 1), position.width, _lineHeight);

			float progressX = position.x - 5 + EditorGUIUtility.labelWidth + 60;
			float progressY = position.y + (_lineHeight + _lineMargin) * (_numberOfLines - 1) + 6;
			float progressHeight = 10f;
			float fullProgressWidth = position.width - EditorGUIUtility.labelWidth - 60 + 5;

			bool negative = false;
			float displayLevel = level;
			if (level < 0f)
			{
				negative = true;
				level = -level;
			}

			float progressLevel = Mathf.Clamp01(level);
			Rect levelProgressBg = new Rect(progressX, progressY, fullProgressWidth, progressHeight);
			float progressWidth = MMMaths.Remap(progressLevel, 0f, 1f, 0f, fullProgressWidth);
			Rect levelProgressFront = new Rect(progressX, progressY, progressWidth, progressHeight);

			EditorGUI.LabelField(levelLabelRect, new GUIContent("Level"));
			EditorGUI.LabelField(levelValueRect, new GUIContent(displayLevel.ToString("F4")));
			EditorGUI.DrawRect(levelProgressBg, _progressBarBackground);
			if (negative)
			{
				EditorGUI.DrawRect(levelProgressFront, negativeColor);
			}
			else
			{
				EditorGUI.DrawRect(levelProgressFront, frontColor);
			}            
		}
	}
}