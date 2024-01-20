using UnityEngine;
using System.Collections;
#if MM_TEXTMESHPRO
using TMPro;
#endif

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback lets you control the color of a target TMP's outline over time
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback lets you control the color of a target TMP's outline over time.")]
	[FeedbackPath("TextMesh Pro/TMP Outline Color")]
	public class MMFeedbackTMPOutlineColor : MMFeedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		public enum ColorModes { Instant, Gradient, Interpolate }

		/// the duration of this feedback is the duration of the color transition, or 0 if instant
		public override float FeedbackDuration { get { return (ColorMode == ColorModes.Instant) ? 0f : ApplyTimeMultiplier(Duration); } set { Duration = value; } }

		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.TMPColor; } }
		#endif

        
		#if MM_TEXTMESHPRO
		[Header("Target")]
		/// the TMP_Text component to control
		[Tooltip("the TMP_Text component to control")]
		public TMP_Text TargetTMPText;
		#endif

		[Header("Outline Color")]
		/// the selected color mode :
		/// None : nothing will happen,
		/// gradient : evaluates the color over time on that gradient, from left to right,
		/// interpolate : lerps from the current color to the destination one 
		[Tooltip("the selected color mode :" +
		         "None : nothing will happen," +
		         "gradient : evaluates the color over time on that gradient, from left to right," +
		         "interpolate : lerps from the current color to the destination one ")]
		public ColorModes ColorMode = ColorModes.Interpolate;
		/// how long the color of the text should change over time
		[Tooltip("how long the color of the text should change over time")]
		[MMFEnumCondition("ColorMode", (int)ColorModes.Interpolate, (int)ColorModes.Gradient)]
		public float Duration = 0.2f;
		/// the color to apply
		[Tooltip("the color to apply")]
		[MMFEnumCondition("ColorMode", (int)ColorModes.Instant)]
		public Color32 InstantColor = Color.yellow;
		/// the gradient to use to animate the color over time
		[Tooltip("the gradient to use to animate the color over time")]
		[MMFEnumCondition("ColorMode", (int)ColorModes.Gradient)]
		[GradientUsage(true)]
		public Gradient ColorGradient;
		/// the destination color when in interpolate mode
		[Tooltip("the destination color when in interpolate mode")]
		[MMFEnumCondition("ColorMode", (int)ColorModes.Interpolate)]
		public Color32 DestinationColor = Color.yellow;
		/// the curve to use when interpolating towards the destination color
		[Tooltip("the curve to use when interpolating towards the destination color")]
		[MMFEnumCondition("ColorMode", (int)ColorModes.Interpolate)]
		public AnimationCurve ColorCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// if this is true, calling that feedback will trigger it, even if it's in progress. If it's false, it'll prevent any new Play until the current one is over
		[Tooltip("if this is true, calling that feedback will trigger it, even if it's in progress. If it's false, it'll prevent any new Play until the current one is over")] 
		public bool AllowAdditivePlays = false;

		protected Color _initialColor;
		protected Coroutine _coroutine;

		/// <summary>
		/// On init we store our initial outline color
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
			_initialColor = TargetTMPText.outlineColor;
			#endif
		}

		/// <summary>
		/// On Play we change our text's outline's color
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
			switch (ColorMode)
			{
				case ColorModes.Instant:
					TargetTMPText.outlineColor = InstantColor;
					break;
				case ColorModes.Gradient:
					if (!AllowAdditivePlays && (_coroutine != null))
					{
						return;
					}
					_coroutine = StartCoroutine(ChangeColor());
					break;
				case ColorModes.Interpolate:
					if (!AllowAdditivePlays && (_coroutine != null))
					{
						return;
					}
					_coroutine = StartCoroutine(ChangeColor());
					break;
			}
			#endif
		}

		/// <summary>
		/// Changes the color of the text's outline over time
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator ChangeColor()
		{
			IsPlaying = true;
			float journey = NormalPlayDirection ? 0f : FeedbackDuration;
			while ((journey >= 0) && (journey <= FeedbackDuration) && (FeedbackDuration > 0))
			{
				float remappedTime = MMFeedbacksHelpers.Remap(journey, 0f, FeedbackDuration, 0f, 1f);

				SetColor(remappedTime);

				journey += NormalPlayDirection ? FeedbackDeltaTime : -FeedbackDeltaTime;
				yield return null;
			}
			SetColor(FinalNormalizedTime);
			_coroutine = null;
			IsPlaying = false;
			yield break;
		}

		/// <summary>
		/// Applies the color change
		/// </summary>
		/// <param name="time"></param>
		protected virtual void SetColor(float time)
		{
			#if MM_TEXTMESHPRO
			if (ColorMode == ColorModes.Gradient)
			{
				// we set our object inactive then active, otherwise for some reason outline color isn't applied.
				TargetTMPText.gameObject.SetActive(false);
				TargetTMPText.outlineColor = ColorGradient.Evaluate(time);
				TargetTMPText.gameObject.SetActive(true);
			}
			else if (ColorMode == ColorModes.Interpolate)
			{
				float factor = ColorCurve.Evaluate(time);
				TargetTMPText.gameObject.SetActive(false);
				TargetTMPText.outlineColor = Color.LerpUnclamped(_initialColor, DestinationColor, factor);
				TargetTMPText.gameObject.SetActive(true);
			}
			#endif
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
	}
}