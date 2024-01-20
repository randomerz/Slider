using MoreMountains.Tools;
using UnityEngine;
using System.Collections;
#if MM_TEXTMESHPRO
using TMPro;
#endif

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback lets you dilate a TMP text over time
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback lets you dilate a TMP text over time.")]
	[FeedbackPath("TextMesh Pro/TMP Dilate")]
	public class MMFeedbackTMPDilate : MMFeedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		
		/// the duration of this feedback is the duration of the transition, or 0 if instant
		public override float FeedbackDuration { get { return (Mode == MMFeedbackBase.Modes.Instant) ? 0f : ApplyTimeMultiplier(Duration); } set { Duration = value; } }

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

		[Header("Dilate")]
		/// whether or not values should be relative
		[Tooltip("whether or not values should be relative")]
		public bool RelativeValues = true;
		/// the selected mode
		[Tooltip("the selected mode")]
		public MMFeedbackBase.Modes Mode = MMFeedbackBase.Modes.OverTime;
		/// the duration of the feedback, in seconds
		[Tooltip("the duration of the feedback, in seconds")]
		[MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.OverTime)]
		public float Duration = 0.5f;
		/// the curve to tween on
		[Tooltip("the curve to tween on")]
		[MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.OverTime)]
		public MMTweenType DilateCurve = new MMTweenType(new AnimationCurve(new Keyframe(0, 0.5f), new Keyframe(0.3f, 1f), new Keyframe(1, 0.5f)));
		/// the value to remap the curve's 0 to
		[Tooltip("the value to remap the curve's 0 to")]
		[MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.OverTime)]
		public float RemapZero = -1f;
		/// the value to remap the curve's 1 to
		[Tooltip("the value to remap the curve's 1 to")]
		[MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.OverTime)]
		public float RemapOne = 1f;
		/// the value to move to in instant mode
		[Tooltip("the value to move to in instant mode")]
		[MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.Instant)]
		public float InstantDilate;
		/// if this is true, calling that feedback will trigger it, even if it's in progress. If it's false, it'll prevent any new Play until the current one is over
		[Tooltip("if this is true, calling that feedback will trigger it, even if it's in progress. If it's false, it'll prevent any new Play until the current one is over")] 
		public bool AllowAdditivePlays = false;

		protected float _initialDilate;
		protected Coroutine _coroutine;

		/// <summary>
		/// On init we grab our initial dilate value
		/// </summary>
		/// <param name="owner"></param>
		protected override void CustomInitialization(GameObject owner)
		{
			base.CustomInitialization(owner);

			if (!Active)
			{
				return;
			}
			#if MM_TEXTMESHPRO
			_initialDilate = TargetTMPText.fontMaterial.GetFloat(ShaderUtilities.ID_FaceDilate);
			#endif
		}

		/// <summary>
		/// On Play we turn animate our transition
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			#if MM_TEXTMESHPRO
			if (TargetTMPText == null)
			{
				return;
			}

			if (Active && FeedbackTypeAuthorized)
			{
				switch (Mode)
				{
					case MMFeedbackBase.Modes.Instant:
						TargetTMPText.fontMaterial.SetFloat(ShaderUtilities.ID_FaceDilate, InstantDilate);
						TargetTMPText.UpdateMeshPadding();
						break;
					case MMFeedbackBase.Modes.OverTime:
						if (!AllowAdditivePlays && (_coroutine != null))
						{
							return;
						}
						_coroutine = StartCoroutine(ApplyValueOverTime());
						break;
				}
			}
			#endif
		}

		/// <summary>
		/// Applies our dilate value over time
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator ApplyValueOverTime()
		{
			float journey = NormalPlayDirection ? 0f : FeedbackDuration;
			IsPlaying = true;
			while ((journey >= 0) && (journey <= FeedbackDuration) && (FeedbackDuration > 0))
			{
				float remappedTime = MMFeedbacksHelpers.Remap(journey, 0f, FeedbackDuration, 0f, 1f);

				SetValue(remappedTime);

				journey += NormalPlayDirection ? FeedbackDeltaTime : -FeedbackDeltaTime;
				yield return null;
			}
			SetValue(FinalNormalizedTime);
			_coroutine = null;
			IsPlaying = false;
			yield return null;
		}

		/// <summary>
		/// Sets the Dilate value
		/// </summary>
		/// <param name="time"></param>
		protected virtual void SetValue(float time)
		{
			#if MM_TEXTMESHPRO
			float intensity = MMTween.Tween(time, 0f, 1f, RemapZero, RemapOne, DilateCurve);
			float newValue = intensity;
			if (RelativeValues)
			{
				newValue += _initialDilate;
			}
			TargetTMPText.fontMaterial.SetFloat(ShaderUtilities.ID_FaceDilate, newValue);
			TargetTMPText.UpdateMeshPadding();
			#endif
		}
	}
}