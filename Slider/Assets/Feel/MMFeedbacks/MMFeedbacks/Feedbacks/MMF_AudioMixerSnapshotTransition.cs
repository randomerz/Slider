using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback will let you transition to a target AudioMixer Snapshot over a specified time
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback will let you transition to a target AudioMixer Snapshot over a specified time")]
	[FeedbackPath("Audio/AudioMixer Snapshot Transition")]
	public class MMF_AudioMixerSnapshotTransition : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.UIColor; } }
		public override bool EvaluateRequiresSetup() { return ((TargetSnapshot == null) || (OriginalSnapshot == null)); }
		public override string RequiredTargetText { get { return ((TargetSnapshot != null) && (OriginalSnapshot != null)) ? TargetSnapshot.name + " to "+ OriginalSnapshot.name : "";  } }
		public override string RequiresSetupText { get { return "This feedback requires that both a OriginalSnapshot and TargetSnapshot be set to be able to work properly. You can set these below."; } }
		#endif
        
		[MMFInspectorGroup("AudioMixer Snapshot", true, 44)]
		/// the target audio mixer snapshot we want to transition to 
		[Tooltip("the target audio mixer snapshot we want to transition to")]
		public AudioMixerSnapshot TargetSnapshot;
		/// the audio mixer snapshot we want to transition from, optional, only needed if you plan to play this feedback in reverse 
		[Tooltip("the audio mixer snapshot we want to transition from, optional, only needed if you plan to play this feedback in reverse")]
		public AudioMixerSnapshot OriginalSnapshot;
		/// the duration, in seconds, over which to transition to the selected snapshot
		[Tooltip("the duration, in seconds, over which to transition to the selected snapshot")]
		public float TransitionDuration = 1f;
        
		/// <summary>
		/// On play we transition to the selected snapshot
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			if (TargetSnapshot == null)
			{
				return;
			}

			if (!NormalPlayDirection)
			{
				if (OriginalSnapshot != null)
				{
					OriginalSnapshot.TransitionTo(TransitionDuration);     
				}
				else
				{
					TargetSnapshot.TransitionTo(TransitionDuration);
				}
			}
			else
			{
				TargetSnapshot.TransitionTo(TransitionDuration);     
			}
		}
	}
}