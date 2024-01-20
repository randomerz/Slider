using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.UI;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback will let you change the alpha of a target sprite renderer over time.
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback will let you change the alpha of a target Image over time.")]
	[FeedbackPath("UI/Image Alpha")]
	public class MMFeedbackImageAlpha : MMFeedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.UIColor; } }
		#endif

		/// the possible modes for this feedback
		public enum Modes { OverTime, Instant, ToDestination }

		[Header("Sprite Renderer")]
        
		/// the Image to affect when playing the feedback
		[Tooltip("the Image to affect when playing the feedback")]
		public Image BoundImage;
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
        
		[Header("Alpha")]
		/// the alpha to move to in instant mode
		[Tooltip("the alpha to move to in instant mode")]
		[MMFEnumCondition("Mode", (int)Modes.Instant)]
		public float InstantAlpha = 1f;
        
		/// the curve to use when interpolating towards the destination alpha
		[Tooltip("the curve to use when interpolating towards the destination alpha")]
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
		/// the alpha to aim towards when in ToDestination mode
		[Tooltip("the alpha to aim towards when in ToDestination mode")]
		[MMFEnumCondition("Mode", (int)Modes.ToDestination)]
		public float DestinationAlpha = 1f;

		/// the duration of this feedback is the duration of the Image, or 0 if instant
		public override float FeedbackDuration { get { return (Mode == Modes.Instant) ? 0f : ApplyTimeMultiplier(Duration); } set { Duration = value; } }

		protected Coroutine _coroutine;
		protected Color _imageColor;
		protected float _initialAlpha;

		/// <summary>
		/// On init we turn the Image off if needed
		/// </summary>
		/// <param name="owner"></param>
		protected override void CustomInitialization(GameObject owner)
		{
			base.CustomInitialization(owner);
		}

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
            
			Turn(true);
			switch (Mode)
			{
				case Modes.Instant:
					_imageColor = BoundImage.color;
					_imageColor.a = InstantAlpha;
					BoundImage.color = _imageColor;
					break;
				case Modes.OverTime:
					if (!AllowAdditivePlays && (_coroutine != null))
					{
						return;
					}

					_coroutine = StartCoroutine(ImageSequence());
					break;
				case Modes.ToDestination:
					if (!AllowAdditivePlays && (_coroutine != null))
					{
						return;
					}

					_coroutine = StartCoroutine(ImageSequence());
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
			_imageColor = BoundImage.color;
			_initialAlpha = BoundImage.color.a;
			IsPlaying = true;
			while ((journey >= 0) && (journey <= FeedbackDuration) && (FeedbackDuration > 0))
			{
				float remappedTime = MMFeedbacksHelpers.Remap(journey, 0f, FeedbackDuration, 0f, 1f);

				SetAlpha(remappedTime);

				journey += NormalPlayDirection ? FeedbackDeltaTime : -FeedbackDeltaTime;
				yield return null;
			}
			SetAlpha(FinalNormalizedTime);
			_coroutine = null;
			IsPlaying = false;
			yield return null;
		}

		/// <summary>
		/// Sets the various values on the sprite renderer on a specified time (between 0 and 1)
		/// </summary>
		/// <param name="time"></param>
		protected virtual void SetAlpha(float time)
		{
			float newAlpha = 0f;
			if (Mode == Modes.OverTime)
			{
				newAlpha = MMTween.Tween(time, 0f, 1f, CurveRemapZero, CurveRemapOne, Curve);    
			}
			else if (Mode == Modes.ToDestination)
			{
				newAlpha = MMTween.Tween(time, 0f, 1f, _initialAlpha, DestinationAlpha, Curve);
			}

			_imageColor.a = newAlpha;
            
			BoundImage.color = _imageColor;
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
			Turn(false);
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
	}
}