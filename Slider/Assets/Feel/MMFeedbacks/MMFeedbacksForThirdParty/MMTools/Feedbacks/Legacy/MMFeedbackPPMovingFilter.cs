using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback will trigger a post processing moving filter event, meant to be caught by a MMPostProcessingMovableFilter object
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback will trigger a post processing moving filter event, meant to be caught by a MMPostProcessingMovableFilter object")]
	[FeedbackPath("PostProcess/PPMovingFilter")]
	public class MMFeedbackPPMovingFilter : MMFeedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.PostProcessColor; } }
		#endif

		/// the possible modes for this feedback 
		public enum Modes { Toggle, On, Off }

		[Header("PostProcessing Profile Moving Filter")]
		/// the selected mode for this feedback 
		[Tooltip("the selected mode for this feedback")]
		public Modes Mode = Modes.Toggle;
		/// the channel to target
		[Tooltip("the channel to target")]
		public int Channel = 0;
		/// the duration of the transition
		[Tooltip("the duration of the transition")]
		public float TransitionDuration = 1f;
		/// the curve to move along to
		[Tooltip("the curve to move along to")]
		public MMTweenType Curve = new MMTweenType(MMTween.MMTweenCurve.EaseInCubic);
        
		/// the duration of this feedback is the duration of the transition
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(TransitionDuration); } set { TransitionDuration = value;  } }

		protected bool _active = false;
		protected bool _toggle = false;

		/// <summary>
		/// On custom play, we trigger a MMPostProcessingMovingFilterEvent with the selected parameters
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
            
			_active = (Mode == Modes.On);
			_toggle = (Mode == Modes.Toggle);

			MMPostProcessingMovingFilterEvent.Trigger(Curve, _active, _toggle, FeedbackDuration, Channel);
		}

		/// <summary>
		/// On stop we stop our transition
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			base.CustomStopFeedback(position, feedbacksIntensity);
            
			MMPostProcessingMovingFilterEvent.Trigger(Curve, _active, _toggle, FeedbackDuration, stop:true);
		}
	}
}