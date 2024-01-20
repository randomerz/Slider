using UnityEngine;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback lets you emit a PositionShake event. This will be caught by MMPositionShakers (on the specified channel).
	/// Position shakers, as the name suggests, are used to shake the position of a transform, along a direction, with optional noise and other fine control options.
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackPath("Transform/Position Shake")]
	[FeedbackHelp("This feedback lets you emit a PositionShake event. This will be caught by MMPositionShakers (on the specified channel)." +
	              " Position shakers, as the name suggests, are used to shake the position of a transform, along a direction, with optional noise and other fine control options.")]
	public class MMF_PositionShake : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.TransformColor; } }
		public override string RequiredTargetText => RequiredChannelText;
		#endif
		/// returns the duration of the feedback
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(Duration); } set { Duration = value; } }
		public override bool HasChannel => true;
		public override bool HasRandomness => true;
		public override bool HasRange => true;
		public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => TargetShaker = FindAutomatedTarget<MMPositionShaker>();

		[MMFInspectorGroup("Optional Target", true, 33)]
		/// a specific (and optional) shaker to target, regardless of its channel
		[Tooltip("a specific (and optional) shaker to target, regardless of its channel")]
		public MMPositionShaker TargetShaker;
		
		[MMFInspectorGroup("Position Shake", true, 28)]
		/// the duration of the shake, in seconds
		[Tooltip("the duration of the shake, in seconds")]
		public float Duration = 0.5f;
		/// whether or not to reset shaker values after shake
		[Tooltip("whether or not to reset shaker values after shake")]
		public bool ResetShakerValuesAfterShake = true;
		/// whether or not to reset the target's values after shake
		[Tooltip("whether or not to reset the target's values after shake")]
		public bool ResetTargetValuesAfterShake = true;
		
		[MMFInspectorGroup("Shake Settings", true, 42)]
		/// the speed at which the transform should shake
		[Tooltip("the speed at which the transform should shake")]
		public float ShakeSpeed = 20f;
		/// the maximum distance from its initial position the transform will move to during the shake
		[Tooltip("the maximum distance from its initial position the transform will move to during the shake")]
		public float ShakeRange = 0.5f;
        
		[MMFInspectorGroup("Direction", true, 43)]
		/// the direction along which to shake the transform's position
		[Tooltip("the direction along which to shake the transform's position")]
		public Vector3 ShakeMainDirection = Vector3.up;
		/// if this is true, instead of using ShakeMainDirection as the direction of the shake, a random vector3 will be generated, randomized between ShakeMainDirection and ShakeAltDirection
		[Tooltip("if this is true, instead of using ShakeMainDirection as the direction of the shake, a random vector3 will be generated, randomized between ShakeMainDirection and ShakeAltDirection")]
		public bool RandomizeDirection = false;
		/// when in RandomizeDirection mode, a vector against which to randomize the main direction
		[Tooltip("when in RandomizeDirection mode, a vector against which to randomize the main direction")]
		[MMFCondition("RandomizeDirection", true)]
		public Vector3 ShakeAltDirection = Vector3.up;
		/// if this is true, a new direction will be randomized every time a shake happens
		[Tooltip("if this is true, a new direction will be randomized every time a shake happens")]
		public bool RandomizeDirectionOnPlay = false;

		[MMFInspectorGroup("Directional Noise", true, 47)]
		/// whether or not to add noise to the main direction
		[Tooltip("whether or not to add noise to the main direction")]
		public bool AddDirectionalNoise = true;
		/// when adding directional noise, noise strength will be randomized between this value and DirectionalNoiseStrengthMax
		[Tooltip("when adding directional noise, noise strength will be randomized between this value and DirectionalNoiseStrengthMax")]
		[MMFCondition("AddDirectionalNoise", true)]
		public Vector3 DirectionalNoiseStrengthMin = new Vector3(0.25f, 0.25f, 0.25f);
		/// when adding directional noise, noise strength will be randomized between this value and DirectionalNoiseStrengthMin
		[Tooltip("when adding directional noise, noise strength will be randomized between this value and DirectionalNoiseStrengthMin")]
		[MMFCondition("AddDirectionalNoise", true)]
		public Vector3 DirectionalNoiseStrengthMax = new Vector3(0.25f, 0.25f, 0.25f);
        
		[MMFInspectorGroup("Randomness", true, 44)]
		/// a unique seed you can use to get different outcomes when shaking more than one transform at once
		[Tooltip("a unique seed you can use to get different outcomes when shaking more than one transform at once")]
		public Vector3 RandomnessSeed;
		/// whether or not to generate a unique seed automatically on every shake
		[Tooltip("whether or not to generate a unique seed automatically on every shake")]
		public bool RandomizeSeedOnShake = true;

		[MMFInspectorGroup("One Time", true, 45)]
		/// whether or not to use attenuation, which will impact the amplitude of the shake, along the defined curve
		[Tooltip("whether or not to use attenuation, which will impact the amplitude of the shake, along the defined curve")]
		public bool UseAttenuation = true;
		/// the animation curve used to define attenuation, impacting the amplitude of the shake
		[Tooltip("the animation curve used to define attenuation, impacting the amplitude of the shake")]
		[MMFCondition("UseAttenuation", true)]
		public AnimationCurve AttenuationCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));

		/// <summary>
		/// Triggers the corresponding coroutine
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			
			float intensityMultiplier = ComputeIntensity(feedbacksIntensity, position);

			if (TargetShaker == null)
			{
				MMPositionShakeEvent.Trigger(Duration, ShakeSpeed,  ShakeRange,  ShakeMainDirection,  RandomizeDirection,  ShakeAltDirection,  RandomizeDirectionOnPlay,  AddDirectionalNoise, 
					DirectionalNoiseStrengthMin,  DirectionalNoiseStrengthMax,  RandomnessSeed,  RandomizeSeedOnShake,  UseAttenuation,  AttenuationCurve,
					UseRange, RangeDistance, UseRangeFalloff, RangeFalloff, RemapRangeFalloff, position,
					intensityMultiplier, ChannelData, ResetShakerValuesAfterShake, ResetTargetValuesAfterShake, NormalPlayDirection, ComputedTimescaleMode);
			}
			else
			{
				TargetShaker?.OnMMPositionShakeEvent(Duration, ShakeSpeed,  ShakeRange,  ShakeMainDirection,  RandomizeDirection,  ShakeAltDirection,  RandomizeDirectionOnPlay,  AddDirectionalNoise, 
					DirectionalNoiseStrengthMin,  DirectionalNoiseStrengthMax,  RandomnessSeed,  RandomizeSeedOnShake,  UseAttenuation,  AttenuationCurve,
					UseRange, RangeDistance, UseRangeFalloff, RangeFalloff, RemapRangeFalloff, position,
					intensityMultiplier, TargetShaker.ChannelData, ResetShakerValuesAfterShake, ResetTargetValuesAfterShake, NormalPlayDirection, ComputedTimescaleMode);	
			}
		}
        
		/// <summary>
		/// On stop we stop our transition
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

			if (TargetShaker == null)
			{
				MMPositionShakeEvent.Trigger(Duration, ShakeSpeed,  ShakeRange,  ShakeMainDirection,  RandomizeDirection,  ShakeAltDirection,  RandomizeDirectionOnPlay,  AddDirectionalNoise, 
					DirectionalNoiseStrengthMin,  DirectionalNoiseStrengthMax,  RandomnessSeed,  RandomizeSeedOnShake,  UseAttenuation,  AttenuationCurve, stop:true);	
			}
			else
			{
				TargetShaker?.OnMMPositionShakeEvent(Duration, ShakeSpeed,  ShakeRange,  ShakeMainDirection,  RandomizeDirection,  ShakeAltDirection,  RandomizeDirectionOnPlay,  AddDirectionalNoise, 
					DirectionalNoiseStrengthMin,  DirectionalNoiseStrengthMax,  RandomnessSeed,  RandomizeSeedOnShake,  UseAttenuation,  AttenuationCurve, channelData:TargetShaker.ChannelData, stop:true);	
			}
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

			if (TargetShaker == null)
			{
				MMPositionShakeEvent.Trigger(Duration, ShakeSpeed,  ShakeRange,  ShakeMainDirection,  RandomizeDirection,  ShakeAltDirection,  RandomizeDirectionOnPlay,  AddDirectionalNoise, 
					DirectionalNoiseStrengthMin,  DirectionalNoiseStrengthMax,  RandomnessSeed,  RandomizeSeedOnShake,  UseAttenuation,  AttenuationCurve, restore:true);	
			}
			else
			{
				TargetShaker?.OnMMPositionShakeEvent(Duration, ShakeSpeed,  ShakeRange,  ShakeMainDirection,  RandomizeDirection,  ShakeAltDirection,  RandomizeDirectionOnPlay,  AddDirectionalNoise, 
					DirectionalNoiseStrengthMin,  DirectionalNoiseStrengthMax,  RandomnessSeed,  RandomizeSeedOnShake,  UseAttenuation,  AttenuationCurve, channelData:TargetShaker.ChannelData, restore:true);	
			}
		}
	}
}