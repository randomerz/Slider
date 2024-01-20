using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback will cause a pause when met, preventing any other feedback lower in the sequence to run until it's complete.
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback will cause a pause when met, preventing any other feedback lower in the sequence to run until it's complete.")]
	[FeedbackPath("Pause/Pause")]
	public class MMF_Pause : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.PauseColor; } }
		public override bool DisplayFullHeaderColor => true;
		#endif
		public override IEnumerator Pause { get { return PauseWait(); } }
        
		[MMFInspectorGroup("Pause", true, 32)]
		/// the duration of the pause, in seconds
		[Tooltip("the duration of the pause, in seconds")]
		public float PauseDuration = 1f;

		public bool RandomizePauseDuration = false;

		[MMFCondition("RandomizePauseDuration", true)]
		public float MinPauseDuration = 1f;
		[MMFCondition("RandomizePauseDuration", true)]
		public float MaxPauseDuration = 3f;
		[MMFCondition("RandomizePauseDuration", true)]
		public bool RandomizeOnEachPlay = true;
        
		/// if this is true, you'll need to call the Resume() method on the host MMFeedbacks for this pause to stop, and the rest of the sequence to play
		[Tooltip("if this is true, you'll need to call the Resume() method on the host MMFeedbacks for this pause to stop, and the rest of the sequence to play")]
		public bool ScriptDriven = false;
		/// if this is true, a script driven pause will resume after its AutoResumeAfter delay, whether it has been manually resumed or not 
		[Tooltip("if this is true, a script driven pause will resume after its AutoResumeAfter delay, whether it has been manually resumed or not")] 
		[MMFCondition("ScriptDriven", true)]
		public bool AutoResume = false;
		/// the duration after which to auto resume, regardless of manual resume calls beforehand
		[Tooltip("the duration after which to auto resume, regardless of manual resume calls beforehand")] 
		[MMFCondition("AutoResume", true)]
		public float AutoResumeAfter = 0.25f;
        
		/// the duration of this feedback is the duration of the pause
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(PauseDuration); } set { PauseDuration = value; } }

		/// <summary>
		/// An IEnumerator used to wait for the duration of the pause, on scaled or unscaled time
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator PauseWait()
		{
			yield return WaitFor(PauseDuration);
		}

		/// <summary>
		/// On init we cache our wait for seconds
		/// </summary>
		/// <param name="owner"></param>
		protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);
			ScriptDrivenPause = ScriptDriven;
			ScriptDrivenPauseAutoResume = AutoResume ? AutoResumeAfter : -1f;
			if (RandomizePauseDuration)
			{
				PauseDuration = Random.Range(MinPauseDuration, MaxPauseDuration);
			}
		}

		/// <summary>
		/// On play we trigger our pause
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
            
			if (RandomizePauseDuration && RandomizeOnEachPlay)
			{
				PauseDuration = Random.Range(MinPauseDuration, MaxPauseDuration);
			}
			Owner.StartCoroutine(PlayPause());
		}

		/// <summary>
		/// Pause coroutine
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator PlayPause()
		{
			yield return Pause;
		}
	}
}