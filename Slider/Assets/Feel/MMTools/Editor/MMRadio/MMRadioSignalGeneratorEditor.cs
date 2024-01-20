using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MoreMountains.Tools
{
	[CustomEditor(typeof(MMRadioSignalGenerator), true)]
	[CanEditMultipleObjects]
	public class MMRadioSignalGeneratorEditor : MMRadioSignalEditor
	{
		protected MMReorderableList _list;
		protected SerializedProperty _globalMultiplier;
		protected SerializedProperty _clamp;
		protected SerializedProperty _clamps;
		protected SerializedProperty _bias;

		protected float _spectrumBoxBottomY;
		protected Vector2 _spectrumBoxPosition;
		protected Vector2 _spectrumBoxSize;
		protected float _spectrumMaxColumnHeight;
		protected const float _externalMargin = 12f;
		protected float _internalMargin = 12f;
		protected const int _rawSpectrumBoxHeight = 125;
		protected const int _numberOfAxisSpectrum = 4;
		protected Vector3 _axisOrigin = Vector3.zero;
		protected Vector3 _axisDestination = Vector3.zero;
		protected float _spectrumPointsCount = 200;
		protected Color _spectrumColor = MMColors.Aqua;
		protected Color _spectrumBoxColor = MMColors.AliceBlue;
		protected Rect _rect;

		protected double _deltaTime = 0f;
		protected double _signalTime = 0f;
		protected double _lastTime;
		protected float _normalizedTime = 0f;
		protected Vector2 _pointA;
		protected Vector2 _pointB;


		protected SerializedProperty _driverTime;
		protected SerializedProperty _animatedPreview;

		protected override void OnEnable()
		{
			base.OnEnable();
			_globalMultiplier = serializedObject.FindProperty("GlobalMultiplier");
			_clamp = serializedObject.FindProperty("Clamp");
			_clamps = serializedObject.FindProperty("Clamps");
			_bias = serializedObject.FindProperty("Bias");

			_list = new MMReorderableList(serializedObject.FindProperty("SignalList"));
			_driverTime = serializedObject.FindProperty("DriverTime");
			_animatedPreview = serializedObject.FindProperty("AnimatedPreview");
			_list.elementNameProperty = "SignalList";
			_list.elementDisplayType = MMReorderableList.ElementDisplayType.Expandable;
			_list.onAddCallback += OnListAdd;
		}

		private void OnListAdd(MMReorderableList list)
		{
			SerializedProperty property = list.AddItem();

			property.FindPropertyRelative("Active").boolValue = true;
			property.FindPropertyRelative("Frequency").floatValue = 1f;
			property.FindPropertyRelative("Amplitude").floatValue = 1f;
			property.FindPropertyRelative("Offset").floatValue = 0f;
			property.FindPropertyRelative("Phase").floatValue = 0f;
		}

		protected override void DrawProperties()
		{
			DrawPropertiesExcluding(serializedObject, "AnimatedPreview", "SignalList", "GlobalMultiplier", "CurrentLevel", "Clamp", "Clamps");
			EditorGUILayout.Space(10);
			_list.DoLayoutList();
			EditorGUILayout.PropertyField(_globalMultiplier);
			EditorGUILayout.PropertyField(_clamps);
			DrawRawSpectrum();
		}

		protected virtual void DrawRawSpectrum()
		{
			_deltaTime = (EditorApplication.timeSinceStartup - _lastTime);
			_signalTime += _deltaTime;
			if (_signalTime > _duration.floatValue)
			{
				_signalTime = 0f;
			}
			_lastTime = EditorApplication.timeSinceStartup;
			_normalizedTime = MMMaths.Remap((float)_signalTime, 0f, _duration.floatValue, 0f, 1f);

			GUILayout.Space(10);
			GUILayout.Label("Preview", EditorStyles.boldLabel);

			EditorGUILayout.PropertyField(_animatedPreview);

			EditorGUILayout.Space(20);
			// box
			GUILayout.Box("", GUILayout.Width(_inspectorWidth - _externalMargin), GUILayout.Height(_rawSpectrumBoxHeight));
			_spectrumBoxPosition = GUILayoutUtility.GetLastRect().position;
			_spectrumBoxSize = GUILayoutUtility.GetLastRect().size;
			_spectrumBoxBottomY = _spectrumBoxPosition.y + _spectrumBoxSize.y;
			_spectrumMaxColumnHeight = _spectrumBoxSize.y - 2 * _externalMargin;
			Handles.BeginGUI();

			// horizontal axis
			Handles.color = Color.grey;
			for (int i = 0; i <= _numberOfAxisSpectrum; i++)
			{
				_axisOrigin.x = _spectrumBoxPosition.x;
				_axisOrigin.y = _spectrumBoxBottomY - i * (_spectrumBoxSize.y / _numberOfAxisSpectrum);
				_axisDestination.x = _spectrumBoxPosition.x + _spectrumBoxSize.x;
				_axisDestination.y = _axisOrigin.y;
				Handles.DrawLine(_axisOrigin, _axisDestination);
			}

			// y one label
			_rect.x = _axisOrigin.x - 12;
			_rect.y = _spectrumBoxBottomY - _spectrumBoxSize.y - 20;
			_rect.width = 40;
			_rect.height = 40;
			EditorGUI.LabelField(_rect, "1", EditorStyles.boldLabel);

			float minX = _axisOrigin.x - 12;
			float maxX = _axisOrigin.x + _spectrumBoxSize.x - 2;


			float zeroX = minX;
			float oneX = maxX;

			if (_animatedPreview.boolValue)
			{
				float currentTime = (float)EditorApplication.timeSinceStartup;
				float normalizedTime = currentTime - Mathf.Floor(currentTime);
				zeroX = maxX - MMMaths.Remap(_normalizedTime, 0f, 1f, _spectrumBoxPosition.x + _externalMargin, _spectrumBoxPosition.x + _spectrumBoxSize.x);
				oneX = zeroX - 10;
			}

			// zero label
			_rect.x = zeroX;
			_rect.y = _spectrumBoxBottomY - 20;
			_rect.width = 40;
			_rect.height = 40;
			EditorGUI.LabelField(_rect, "0", EditorStyles.boldLabel);

			// one label
			_rect.x = oneX;
			_rect.y = _spectrumBoxBottomY - 20;
			_rect.width = 40;
			_rect.height = 40;
			EditorGUI.LabelField(_rect, "1", EditorStyles.boldLabel);

			// level 
			if (Application.isPlaying)
			{
				_rect.x = _axisOrigin.x + _spectrumBoxSize.x - 40;
				_rect.y = _spectrumBoxBottomY - _spectrumBoxSize.y - 40;
				_rect.width = 40;
				_rect.height = 40;
				EditorGUI.LabelField(_rect, _currentLevel.floatValue.ToString("F3"), EditorStyles.boldLabel);
			}

			// cube  
			_rect.x = _spectrumBoxPosition.x + _externalMargin / 4;
			if (_duration.floatValue > 0f)
			{
				float boxSpectrumValue;
				if (Application.isPlaying)
				{
					boxSpectrumValue = MMMaths.Remap(_radioSignal.GraphValue(_driverTime.floatValue), 0f, 1f, 0f, _spectrumBoxSize.y);
				}
				else
				{
					boxSpectrumValue = MMMaths.Remap(_radioSignal.GraphValue(_normalizedTime), 0f, 1f, 0f, _spectrumBoxSize.y);
				}
				_rect.y = _spectrumBoxBottomY - boxSpectrumValue - _externalMargin / 4;
			}
			else
			{
				_rect.y = _spectrumBoxBottomY;
			}
			_rect.width = _externalMargin / 2;
			_rect.height = _externalMargin / 2;
			EditorGUI.DrawRect(_rect, _spectrumBoxColor);

			// progress line
			if (Application.isPlaying && !_animatedPreview.boolValue)
			{
				_rect.x = _spectrumBoxPosition.x 
				          + MMMaths.Remap(_driverTime.floatValue, 0f, 1f, 0f, _spectrumBoxSize.x);
				_rect.y = _spectrumBoxBottomY - _spectrumBoxSize.y;
				_rect.width = 1;
				_rect.height = _spectrumBoxSize.y;
				EditorGUI.DrawRect(_rect, _spectrumBoxColor);
			}

			for (int i = 1; i < _spectrumPointsCount; i++)
			{
				float xPosition = _spectrumBoxPosition.x + _externalMargin + MMMaths.Remap(i, 0, _spectrumPointsCount, 0f, _spectrumBoxSize.x - _externalMargin * 2);
				float deltaBetweenXandXPrevious = (_spectrumBoxSize.x - _externalMargin * 2) / _spectrumPointsCount;

				float time = i * (1 / _spectrumPointsCount);
				float timePrevious = (i - 1) * (1 / _spectrumPointsCount);

				if (_animatedPreview.boolValue)
				{
					if (Application.isPlaying)
					{
						time += _driverTime.floatValue;
						timePrevious += _driverTime.floatValue;
					}
					else
					{
						time += (float)_normalizedTime;
						timePrevious += (float)_normalizedTime;
					}
					if (time > _duration.floatValue)
					{
						time = 0f;
					}
					if (timePrevious > _duration.floatValue)
					{
						timePrevious = 0f;
					}
				}

				float t2 = Mathf.Pow(time, _bias.floatValue);
                
				float spectrumValue = MMMaths.Remap(_radioSignal.GraphValue(time), 0f, 1f, 0f, _spectrumBoxSize.y);
				float spectrumValuePrevious = MMMaths.Remap(_radioSignal.GraphValue(timePrevious), 0f, 1f, 0f, _spectrumBoxSize.y);

				Handles.color = _spectrumColor;
				_axisOrigin.x = xPosition - deltaBetweenXandXPrevious;
				_axisOrigin.y = _spectrumBoxBottomY - spectrumValuePrevious;
				_axisDestination.x = xPosition;
				_axisDestination.y = _spectrumBoxBottomY - spectrumValue;

				var p1 = _axisOrigin;
				var p2 = _axisDestination;
				var thickness = 3;
				Handles.DrawBezier(p1, p2, p1, p2, _spectrumColor, null, thickness);
			}

			EditorGUILayout.Space(50);

			Handles.EndGUI();
		}
	}
}