using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MoreMountains.Tools
{
	/// <summary>
	/// Custom editor for the MMSoundManager, used to display custom track controls
	/// </summary>
	#if UNITY_EDITOR
	[CustomEditor(typeof(MMSoundManager), true)]
	[CanEditMultipleObjects]
	public class MMSoundManagerEditor : Editor
	{
		public override bool RequiresConstantRepaint()
		{
			return true;
		}
		protected MMSoundManagerSettingsSO _settingsSO;
		protected MMSoundManager _mmSoundManager;
        
		private static float _masterVolume, _musicVolume, _sfxVolume, _uiVolume;

		protected Color _originalBackgroundColor;
        
		protected Color _saveButtonColor = new Color32(80, 80, 80, 255);
		protected Color _loadButtonColor = new Color32(107, 107, 107, 255);
		protected Color _resetButtonColor = new Color32(120, 120, 120, 255);
        
		protected Color _baseColor = new Color32(150, 150, 150, 255);

		protected Color _masterColorBase = MMColors.ReunoYellow;
		protected Color _masterColorMute;
		protected Color _masterColorUnmute;
		protected Color _masterColorPause;
		protected Color _masterColorStop;
		protected Color _masterColorPlay;
		protected Color _masterColorFree;

		protected Color _musicColorBase = MMColors.Aquamarine;
		protected Color _musicColorMute;
		protected Color _musicColorUnmute;
		protected Color _musicColorPause;
		protected Color _musicColorStop;
		protected Color _musicColorPlay;
		protected Color _musicColorFree;
        
		protected Color _sfxColorBase = MMColors.Coral;
		protected Color _sfxColorMute;
		protected Color _sfxColorUnmute;
		protected Color _sfxColorPause;
		protected Color _sfxColorStop;
		protected Color _sfxColorPlay;
		protected Color _sfxColorFree;

		protected Color _uiColorBase = MMColors.SteelBlue;
		protected Color _uiColorMute;
		protected Color _uiColorUnmute;
		protected Color _uiColorPause;
		protected Color _uiColorStop;
		protected Color _uiColorPlay;
		protected Color _uiColorFree;

		protected MMColors.ColoringMode _coloringMode = MMColors.ColoringMode.Add;

		/// <summary>
		/// On Enable, we initialize our button colors. Why? Because we can.
		/// </summary>
		protected virtual void OnEnable()
		{
			_masterColorMute = MMColors.MMColorize(_baseColor, _masterColorBase, _coloringMode, 1f);
			_masterColorUnmute = MMColors.MMColorize(_baseColor, _masterColorBase, _coloringMode, 0.9f);
			_masterColorPause = MMColors.MMColorize(_baseColor, _masterColorBase, _coloringMode, 0.8f);
			_masterColorStop = MMColors.MMColorize(_baseColor, _masterColorBase, _coloringMode, 0.7f);
			_masterColorPlay = MMColors.MMColorize(_baseColor, _masterColorBase, _coloringMode, 0.5f);
			_masterColorFree = MMColors.MMColorize(_baseColor, _masterColorBase, _coloringMode, 0.4f);
            
			_musicColorMute = MMColors.MMColorize(_baseColor, _musicColorBase, _coloringMode, 1f);
			_musicColorUnmute = MMColors.MMColorize(_baseColor, _musicColorBase, _coloringMode, 0.9f);
			_musicColorPause = MMColors.MMColorize(_baseColor, _musicColorBase, _coloringMode, 0.8f);
			_musicColorStop = MMColors.MMColorize(_baseColor, _musicColorBase, _coloringMode, 0.7f);
			_musicColorPlay = MMColors.MMColorize(_baseColor, _musicColorBase, _coloringMode, 0.5f);
			_musicColorFree = MMColors.MMColorize(_baseColor, _musicColorBase, _coloringMode, 0.4f);
            
			_sfxColorMute = MMColors.MMColorize(_baseColor, _sfxColorBase, _coloringMode, 1f);
			_sfxColorUnmute = MMColors.MMColorize(_baseColor, _sfxColorBase, _coloringMode, 0.9f);
			_sfxColorPause = MMColors.MMColorize(_baseColor, _sfxColorBase, _coloringMode, 0.8f);
			_sfxColorStop = MMColors.MMColorize(_baseColor, _sfxColorBase, _coloringMode, 0.7f);
			_sfxColorPlay = MMColors.MMColorize(_baseColor, _sfxColorBase, _coloringMode, 0.5f);
			_sfxColorFree = MMColors.MMColorize(_baseColor, _sfxColorBase, _coloringMode, 0.4f);
            
			_uiColorMute = MMColors.MMColorize(_baseColor, _uiColorBase, _coloringMode, 1f);
			_uiColorUnmute = MMColors.MMColorize(_baseColor, _uiColorBase, _coloringMode, 0.9f);
			_uiColorPause = MMColors.MMColorize(_baseColor, _uiColorBase, _coloringMode, 0.8f);
			_uiColorStop = MMColors.MMColorize(_baseColor, _uiColorBase, _coloringMode, 0.7f);
			_uiColorPlay = MMColors.MMColorize(_baseColor, _uiColorBase, _coloringMode, 0.5f);
			_uiColorFree = MMColors.MMColorize(_baseColor, _uiColorBase, _coloringMode, 0.4f);
		}

		/// <summary>
		/// On GUI, draws the base inspector and track controls
		/// </summary>
		public override void OnInspectorGUI()
		{
			_settingsSO = (target as MMSoundManager).settingsSo;
			_mmSoundManager = target as MMSoundManager;
            
			if (_settingsSO != null)
			{
				_masterVolume = _settingsSO.GetTrackVolume(MMSoundManager.MMSoundManagerTracks.Master);
				_musicVolume = _settingsSO.GetTrackVolume(MMSoundManager.MMSoundManagerTracks.Music);
				_sfxVolume = _settingsSO.GetTrackVolume(MMSoundManager.MMSoundManagerTracks.Sfx);
				_uiVolume = _settingsSO.GetTrackVolume(MMSoundManager.MMSoundManagerTracks.UI);    
			}

			serializedObject.Update();
			DrawDefaultInspector();
			serializedObject.ApplyModifiedProperties();

			if ( ((_settingsSO != null) && _mmSoundManager.gameObject.activeInHierarchy))
			{
				DrawTrack("Master Track", _mmSoundManager.settingsSo.Settings.MasterOn, MMSoundManager.MMSoundManagerTracks.Master, _masterColorMute, _masterColorUnmute, _masterColorPause, _masterColorStop, _masterColorPlay, _masterColorFree);
				DrawTrack("Music Track", _mmSoundManager.settingsSo.Settings.MusicOn, MMSoundManager.MMSoundManagerTracks.Music, _musicColorMute, _musicColorUnmute, _musicColorPause, _musicColorStop, _musicColorPlay, _musicColorFree);
				DrawTrack("SFX Track", _mmSoundManager.settingsSo.Settings.SfxOn, MMSoundManager.MMSoundManagerTracks.Sfx, _sfxColorMute, _sfxColorUnmute, _sfxColorPause, _sfxColorStop, _sfxColorPlay, _sfxColorFree);
				DrawTrack("UI Track", _mmSoundManager.settingsSo.Settings.UIOn, MMSoundManager.MMSoundManagerTracks.UI, _uiColorMute, _uiColorUnmute, _uiColorPause, _uiColorStop, _uiColorPlay, _uiColorFree);
				DrawSaveLoadButtons();
			}
		}

		/// <summary>
		/// Draws track controls for the specified track
		/// </summary>
		/// <param name="title"></param>
		/// <param name="mute"></param>
		/// <param name="track"></param>
		/// <param name="muteColor"></param>
		/// <param name="unmuteColor"></param>
		/// <param name="pauseColor"></param>
		/// <param name="stopColor"></param>
		/// <param name="playColor"></param>
		/// <param name="freeColor"></param>
		protected virtual void DrawTrack(string title, bool mute, MMSoundManager.MMSoundManagerTracks track, Color muteColor, Color unmuteColor, Color pauseColor, Color stopColor, Color playColor, Color freeColor)
		{
			GUILayout.Space(10);
			GUILayout.Label(title, EditorStyles.boldLabel);
            
			EditorGUI.BeginDisabledGroup(!Application.isPlaying);

			// we draw the volume slider
			EditorGUILayout.BeginHorizontal();
            
			GUILayout.Label("Volume");
            
			float newVolume = 0;
			switch (track)
			{
				case MMSoundManager.MMSoundManagerTracks.Master:
					newVolume = EditorGUILayout.Slider(_masterVolume, MMSoundManagerSettings._minimalVolume, MMSoundManagerSettings._maxVolume);
					if (newVolume != _masterVolume) { _mmSoundManager.settingsSo.SetTrackVolume(MMSoundManager.MMSoundManagerTracks.Master, newVolume); }
					break;
				case MMSoundManager.MMSoundManagerTracks.Music:
					newVolume = EditorGUILayout.Slider(_musicVolume, MMSoundManagerSettings._minimalVolume, MMSoundManagerSettings._maxVolume);
					if (newVolume != _musicVolume) { _mmSoundManager.settingsSo.SetTrackVolume(MMSoundManager.MMSoundManagerTracks.Music, newVolume); }
					break;
				case MMSoundManager.MMSoundManagerTracks.Sfx:
					newVolume = EditorGUILayout.Slider(_sfxVolume, MMSoundManagerSettings._minimalVolume, MMSoundManagerSettings._maxVolume);
					if (newVolume != _sfxVolume) { _mmSoundManager.settingsSo.SetTrackVolume(MMSoundManager.MMSoundManagerTracks.Sfx, newVolume); }
					break;
				case MMSoundManager.MMSoundManagerTracks.UI:
					newVolume = EditorGUILayout.Slider(_uiVolume, MMSoundManagerSettings._minimalVolume, MMSoundManagerSettings._maxVolume);
					if (newVolume != _uiVolume) { _mmSoundManager.settingsSo.SetTrackVolume(MMSoundManager.MMSoundManagerTracks.UI, newVolume); }
					break;
			}
			EditorGUILayout.EndHorizontal();

			// we draw the buttons
			EditorGUILayout.BeginHorizontal();
			{
				if (mute)
				{
					DrawColoredButton("Mute", muteColor, track, _mmSoundManager.MuteTrack, EditorStyles.miniButtonLeft);    
				}
				else
				{
					DrawColoredButton("Unmute", unmuteColor, track, _mmSoundManager.UnmuteTrack, EditorStyles.miniButtonMid);    
				}
				DrawColoredButton("Pause", pauseColor, track, _mmSoundManager.PauseTrack, EditorStyles.miniButtonMid);
				DrawColoredButton("Stop", stopColor, track, _mmSoundManager.StopTrack, EditorStyles.miniButtonMid);
				DrawColoredButton("Play", playColor, track, _mmSoundManager.PlayTrack, EditorStyles.miniButtonMid);
				DrawColoredButton("Free", freeColor, track, _mmSoundManager.FreeTrack, EditorStyles.miniButtonRight);
                
			}
			EditorGUILayout.EndHorizontal();
            
			EditorGUI.EndDisabledGroup();
		}
        
		/// <summary>
		/// Draws save related buttons
		/// </summary>
		protected virtual void DrawSaveLoadButtons()
		{
			EditorGUI.BeginDisabledGroup(!Application.isPlaying);
            
			GUILayout.Space(10);
			GUILayout.Label("Settings", EditorStyles.boldLabel);
            
			EditorGUILayout.BeginHorizontal();
            
			DrawColoredButton("Save", _saveButtonColor, _settingsSO.SaveSoundSettings, EditorStyles.miniButtonLeft);
			DrawColoredButton("Load", _loadButtonColor, _settingsSO.LoadSoundSettings, EditorStyles.miniButtonMid);
			DrawColoredButton("Reset", _resetButtonColor, _settingsSO.ResetSoundSettings, EditorStyles.miniButtonRight);
            
			EditorGUILayout.EndHorizontal();
            
			EditorGUI.EndDisabledGroup();
		}

		/// <summary>
		/// Draws a button
		/// </summary>
		/// <param name="buttonLabel"></param>
		/// <param name="buttonColor"></param>
		/// <param name="track"></param>
		/// <param name="action"></param>
		/// <param name="styles"></param>
		public virtual void DrawColoredButton(string buttonLabel, Color buttonColor, MMSoundManager.MMSoundManagerTracks track, System.Action<MMSoundManager.MMSoundManagerTracks> action, GUIStyle styles)
		{
			_originalBackgroundColor = GUI.backgroundColor;
			GUI.backgroundColor = buttonColor;
			if (GUILayout.Button(buttonLabel, styles))
			{
				action.Invoke(track);
			}
			GUI.backgroundColor = _originalBackgroundColor;
		}

		/// <summary>
		/// Draws a button
		/// </summary>
		/// <param name="buttonLabel"></param>
		/// <param name="buttonColor"></param>
		/// <param name="action"></param>
		/// <param name="styles"></param>
		protected virtual void DrawColoredButton(string buttonLabel, Color buttonColor, Action action, GUIStyle styles)
		{
			_originalBackgroundColor = GUI.backgroundColor;
			GUI.backgroundColor = buttonColor;
			if (GUILayout.Button(buttonLabel, styles))
			{
				action.Invoke();
			}
			GUI.backgroundColor = _originalBackgroundColor;
		}
        
	}
	#endif
}