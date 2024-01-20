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
	public class MMFeedbackPlaylist : MMFeedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.SoundsColor; } }
		#endif
		
		public enum Modes { Play, PlayNext, PlayPrevious, Stop, Pause, PlaySongAt }

		[Header("MMPlaylist")] 
		/// the channel of the target MMPlaylist
		[Tooltip("the channel of the target MMPlaylist")]
		public int Channel = 0;
		/// the action to call on the playlist
		[Tooltip("the action to call on the playlist")]
		public Modes Mode = Modes.PlayNext;
		/// the index of the song to play
		[Tooltip("the index of the song to play")]
		[MMEnumCondition("Mode", (int)Modes.PlaySongAt)]
		public int SongIndex = 0;
        
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
				default:
					throw new ArgumentOutOfRangeException();
			}
            
		}
	}
}