using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback will trigger a one time play on a target FloatController
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback lets you trigger a one time play on a target FloatController.")]
	[FeedbackPath("GameObject/FloatController")]
	public class MMFeedbackFloatController : MMFeedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// the different possible modes 
		public enum Modes { OneTime, ToDestination }
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.GameObjectColor; } }
		#endif

		[Header("Float Controller")]
		/// the mode this controller is in
		[Tooltip("the mode this controller is in")]
		public Modes Mode = Modes.OneTime;
		/// the float controller to trigger a one time play on
		[Tooltip("the float controller to trigger a one time play on")]
		public FloatController TargetFloatController;
		/// whether this should revert to original at the end
		[Tooltip("whether this should revert to original at the end")]
		public bool RevertToInitialValueAfterEnd = false;
		/// the duration of the One Time shake
		[Tooltip("the duration of the One Time shake")]
		[MMFEnumCondition("Mode", (int)Modes.OneTime)]
		public float OneTimeDuration = 1f;
		/// the amplitude of the One Time shake (this will be multiplied by the curve's height)
		[Tooltip("the amplitude of the One Time shake (this will be multiplied by the curve's height)")]
		[MMFEnumCondition("Mode", (int)Modes.OneTime)]
		public float OneTimeAmplitude = 1f;
		/// the low value to remap the normalized curve value to 
		[Tooltip("the low value to remap the normalized curve value to")]
		[MMFEnumCondition("Mode", (int)Modes.OneTime)]
		public float OneTimeRemapMin = 0f;
		/// the high value to remap the normalized curve value to 
		[Tooltip("the high value to remap the normalized curve value to")]
		[MMFEnumCondition("Mode", (int)Modes.OneTime)]
		public float OneTimeRemapMax = 1f;
		/// the curve to apply to the one time shake
		[Tooltip("the curve to apply to the one time shake")]
		[MMFEnumCondition("Mode", (int)Modes.OneTime)]
		public AnimationCurve OneTimeCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to move this float controller to
		[Tooltip("the value to move this float controller to")]
		[MMFEnumCondition("Mode", (int)Modes.ToDestination)]
		public float ToDestinationValue = 1f;
		/// the duration over which to move the value
		[Tooltip("the duration over which to move the value")]
		[MMFEnumCondition("Mode", (int)Modes.ToDestination)]
		public float ToDestinationDuration = 1f;
		/// the curve over which to move the value in ToDestination mode
		[Tooltip("the curve over which to move the value in ToDestination mode")]
		[MMFEnumCondition("Mode", (int)Modes.ToDestination)]
		public AnimationCurve ToDestinationCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));

		/// the duration of this feedback is the duration of the one time hit
		public override float FeedbackDuration
		{
			get { return (Mode == Modes.OneTime) ? ApplyTimeMultiplier(OneTimeDuration) : ApplyTimeMultiplier(ToDestinationDuration); } 
			set { OneTimeDuration = value; ToDestinationDuration = value; }
		}

		protected float _oneTimeDurationStorage;
		protected float _oneTimeAmplitudeStorage;
		protected float _oneTimeRemapMinStorage;
		protected float _oneTimeRemapMaxStorage;
		protected AnimationCurve _oneTimeCurveStorage;
		protected float _toDestinationValueStorage;
		protected float _toDestinationDurationStorage;
		protected AnimationCurve _toDestinationCurveStorage;
		protected bool _revertToInitialValueAfterEndStorage;

		/// <summary>
		/// On init we grab our initial values on the target float controller
		/// </summary>
		/// <param name="owner"></param>
		protected override void CustomInitialization(GameObject owner)
		{
			if (Active && (TargetFloatController != null))
			{
				_oneTimeDurationStorage = TargetFloatController.OneTimeDuration;
				_oneTimeAmplitudeStorage = TargetFloatController.OneTimeAmplitude;
				_oneTimeCurveStorage = TargetFloatController.OneTimeCurve;
				_oneTimeRemapMinStorage = TargetFloatController.OneTimeRemapMin;
				_oneTimeRemapMaxStorage = TargetFloatController.OneTimeRemapMax;
				_toDestinationCurveStorage = TargetFloatController.ToDestinationCurve;
				_toDestinationDurationStorage = TargetFloatController.ToDestinationDuration;
				_toDestinationValueStorage = TargetFloatController.ToDestinationValue;
				_revertToInitialValueAfterEndStorage = TargetFloatController.RevertToInitialValueAfterEnd;
			}
		}

		/// <summary>
		/// On play we trigger a one time or ToDestination play on our target float controller
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized || (TargetFloatController == null))
			{
				return;
			}
            
			float intensityMultiplier = Timing.ConstantIntensity ? 1f : feedbacksIntensity;
			TargetFloatController.RevertToInitialValueAfterEnd = RevertToInitialValueAfterEnd;

			if (Mode == Modes.OneTime)
			{
				TargetFloatController.OneTimeDuration = FeedbackDuration;
				TargetFloatController.OneTimeAmplitude = OneTimeAmplitude;
				TargetFloatController.OneTimeCurve = OneTimeCurve;
				if (NormalPlayDirection)
				{
					TargetFloatController.OneTimeRemapMin = OneTimeRemapMin * intensityMultiplier;
					TargetFloatController.OneTimeRemapMax = OneTimeRemapMax * intensityMultiplier;
				}
				else
				{
					TargetFloatController.OneTimeRemapMin = OneTimeRemapMax * intensityMultiplier;
					TargetFloatController.OneTimeRemapMax = OneTimeRemapMin * intensityMultiplier;   
				}
				TargetFloatController.OneTime();
			}
			if (Mode == Modes.ToDestination)
			{
				TargetFloatController.ToDestinationCurve = ToDestinationCurve;
				TargetFloatController.ToDestinationDuration = FeedbackDuration;
				TargetFloatController.ToDestinationValue = ToDestinationValue;
				TargetFloatController.ToDestination();
			}
		}

		/// <summary>
		/// On reset we reset our values on the target controller with the ones stored initially
		/// </summary>
		protected override void CustomReset()
		{
			base.CustomReset();
			if (Active && FeedbackTypeAuthorized && (TargetFloatController != null))
			{
				TargetFloatController.OneTimeDuration = _oneTimeDurationStorage;
				TargetFloatController.OneTimeAmplitude = _oneTimeAmplitudeStorage;
				TargetFloatController.OneTimeCurve = _oneTimeCurveStorage;
				TargetFloatController.OneTimeRemapMin = _oneTimeRemapMinStorage;
				TargetFloatController.OneTimeRemapMax = _oneTimeRemapMaxStorage;
				TargetFloatController.ToDestinationCurve = _toDestinationCurveStorage;
				TargetFloatController.ToDestinationDuration = _toDestinationDurationStorage;
				TargetFloatController.ToDestinationValue = _toDestinationValueStorage;
				TargetFloatController.RevertToInitialValueAfterEnd = _revertToInitialValueAfterEndStorage;
			}
		}


		/// <summary>
		/// On stop, we interrupt movement if it was active
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
            
			if (TargetFloatController != null)
			{
				TargetFloatController.Stop();
			}
		}
	}
}