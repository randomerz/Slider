using System;
using System.Collections;
using MoreMountains.Tools;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This shaker will let you move the rotation of a transform, either once or permanently, shaking its rotation for the specified duration and within the specified range.
	/// You can apply that shake along a direction, randomized or not, with optional noise and attenuation
	/// </summary>
	public class MMRotationShaker : MMShaker
	{
		public enum Modes { Transform, RectTransform }

		[MMInspectorGroup("Target", true, 41)]
		/// whether this shaker should target Transforms or RectTransforms
		[Tooltip("whether this shaker should target Transforms or RectTransforms")]
		public Modes Mode = Modes.Transform;
		/// the transform to shake the rotation of. If left blank, this component will target the transform it's put on.
		[Tooltip("the transform to shake the rotation of. If left blank, this component will target the transform it's put on.")]
		[MMEnumCondition("Mode", (int)Modes.Transform)]
		public Transform TargetTransform;
		/// the rect transform to shake the rotation of. If left blank, this component will target the transform it's put on.
		[Tooltip("the rect transform to shake the rotation of. If left blank, this component will target the transform it's put on.")]
		[MMEnumCondition("Mode", (int)Modes.RectTransform)]
		public RectTransform TargetRectTransform;
        
		[MMInspectorGroup("Shake Settings", true, 42)]
		/// the speed at which the transform should shake
		[Tooltip("the speed at which the transform should shake")]
		public float ShakeSpeed = 20f;
		/// the maximum distance from its initial rotation the transform will move to during the shake
		[Tooltip("the maximum distance from its initial rotation the transform will move to during the shake")]
		public float ShakeRange = 50f;
        
		[MMInspectorGroup("Direction", true, 43)]
		/// the direction along which to shake the transform's rotation
		[Tooltip("the direction along which to shake the transform's rotation")]
		public Vector3 ShakeMainDirection = Vector3.up;
		/// if this is true, instead of using ShakeMainDirection as the direction of the shake, a random vector3 will be generated, randomized between ShakeMainDirection and ShakeAltDirection
		[Tooltip("if this is true, instead of using ShakeMainDirection as the direction of the shake, a random vector3 will be generated, randomized between ShakeMainDirection and ShakeAltDirection")]
		public bool RandomizeDirection = false;
		/// when in RandomizeDirection mode, a vector against which to randomize the main direction
		[Tooltip("when in RandomizeDirection mode, a vector against which to randomize the main direction")]
		[MMCondition("RandomizeDirection", true)]
		public Vector3 ShakeAltDirection = Vector3.up;
		/// if this is true, a new direction will be randomized every time a shake happens
		[Tooltip("if this is true, a new direction will be randomized every time a shake happens")]
		public bool RandomizeDirectionOnPlay = false;

		[MMInspectorGroup("Directional Noise", true, 47)]
		/// whether or not to add noise to the main direction
		[Tooltip("whether or not to add noise to the main direction")]
		public bool AddDirectionalNoise = true;
		/// when adding directional noise, noise strength will be randomized between this value and DirectionalNoiseStrengthMax
		[Tooltip("when adding directional noise, noise strength will be randomized between this value and DirectionalNoiseStrengthMax")]
		[MMCondition("AddDirectionalNoise", true)]
		public Vector3 DirectionalNoiseStrengthMin = new Vector3(0.25f, 0.25f, 0.25f);
		/// when adding directional noise, noise strength will be randomized between this value and DirectionalNoiseStrengthMin
		[Tooltip("when adding directional noise, noise strength will be randomized between this value and DirectionalNoiseStrengthMin")]
		[MMCondition("AddDirectionalNoise", true)]
		public Vector3 DirectionalNoiseStrengthMax = new Vector3(0.25f, 0.25f, 0.25f);
        
		[MMInspectorGroup("Randomness", true, 44)]
		/// a unique seed you can use to get different outcomes when shaking more than one transform at once
		[Tooltip("a unique seed you can use to get different outcomes when shaking more than one transform at once")]
		public Vector3 RandomnessSeed;
		/// whether or not to generate a unique seed automatically on every shake
		[Tooltip("whether or not to generate a unique seed automatically on every shake")]
		public bool RandomizeSeedOnShake = true;

		[MMInspectorGroup("One Time", true, 45)]
		/// whether or not to use attenuation, which will impact the amplitude of the shake, along the defined curve
		[Tooltip("whether or not to use attenuation, which will impact the amplitude of the shake, along the defined curve")]
		public bool UseAttenuation = true;
		/// the animation curve used to define attenuation, impacting the amplitude of the shake
		[Tooltip("the animation curve used to define attenuation, impacting the amplitude of the shake")]
		[MMCondition("UseAttenuation", true)]
		public AnimationCurve AttenuationCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));

		[MMInspectorGroup("Test", true, 46)]
		[MMInspectorButton("StartShaking")] 
		public bool StartShakingButton;

		public virtual float Randomness => RandomnessSeed.x + RandomnessSeed.y + RandomnessSeed.z;

		protected float _attenuation = 1f;
		protected float _oscillation;
		protected Vector3 _initialRotation;
		protected Vector3 _workDirection;
		protected Vector3 _noiseVector;
		protected Vector3 _newRotation;
		protected Vector3 _randomNoiseStrength;
		protected Vector3 _noNoise = Vector3.zero;
		protected Vector3 _randomizedDirection;

		/// <summary>
		/// On init we initialize our values
		/// </summary>
		protected override void Initialization()
		{
			base.Initialization();
			if (TargetTransform == null) { TargetTransform = this.transform; }
			if (TargetRectTransform == null) { TargetRectTransform = this.GetComponent<RectTransform>(); }
			GrabLocalRotation();
		}

		public virtual void GrabLocalRotation()
		{
			switch (Mode)
			{
				case Modes.Transform:
					_initialRotation = TargetTransform.localRotation.eulerAngles;
					break;
				case Modes.RectTransform:
					_initialRotation = TargetRectTransform.localRotation.eulerAngles;
					break;
			}
		}
               
		/// <summary>
		/// When that shaker gets added, we initialize its shake duration
		/// </summary>
		protected virtual void Reset()
		{
			ShakeDuration = 0.5f;
		}

		protected override void ShakeStarts()
		{
			GrabLocalRotation();
			if (RandomizeSeedOnShake)
			{
				RandomnessSeed = Random.insideUnitSphere;
			}

			if (RandomizeDirectionOnPlay)
			{
				ShakeMainDirection = Random.insideUnitSphere;
				ShakeAltDirection = Random.insideUnitSphere;
			}
           
			_randomizedDirection = RandomizeDirection ? MMMaths.RandomVector3(ShakeMainDirection, ShakeAltDirection) : ShakeMainDirection;
		}
        
		protected override void Shake()
		{
			_oscillation = Mathf.Sin(ShakeSpeed * (Randomness + _journey));
			float remappedTime = MMFeedbacksHelpers.Remap(_journey, 0f, ShakeDuration, 0f, 1f);
           
			_attenuation = ComputeAttenuation(remappedTime);
			_workDirection = ShakeMainDirection + ComputeNoise(_journey);
			_workDirection.Normalize();
			_newRotation = ComputeNewRotation();
			ApplyNewRotation(_newRotation);
		}
        
		protected override void ShakeComplete()
		{
			base.ShakeComplete();
			_attenuation = 0f;
			_newRotation = ComputeNewRotation();
			if (TargetTransform != null)
			{
				ApplyNewRotation(_newRotation);	
			}
		}

		protected virtual void ApplyNewRotation(Vector3 newRotation)
		{
			switch (Mode)
			{
				case Modes.Transform:
					TargetTransform.localRotation = Quaternion.Euler(newRotation);
					break;
				case Modes.RectTransform:
					TargetRectTransform.localRotation = Quaternion.Euler(newRotation);
					break;
			}
		}

		protected virtual Vector3 ComputeNewRotation()
		{
			return _initialRotation + _workDirection * _oscillation * ShakeRange * _attenuation;
		}

		protected virtual float ComputeAttenuation(float remappedTime)
		{
			return (UseAttenuation && !PermanentShake) ? AttenuationCurve.Evaluate(remappedTime) : 1f;
		}
        
		protected virtual Vector3 ComputeNoise(float time)
		{
			if (!AddDirectionalNoise)
			{
				return _noNoise;
			}

			_randomNoiseStrength = MMMaths.RandomVector3(DirectionalNoiseStrengthMin, DirectionalNoiseStrengthMax); 
	        
			_noiseVector.x = _randomNoiseStrength.x * (Mathf.PerlinNoise(RandomnessSeed.x, time) - 0.5f) ;
			_noiseVector.y = _randomNoiseStrength.y * (Mathf.PerlinNoise(RandomnessSeed.y, time) - 0.5f);
			_noiseVector.z = _randomNoiseStrength.z * (Mathf.PerlinNoise(RandomnessSeed.z, time) - 0.5f);
	        
			return _noiseVector;
		}
        
		protected float _originalDuration;
		protected float _originalShakeSpeed;
		protected float _originalShakeRange;
		protected Vector3 _originalShakeMainDirection;
		protected bool _originalRandomizeDirection;
		protected Vector3 _originalShakeAltDirection;
		protected bool _originalRandomizeDirectionOnPlay;
		protected bool _originalAddDirectionalNoise;
		protected Vector3 _originalDirectionalNoiseStrengthMin;
		protected Vector3 _originalDirectionalNoiseStrengthMax;
		protected Vector3 _originalRandomnessSeed;
		protected bool _originalRandomizeSeedOnShake;
		protected bool _originalUseAttenuation;
		protected AnimationCurve _originalAttenuationCurve;

		public virtual void OnMMRotationShakeEvent(float duration, float shakeSpeed, float shakeRange, Vector3 shakeMainDirection, bool randomizeDirection, Vector3 shakeAltDirection, bool randomizeDirectionOnPlay, bool addDirectionalNoise, 
			Vector3 directionalNoiseStrengthMin, Vector3 directionalNoiseStrengthMax, Vector3 randomnessSeed, bool randomizeSeedOnShake, bool useAttenuation, AnimationCurve attenuationCurve,
			bool useRange = false, float rangeDistance = 0f, bool useRangeFalloff = false, AnimationCurve rangeFalloff = null, Vector2 remapRangeFalloff = default(Vector2), Vector3 rangePosition = default(Vector3),
			float feedbacksIntensity = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, 
			bool resetTargetValuesAfterShake = true, bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false)
		{
			if (!CheckEventAllowed(channelData) || (!Interruptible && Shaking))
			{
				return;
			}
            
			if (stop)
			{
				Stop();
				return;
			}
            
			if (restore)
			{
				ResetTargetValues();
				return;
			}
            
			_resetShakerValuesAfterShake = resetShakerValuesAfterShake;
			_resetTargetValuesAfterShake = resetTargetValuesAfterShake;

			if (resetShakerValuesAfterShake)
			{
				_originalDuration = ShakeDuration;
				_originalShakeSpeed = ShakeSpeed;
				_originalShakeRange = ShakeRange;
				_originalShakeMainDirection = ShakeMainDirection;
				_originalRandomizeDirection = RandomizeDirection;
				_originalShakeAltDirection = ShakeAltDirection;
				_originalRandomizeDirectionOnPlay = RandomizeDirectionOnPlay;
				_originalAddDirectionalNoise = AddDirectionalNoise;
				_originalDirectionalNoiseStrengthMin = DirectionalNoiseStrengthMin;
				_originalDirectionalNoiseStrengthMax = DirectionalNoiseStrengthMax;
				_originalRandomnessSeed = RandomnessSeed;
				_originalRandomizeSeedOnShake = RandomizeSeedOnShake;
				_originalUseAttenuation = UseAttenuation;
				_originalAttenuationCurve = AttenuationCurve;
			}

			if (!OnlyUseShakerValues)
			{
				TimescaleMode = timescaleMode;
				ShakeDuration = duration;
				ShakeSpeed = shakeSpeed;
				ShakeRange = shakeRange * feedbacksIntensity * ComputeRangeIntensity(useRange, rangeDistance,
					useRangeFalloff, rangeFalloff, remapRangeFalloff, rangePosition);
				ShakeMainDirection = shakeMainDirection;
				RandomizeDirection = randomizeDirection;
				ShakeAltDirection = shakeAltDirection;
				RandomizeDirectionOnPlay = randomizeDirectionOnPlay;
				AddDirectionalNoise = addDirectionalNoise;
				DirectionalNoiseStrengthMin = directionalNoiseStrengthMin;
				DirectionalNoiseStrengthMax = directionalNoiseStrengthMax;
				RandomnessSeed = randomnessSeed;
				RandomizeSeedOnShake = randomizeSeedOnShake;
				UseAttenuation = useAttenuation;
				AttenuationCurve = attenuationCurve;
				ForwardDirection = forwardDirection;
			}

			Play();
		}

		/// <summary>
		/// Resets the target's values
		/// </summary>
		protected override void ResetTargetValues()
		{
			base.ResetTargetValues();
			switch (Mode)
			{
				case Modes.Transform:
					TargetTransform.localRotation = Quaternion.Euler(_initialRotation);
					break;
				case Modes.RectTransform:
					TargetRectTransform.localRotation = Quaternion.Euler(_initialRotation);
					break;
			}
		}

		/// <summary>
		/// Resets the shaker's values
		/// </summary>
		protected override void ResetShakerValues()
		{
			base.ResetShakerValues();
			ShakeDuration = _originalDuration;
			ShakeSpeed = _originalShakeSpeed;
			ShakeRange = _originalShakeRange;
			ShakeMainDirection = _originalShakeMainDirection;
			RandomizeDirection = _originalRandomizeDirection;
			ShakeAltDirection = _originalShakeAltDirection;
			RandomizeDirectionOnPlay = _originalRandomizeDirectionOnPlay;
			AddDirectionalNoise = _originalAddDirectionalNoise;
			DirectionalNoiseStrengthMin = _originalDirectionalNoiseStrengthMin;
			DirectionalNoiseStrengthMax = _originalDirectionalNoiseStrengthMax;
			RandomnessSeed = _originalRandomnessSeed;
			RandomizeSeedOnShake = _originalRandomizeSeedOnShake;
			UseAttenuation = _originalUseAttenuation;
			AttenuationCurve = _originalAttenuationCurve;
		}

		/// <summary>
		/// Starts listening for events
		/// </summary>
		public override void StartListening()
		{
			base.StartListening();
			MMRotationShakeEvent.Register(OnMMRotationShakeEvent);
		}

		/// <summary>
		/// Stops listening for events
		/// </summary>
		public override void StopListening()
		{
			base.StopListening();
			MMRotationShakeEvent.Unregister(OnMMRotationShakeEvent);
		}
	}
	
	public struct MMRotationShakeEvent
	{
		static private event Delegate OnEvent;
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void RuntimeInitialization() { OnEvent = null; }
		static public void Register(Delegate callback) { OnEvent += callback; }
		static public void Unregister(Delegate callback) { OnEvent -= callback; }

		public delegate void Delegate(float duration, float shakeSpeed, float shakeRange, Vector3 shakeMainDirection, bool randomizeDirection, Vector3 shakeAltDirection, bool randomizeDirectionOnPlay, bool addDirectionalNoise, 
			Vector3 directionalNoiseStrengthMin, Vector3 directionalNoiseStrengthMax, Vector3 randomnessSeed, bool randomizeSeedOnShake, bool useAttenuation, AnimationCurve attenuationCurve,
			bool useRange = false, float rangeDistance = 0f, bool useRangeFalloff = false, AnimationCurve rangeFalloff = null, Vector2 remapRangeFalloff = default(Vector2), Vector3 rangePosition = default(Vector3),
			float feedbacksIntensity = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, 
			bool resetTargetValuesAfterShake = true, bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false);

		static public void Trigger(float duration, float shakeSpeed, float shakeRange, Vector3 shakeMainDirection, bool randomizeDirection, Vector3 shakeAltDirection, bool randomizeDirectionOnPlay, bool addDirectionalNoise, 
			Vector3 directionalNoiseStrengthMin, Vector3 directionalNoiseStrengthMax, Vector3 randomnessSeed, bool randomizeSeedOnShake, bool useAttenuation, AnimationCurve attenuationCurve,
			bool useRange = false, float rangeDistance = 0f, bool useRangeFalloff = false, AnimationCurve rangeFalloff = null, Vector2 remapRangeFalloff = default(Vector2), Vector3 rangePosition = default(Vector3),
			float feedbacksIntensity = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, 
			bool resetTargetValuesAfterShake = true, bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false)
		{
			OnEvent?.Invoke( duration, shakeSpeed,  shakeRange,  shakeMainDirection,  randomizeDirection,  shakeAltDirection,  randomizeDirectionOnPlay,  addDirectionalNoise, 
				directionalNoiseStrengthMin,  directionalNoiseStrengthMax,  randomnessSeed,  randomizeSeedOnShake,  useAttenuation,  attenuationCurve,
				useRange, rangeDistance, useRangeFalloff, rangeFalloff, remapRangeFalloff, rangePosition,
				feedbacksIntensity, channelData, resetShakerValuesAfterShake, resetTargetValuesAfterShake, forwardDirection, timescaleMode, stop, restore);
		}
	}
}