using MoreMountains.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
	public class MMFeedbackBaseTarget
	{
		/// the receiver to write the level to
		public MMPropertyReceiver Target;
		/// the curve to tween the intensity on
		public MMTweenType LevelCurve = new MMTweenType(new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0)));
		/// the value to remap the intensity curve's 0 to
		public float RemapLevelZero = 0f;
		/// the value to remap the intensity curve's 1 to
		public float RemapLevelOne = 1f;
		/// the value to move the intensity to in instant mode
		public float InstantLevel;
		/// the initial value for this level
		public float InitialLevel;
	}

	public abstract class MMFeedbackBase : MMFeedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		
		/// the possible modes for this feedback
		public enum Modes { OverTime, Instant } 
        
		[Header("Mode")]
		/// whether the feedback should affect the target property instantly or over a period of time
		[Tooltip("whether the feedback should affect the target property instantly or over a period of time")]
		public Modes Mode = Modes.OverTime;
		/// how long the target property should change over time
		[Tooltip("how long the target property should change over time")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime)]
		public float Duration = 0.2f;
		/// whether or not that target property should be turned off on start
		[Tooltip("whether or not that target property should be turned off on start")]
		public bool StartsOff = false;
		/// whether or not the values should be relative or not
		[Tooltip("whether or not the values should be relative or not")]
		public bool RelativeValues = true;
		/// if this is true, calling that feedback will trigger it, even if it's in progress. If it's false, it'll prevent any new Play until the current one is over
		[Tooltip("if this is true, calling that feedback will trigger it, even if it's in progress. If it's false, it'll prevent any new Play until the current one is over")] 
		public bool AllowAdditivePlays = false;

		/// if this is true, the target object will be disabled on stop
		[Tooltip("if this is true, the target object will be disabled on stop")]
		public bool DisableOnStop = false;
        
		/// the duration of this feedback is the duration of the target property, or 0 if instant
		public override float FeedbackDuration { get { return (Mode == Modes.Instant) ? 0f : ApplyTimeMultiplier(Duration); } set { if (Mode != Modes.Instant) { Duration = value; } } }

		protected List<MMFeedbackBaseTarget> _targets;
		protected Coroutine _coroutine = null;

		/// <summary>
		/// On init we turn the target property off if needed
		/// </summary>
		/// <param name="owner"></param>
		protected override void CustomInitialization(GameObject owner)
		{
			base.CustomInitialization(owner);

			PrepareTargets();

			if (Active)
			{
				if (StartsOff)
				{
					Turn(false);
				}
			}
		}

		/// <summary>
		/// Creates a new list, fills the targets, and initializes them
		/// </summary>
		protected virtual void PrepareTargets()
		{
			_targets = new List<MMFeedbackBaseTarget>();
			FillTargets();
			InitializeTargets();
		}

		/// <summary>
		/// On validate (if a value has changed in the inspector), we reinitialize what needs to be
		/// </summary>
		protected virtual void OnValidate()
		{
			PrepareTargets();
		}

		/// <summary>
		/// Fills our list of targets, meant to be extended
		/// </summary>
		protected abstract void FillTargets();

		/// <summary>
		/// Initializes each target in the list
		/// </summary>
		protected virtual void InitializeTargets()
		{
			if (_targets.Count == 0)
			{
				return;
			}

			foreach(MMFeedbackBaseTarget target in _targets)
			{
				target.Target.Initialization(this.gameObject);
				target.InitialLevel = target.Target.Level;
			}
		}

		/// <summary>
		/// On Play we turn our target property on and start an over time coroutine if needed
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (Active && FeedbackTypeAuthorized)
			{
				Turn(true);
				switch (Mode)
				{
					case Modes.Instant:
						Instant();
						break;
					case Modes.OverTime:
						if (!AllowAdditivePlays && (_coroutine != null))
						{
							return;
						}
						_coroutine = StartCoroutine(UpdateValueSequence(feedbacksIntensity));
						break;
				}
			}
		}

		/// <summary>
		/// Plays an instant feedback
		/// </summary>
		protected virtual void Instant()
		{
			if (_targets.Count == 0)
			{
				return;
			}

			foreach (MMFeedbackBaseTarget target in _targets)
			{
				target.Target.SetLevel(target.InstantLevel);
			}
		}

		/// <summary>
		/// This coroutine will modify the values on the target property
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator UpdateValueSequence(float feedbacksIntensity)
		{
			float journey = NormalPlayDirection ? 0f : FeedbackDuration;
			IsPlaying = true;
			while ((journey >= 0) && (journey <= FeedbackDuration) && (FeedbackDuration > 0))
			{
				float remappedTime = MMFeedbacksHelpers.Remap(journey, 0f, FeedbackDuration, 0f, 1f);
				SetValues(remappedTime, feedbacksIntensity);

				journey += NormalPlayDirection ? FeedbackDeltaTime : -FeedbackDeltaTime;
				yield return null;
			}
			SetValues(FinalNormalizedTime, feedbacksIntensity);
			if (StartsOff)
			{
				Turn(false);
			}
			IsPlaying = false;
			_coroutine = null;
			yield return null;
		}
        
        

		/// <summary>
		/// Sets the various values on the target property on a specified time (between 0 and 1)
		/// </summary>
		/// <param name="time"></param>
		protected virtual void SetValues(float time, float feedbacksIntensity)
		{
			if (_targets.Count == 0)
			{
				return;
			}
            
			float intensityMultiplier = Timing.ConstantIntensity ? 1f : feedbacksIntensity;
            
			foreach (MMFeedbackBaseTarget target in _targets)
			{
				float intensity = MMTween.Tween(time, 0f, 1f, target.RemapLevelZero, target.RemapLevelOne, target.LevelCurve);
				if (RelativeValues)
				{
					intensity += target.InitialLevel;
				}
				target.Target.SetLevel(intensity * intensityMultiplier);
			}
		}

		/// <summary>
		/// Turns the target property object off on stop if needed
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
		{
			base.CustomStopFeedback(position, feedbacksIntensity);
			if (Active)
			{
				if (_coroutine != null)
				{
					StopCoroutine(_coroutine);
					_coroutine = null;
				}
				IsPlaying = false;
				if (DisableOnStop)
				{
					Turn(false);    
				}
			}
		}

		/// <summary>
		/// Turns the target object on or off
		/// </summary>
		/// <param name="status"></param>
		protected virtual void Turn(bool status)
		{
			if (_targets.Count == 0)
			{
				return;
			}
			foreach (MMFeedbackBaseTarget target in _targets)
			{
				if (target.Target.TargetComponent.gameObject != null)
				{
					target.Target.TargetComponent.gameObject.SetActive(status);
				}
			}
		}
	}
}