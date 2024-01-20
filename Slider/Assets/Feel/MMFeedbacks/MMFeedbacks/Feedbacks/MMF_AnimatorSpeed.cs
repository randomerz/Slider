using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback will let you change the speed of a target animator, either once, or instantly and then reset it, or interpolate it over time
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback will let you change the speed of a target animator, either once, or instantly and then reset it, or interpolate it over time")]
	[FeedbackPath("Animation/Animator Speed")]
	public class MMF_AnimatorSpeed : MMF_Feedback 
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
        
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.AnimationColor; } }
		public override bool EvaluateRequiresSetup() { return (BoundAnimator == null); }
		public override string RequiredTargetText { get { return BoundAnimator != null ? BoundAnimator.name : "";  } }
		public override string RequiresSetupText { get { return "This feedback requires that a BoundAnimator be set to be able to work properly. You can set one below."; } }
		#endif
		public override bool HasRandomness => true;
		public override bool CanForceInitialValue => true;
		public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => BoundAnimator = FindAutomatedTarget<Animator>();

		public enum SpeedModes { Once, InstantThenReset, OverTime }
		
		[MMFInspectorGroup("Animation", true, 12, true)]
		/// the animator whose parameters you want to update
		[Tooltip("the animator whose parameters you want to update")]
		public Animator BoundAnimator;

		[MMFInspectorGroup("Speed", true, 14, true)]
		/// whether to change the speed of the target animator once, instantly and reset it later, or have it change over time
		[Tooltip("whether to change the speed of the target animator once, instantly and reset it later, or have it change over time")]
		public SpeedModes Mode = SpeedModes.Once; 
		/// the new minimum speed at which to set the animator - value will be randomized between min and max
		[Tooltip("the new minimum speed at which to set the animator - value will be randomized between min and max")]
		public float NewSpeedMin = 0f; 
		/// the new maximum speed at which to set the animator - value will be randomized between min and max
		[Tooltip("the new maximum speed at which to set the animator - value will be randomized between min and max")]
		public float NewSpeedMax = 0f;
		/// when in instant then reset or over time modes, the duration of the effect
		[Tooltip("when in instant then reset or over time modes, the duration of the effect")]
		[MMFEnumCondition("Mode", (int)SpeedModes.InstantThenReset, (int)SpeedModes.OverTime)]
		public float Duration = 1f;
		/// when in over time mode, the curve against which to evaluate the new speed
		[Tooltip("when in over time mode, the curve against which to evaluate the new speed")]
		[MMFEnumCondition("Mode", (int)SpeedModes.OverTime)]
		public AnimationCurve Curve = new AnimationCurve(new Keyframe(0, 0f), new Keyframe(0.5f, 1f), new Keyframe(1, 0f));

		protected Coroutine _coroutine;
		protected float _initialSpeed;
		protected float _startedAt;
        
		/// <summary>
		/// On Play, checks if an animator is bound and triggers parameters
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			if (BoundAnimator == null)
			{
				Debug.LogWarning("No animator was set for " + Owner.name);
				return;
			}

			if (!IsPlaying)
			{
				_initialSpeed = BoundAnimator.speed;	
			}

			if (Mode == SpeedModes.Once)
			{
				BoundAnimator.speed = ComputeIntensity(DetermineNewSpeed(), position);
			}
			else
			{
				_coroutine = Owner.StartCoroutine(ChangeSpeedCo());
			}
		}

		/// <summary>
		/// A coroutine used in ForDuration mode
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator ChangeSpeedCo()
		{
			if (Mode == SpeedModes.InstantThenReset)
			{
				IsPlaying = true;
				BoundAnimator.speed = DetermineNewSpeed();
				yield return MMCoroutine.WaitFor(Duration);
				BoundAnimator.speed = _initialSpeed;	
				IsPlaying = false;
			}
			else if (Mode == SpeedModes.OverTime)
			{
				IsPlaying = true;
				_startedAt = FeedbackTime;
				float newTargetSpeed = DetermineNewSpeed();
				while (FeedbackTime - _startedAt < Duration)
				{
					float time = MMFeedbacksHelpers.Remap(FeedbackTime - _startedAt, 0f, Duration, 0f, 1f);
					float t = Curve.Evaluate(time);
					BoundAnimator.speed = Mathf.Max(0f, MMFeedbacksHelpers.Remap(t, 0f, 1f, _initialSpeed, newTargetSpeed));
					yield return null;
				}
				BoundAnimator.speed = _initialSpeed;	
				IsPlaying = false;
			}
		}

		/// <summary>
		/// Determines the new speed for the target animator
		/// </summary>
		/// <returns></returns>
		protected virtual float DetermineNewSpeed()
		{
			return Mathf.Abs(Random.Range(NewSpeedMin, NewSpeedMax));
		}
        
		/// <summary>
		/// On stop, turns the bool parameter to false
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			if (_coroutine != null)
			{
				Owner.StopCoroutine(_coroutine);	
			}

			BoundAnimator.speed = _initialSpeed;
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

			BoundAnimator.speed = _initialSpeed;
		}
	}
}