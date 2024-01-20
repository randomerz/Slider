using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback will let you pilot a MMPlaylist
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback will let you pilot a MMPlaylist")]
	[FeedbackPath("Audio/MMPlaylist")]
	public class MMF_Playlist : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.SoundsColor; } }
		public override string RequiredTargetText { get => Mode.ToString(); }
		public override bool HasChannel => true;
		#endif
		
		public enum Modes { Play, PlayNext, PlayPrevious, Stop, Pause, PlaySongAt, SetVolumeMultiplier, ChangePlaylist }
 
		[MMFInspectorGroup("MMPlaylist", true, 13)]
		/// the action to call on the playlist
		[Tooltip("the action to call on the playlist")]
		public Modes Mode = Modes.PlayNext;
		/// the index of the song to play
		[Tooltip("the index of the song to play")]
		[MMEnumCondition("Mode", (int)Modes.PlaySongAt)]
		public int SongIndex = 0;
		/// the volume multiplier to apply
		[Tooltip("the volume multiplier to apply")]
		[MMEnumCondition("Mode", (int)Modes.SetVolumeMultiplier)]
		public float VolumeMultiplier = 1f;
		/// whether to apply the volume multiplier instantly (true) or only when the next song starts playing (false)
		[Tooltip("whether to apply the volume multiplier instantly (true) or only when the next song starts playing (false)")]
		[MMEnumCondition("Mode", (int)Modes.SetVolumeMultiplier)]
		public bool ApplyVolumeMultiplierInstantly = false;
		/// in change playlist mode, the playlist to which to switch to. Only works with MMSMPlaylistManager
		[Tooltip("in change playlist mode, the playlist to which to switch to. Only works with MMSMPlaylistManager")]
		[MMEnumCondition("Mode", (int)Modes.ChangePlaylist)]
		public MMSMPlaylist NewPlaylist;
		/// in change playlist mode, whether or not to play the new playlist after the switch. Only works with MMSMPlaylistManager
		[Tooltip("in change playlist mode, whether or not to play the new playlist after the switch. Only works with MMSMPlaylistManager")]
		[MMEnumCondition("Mode", (int)Modes.ChangePlaylist)]
		public bool ChangePlaylistAndPlay = true;
        
		protected Coroutine _coroutine;

		/// <summary>
		/// On Play we change the values of our fog
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			switch (Mode)
			{
				case Modes.Play:
					MMPlaylistPlayEvent.Trigger(Channel);
					break;
				case Modes.PlayNext:
					MMPlaylistPlayNextEvent.Trigger(Channel);
					break;
				case Modes.PlayPrevious:
					MMPlaylistPlayPreviousEvent.Trigger(Channel);
					break;
				case Modes.Stop:
					MMPlaylistStopEvent.Trigger(Channel);
					break;
				case Modes.Pause:
					MMPlaylistPauseEvent.Trigger(Channel);
					break;
				case Modes.PlaySongAt:
					MMPlaylistPlayIndexEvent.Trigger(Channel, SongIndex);
					break;
				case Modes.SetVolumeMultiplier:
					MMPlaylistVolumeMultiplierEvent.Trigger(Channel, VolumeMultiplier, ApplyVolumeMultiplierInstantly);
					break;
				case Modes.ChangePlaylist:
					MMPlaylistChangeEvent.Trigger(Channel, NewPlaylist, ChangePlaylistAndPlay);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
            
		}
	}
}