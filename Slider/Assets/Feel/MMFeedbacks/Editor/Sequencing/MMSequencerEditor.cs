using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// Custom editor for MMSequencer, handles recalibration and sequencer display
	/// </summary>
	[CustomEditor(typeof(MMSequencer), true)]
	[CanEditMultipleObjects]
	public class MMSequencerEditor : Editor
	{
		protected MMSequencer _targetSequencer;
		protected float _inspectorWidth;
		protected GUIStyle _buttonStyle;
		protected GUIStyle _trackControlStyle;
		protected GUIStyle _indexStyle;
		protected Texture2D _buttonBackground;
		protected Texture2D _dotBackground;
		protected Color _buttonColor;
		protected Color _trackControlColor;

		protected Color _emptyButtonColor = new Color(0,0,0,0.5f);
		protected Color _empty4ButtonColor = new Color(0, 0, 0, 0.75f);
		protected const float _buttonWidth = 24;
		protected const float _trackControlWidth = 11;
		protected const float _distanceBetweenButtons = 6f;
		protected int _boxesPerLine;
		protected Color _originalBackgroundColor;
		protected Color _controlColor;

		protected List<float> _trackControlLastUseTimestamps;

		/// <summary>
		/// We want constant repaint on this inspector
		/// </summary>
		/// <returns></returns>
		public override bool RequiresConstantRepaint()
		{
			return true;
		}

		/// <summary>
		/// On enable we grab our textures and initialize our styles
		/// </summary>
		protected virtual void OnEnable()
		{
			_targetSequencer = (MMSequencer)target;
			_buttonBackground = Resources.Load("SequencerButtonBackground") as Texture2D;
			_dotBackground = Resources.Load("SequencerDotBackground") as Texture2D;
			_originalBackgroundColor = GUI.backgroundColor;

			_buttonStyle = new GUIStyle();
			_buttonStyle.normal.background = _buttonBackground;
			_buttonStyle.fixedWidth = _buttonWidth;
			_buttonStyle.fixedHeight = _buttonWidth;

			_trackControlStyle = new GUIStyle();
			_trackControlStyle.normal.background = _dotBackground;
			_trackControlStyle.normal.textColor = (Application.isPlaying) ? Color.black : Color.white;
			_trackControlStyle.fixedWidth = _trackControlWidth;
			_trackControlStyle.fixedHeight = _trackControlWidth;            
			_trackControlStyle.margin = new RectOffset(0, 0, 1, 0);
			_trackControlStyle.alignment = TextAnchor.MiddleCenter;
			_trackControlStyle.fontSize = 10;


			_indexStyle = new GUIStyle();
			_indexStyle.normal.background = _dotBackground;
			_indexStyle.normal.textColor = Color.white;
			_indexStyle.alignment = TextAnchor.MiddleCenter;
			_indexStyle.fixedWidth = _trackControlWidth * 1.5f;
			_indexStyle.fixedHeight = _trackControlWidth * 1.5f;

			FillControlList();
		}

		protected virtual void FillControlList()
		{
			// fill the control timer list
			if (_targetSequencer.Sequence != null)
			{
				_trackControlLastUseTimestamps = new List<float>();
				foreach (MMSequenceTrack track in _targetSequencer.Sequence.SequenceTracks)
				{
					_trackControlLastUseTimestamps.Add(0f);
				}
			}
		}

		/// <summary>
		/// Draws the default inspector and the sequencer
		/// </summary>
		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			Undo.RecordObject(target, "Modified Sequence Recorder");

			DrawDefaultInspector();

			if (_targetSequencer.Sequence == null)
			{
				_targetSequencer.LastSequence = null;
				return;
			}

			// gets the width and computes how many boxes we can fit per line
			_inspectorWidth = EditorGUIUtility.currentViewWidth - 24;
			_boxesPerLine = (int)Mathf.Round(
				(_inspectorWidth - ((_targetSequencer.Sequence.SequenceTracks.Count) * _distanceBetweenButtons) - _trackControlWidth - _distanceBetweenButtons)
				/ (_buttonWidth + _distanceBetweenButtons)
			) + 1;

			LookForChanges();
            
			// separator
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Sequencer", EditorStyles.boldLabel);

			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Destroy and rebuild sequence", EditorStyles.miniButtonLeft))
			{
				_targetSequencer.Sequence.QuantizedSequence = null;
				_targetSequencer.LastTracksCount = -1;
				_targetSequencer.ApplySequencerLengthToSequence();
				EditorUtility.SetDirty(_targetSequencer.Sequence);
			}
			if (GUILayout.Button("Clear Sequence", EditorStyles.miniButtonMid))
			{
				_targetSequencer.ClearSequence();
				EditorUtility.SetDirty(_targetSequencer.Sequence);
			}
			if (GUILayout.Button("[ - ] Length - 1", EditorStyles.miniButtonMid))
			{
				_targetSequencer.DecrementLength();
				EditorUtility.SetDirty(_targetSequencer.Sequence);
			}
			if (GUILayout.Button("[ + ] Length + 1", EditorStyles.miniButtonRight))
			{
				_targetSequencer.IncrementLength();
				EditorUtility.SetDirty(_targetSequencer.Sequence);
			}
			EditorGUILayout.EndHorizontal();
            
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			DrawSequenceIndexes();
			for (int i = 0; i < _targetSequencer.Sequence.SequenceTracks.Count; i++)
			{
				DrawTrack(i);
			}

			DrawControlButtons();

			serializedObject.ApplyModifiedProperties();
		}

		/// <summary>
		/// Whenever we detect a change in the settings we recalibrate our sequence accordingly
		/// </summary>
		protected virtual void LookForChanges()
		{
			if (_targetSequencer.LastSequence != _targetSequencer.Sequence)
			{
				FillControlList();
				if (_targetSequencer.Sequence.QuantizedSequence.Count > 0)
				{
					if (_targetSequencer.Sequence.QuantizedSequence[0].Line.Count != _targetSequencer.SequencerLength)
					{
						_targetSequencer.SequencerLength = _targetSequencer.Sequence.QuantizedSequence[0].Line.Count;
						_targetSequencer.LastSequencerLength = _targetSequencer.SequencerLength;
						_targetSequencer.LastSequence = _targetSequencer.Sequence;
						_targetSequencer.LastTracksCount = _targetSequencer.Sequence.SequenceTracks.Count;
					}
				}
				else
				{
					_targetSequencer.ApplySequencerLengthToSequence();
					_targetSequencer.LastSequence = _targetSequencer.Sequence;
					EditorUtility.SetDirty(_targetSequencer.Sequence);
				}                
			}   
            
			if (_targetSequencer.LastSequence == _targetSequencer.Sequence)
			{
				if (_targetSequencer.LastTracksCount != _targetSequencer.Sequence.SequenceTracks.Count)
				{
					FillControlList();
					_targetSequencer.ApplySequencerLengthToSequence();
					EditorUtility.SetDirty(_targetSequencer.Sequence);
				}
				if (_targetSequencer.LastBPM != _targetSequencer.BPM)
				{
					_targetSequencer.UpdateTimestampsToMatchNewBPM();
					EditorUtility.SetDirty(_targetSequencer.Sequence);
				}
			}

			_targetSequencer.EditorMaintenance();
		}

		/// <summary>
		/// Draws control buttons
		/// </summary>
		protected virtual void DrawControlButtons()
		{
			if (!Application.isPlaying)
			{
				return;
			}
			GUI.backgroundColor = _originalBackgroundColor;
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Controls", EditorStyles.boldLabel);

			if (_targetSequencer.Playing)
			{
				if (GUILayout.Button("Stop Playing"))
				{
					_targetSequencer.StopSequence();
				}
			}
			else
			{
				if (GUILayout.Button("Start Playing"))
				{
					_targetSequencer.PlaySequence();
				}
			}
			if (GUILayout.Button("Play Next Beat"))
			{
				_targetSequencer.PlayBeat();
			}
		}

		/// <summary>
		/// Draws an index for each sequence item
		/// </summary>
		protected virtual void DrawSequenceIndexes()
		{
			GUILayout.BeginHorizontal();

			GUI.backgroundColor = _emptyButtonColor;
			GUILayout.Label("", GUILayout.Width(_trackControlWidth + _distanceBetweenButtons), GUILayout.Height(_trackControlWidth));
			string label = "";
			for (int i = 0; i < _targetSequencer.SequencerLength; i++)
			{
				label = i < 10 ? " " + i.ToString() : i.ToString();
				_trackControlStyle.fontStyle = (i % 4 == 0) ? FontStyle.Bold : FontStyle.Normal;
				//GUILayout.Label(label, _indexStyle, GUILayout.Width(_buttonWidth + _distanceBetweenButtons), GUILayout.Height(_trackControlWidth));
				if (GUILayout.Button(label, _indexStyle, GUILayout.Width(_buttonWidth + _distanceBetweenButtons), GUILayout.Height(_trackControlWidth)))
				{
					_targetSequencer.ToggleStep(i);
					EditorUtility.SetDirty(_targetSequencer.Sequence);
				}
			}
			GUI.backgroundColor = _originalBackgroundColor;
			GUILayout.EndHorizontal();
			GUILayout.Space(_distanceBetweenButtons * 1.5f);
		}

		/// <summary>
		/// Draws a line of the sequencer
		/// </summary>
		/// <param name="trackIndex"></param>
		protected virtual void DrawTrack(int trackIndex)
		{
			int counter = 0;

			if (_targetSequencer.Sequence.QuantizedSequence == null)
			{
				return;
			}

			if (_targetSequencer.Sequence.QuantizedSequence.Count != _targetSequencer.Sequence.SequenceTracks.Count)
			{
				return;
			}

			GUILayout.BeginHorizontal();


			GUILayout.BeginVertical();

			// draw active track control

			if (_targetSequencer.Sequence.SequenceTracks[trackIndex].Active)
			{
				_trackControlColor = _targetSequencer.Sequence.SequenceTracks[trackIndex].TrackColor;
			}
			else
			{
				_trackControlColor = _emptyButtonColor;
			}

			GUI.backgroundColor = _trackControlColor;
			if (GUILayout.Button("", _trackControlStyle, GUILayout.Width(_trackControlWidth), GUILayout.Height(_trackControlWidth)))
			{
				if (_targetSequencer.TrackEvents[trackIndex] != null)
				{
					_targetSequencer.ToggleActive(trackIndex);
					EditorUtility.SetDirty(_targetSequencer.Sequence);
				}
			}

			GUILayout.Space(_distanceBetweenButtons / 5);

			// draw test track control
			_trackControlColor = _targetSequencer.Sequence.SequenceTracks[trackIndex].TrackColor;
			_controlColor = Application.isPlaying ? SequencerColor(_trackControlLastUseTimestamps[trackIndex], _trackControlColor) : Color.black;
			GUI.backgroundColor = _controlColor;
			if (GUILayout.Button(trackIndex.ToString(), _trackControlStyle, GUILayout.Width(_trackControlWidth), GUILayout.Height(_trackControlWidth)))
			{
				if (_targetSequencer.TrackEvents[trackIndex] != null)
				{
					_trackControlLastUseTimestamps[trackIndex] = Time.time;
					_targetSequencer.PlayTrackEvent(trackIndex);
				}
			}
			GUILayout.EndVertical();

			GUILayout.Space(_distanceBetweenButtons);

			for (int i = 0; i < _targetSequencer.Sequence.QuantizedSequence[trackIndex].Line.Count; i++)
			{
				// handle new lines
				if (counter > _boxesPerLine )
				{
					GUILayout.EndHorizontal();
					GUILayout.Space(_distanceBetweenButtons);
					GUILayout.BeginHorizontal();
					counter = 0;
				}

				if (_targetSequencer.Sequence.QuantizedSequence[trackIndex].Line[i].ID != -1)
				{
					_buttonColor = _targetSequencer.Sequence.SequenceTracks[trackIndex].TrackColor;
				}
				else
				{
					if (i % 4 == 0)
					{
						_buttonColor = _empty4ButtonColor;
					}
					else
					{
						_buttonColor = _emptyButtonColor;
					}
				}
				// if the track is inactive, we reduce colors
				if (!_targetSequencer.Sequence.SequenceTracks[trackIndex].Active)
				{
					_buttonColor = _buttonColor / 2f;
				}

				DrawSequenceButton(trackIndex, i, _buttonColor);
                
				counter++;
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			GUILayout.Space(_distanceBetweenButtons);
		}

		/// <summary>
		/// Draws an interactive button for the sequencer
		/// </summary>
		/// <param name="trackIndex"></param>
		/// <param name="sequenceIndex"></param>
		/// <param name="buttonColor"></param>
		protected virtual void DrawSequenceButton(int trackIndex, int sequenceIndex, Color buttonColor)
		{

			if (Application.isPlaying && _targetSequencer.PlayedOnce && (_targetSequencer.LastBeatIndex == sequenceIndex))
			{
				if (_targetSequencer.BeatThisFrame)
				{
					_buttonColor = Color.white;
				}
				else
				{
					_buttonColor = SequencerColor(_targetSequencer.LastBeatTimestamp, buttonColor);
				}
			}
			else
			{
				_buttonColor = buttonColor;
			}

			GUI.backgroundColor = _buttonColor;
			if (GUILayout.Button("", _buttonStyle, GUILayout.Width(_buttonWidth), GUILayout.Height(_buttonWidth)))
			{
				bool active = (_targetSequencer.Sequence.QuantizedSequence[trackIndex].Line[sequenceIndex].ID == _targetSequencer.Sequence.SequenceTracks[trackIndex].ID);
				_targetSequencer.Sequence.QuantizedSequence[trackIndex].Line[sequenceIndex].ID = active ? -1 : _targetSequencer.Sequence.SequenceTracks[trackIndex].ID;
				EditorUtility.SetDirty(_targetSequencer.Sequence);
			}
			GUILayout.Space(_distanceBetweenButtons);
		}

		/// <summary>
		/// Color interpolation on hits
		/// </summary>
		/// <param name="lastBeatTimestamp"></param>
		/// <param name="buttonColor"></param>
		/// <returns></returns>
		protected virtual Color SequencerColor(float lastBeatTimestamp, Color buttonColor)
		{
			float x = (Time.time - lastBeatTimestamp);
			float A = 0f;
			float B = (60f / _targetSequencer.BPM) / 4f;
			float C = 0f;
			float D = 1f;
			float t = C + (x - A) / (B - A) * (D - C);
			return Color.Lerp(Color.white, buttonColor, t);
		}
	}
}