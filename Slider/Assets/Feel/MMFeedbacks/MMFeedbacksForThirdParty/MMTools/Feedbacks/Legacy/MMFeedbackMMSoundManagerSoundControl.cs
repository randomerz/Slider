using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using MoreMountains.Tools;
using UnityEngine.Audio;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback will let you control a specific sound (or sounds), targeted by SoundID, which has to match the SoundID of the sound you intially played. You will need a MMSoundManager in your scene for this to work.
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackPath("Audio/MMSoundManager Sound Control")]
	[FeedbackHelp("This feedback will let you control a specific sound (or sounds), targeted by SoundID, which has to match the SoundID of the sound you intially played. You will need a MMSoundManager in your scene for this to work.")]
	public class MMFeedbackMMSoundManagerSoundControl : MMFeedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.SoundsColor; } }
		#endif

		[Header("MMSoundManager Sound Control")]
		/// the action to trigger on the specified sound
		[Tooltip("the action to trigger on the specified sound")]
		public MMSoundManagerSoundControlEventTypes ControlMode = MMSoundManagerSoundControlEventTypes.Pause;
		/// the ID of the sound, has to match the one you specified when playing it
		[Tooltip("the ID of the sound, has to match the one you specified when playing it")]
		public int SoundID = 0;

		protected AudioSource _targetAudioSource;
        
		/// <summary>
		/// On play, triggers an event meant to be caught by the MMSoundManager and acted upon
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			MMSoundManagerSoundControlEvent.Trigger(ControlMode, SoundID);
		}
	}
}