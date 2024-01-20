using MoreMountains.Tools;
using UnityEngine;
using System.Collections;
#if MM_TEXTMESHPRO
using TMPro;
#endif

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback lets you control the alpha of a target TMP over time
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback lets you control the alpha of a target TMP over time.")]
	[FeedbackPath("TextMesh Pro/TMP Alpha")]
	public class MMFeedbackTMPAlpha : MMFeedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		public enum AlphaModes { Instant, Interpolate, ToDestination }

		/// the duration of this feedback is the duration of the color transition, or 0 if instant
		public override float FeedbackDuration { get { return (AlphaMode == AlphaModes.Instant) ? 0f : ApplyTimeMultiplier(Duration); } set { Duration = value; } }

		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.TMPColor; } }
		#endif

		#if MM_TEXTMESHPRO
		[Header("Target")]
		/// the TMP_Text component to control
		[Tooltip(" TMP_Text component to control")]
		public TMP_Text TargetTMPText;
		#endif

		[Header("Alpha")]
		/// the selected color mode :
		/// None : nothing will happen,
		/// gradient : evaluates the color over time on that gradient, from left to right,
		/// interpolate : lerps from the current color to the destination one 
		[Tooltip("the selected color mode :" +
		         "Instant : the alpha will change instantly to the target one," +
		         "Curve : the alpha will be interpolated along the curve," +
		         "interpolate : lerps from the current color to the destination one ")]
		public AlphaModes AlphaMode = AlphaModes.Interpolate;
		/// how long the color of the text should change over time
		[Tooltip("how long the color of the text should change over time")]
		[MMFEnumCondition("AlphaMode", (int)AlphaModes.Interpolate, (int)AlphaModes.ToDestination)]
		public float Duration = 0.2f;
		/// the alpha to apply when in instant mode
		[Tooltip("the alpha to apply when in instant mode")]
		[MMFEnumCondition("AlphaMode", (int)AlphaModes.Instant)]
		public float InstantAlpha = 1f;

		/// the curve to use when interpolating towards the destination alpha
		[Tooltip("the curve to use when interpolating towards the destination alpha")]
		[MMFEnumCondition("AlphaMode", (int)AlphaModes.Interpolate, (int)AlphaModes.ToDestination)]
		public MMTweenType Curve = new MMTweenType(MMTween.MMTweenCurve.EaseInCubic);
		/// the value to which the curve's 0 should be remapped
		[Tooltip("the value to which the curve's 0 should be remapped")]
		[MMFEnumCondition("AlphaMode", (int)AlphaModes.Interpolate)]
		public float CurveRemapZero = 0f;
		/// the value to which the curve's 1 should be remapped
		[Tooltip("the value to which the curve's 1 should be remapped")]
		[MMFEnumCondition("AlphaMode", (int)AlphaModes.Interpolate)]
		public float CurveRemapOne = 1f;
		/// the alpha to aim towards when in ToDestination mode
		[Tooltip("the alpha to aim towards when in ToDestination mode")]
		[MMFEnumCondition("AlphaMode", (int)AlphaModes.ToDestination)]
		public float DestinationAlpha = 1f;

		/// if this is true, calling that feedback will trigger it, even if it's in progress. If it's false, it'll prevent any new Play until the current one is over
		[Tooltip("if this is true, calling that feedback will trigger it, even if it's in progress. If it's false, it'll prevent any new Play until the current one is over")] 
		public bool AllowAdditivePlays = false;

		protected float _initialAlpha;
		protected Coroutine _coroutine;

		/// <summary>
		/// On init we store our initial alpha
		/// </summary>
		/// <param name="owner"></param>
		protected override void CustomInitialization(GameObject owner)
		{
			base.CustomInitialization(owner);

			#if MM_TEXTMESHPRO
			if (TargetTMPText == null)
			{
				return;
			}
			_initialAlpha = TargetTMPText.alpha;
			#endif
		}

		/// <summary>
		/// On Play we change our text's alpha
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			#if MM_TEXTMESHPRO
			if (TargetTMPText == null)
			{
				return;
			}

			switch (AlphaMode)
			{
				case AlphaModes.Instant:
					TargetTMPText.alpha = InstantAlpha;
					break;
				case AlphaModes.Interpolate:
					if (!AllowAdditivePlays && (_coroutine != null))
					{
						return;
					}
					_coroutine = StartCoroutine(ChangeAlpha());
					break;
				case AlphaModes.ToDestination:
					if (!AllowAdditivePlays && (_coroutine != null))
					{
						return;
					}
					_initialAlpha = TargetTMPText.alpha;
					_coroutine = StartCoroutine(ChangeAlpha());
					break;
			}
			#endif
		}

		/// <summary>
		/// Changes the color of the text over time
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator ChangeAlpha()
		{
			float journey = NormalPlayDirection ? 0f : FeedbackDuration;
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
			yield break;
		}

		/// <summary>
		/// Stops the animation if needed
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
			IsPlaying = false;
			if (_coroutine != null)
			{
				StopCoroutine(_coroutine);
				_coroutine = null;
			}
		}

		/// <summary>
		/// Applies the alpha change
		/// </summary>
		/// <param name="time"></param>
		protected virtual void SetAlpha(float time)
		{
			#if MM_TEXTMESHPRO
			float newAlpha = 0f;
			if (AlphaMode == AlphaModes.Interpolate)
			{
				newAlpha = MMTween.Tween(time, 0f, 1f, CurveRemapZero, CurveRemapOne, Curve);    
			}
			else if (AlphaMode == AlphaModes.ToDestination)
			{
				newAlpha = MMTween.Tween(time, 0f, 1f, _initialAlpha, DestinationAlpha, Curve);
			}
			TargetTMPText.alpha = newAlpha;
			#endif
		}
	}
}