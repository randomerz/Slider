using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MoreMountains.Tools
{
	/// <summary>
	/// Custom editor for the ShaderController, conditional hiding 
	/// </summary>
	[CustomEditor(typeof(ShaderController), true)]
	[CanEditMultipleObjects]
	public class ShaderControllerEditor : Editor
	{
		protected SerializedProperty _TargetRenderer;
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
		protected SerializedProperty _OneTimeButton;
		protected SerializedProperty _DisableAfterOneTime;
		protected SerializedProperty _DisableGameObjectAfterOneTime;
		protected SerializedProperty _DrivenLevel;
		protected SerializedProperty _AudioAnalyzer;
		protected SerializedProperty _BeatID;
		protected SerializedProperty _AudioAnalyzerMultiplier;
		protected SerializedProperty _AudioAnalyzerOffset;
		protected SerializedProperty _AudioAnalyzerLerp;
		protected SerializedProperty _ToDestinationValue;
		protected SerializedProperty _ToDestinationDuration;
		protected SerializedProperty _ToDestinationCurve;
		protected SerializedProperty _ToDestinationButton;
		protected SerializedProperty _DisableAfterToDestination;
		protected SerializedProperty _InitialValue;
		protected SerializedProperty _CurrentValue;
		protected SerializedProperty _CurrentValueNormalized;
		protected SerializedProperty _InitialColor;
		protected SerializedProperty _ColorMode;
		protected SerializedProperty _ColorRamp;
		protected SerializedProperty _PropertyID;
		protected SerializedProperty _PropertyFound;
		protected SerializedProperty _TargetMaterial;
		protected SerializedProperty _FromColor;
		protected SerializedProperty _ToColor;
		protected SerializedProperty _LoopCurve;
		protected SerializedProperty _LoopStartValue;
		protected SerializedProperty _LoopEndValue;
		protected SerializedProperty _LoopDuration;
		protected SerializedProperty _LoopPauseDuration;
		protected SerializedProperty _SpriteRendererTextureProperty;


		public override bool RequiresConstantRepaint()
		{
			return true;
		}

		/// <summary>
		/// On enable we grab our properties
		/// </summary>
		protected virtual void OnEnable()
		{
			ShaderController myTarget = (ShaderController)target;
			_TargetRenderer = serializedObject.FindProperty("TargetRenderer");
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
			_AudioAnalyzer = serializedObject.FindProperty("AudioAnalyzer");
			_BeatID = serializedObject.FindProperty("BeatID");
			_AudioAnalyzerMultiplier = serializedObject.FindProperty("AudioAnalyzerMultiplier");
			_AudioAnalyzerOffset = serializedObject.FindProperty("AudioAnalyzerOffset");
			_AudioAnalyzerLerp = serializedObject.FindProperty("AudioAnalyzerLerp");
			_ToDestinationValue = serializedObject.FindProperty("ToDestinationValue");
			_ToDestinationDuration = serializedObject.FindProperty("ToDestinationDuration");
			_ToDestinationCurve = serializedObject.FindProperty("ToDestinationCurve");
			_DisableAfterToDestination = serializedObject.FindProperty("DisableAfterToDestination");
			_ToDestinationButton = serializedObject.FindProperty("ToDestinationButton");
			_InitialValue = serializedObject.FindProperty("InitialValue");
			_CurrentValue = serializedObject.FindProperty("CurrentValue");
			_CurrentValueNormalized = serializedObject.FindProperty("CurrentValueNormalized");
			_InitialColor = serializedObject.FindProperty("InitialColor");
			_ColorMode = serializedObject.FindProperty("ColorMode");
			_ColorRamp = serializedObject.FindProperty("ColorRamp");
			_PropertyID = serializedObject.FindProperty("PropertyID");
			_PropertyFound = serializedObject.FindProperty("PropertyFound");
			_TargetMaterial = serializedObject.FindProperty("TargetMaterial");
			_DrivenLevel = serializedObject.FindProperty("DrivenLevel");
			_FromColor = serializedObject.FindProperty("FromColor");
			_ToColor = serializedObject.FindProperty("ToColor");
			_LoopCurve = serializedObject.FindProperty("LoopCurve");
			_LoopStartValue = serializedObject.FindProperty("LoopStartValue");
			_LoopEndValue = serializedObject.FindProperty("LoopEndValue");
			_LoopDuration = serializedObject.FindProperty("LoopDuration");
			_LoopPauseDuration = serializedObject.FindProperty("LoopPauseDuration");
			_SpriteRendererTextureProperty = serializedObject.FindProperty("SpriteRendererTextureProperty");
		}

		/// <summary>
		/// Draws a conditional inspector
		/// </summary>
		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			Undo.RecordObject(target, "Modified ShaderController");

			ShaderController myTarget = (ShaderController)target;
                        
			Editor.DrawPropertiesExcluding(serializedObject, new string[] { "m_Script", "Curve", "MinValue", "MaxValue", "Duration", "PingPongPauseDuration",
				"Amplitude", "Frequency", "Shift", "OneTimeDuration", "OneTimeAmplitude", "OneTimeRemapMin",
				"OneTimeRemapMax", "OneTimeCurve", "DisableAfterOneTime", "DisableGameObjectAfterOneTime", "OneTimeButton", "AudioAnalyzer",
				"BeatID", "AudioAnalyzerMultiplier", "AudioAnalyzerOffset",
				"AudioAnalyzerLerp", "ToDestinationValue", "RemapNoiseValues","RemapNoiseZero","RemapNoiseOne", "ColorMode", "ColorRamp",
				"ToDestinationDuration", "ToDestinationCurve", "DisableAfterToDestination", "ToDestinationButton", "DrivenLevel", "FromColor", "ToColor",
				"LoopCurve", "LoopStartValue", "LoopEndValue", "LoopDuration", "LoopPauseDuration", 
				"InitialValue","CurrentValue", "CurrentValueNormalized","InitialColor","PropertyID","PropertyFound","TargetMaterial"});

			if (myTarget.PropertyType == ShaderController.PropertyTypes.Color)
			{
				EditorGUILayout.PropertyField(_ColorMode);
				if (myTarget.ColorMode == ShaderController.ColorModes.TwoColors)
				{
					if (myTarget.ControlMode != ShaderController.ControlModes.ToDestination)
					{
						EditorGUILayout.PropertyField(_FromColor);	
					}
					EditorGUILayout.PropertyField(_ToColor);	
				}
				else
				{
					EditorGUILayout.PropertyField(_ColorRamp);
				}
			}

			if (myTarget.ControlMode == ShaderController.ControlModes.PingPong)
			{
				EditorGUILayout.PropertyField(_Curve);
				EditorGUILayout.PropertyField(_MinValue);
				EditorGUILayout.PropertyField(_MaxValue);
				EditorGUILayout.PropertyField(_Duration);
				EditorGUILayout.PropertyField(_PingPongPauseDuration);
			}
			if (myTarget.ControlMode == ShaderController.ControlModes.Loop)
			{
				EditorGUILayout.PropertyField(_LoopCurve);
				EditorGUILayout.PropertyField(_LoopStartValue);
				EditorGUILayout.PropertyField(_LoopEndValue);
				EditorGUILayout.PropertyField(_LoopDuration);
				EditorGUILayout.PropertyField(_LoopPauseDuration);
			}
			else if (myTarget.ControlMode == ShaderController.ControlModes.Random)
			{
				EditorGUILayout.PropertyField(_Amplitude);
				EditorGUILayout.PropertyField(_Frequency);
				EditorGUILayout.PropertyField(_Shift);
				EditorGUILayout.PropertyField(_RemapNoiseValues);
				EditorGUILayout.PropertyField(_RemapNoiseZero);
				EditorGUILayout.PropertyField(_RemapNoiseOne);
			}
			else if (myTarget.ControlMode == ShaderController.ControlModes.OneTime)
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
			else if (myTarget.ControlMode == ShaderController.ControlModes.AudioAnalyzer)
			{
				EditorGUILayout.PropertyField(_AudioAnalyzer);
				EditorGUILayout.PropertyField(_BeatID);
				EditorGUILayout.PropertyField(_AudioAnalyzerMultiplier);
				EditorGUILayout.PropertyField(_AudioAnalyzerOffset);
				EditorGUILayout.PropertyField(_AudioAnalyzerLerp);
			}
			else if (myTarget.ControlMode == ShaderController.ControlModes.Driven)
			{
				EditorGUILayout.PropertyField(_DrivenLevel);
			}
			else if (myTarget.ControlMode == ShaderController.ControlModes.ToDestination)
			{
				EditorGUILayout.PropertyField(_ToDestinationValue);
				EditorGUILayout.PropertyField(_ToDestinationDuration);
				EditorGUILayout.PropertyField(_ToDestinationCurve);
				EditorGUILayout.PropertyField(_DisableAfterToDestination);
				EditorGUILayout.PropertyField(_ToDestinationButton);
			}

			EditorGUILayout.PropertyField(_InitialValue);
			EditorGUILayout.PropertyField(_CurrentValue);
			EditorGUILayout.PropertyField(_CurrentValueNormalized);
			EditorGUILayout.PropertyField(_InitialColor);
			EditorGUILayout.PropertyField(_PropertyID);
			EditorGUILayout.PropertyField(_PropertyFound);
			EditorGUILayout.PropertyField(_TargetMaterial);

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