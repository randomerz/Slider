using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback will let you trigger cross fades on a target Graphic.
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback will let you trigger cross fades on a target Graphic.")]
	[FeedbackPath("UI/Graphic CrossFade")]
	public class MMF_GraphicCrossFade : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.UIColor; } }
		public override bool EvaluateRequiresSetup() { return (TargetGraphic == null); }
		public override string RequiredTargetText { get { return TargetGraphic != null ? TargetGraphic.name : "";  } }
		public override string RequiresSetupText { get { return "This feedback requires that a TargetGraphic be set to be able to work properly. You can set one below."; } }
		#endif

		/// the duration of this feedback is the duration of the Image, or 0 if instant
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(Duration); } set { Duration = value; } }
		public override bool HasChannel => true;
		public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => TargetGraphic = FindAutomatedTarget<Graphic>();

		/// the possible modes for this feedback
		public enum Modes { Alpha, Color }

		[MMFInspectorGroup("Graphic Cross Fade", true, 54, true)]
		/// the Graphic to affect when playing the feedback
		[Tooltip("the Graphic to affect when playing the feedback")]
		public Graphic TargetGraphic;
		/// whether the feedback should affect the Image instantly or over a period of time
		[Tooltip("whether the feedback should affect the Image instantly or over a period of time")]
		public Modes Mode = Modes.Alpha;
		/// how long the Graphic should change over time
		[Tooltip("how long the Graphic should change over time")]
		public float Duration = 0.2f;
		/// the target alpha
		[Tooltip("the target alpha")]
		[MMFEnumCondition("Mode", (int)Modes.Alpha)]
		public float TargetAlpha = 0.2f;
		/// the target color
		[Tooltip("the target color")]
		[MMFEnumCondition("Mode", (int)Modes.Color)]
		public Color TargetColor = Color.red;
		/// whether or not the crossfade should also tween the alpha channel
		[Tooltip("whether or not the crossfade should also tween the alpha channel")]
		[MMFEnumCondition("Mode", (int)Modes.Color)]
		public bool UseAlpha = true;
		/// if this is true, the target will be disabled when this feedbacks is stopped
		[Tooltip("if this is true, the target will be disabled when this feedbacks is stopped")] 
		public bool DisableOnStop = false;
        
		protected Coroutine _coroutine;
		protected Color _initialColor;
		
		protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);

			if (TargetGraphic != null)
			{
				_initialColor = TargetGraphic.color;	
			}
		}
        
		/// <summary>
		/// On Play we turn our Graphic on and start an over time coroutine if needed
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized || (TargetGraphic == null))
			{
				return;
			}
        
			Turn(true);
			bool ignoreTimeScale = !InScaledTimescaleMode;
			switch (Mode)
			{
				case Modes.Alpha:
					// the following lines fix a bug with CrossFadeAlpha
					_initialColor.a = NormalPlayDirection ? 1 : 0;
					TargetGraphic.color = NormalPlayDirection ? _initialColor : TargetColor;
					TargetGraphic.CrossFadeAlpha(NormalPlayDirection ? 0f : 1f, 0f, true);
	                
					TargetGraphic.CrossFadeAlpha(NormalPlayDirection ? TargetAlpha : _initialColor.a, Duration, ignoreTimeScale);
					break;
				case Modes.Color:
					TargetGraphic.CrossFadeColor(NormalPlayDirection ? TargetColor : _initialColor, Duration, ignoreTimeScale, UseAlpha);
					break;
			}
		}

		/// <summary>
		/// Turns the Graphic off on stop
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			IsPlaying = false;
			base.CustomStopFeedback(position, feedbacksIntensity);
			if (Active && DisableOnStop)
			{
				Turn(false);    
			}
		}

		/// <summary>
		/// Turns the Graphic on or off
		/// </summary>
		/// <param name="status"></param>
		protected virtual void Turn(bool status)
		{
			TargetGraphic.gameObject.SetActive(status);
			TargetGraphic.enabled = status;
		}
		
		/// <summary>
		/// On restore, we restore our initial state
		/// </summary>
		protected override void CustomRestoreInitialValues()
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			TargetGraphic.color = _initialColor;
		}
	}
}