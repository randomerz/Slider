using UnityEngine;
using MoreMountains.Feedbacks;

namespace MoreMountains.FeedbacksForThirdParty
{
	/// <summary>
	/// Add this feedback to be able to trigger haptic feedbacks via the NiceVibration library.
	/// It'll let you create transient or continuous vibrations, play presets or advanced patterns via AHAP files, and stop any vibration at any time
	/// This feedback has been deprecated, and is just here to avoid errors in case you were to update from an old version. Use the new haptic feedbacks instead.
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackPath("Haptics/Haptics DEPRECATED!")]
	[FeedbackHelp("This feedback has been deprecated, and is just here to avoid errors in case you were to update from an old version. Use any of the new haptic feedbacks instead.")]
	public class MMF_Haptics : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		
		[Header("Deprecated Feedback")] 
		/// if this is true, this feedback will output a warning when played
		public bool OutputDeprecationWarning = true;
	    
		/// <summary>
		/// When this feedback gets played
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			if (OutputDeprecationWarning)
			{
				Debug.LogWarning(Owner.name + " : the haptic feedback on this object is using the old version of Nice Vibrations, and won't work anymore. Replace it with any of the new haptic feedbacks.");
			}
		}
	}
}