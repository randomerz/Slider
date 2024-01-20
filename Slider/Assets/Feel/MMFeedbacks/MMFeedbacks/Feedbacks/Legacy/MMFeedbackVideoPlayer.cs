using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Video;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback lets you control video players in all sorts of ways (Play, Pause, Toggle, Stop, Prepare, StepForward, StepBackward, SetPlaybackSpeed, SetDirectAudioVolume, SetDirectAudioMute, GoToFrame, ToggleLoop)
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback lets you control video players in all sorts of ways (Play, Pause, Toggle, Stop, Prepare, StepForward, StepBackward, SetPlaybackSpeed, SetDirectAudioVolume, SetDirectAudioMute, GoToFrame, ToggleLoop)")]
	[FeedbackPath("UI/Video Player")]
	public class MMFeedbackVideoPlayer : MMFeedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		public enum VideoActions { Play, Pause, Toggle, Stop, Prepare, StepForward, StepBackward, SetPlaybackSpeed, SetDirectAudioVolume, SetDirectAudioMute, GoToFrame, ToggleLoop  }

		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.UIColor; } }
		#endif

		[Header("Video Player")]
		/// the Video Player to control with this feedback
		[Tooltip("the Video Player to control with this feedback")]
		public VideoPlayer TargetVideoPlayer;

		/// the Video Player to control with this feedback
		[Tooltip("the Video Player to control with this feedback")]
		public VideoActions VideoAction = VideoActions.Pause;

		/// the frame at which to jump when in GoToFrame mode
		[Tooltip("the frame at which to jump when in GoToFrame mode")]
		[MMFEnumCondition("VideoAction", (int)VideoActions.GoToFrame)]
		public long TargetFrame = 10;
        
		/// the new playback speed (between 0 and 10)
		[Tooltip("the new playback speed (between 0 and 10)")]
		[MMFEnumCondition("VideoAction", (int)VideoActions.SetPlaybackSpeed)]
		public float PlaybackSpeed = 2f;

		/// the track index on which to control volume
		[Tooltip("the track index on which to control volume")]
		[MMFEnumCondition("VideoAction", (int)VideoActions.SetDirectAudioMute, (int)VideoActions.SetDirectAudioVolume)]
		public int TrackIndex = 0;
		/// the new volume for the specified track, between 0 and 1
		[Tooltip("the new volume for the specified track, between 0 and 1")]
		[MMFEnumCondition("VideoAction", (int)VideoActions.SetDirectAudioVolume)]
		public float Volume = 1f;
		/// whether to mute the track or not when that feedback plays
		[Tooltip("whether to mute the track or not when that feedback plays")]
		[MMFEnumCondition("VideoAction", (int)VideoActions.SetDirectAudioMute)]
		public bool Mute = true;

		/// <summary>
		/// On play we apply the selected command to our target video player
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			if (TargetVideoPlayer == null)
			{
				return;
			}

			switch (VideoAction)
			{
				case VideoActions.Play:
					TargetVideoPlayer.Play();
					break;
				case VideoActions.Pause:
					TargetVideoPlayer.Pause();
					break;
				case VideoActions.Toggle:
					if (TargetVideoPlayer.isPlaying)
					{
						TargetVideoPlayer.Pause();
					}
					else
					{
						TargetVideoPlayer.Play();
					}
					break;
				case VideoActions.Stop:
					TargetVideoPlayer.Stop();
					break;
				case VideoActions.Prepare:
					TargetVideoPlayer.Prepare();
					break;
				case VideoActions.StepForward:
					TargetVideoPlayer.StepForward();
					break;
				case VideoActions.StepBackward:
					TargetVideoPlayer.Pause();
					TargetVideoPlayer.frame = TargetVideoPlayer.frame - 1;
					break;
				case VideoActions.SetPlaybackSpeed:
					TargetVideoPlayer.playbackSpeed = PlaybackSpeed;
					break;
				case VideoActions.SetDirectAudioVolume:
					TargetVideoPlayer.SetDirectAudioVolume((ushort)TrackIndex, Volume);
					break;
				case VideoActions.SetDirectAudioMute:
					TargetVideoPlayer.SetDirectAudioMute((ushort)TrackIndex, Mute);
					break;
				case VideoActions.GoToFrame:
					TargetVideoPlayer.frame = TargetFrame;
					break;
				case VideoActions.ToggleLoop:
					TargetVideoPlayer.isLooping = !TargetVideoPlayer.isLooping;
					break;
			}

		}
	}
}