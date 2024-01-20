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
	/// This feedback will let you trigger save, load, and reset on MMSoundManager settings. You will need a MMSoundManager in your scene for this to work.
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackPath("Audio/MMSoundManager Save and Load")]
	[FeedbackHelp("This feedback will let you trigger save, load, and reset on MMSoundManager settings. You will need a MMSoundManager in your scene for this to work.")]
	public class MMF_MMSoundManagerSaveLoad : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.SoundsColor; } }
		public override string RequiredTargetText { get { return Mode.ToString();  } }
		#endif

		/// the possible modes you can use to interact with save settings
		public enum Modes { Save, Load, Reset }

		[MMFInspectorGroup("MMSoundManager Save and Load", true, 30)]
		/// the selected mode to interact with save settings on the MMSoundManager
		[Tooltip("the selected mode to interact with save settings on the MMSoundManager")]
		public Modes Mode = Modes.Save;
        
		/// <summary>
		/// On Play, saves, loads or resets settings
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
				case Modes.Save:
					MMSoundManagerEvent.Trigger(MMSoundManagerEventTypes.SaveSettings);
					break;
				case Modes.Load:
					MMSoundManagerEvent.Trigger(MMSoundManagerEventTypes.LoadSettings);
					break;
				case Modes.Reset:
					MMSoundManagerEvent.Trigger(MMSoundManagerEventTypes.ResetSettings);
					break;
			}
		}
	}
}