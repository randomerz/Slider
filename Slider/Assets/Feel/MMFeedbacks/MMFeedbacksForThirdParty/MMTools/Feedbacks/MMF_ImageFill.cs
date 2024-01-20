using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.UI;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback will let you change the fill value of a target Image over time.
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback will let you modify the fill value of a target Image over time.")]
	[FeedbackPath("UI/Image Fill")]
	public class MMF_ImageFill : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.UIColor; } }
		public override bool EvaluateRequiresSetup() { return (BoundImage == null); }
		public override string RequiredTargetText { get { return BoundImage != null ? BoundImage.name : "";  } }
		public override string RequiresSetupText { get { return "This feedback requires that a BoundImage be set to be able to work properly. You can set one below."; } }
		#endif
		public override bool HasCustomInspectors => true;
		public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => BoundImage = FindAutomatedTarget<Image>();

		/// the possible modes for this feedback
		public enum Modes { OverTime, Instant, ToDestination }

		[MMFInspectorGroup("Target Image", true, 12, true)]
        
		/// the Image to affect when playing the feedback
		[Tooltip("the Image to affect when playing the feedback")]
		public Image BoundImage;

		[MMFInspectorGroup("Image Fill Animation", true, 24)]
		/// whether the feedback should affect the Image instantly or over a period of time
		[Tooltip("whether the feedback should affect the Image instantly or over a period of time")]
		public Modes Mode = Modes.OverTime;
		/// how long the Image should change over time
		[Tooltip("how long the Image should change over time")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime, (int)Modes.ToDestination)]
		public float Duration = 0.2f;
		/// if this is true, calling that feedback will trigger it, even if it's in progress. If it's false, it'll prevent any new Play until the current one is over
		[Tooltip("if this is true, calling that feedback will trigger it, even if it's in progress. If it's false, it'll prevent any new Play until the current one is over")] 
		public bool AllowAdditivePlays = false;
		/// the fill to move to in instant mode
		[Tooltip("the fill to move to in instant mode")]
		[MMFEnumCondition("Mode", (int)Modes.Instant)]
		public float InstantFill = 1f;
		/// the curve to use when interpolating towards the destination fill
		[Tooltip("the curve to use when interpolating towards the destination fill")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime, (int)Modes.ToDestination)]
		public MMTweenType Curve = new MMTweenType(MMTween.MMTweenCurve.EaseInCubic);
		/// the value to which the curve's 0 should be remapped
		[Tooltip("the value to which the curve's 0 should be remapped")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime)]
		public float CurveRemapZero = 0f;
		/// the value to which the curve's 1 should be remapped
		[Tooltip("the value to which the curve's 1 should be remapped")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime)]
		public float CurveRemapOne = 1f;
		/// the fill to aim towards when in ToDestination mode
		[Tooltip("the fill to aim towards when in ToDestination mode")]
		[MMFEnumCondition("Mode", (int)Modes.ToDestination)]
		public float DestinationFill = 1f;
		/// if this is true, the target will be disabled when this feedbacks is stopped
		[Tooltip("if this is true, the target will be disabled when this feedbacks is stopped")] 
		public bool DisableOnStop = false;

		/// the duration of this feedback is the duration of the Image, or 0 if instant
		public override float FeedbackDuration { get { return (Mode == Modes.Instant) ? 0f : ApplyTimeMultiplier(Duration); } set { Duration = value; } }

		protected Coroutine _coroutine;
		protected float _initialFill;
		protected bool _initialState;

		/// <summary>
		/// On Play we turn our Image on and start an over time coroutine if needed
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			_initialState = BoundImage.gameObject.activeInHierarchy;
			Turn(true);
			_initialFill = BoundImage.fillAmount;
			switch (Mode)
			{
				case Modes.Instant:
					BoundImage.fillAmount = InstantFill;
					break;
				case Modes.OverTime:
					if (!AllowAdditivePlays && (_coroutine != null))
					{
						return;
					}

					_coroutine = Owner.StartCoroutine(ImageSequence());
					break;
				case Modes.ToDestination:
					if (!AllowAdditivePlays && (_coroutine != null))
					{
						return;
					}

					_coroutine = Owner.StartCoroutine(ImageSequence());
					break;
			}
		}

		/// <summary>
		/// This coroutine will modify the values on the Image
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator ImageSequence()
		{
			float journey = NormalPlayDirection ? 0f : FeedbackDuration;
			_initialFill = BoundImage.fillAmount;
			IsPlaying = true;
			while ((journey >= 0) && (journey <= FeedbackDuration) && (FeedbackDuration > 0))
			{
				float remappedTime = MMFeedbacksHelpers.Remap(journey, 0f, FeedbackDuration, 0f, 1f);

				SetFill(remappedTime);

				journey += NormalPlayDirection ? FeedbackDeltaTime : -FeedbackDeltaTime;
				yield return null;
			}
			SetFill(FinalNormalizedTime);
			_coroutine = null;
			IsPlaying = false;
			yield return null;
		}

		/// <summary>
		/// Sets the various values on the sprite renderer on a specified time (between 0 and 1)
		/// </summary>
		/// <param name="time"></param>
		protected virtual void SetFill(float time)
		{
			float newFill = 0f;
			if (Mode == Modes.OverTime)
			{
				newFill = MMTween.Tween(time, 0f, 1f, CurveRemapZero, CurveRemapOne, Curve);    
			}
			else if (Mode == Modes.ToDestination)
			{
				newFill = MMTween.Tween(time, 0f, 1f, _initialFill, DestinationFill, Curve);
			}
            
			BoundImage.fillAmount = newFill;
		}

		/// <summary>
		/// Turns the sprite renderer off on stop
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
			_coroutine = null;
		}

		/// <summary>
		/// Turns the sprite renderer on or off
		/// </summary>
		/// <param name="status"></param>
		protected virtual void Turn(bool status)
		{
			BoundImage.gameObject.SetActive(status);
			BoundImage.enabled = status;
		}

		/// <summary>
		/// On restore, we put our object back at its initial position
		/// </summary>
		protected override void CustomRestoreInitialValues()
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			
			Turn(_initialState);
			BoundImage.fillAmount = _initialFill;
		}
	}
}