using MoreMountains.Tools;
using System.Collections;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback will let you target (almost) any property, on any object in your scene. 
	/// It also works on scriptable objects. Drag an object, select a property, and setup your feedback " +
	/// to update that property over time
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback will let you target (almost) any property, on any object in your scene. " +
	              "It also works on scriptable objects. Drag an object, select a property, and setup your feedback " +
	              "to update that property over time.")]
	[FeedbackPath("GameObject/Property")]
	public class MMFeedbackProperty : MMFeedback
	{
		/// the duration of this feedback is the duration of the target property, or 0 if instant
		public override float FeedbackDuration { get { return (Mode == Modes.Instant) ? 0f : ApplyTimeMultiplier(Duration); } set { if (Mode != Modes.Instant) { Duration = value; } } }
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.GameObjectColor; } }
		#endif
        
		/// the possible modes for this feedback
		public enum Modes { OverTime, Instant } 
        
		[Header("Target Property")]
		/// the receiver to write the level to
		[Tooltip("the receiver to write the level to")]
		public MMPropertyReceiver Target;

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

		[Header("Level")]
		/// the curve to tween the intensity on
		[Tooltip("the curve to tween the intensity on")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime)]
		public MMTweenType LevelCurve = new MMTweenType(new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0)));
		/// the value to remap the intensity curve's 0 to
		[Tooltip("the value to remap the intensity curve's 0 to")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime)]
		public float RemapLevelZero = 0f;
		/// the value to remap the intensity curve's 1 to
		[Tooltip("the value to remap the intensity curve's 1 to")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime)]
		public float RemapLevelOne = 1f;
		/// the value to move the intensity to in instant mode
		[Tooltip("the value to move the intensity to in instant mode")]
		[MMFEnumCondition("Mode", (int)Modes.Instant)]
		public float InstantLevel;

		protected float _initialIntensity;
		protected Coroutine _coroutine; 

		/// <summary>
		/// On init we turn the target property off if needed
		/// </summary>
		/// <param name="owner"></param>
		protected override void CustomInitialization(GameObject owner)
		{
			base.CustomInitialization(owner);

			Target.Initialization(this.gameObject);
			_initialIntensity = Target.Level; 
            
			if (Active)
			{
				if (StartsOff)
				{
					Turn(false);
				}
			}

		}

		/// <summary>
		/// On Play we turn our target property on and start an over time coroutine if needed
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (Active)
			{
				Turn(true);
                
				float intensityMultiplier = Timing.ConstantIntensity ? 1f : feedbacksIntensity;
                
				switch (Mode)
				{
					case Modes.Instant:
						Target.SetLevel(InstantLevel);
						break;
					case Modes.OverTime:
						if (!AllowAdditivePlays && (_coroutine != null))
						{
							return;
						}
						_coroutine = StartCoroutine(UpdateValueSequence(intensityMultiplier));
						break;
				}
			}
		}

		/// <summary>
		/// This coroutine will modify the values on the target property
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator UpdateValueSequence(float intensityMultiplier)
		{
			float journey = NormalPlayDirection ? 0f : FeedbackDuration;

			while ((journey >= 0) && (journey <= FeedbackDuration) && (FeedbackDuration > 0))
			{
				float remappedTime = MMFeedbacksHelpers.Remap(journey, 0f, FeedbackDuration, 0f, 1f);

				SetValues(remappedTime, intensityMultiplier);

				journey += NormalPlayDirection ? FeedbackDeltaTime : -FeedbackDeltaTime;
				yield return null;
			}
			SetValues(FinalNormalizedTime, intensityMultiplier);
			if (StartsOff)
			{
				Turn(false);
			}

			_coroutine = null;
			yield return null;
		}

		/// <summary>
		/// Sets the various values on the target property on a specified time (between 0 and 1)
		/// </summary>
		/// <param name="time"></param>
		protected virtual void SetValues(float time, float intensityMultiplier)
		{
			float intensity = MMTween.Tween(time, 0f, 1f, RemapLevelZero, RemapLevelOne, LevelCurve);

			intensity *= intensityMultiplier;
            
			if (RelativeValues)
			{
				intensity += _initialIntensity;
			}

			Target.SetLevel(intensity);
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
					SetValues(_initialIntensity, 1f);
				}

				if (StartsOff)
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
			if (Target.TargetComponent.gameObject != null)
			{
				Target.TargetComponent.gameObject.SetActive(status);
			}
		}
	}
}