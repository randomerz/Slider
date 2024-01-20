using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// A custom editor displaying a foldable list of MMFeedbacks, a dropdown to add more, as well as test buttons to test your feedbacks at runtime
	/// </summary>
	[CustomEditor(typeof(MMF_Player), true)]
	public class MMF_PlayerEditor : Editor
	{
		/// <summary>
		/// A data structure to store types and names
		/// </summary>
		public struct FeedbackTypePair
		{
			public System.Type FeedbackType;
			public string FeedbackName;
		}

		public MMF_Player TargetMmfPlayer;
		protected SerializedProperty _mmfeedbacksList;
		protected SerializedProperty _mmfeedbacksInitializationMode;
		protected SerializedProperty _mmfeedbacksAutoInitialization;
		protected SerializedProperty _mmfeedbacksAutoPlayOnStart;
		protected SerializedProperty _mmfeedbacksAutoPlayOnEnable;
		protected SerializedProperty _mmfeedbacksDirection;
		protected SerializedProperty _mmfeedbacksFeedbacksIntensity;
		protected SerializedProperty _mmfeedbacksAutoChangeDirectionOnEnd;
		protected SerializedProperty _mmfeedbacksDurationMultiplier;
		protected SerializedProperty _mmfeedbacksForceTimescaleMode;
		protected SerializedProperty _mmfeedbacksForcedTimescaleMode;
		protected SerializedProperty _mmfeedbacksPlayerTimescaleMode;
		protected SerializedProperty _mmfeedbacksDisplayFullDurationDetails;
		protected SerializedProperty _mmfeedbacksCooldownDuration;
		protected SerializedProperty _mmfeedbacksInitialDelay;
		protected SerializedProperty _mmfeedbacksCanPlay;
		protected SerializedProperty _mmfeedbacksCanPlayWhileAlreadyPlaying;
		protected SerializedProperty _mmfeedbacksPerformanceMode;
		protected SerializedProperty _mmfeedbacksStopFeedbacksOnDisable;
		protected SerializedProperty _mmfeedbacksPlayCount;
		protected SerializedProperty _mmfeedbacksEvents;
		protected SerializedProperty _keepPlayModeChanges;
		protected SerializedProperty _mmfeedbacksChanceToPlay;
		protected SerializedProperty _mmfeedbacksRandomizeDuration;
		protected SerializedProperty _mmfeedbacksRandomDurationMultiplier;

		protected SerializedProperty _mmfeedbacksOnlyPlayIfWithinRange;
		protected SerializedProperty _mmfeedbacksRangeCenter;
		protected SerializedProperty _mmfeedbacksRangeDistance;
		protected SerializedProperty _mmfeedbacksUseRangeFalloff;
		protected SerializedProperty _mmfeedbacksRangeFalloff;
		protected SerializedProperty _mmfeedbacksRemapRangeFalloff;
		protected SerializedProperty _mmfeedbacksIgnoreRangeEvents;

		protected bool _feedbackListIsExpanded;
		protected string _feedbackListLabel;
		protected bool _feedbackListPause;
		protected SerializedProperty _feedbackListProperty;
		protected MMF_Feedback _feedbackListFeedback;
		protected Dictionary<MMF_Feedback, Editor> _editors;
		protected List<string> _typeNames = new List<string>();
		public static List<FeedbackTypePair> _typesAndNames = new List<FeedbackTypePair>();
		public static string[] _typeDisplays;
		protected int _draggedStartID = -1;
		protected int _draggedEndID = -1;
		private static bool _debugView = false;
		protected Texture2D _scriptDrivenBoxBackgroundTexture;
		private static bool _settingsMenuDropdown;
		protected Rect _helpBoxRect;
		protected Event _currentEvent;
		protected bool _cachedGUI = false;
        
		// GUI Styles
		protected GUIStyle _directionButtonStyle = new GUIStyle();
		protected GUIStyle _playingStyle = new GUIStyle();
        
		// Icons
		protected GUIContent _directionIconUp;
		protected GUIContent _directionIconDown;
        
		// Colors
		protected Color _scriptDrivenBoxColorFrom = new Color(1f,0f,0f,1f);
		protected Color _scriptDrivenBoxColorTo = new Color(0.7f,0.1f,0.1f,1f);
		protected Color _playButtonColor = new Color32(193, 255, 2, 255);
		protected Color _keepPlaymodeChangesButtonColor = new Color32(255, 97, 33, 255);
		protected Color _originalBackgroundColor;
		protected Color _scriptDrivenBoxColor;
		protected Color _baseColor;
		protected Color _draggedColor = new Color(0, 1, 1, 0.2f);
		protected Color _redBackgroundColor = new Color32(255, 97, 33, 255);
		protected Color _savedTextColor;

		protected Texture2D _whiteTexture;
		protected Texture2D _redWarningBoxBackgroundTexture;
        
		protected float _durationRectWidth = 70f;
		protected float _playingRectWidth = 70f;
		protected float _directionRectWidth = 16f;
		protected Rect _durationRect = new Rect(); 
		protected Rect _playingRect = new Rect();
		protected Rect _directionRect = new Rect();

		protected GUIContent _pasteAsNewGUIContent;
		protected GUIContent _replaceAllGUIContent;
		protected GUIContent _pasteAllAsNewGUIContent;
		protected GUIContent _feedbackPlayGUIContent;
		protected GUIContent _feedbackRemoveGUIContent;
		protected GUIContent _feedbackResetGUIContent;
		protected GUIContent _feedbackDuplicateGUIContent;
		protected GUIContent _feedbackCopyGUIContent;
		protected GUIContent _feedbackPasteGUIContent;
        
		protected GUIStyle _helptextStyle;
		protected GUIStyle _redWarningBoxStyle;
		protected Texture2D _savedBackground;
        
		protected GUILayoutOption _pasteAsNewOption;
		protected GUILayoutOption _replaceAllOption;
		protected GUILayoutOption _pasteAllAsNewOption;

		protected Dictionary<int, MMF_FeedbackInspector> MMF_FeedbackInspectors =
			new Dictionary<int, MMF_FeedbackInspector>();

		protected const string _copyAllText = "Copy all";
		protected const string _pasteAsNewText = "Paste as new";
		protected const string _replaceAllText = "Replace all";
		protected const string _pasteAllAsNewText = "Paste all as new";
		protected const string _inactiveMessage = "All MMFeedbacks, including this one, are currently disabled. This is done via script, by changing the value of the MMFeedbacks.GlobalMMFeedbacksActive boolean. Right now this value has been set to false. Setting it back to true will allow MMFeedbacks to play again.";
		protected const string _instructionsMessage = "Select Feedbacks from the 'add a feedback' dropdown and customize them. Remember, if you don't use auto initialization (Awake or Start), you'll need to initialize them via script.";
		protected const string _initializationText = "Initialization";
		protected const string _directionText = "Direction";
		protected const string _intensityText = "Intensity";
		protected const string _timingText = "Timing";
		protected const string _rangeText = "Range";
		protected const string _playConditionsText = "Play Settings";
		protected const string _eventsText = "Events";
		protected const string _settingsText = "Settings";
		protected const string _playingBracketsText = "[PLAYING] ";
		protected const string _infiniteLoopText = "[Infinite Loop] ";
		protected const string _allFeedbacksDebugText = "All Feedbacks Debug";
		protected const string _initializeText = "Initialize";
		protected const string _playText = "Play";
		protected const string _removeText = "Remove";
		protected const string _pauseText = "Pause";
		protected const string _stopText = "Stop";
		protected const string _resetText = "Reset";
		protected const string _revertText = "Revert";
		protected const string _duplicateText = "Duplicate";
		protected const string _copyText = "Copy";
		protected const string _pasteText = "Paste";
		protected const string _skipText = "Skip";
		protected const string _restoreText = "Restore";
		protected const string _keepPlaymodeChangesText = "Keep Playmode Changes";
		protected const string _scriptDrivenInProgressText = "Script driven pause in progress, call Resume() to exit pause";
		protected const string _resumeText = "Resume";
		protected const string _undoText = "Modified Feedback Manager";
		protected const string _feedbacksSectionTitle = "Feedbacks";
		protected bool _expandGroupsInInspectors = true;
        
		#region Initialization
        
		/// <summary>
		/// On Enable, grabs properties and initializes the add feedback dropdown's contents
		/// </summary>
		protected virtual void OnEnable()
		{
			Initialization();
			EditorApplication.playModeStateChanged += ModeChanged;
		}

		protected virtual void Initialization()
		{
			// Get properties
			TargetMmfPlayer = target as MMF_Player;
			_mmfeedbacksList = serializedObject.FindProperty("FeedbacksList");
			_mmfeedbacksInitializationMode = serializedObject.FindProperty("InitializationMode");
			_mmfeedbacksAutoInitialization = serializedObject.FindProperty("AutoInitialization");
			_mmfeedbacksAutoPlayOnStart = serializedObject.FindProperty("AutoPlayOnStart");
			_mmfeedbacksAutoPlayOnEnable = serializedObject.FindProperty("AutoPlayOnEnable");
			_mmfeedbacksDirection = serializedObject.FindProperty("Direction");
			_mmfeedbacksAutoChangeDirectionOnEnd = serializedObject.FindProperty("AutoChangeDirectionOnEnd");
			_mmfeedbacksDurationMultiplier = serializedObject.FindProperty("DurationMultiplier");
			_mmfeedbacksRandomizeDuration = serializedObject.FindProperty("RandomizeDuration");
			_mmfeedbacksRandomDurationMultiplier = serializedObject.FindProperty("RandomDurationMultiplier");
			_mmfeedbacksForceTimescaleMode = serializedObject.FindProperty("ForceTimescaleMode");
			_mmfeedbacksForcedTimescaleMode = serializedObject.FindProperty("ForcedTimescaleMode");
			_mmfeedbacksPlayerTimescaleMode = serializedObject.FindProperty("PlayerTimescaleMode");
			_mmfeedbacksDisplayFullDurationDetails = serializedObject.FindProperty("DisplayFullDurationDetails");
			_mmfeedbacksCooldownDuration = serializedObject.FindProperty("CooldownDuration");
			_mmfeedbacksInitialDelay = serializedObject.FindProperty("InitialDelay");
			_mmfeedbacksCanPlay = serializedObject.FindProperty("CanPlay");
			_mmfeedbacksCanPlayWhileAlreadyPlaying = serializedObject.FindProperty("CanPlayWhileAlreadyPlaying");
			_mmfeedbacksFeedbacksIntensity = serializedObject.FindProperty("FeedbacksIntensity");
			_keepPlayModeChanges = serializedObject.FindProperty("KeepPlayModeChanges");
			_mmfeedbacksPerformanceMode = serializedObject.FindProperty("PerformanceMode");
			_mmfeedbacksStopFeedbacksOnDisable = serializedObject.FindProperty("StopFeedbacksOnDisable");
			_mmfeedbacksPlayCount = serializedObject.FindProperty("PlayCount");
			_mmfeedbacksChanceToPlay = serializedObject.FindProperty("ChanceToPlay");
			
			_mmfeedbacksOnlyPlayIfWithinRange = serializedObject.FindProperty("OnlyPlayIfWithinRange");
			_mmfeedbacksRangeCenter = serializedObject.FindProperty("RangeCenter");
			_mmfeedbacksRangeDistance = serializedObject.FindProperty("RangeDistance");
			_mmfeedbacksUseRangeFalloff = serializedObject.FindProperty("UseRangeFalloff");
			_mmfeedbacksRangeFalloff = serializedObject.FindProperty("RangeFalloff");
			_mmfeedbacksRemapRangeFalloff = serializedObject.FindProperty("RemapRangeFalloff");
			_mmfeedbacksIgnoreRangeEvents = serializedObject.FindProperty("IgnoreRangeEvents");	

			_expandGroupsInInspectors = MMF_PlayerConfiguration.Instance.InspectorGroupsExpandedByDefault;

			_mmfeedbacksEvents = serializedObject.FindProperty("Events");
            
			// store GUI bg color
			_originalBackgroundColor = GUI.backgroundColor;

			PrepareFeedbackTypeList();

			foreach (KeyValuePair<int, MMF_FeedbackInspector> inspector in MMF_FeedbackInspectors)
			{
				inspector.Value.OnEnable();
			}

			// we force the styles to initialize on the next OnInspectorGUI call
			_cachedGUI = false;
		}

		/// <summary>
		/// Lists all feedbacks, builds the dropdown list, and stores it in a static 
		protected virtual void PrepareFeedbackTypeList()
		{
			if ((_typeDisplays != null) && (_typeDisplays.Length > 0))
			{
				return;
			}
	        
			// Retrieve available feedbacks
			List<System.Type> types = (from domainAssembly in System.AppDomain.CurrentDomain.GetAssemblies()
				from assemblyType in domainAssembly.GetTypes()
				where assemblyType.IsSubclassOf(typeof(MMF_Feedback))
				select assemblyType).ToList(); 
            
			// Create display list from types
			_typeNames.Clear();
			for (int i = 0; i < types.Count; i++)
			{
				FeedbackTypePair _newType = new FeedbackTypePair();
				_newType.FeedbackType = types[i];
				_newType.FeedbackName = FeedbackPathAttribute.GetFeedbackDefaultPath(types[i]);
				if ((_newType.FeedbackName == "MMF_FeedbackBase") || (_newType.FeedbackName == null))
				{
					continue;
				}
				_typesAndNames.Add(_newType);
			}

			_typesAndNames = _typesAndNames.OrderBy(t => t.FeedbackName).ToList(); 
            
			_typeNames.Add("Add new feedback..."); 
			for (int i = 0; i < _typesAndNames.Count; i++)
			{
				_typeNames.Add(_typesAndNames[i].FeedbackName);
			}

			_typeDisplays = _typeNames.ToArray(); 
		}
        
		#endregion Initialization

		#region InspectorMain
        
		/// <summary>
		/// Draws the inspector, complete with helpbox, init mode selection, list of feedbacks, feedback selection and test buttons 
		/// </summary>
		public override void OnInspectorGUI()
		{
			_currentEvent = Event.current;
			serializedObject.Update();
			Undo.RecordObject(target, _undoText);
            
			InspectorCaching();
			DrawInspectorActiveWarning();
			DrawHelpBox();
			DrawSettingsDropDown();
			DrawDurationAndDirection();
			DrawFeedbacksList();
			DrawBottomBar();
			HandleReordering();
			DrawDebugControls();
			DrawDebugView();

			serializedObject.ApplyModifiedProperties();
		}

		protected virtual void InspectorCaching()
		{
			if (_cachedGUI)
			{
				return;
			}

			MMF_PlayerStyling.CacheStyling();
            
			_directionIconUp = new GUIContent(Resources.Load("FeelArrowUp") as Texture);
			_directionIconDown = new GUIContent(Resources.Load("FeelArrowDown") as Texture);
			_whiteTexture = Texture2D.whiteTexture;
            
			_redWarningBoxBackgroundTexture = new Texture2D(2,2);
			_redWarningBoxBackgroundTexture.CreateColorTexture(_redBackgroundColor);

			_directionButtonStyle.border.left = 0;
			_directionButtonStyle.border.right = 0;
			_directionButtonStyle.border.top = 0;
			_directionButtonStyle.border.bottom = 0;

			_playingStyle.normal.textColor = Color.yellow;
            
			_pasteAsNewGUIContent = new GUIContent(_pasteAsNewText);
			_replaceAllGUIContent = new GUIContent(_replaceAllText);
			_pasteAllAsNewGUIContent = new GUIContent(_pasteAllAsNewText);
			_feedbackPlayGUIContent = new GUIContent(_playText);
			_feedbackRemoveGUIContent = new GUIContent(_removeText);
			_feedbackResetGUIContent = new GUIContent(_resetText);
			_feedbackDuplicateGUIContent = new GUIContent(_duplicateText);
			_feedbackCopyGUIContent = new GUIContent(_copyText);
			_feedbackPasteGUIContent = new GUIContent(_pasteText);
			_pasteAsNewOption = GUILayout.Width(EditorStyles.miniButton.CalcSize(_pasteAsNewGUIContent).x);
			_replaceAllOption = GUILayout.Width(EditorStyles.miniButton.CalcSize(_replaceAllGUIContent).x);  
			_pasteAllAsNewOption = GUILayout.Width(EditorStyles.miniButton.CalcSize(_pasteAllAsNewGUIContent).x);    
            
			_helptextStyle = new GUIStyle(EditorStyles.helpBox);
			_helptextStyle.richText = true;
			_helptextStyle.fontSize = 11;
			_helptextStyle.padding = new RectOffset(8, 8, 8, 8);
		}
        
		protected virtual void DrawInspectorActiveWarning()
		{
			EditorGUILayout.Space();

			if (!MMF_Player.GlobalMMFeedbacksActive)
			{
				_baseColor = GUI.color;
				GUI.color = Color.red;
				EditorGUILayout.HelpBox(_inactiveMessage, MessageType.Warning);
				EditorGUILayout.Space();
				GUI.color = _baseColor;
			}
		}

		protected virtual void DrawHelpBox()
		{
			if (MMF_PlayerConfiguration.Instance.ShowInspectorTips)
			{
				EditorGUILayout.HelpBox(_instructionsMessage, MessageType.None);    
			}

			_helpBoxRect = GUILayoutUtility.GetLastRect();
		}

        
		protected virtual void DrawSettingsDropDown()
		{
			_settingsMenuDropdown = EditorGUILayout.Foldout(_settingsMenuDropdown, _settingsText, true, EditorStyles.foldout);
			if (_settingsMenuDropdown)
			{
				EditorGUILayout.Space(10);
				EditorGUILayout.LabelField(_initializationText, EditorStyles.boldLabel);
				EditorGUILayout.PropertyField(_mmfeedbacksInitializationMode);
				EditorGUILayout.PropertyField(_mmfeedbacksAutoPlayOnStart);
				EditorGUILayout.PropertyField(_mmfeedbacksAutoPlayOnEnable);
				EditorGUILayout.PropertyField(_mmfeedbacksAutoInitialization);
                
				EditorGUILayout.Space(10);
				EditorGUILayout.LabelField(_directionText, EditorStyles.boldLabel);
				EditorGUILayout.PropertyField(_mmfeedbacksDirection);
				EditorGUILayout.PropertyField(_mmfeedbacksAutoChangeDirectionOnEnd);
                
				EditorGUILayout.Space(10);
				EditorGUILayout.LabelField(_intensityText, EditorStyles.boldLabel);
				EditorGUILayout.PropertyField(_mmfeedbacksFeedbacksIntensity);    
                
				EditorGUILayout.Space(10);
				EditorGUILayout.LabelField(_timingText, EditorStyles.boldLabel);
				EditorGUILayout.PropertyField(_mmfeedbacksForceTimescaleMode);
				EditorGUILayout.PropertyField(_mmfeedbacksForcedTimescaleMode);
				EditorGUILayout.PropertyField(_mmfeedbacksDurationMultiplier);
				EditorGUILayout.PropertyField(_mmfeedbacksRandomizeDuration);
				EditorGUILayout.PropertyField(_mmfeedbacksRandomDurationMultiplier);
				EditorGUILayout.PropertyField(_mmfeedbacksDisplayFullDurationDetails);
				EditorGUILayout.PropertyField(_mmfeedbacksCooldownDuration);
				EditorGUILayout.PropertyField(_mmfeedbacksInitialDelay);
				EditorGUILayout.PropertyField(_mmfeedbacksChanceToPlay);
				EditorGUILayout.PropertyField(_mmfeedbacksPlayerTimescaleMode);

				EditorGUILayout.Space(10);
				EditorGUILayout.LabelField(_rangeText, EditorStyles.boldLabel);
				EditorGUILayout.PropertyField(_mmfeedbacksOnlyPlayIfWithinRange);
				
				if (TargetMmfPlayer.OnlyPlayIfWithinRange)
				{
					EditorGUILayout.PropertyField(_mmfeedbacksRangeCenter);
					EditorGUILayout.PropertyField(_mmfeedbacksRangeDistance);
					EditorGUILayout.PropertyField(_mmfeedbacksUseRangeFalloff);
					EditorGUILayout.PropertyField(_mmfeedbacksRangeFalloff);
					if (TargetMmfPlayer.UseRangeFalloff)
					{
						EditorGUILayout.PropertyField(_mmfeedbacksRemapRangeFalloff);
					}
					EditorGUILayout.PropertyField(_mmfeedbacksIgnoreRangeEvents);
				}

				EditorGUILayout.Space(10);
				EditorGUILayout.LabelField(_playConditionsText, EditorStyles.boldLabel);
				EditorGUILayout.PropertyField(_mmfeedbacksCanPlay);
				EditorGUILayout.PropertyField(_mmfeedbacksCanPlayWhileAlreadyPlaying);
				EditorGUILayout.PropertyField(_mmfeedbacksPerformanceMode);
				EditorGUILayout.PropertyField(_mmfeedbacksStopFeedbacksOnDisable);
				if (Application.isPlaying)
				{
					EditorGUILayout.PropertyField(_mmfeedbacksPlayCount);	
				}

				EditorGUILayout.Space(10);
				EditorGUILayout.LabelField(_eventsText, EditorStyles.boldLabel);
				EditorGUILayout.PropertyField(_mmfeedbacksEvents);
			}
		}
        
		protected virtual void DrawDurationAndDirection()
		{
			_durationRect.x = _helpBoxRect.xMax - _durationRectWidth;
			_durationRect.y = _helpBoxRect.yMax + 6;
			_durationRect.width = _durationRectWidth;
			_durationRect.height = 17f;
			_durationRect.xMin = _helpBoxRect.xMax - _durationRectWidth;
			_durationRect.xMax = _helpBoxRect.xMax;
            
			_playingRect.x = _helpBoxRect.xMax - _playingRectWidth - _durationRectWidth;
			_playingRect.y = _helpBoxRect.yMax + 6;
			_playingRect.width = _playingRectWidth;
			_playingRect.height = 17f;
			_playingRect.xMin = _helpBoxRect.xMax - _durationRectWidth- _playingRectWidth;
			_playingRect.xMax = _helpBoxRect.xMax;

			_directionRect.x = _helpBoxRect.xMax - _directionRectWidth;
			_directionRect.y = _helpBoxRect.yMax + 5;
			_directionRect.width = _directionRectWidth;
			_directionRect.height = 17f;
			_directionRect.xMin = _helpBoxRect.xMax - _directionRectWidth;
			_directionRect.xMax = _helpBoxRect.xMax;

			if ((target as MMF_Player).IsPlaying)
			{
				GUI.Label(_playingRect, _playingBracketsText, _playingStyle);    
			}
            
			GUI.Label(_durationRect, "["+TargetMmfPlayer.TotalDuration.ToString("F2")+"s]");

			if (TargetMmfPlayer.Direction == MMF_Player.Directions.BottomToTop)
			{

				if (GUI.Button(_directionRect, _directionIconUp, _directionButtonStyle))
				{
					TargetMmfPlayer.Revert();
				}
			}
			else
			{

				if (GUI.Button(_directionRect, _directionIconDown, _directionButtonStyle))
				{
					TargetMmfPlayer.Revert();
				}
			}
		}
        
		protected virtual void DrawDebugControls()
		{
			MMF_PlayerStyling.DrawSection(_allFeedbacksDebugText);
            
			// Testing buttons

			EditorGUI.BeginDisabledGroup(!Application.isPlaying);
			EditorGUILayout.BeginHorizontal();
			{
				// initialize button
				if (GUILayout.Button(_initializeText, EditorStyles.miniButtonLeft))
				{
					(target as MMF_Player).Initialization();
				}

				// play button
				_originalBackgroundColor = GUI.backgroundColor;
				GUI.backgroundColor = _playButtonColor;
				if (GUILayout.Button(_playText, EditorStyles.miniButtonMid))
				{
					(target as MMF_Player).PlayFeedbacks();
				}
				GUI.backgroundColor = _originalBackgroundColor;
                
				// pause button
				if ((target as MMF_Player).ContainsLoop)
				{
					if (GUILayout.Button(_pauseText, EditorStyles.miniButtonMid))
					{
						(target as MMF_Player).PauseFeedbacks();
					}   
				}
                
				// stop button
				if (GUILayout.Button(_stopText, EditorStyles.miniButtonMid))
				{
					(target as MMF_Player).StopFeedbacks();
				}
                
				// skip button
				if (GUILayout.Button(_skipText, EditorStyles.miniButtonMid))
				{
					(target as MMF_Player).SkipToTheEnd();
				}
                
				// restore button
				if (GUILayout.Button(_restoreText, EditorStyles.miniButtonMid))
				{
					(target as MMF_Player).RestoreInitialValues();
				}
                
				// reset button
				if (GUILayout.Button(_resetText, EditorStyles.miniButtonMid))
				{
					(target as MMF_Player).ResetFeedbacks();
				}
				EditorGUI.EndDisabledGroup();
                
				// reverse button
				if (GUILayout.Button(_revertText, EditorStyles.miniButtonMid))
				{
					(target as MMF_Player).Revert();
				}
			}
			EditorGUILayout.EndHorizontal();
            
			// keep runtime changes button
			_originalBackgroundColor = GUI.backgroundColor;
			if (_keepPlayModeChanges.boolValue)
			{
				GUI.backgroundColor = _keepPlaymodeChangesButtonColor;    
			}
			if (GUILayout.Button(_keepPlaymodeChangesText))
			{
				_keepPlayModeChanges.boolValue = !_keepPlayModeChanges.boolValue;
			}
			GUI.backgroundColor = _originalBackgroundColor;

			float pingPong = Mathf.PingPong(Time.unscaledTime, 0.25f);
            
			// if in pause, we display additional controls
			if (TargetMmfPlayer.InScriptDrivenPause)
			{
				// draws a warning box
				_scriptDrivenBoxColor = Color.Lerp(_scriptDrivenBoxColorFrom, _scriptDrivenBoxColorTo, pingPong);
				GUI.skin.box.normal.background = _whiteTexture;
				GUI.backgroundColor = _scriptDrivenBoxColor;
				GUI.skin.box.normal.textColor = Color.black;
				GUILayout.Box(_scriptDrivenInProgressText, GUILayout.ExpandWidth(true));
				GUI.backgroundColor = _originalBackgroundColor;
				GUI.skin.box.normal.background = _scriptDrivenBoxBackgroundTexture; 
                
				// draws resume button
				if (GUILayout.Button(_resumeText))
				{
					TargetMmfPlayer.ResumeFeedbacks();
				}
			}
		}
        
		protected virtual void DrawBottomBar()
		{
			// Draw add new item

			if (_mmfeedbacksList.arraySize > 0)
			{
				MMF_PlayerStyling.DrawSplitter();
			}

			EditorGUILayout.Space();

			EditorGUILayout.BeginHorizontal();
			{
				// Feedback list

				int newItem = EditorGUILayout.Popup(0, _typeDisplays) - 1;
				if (newItem >= 0)
				{
					serializedObject.Update();
					Undo.RecordObject(target, "Add new feedback");
					AddFeedback(_typesAndNames[newItem].FeedbackType);
					serializedObject.ApplyModifiedProperties();
					PrefabUtility.RecordPrefabInstancePropertyModifications(TargetMmfPlayer);
					ForceRepaint();
				}

				// Paste feedback copy as new

				if (MMF_PlayerCopy.HasCopy())
				{
					if (GUILayout.Button(_pasteAsNewText, EditorStyles.miniButton, _pasteAsNewOption))
					{
						PasteAsNew();
					}                        
				}

				if (MMF_PlayerCopy.HasMultipleCopies())
				{
					if (GUILayout.Button(_replaceAllText, EditorStyles.miniButton, _replaceAllOption))
					{
						ReplaceAll();
					}  
					if (GUILayout.Button(_pasteAllAsNewText, EditorStyles.miniButton, _pasteAllAsNewOption))
					{
						PasteAllAsNew();
					}                        
				}
			}

			if (!MMF_PlayerCopy.HasMultipleCopies())
			{
				if (GUILayout.Button(_copyAllText, EditorStyles.miniButton, _pasteAsNewOption))
				{
					CopyAll();
				}
			}                

			EditorGUILayout.EndHorizontal();
		}

		protected virtual void DrawDebugView()
		{
			if (_debugView)
			{
				EditorGUI.BeginDisabledGroup(true);
				EditorGUILayout.PropertyField(_mmfeedbacksList, true);
				EditorGUI.EndDisabledGroup();
			}
		}

		protected virtual void HandleReordering()
		{
			if (_draggedStartID >= 0 && _draggedEndID >= 0)
			{
				if (_draggedEndID != _draggedStartID)
				{
					if (_draggedEndID > _draggedStartID)
						_draggedEndID--;
					_mmfeedbacksList.MoveArrayElement(_draggedStartID, _draggedEndID);
					_draggedStartID = _draggedEndID;
					MMF_FeedbackInspectors.Clear();
				}
			}

			if (_draggedStartID >= 0 || _draggedEndID >= 0)
			{
				switch (_currentEvent.type)
				{
					case EventType.MouseUp:
						_draggedStartID = -1;
						_draggedEndID = -1;
						_currentEvent.Use();
						break;
					default:
						break;
				}
			}
		}

		#endregion InspectorMain

		#region InspectorList

		protected MMF_FeedbackInspector _mmfFeedbackInspector;

		protected virtual void DrawFeedbacksList()
		{
			MMF_PlayerStyling.DrawSection(_feedbacksSectionTitle);
			for (int i = 0; i < _mmfeedbacksList.arraySize; i++)
			{
				DrawFeedbackHeader(i);
                
				// If expanded, draw feedback editor
				_feedbackListFeedback.IsExpanded = _feedbackListIsExpanded;
				if (_feedbackListIsExpanded)
				{
					MMF_PlayerStyling.DrawSplitter();
					EditorGUI.BeginDisabledGroup(!_feedbackListFeedback.Active);

					DrawFeedbackHelp();

					EditorGUILayout.Space();
                    
					// ---------------------------------------------------------------------------------------------------------------------------------
                    
					SerializedProperty currentProperty = _feedbackListProperty;

					if (_feedbackListFeedback.IsExpanded)
					{
						if(MMF_FeedbackInspectors.TryGetValue(_feedbackListFeedback.UniqueID, out _mmfFeedbackInspector))
						{
							_mmfFeedbackInspector.DrawInspector(currentProperty, _feedbackListFeedback);
						}
						else
						{
							MMF_FeedbackInspector newInspector = new MMF_FeedbackInspector(); 
							MMF_FeedbackInspectors.Add(_feedbackListFeedback.UniqueID, newInspector);
							newInspector.Initialization(currentProperty, _feedbackListFeedback, _expandGroupsInInspectors);
						}
					}
                    
					// ---------------------------------------------------------------------------------------------------------------------------------

					EditorGUI.EndDisabledGroup();

					DrawFeedbackBottomBar(i);
				}
			}
		}

		protected virtual bool DrawCustomInspectors(SerializedProperty currentProperty)
		{
			if (_feedbackListFeedback.HasCustomInspectors)
			{
				switch (currentProperty.type)
				{
					case "MMF_Button":
						MMF_Button myButton = (MMF_Button)(currentProperty.MMFGetObjectValue());
						if (GUILayout.Button(myButton.ButtonText))
						{
							myButton.TargetMethod();
						}
						return true;
				}
			}

			return false;
		}

		protected virtual void DrawFeedbackHeader(int i)
		{
			MMF_PlayerStyling.DrawSplitter();
			_feedbackListProperty = _mmfeedbacksList.GetArrayElementAtIndex(i);

			// Retrieve feedback
			_feedbackListFeedback = TargetMmfPlayer.FeedbacksList[i];

			if (_feedbackListFeedback == null)
			{
				return;
			}
            
			// Draw header
			_feedbackListIsExpanded = _feedbackListFeedback.IsExpanded;
			_feedbackListLabel = _feedbackListFeedback.Label;
			_feedbackListPause = false;

			if (_feedbackListFeedback.Pause != null)
			{
				_feedbackListPause = true;
			}
			if ((_feedbackListFeedback.LooperPause == true) && (Application.isPlaying))
			{
				if ((_feedbackListFeedback as MMF_Looper).InfiniteLoop)
				{
					_feedbackListLabel = _feedbackListLabel + _infiniteLoopText;
				}
				else
				{
					_feedbackListLabel = _feedbackListLabel + "[ " + (_feedbackListFeedback as MMF_Looper).NumberOfLoopsLeft + " loops left ] ";
				}                  
			}

			Rect headerRect = MMF_PlayerStyling.DrawHeader(
				ref _feedbackListIsExpanded,
				ref _feedbackListFeedback.Active,
				_feedbackListLabel,
				_feedbackListFeedback.FeedbackColor,
				(GenericMenu menu) =>
				{
					if (Application.isPlaying)
						menu.AddItem(_feedbackPlayGUIContent, false, () => PlayFeedback(i));
					else
						menu.AddDisabledItem(_feedbackPlayGUIContent);
					menu.AddSeparator(null);
					menu.AddItem(_feedbackRemoveGUIContent, false, () => RemoveFeedback(i));
					menu.AddItem(_feedbackResetGUIContent, false, () => ResetContextMenuFeedback(i));
					menu.AddSeparator(null);
					menu.AddItem(_feedbackDuplicateGUIContent, false, () => DuplicateFeedback(i));
					menu.AddItem(_feedbackCopyGUIContent, false, () => CopyFeedback(i));
					if (MMF_PlayerCopy.HasCopy())
						menu.AddItem(_feedbackPasteGUIContent, false, PasteAsNew);
					else
						menu.AddDisabledItem(_feedbackPasteGUIContent);
				},
				_feedbackListFeedback.FeedbackStartedAt,
				_feedbackListFeedback.FeedbackDuration,
				_feedbackListFeedback.TotalDuration,
				_feedbackListFeedback.Timing,
				_feedbackListPause,
				_feedbackListFeedback.RequiresSetup,
				_feedbackListFeedback.RequiredTarget,
				_feedbackListFeedback.DisplayColor,
				_feedbackListFeedback.DisplayFullHeaderColor,
				TargetMmfPlayer 
			);

			// Check if we start dragging this feedback

			switch (_currentEvent.type)
			{
				case EventType.MouseDown:
					if (headerRect.Contains(_currentEvent.mousePosition))
					{
						_draggedStartID = i;
						_currentEvent.Use();
					}
					break;
			}

			// Draw blue rect if feedback is being dragged

			if (_draggedStartID == i && headerRect != Rect.zero)
			{
				EditorGUI.DrawRect(headerRect, _draggedColor);
			}

			// If hovering at the top of the feedback while dragging one, check where the feedback should be dropped : top or bottom

			if (headerRect.Contains(_currentEvent.mousePosition))
			{
				if (_draggedStartID >= 0)
				{
					_draggedEndID = i;
					Rect headerSplit = headerRect;
					headerSplit.height *= 0.5f;
					headerSplit.y += headerSplit.height;
					if (headerSplit.Contains(_currentEvent.mousePosition))
					{
						_draggedEndID = i + 1;
					}
				}
			}
		}

		protected virtual void DrawFeedbackHelp()
		{
			string helpText = FeedbackHelpAttribute.GetFeedbackHelpText(_feedbackListFeedback.GetType());
                    
			if ( (!string.IsNullOrEmpty(helpText)) && (MMF_PlayerConfiguration.Instance.ShowInspectorTips))
			{
				float newHeight = _helptextStyle.CalcHeight(new GUIContent(helpText), EditorGUIUtility.currentViewWidth);
				EditorGUILayout.LabelField(helpText, _helptextStyle);
			}

			if (_feedbackListFeedback.RequiresSetup)
			{
				_redWarningBoxStyle = GUI.skin.GetStyle("helpbox");
				_savedBackground = _redWarningBoxStyle.normal.background;
				_savedTextColor = _redWarningBoxStyle.normal.textColor;
				_redWarningBoxStyle.normal.background = _redWarningBoxBackgroundTexture;
				_redWarningBoxStyle.normal.textColor = Color.black;
				_redWarningBoxStyle.fontSize = 11;
				_redWarningBoxStyle.padding = new RectOffset(8, 8, 8, 8);
				EditorGUILayout.Space();
				EditorGUILayout.HelpBox(_feedbackListFeedback.RequiresSetupText, MessageType.Warning);
				_redWarningBoxStyle.normal.background = _savedBackground;
				_redWarningBoxStyle.normal.textColor = _savedTextColor;
			}
		}

		protected virtual void DrawFeedbackBottomBar(int i)
		{
			EditorGUILayout.Space();

			EditorGUI.BeginDisabledGroup(!Application.isPlaying);
			EditorGUILayout.BeginHorizontal();
			{
				if (GUILayout.Button(_playText, EditorStyles.miniButtonMid))
				{
					PlayFeedback(i);
				}
				if (GUILayout.Button(_stopText, EditorStyles.miniButtonMid))
				{
					StopFeedback(i);
				}
			}
			EditorGUILayout.EndHorizontal();
			EditorGUI.EndDisabledGroup();

			EditorGUILayout.Space();
			EditorGUILayout.Space();
		}

		#endregion InspectorList

		#region Helpers
        
		/// <summary>
		/// We need to repaint constantly if dragging a feedback around
		/// </summary>
		public override bool RequiresConstantRepaint()
		{
			return !TargetMmfPlayer.PerformanceMode && TargetMmfPlayer.IsPlaying;
		}

		#endregion Helpers

		#region FeedbacksControls
        
		/// <summary>
		/// Add a feedback to the list
		/// </summary>
		protected virtual MMF_Feedback AddFeedback(System.Type type)
		{
			return (target as MMF_Player).AddFeedback(type);
		}

		/// <summary>
		/// Remove the selected feedback
		/// </summary>
		protected virtual void RemoveFeedback(int id)
		{
			Undo.RecordObject(target, "Remove feedback");
			MMF_FeedbackInspectors.Remove(TargetMmfPlayer.FeedbacksList[id].UniqueID);
			(target as MMF_Player).RemoveFeedback(id);
			serializedObject.ApplyModifiedProperties();
			ForceRepaint();
			PrefabUtility.RecordPrefabInstancePropertyModifications(TargetMmfPlayer);
		}

		protected virtual void ResetContextMenuFeedback(int id)
		{
			Undo.RecordObject(target, "Reset feedback");

			Type feedbackType = (target as MMF_Player).FeedbacksList[id].GetType();
			MMF_Feedback newFeedback = (target as MMF_Player).AddFeedback(feedbackType, false);
			(target as MMF_Player).FeedbacksList[id] = newFeedback;
			serializedObject.ApplyModifiedProperties();
			ForceRepaint();
			PrefabUtility.RecordPrefabInstancePropertyModifications(TargetMmfPlayer);
		}

		/// <summary>
		/// Play the selected feedback
		/// </summary>
		protected virtual void InitializeFeedback(int id)
		{
			MMF_Feedback feedback = TargetMmfPlayer.FeedbacksList[id];
			feedback.Initialization(TargetMmfPlayer, id);
		}

		/// <summary>
		/// Play the selected feedback
		/// </summary>
		protected virtual void PlayFeedback(int id)
		{
			MMF_Feedback feedback = TargetMmfPlayer.FeedbacksList[id];
			feedback.Play(TargetMmfPlayer.transform.position, TargetMmfPlayer.FeedbacksIntensity);
		}

		/// <summary>
		/// Play the selected feedback
		/// </summary>
		protected virtual void StopFeedback(int id)
		{
			MMF_Feedback feedback = TargetMmfPlayer.FeedbacksList[id];
			feedback.Stop(TargetMmfPlayer.transform.position);
		}

		/// <summary>
		/// Resets the selected feedback
		/// </summary>
		/// <param name="id"></param>
		protected virtual void ResetFeedback(int id)
		{
			MMF_Feedback feedback = TargetMmfPlayer.FeedbacksList[id];
			feedback.ResetFeedback();
		}
        
		#endregion

		#region FeedbacksCopy

		/// <summary>
		/// Copy the selected feedback
		/// </summary>
		protected virtual void CopyFeedback(int id)
		{
			MMF_Feedback feedback = TargetMmfPlayer.FeedbacksList[id];

			MMF_PlayerCopy.Copy(feedback);
		}
		
		/// <summary>
		/// Copies and instantly pastes the selected feedback
		/// </summary>
		protected virtual void DuplicateFeedback(int id)
		{
			MMF_Feedback feedback = TargetMmfPlayer.FeedbacksList[id];

			MMF_PlayerCopy.Copy(feedback);
			PasteAsNew();
		}

		/// <summary>
		/// Asks for a full copy of the source
		/// </summary>
		protected virtual void CopyAll()
		{
			MMF_PlayerCopy.CopyAll(target as MMF_Player);
		}

		/// <summary>
		/// Creates a new feedback and applies the previoulsy copied feedback values
		/// </summary>
		protected virtual void PasteAsNew()
		{
			serializedObject.Update();
			Undo.RecordObject(target, "Paste feedback");
			MMF_PlayerCopy.PasteAll(this);
			serializedObject.ApplyModifiedProperties();
			PrefabUtility.RecordPrefabInstancePropertyModifications(TargetMmfPlayer);
		}

		/// <summary>
		/// Asks for a paste of all feedbacks in the source
		/// </summary>
		protected virtual void PasteAllAsNew()
		{
			serializedObject.Update();
			Undo.RecordObject(target, "Paste all feedbacks as new");
			MMF_PlayerCopy.PasteAll(this);
			serializedObject.ApplyModifiedProperties();
			PrefabUtility.RecordPrefabInstancePropertyModifications(TargetMmfPlayer);
		}

		protected virtual void ReplaceAll()
		{
			serializedObject.Update();
			Undo.RecordObject(target, "Replace all feedbacks");
			TargetMmfPlayer.FeedbacksList.Clear();
			MMF_PlayerCopy.PasteAll(this);
			serializedObject.ApplyModifiedProperties();
			PrefabUtility.RecordPrefabInstancePropertyModifications(TargetMmfPlayer);
		}

		#endregion

		#region Events

		protected virtual void OnDisable()
		{
			foreach (KeyValuePair<int, MMF_FeedbackInspector> inspector in MMF_FeedbackInspectors)
			{
				inspector.Value.OnDisable();
			}
			EditorApplication.playModeStateChanged -= ModeChanged;
		}

		public virtual void ModeChanged(PlayModeStateChange playModeState)
		{
			switch (playModeState)
			{
				case PlayModeStateChange.ExitingPlayMode:
					StoreRuntimeChanges();
					break;
        
				case PlayModeStateChange.EnteredEditMode:
					ApplyRuntimeChanges();
					break;
			}
		}

		protected virtual void StoreRuntimeChanges()
		{
			foreach (MMF_Player player in FindObjectsOfType<MMF_Player>().Where(p => p.KeepPlayModeChanges))
			{
				MMF_PlayerCopy.StoreRuntimeChanges(player);
			}
		}

		protected virtual void ApplyRuntimeChanges()
		{
			foreach (MMF_Player player in FindObjectsOfType<MMF_Player>().Where(MMF_PlayerCopy.RuntimeChanges.ContainsKey))
			{
				MMF_PlayerCopy.ApplyRuntimeChanges(player);
			}
			ForceRepaint();
		}

		public virtual void ForceRepaint()
		{
			MMF_FeedbackInspectors.Clear();
			Initialization();
			(target as MMF_Player).RefreshCache();
			Repaint();
		}
        
		protected virtual void Reset()
		{
			ForceRepaint();
		}

		#endregion
        
        
	}
}