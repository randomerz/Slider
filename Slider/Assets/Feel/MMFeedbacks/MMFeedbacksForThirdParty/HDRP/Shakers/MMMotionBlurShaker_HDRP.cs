using UnityEngine;
using UnityEngine.Rendering;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
#if MM_HDRP
using UnityEngine.Rendering.HighDefinition;
#endif

namespace MoreMountains.FeedbacksForThirdParty
{
	/// <summary>
	/// Add this class to a Camera with a HDRP vignette post processing and it'll be able to "shake" its values by getting events
	/// </summary>
	#if MM_HDRP
	[RequireComponent(typeof(Volume))]
	#endif
	[AddComponentMenu("More Mountains/Feedbacks/Shakers/PostProcessing/MMMotionBlurShaker_HDRP")]
	public class MMMotionBlurShaker_HDRP : MMShaker
	{
		[MMInspectorGroup("Motion Blur Intensity", true, 48)]
		/// whether or not to add to the initial value
		[Tooltip("whether or not to add to the initial value")]
		public bool RelativeIntensity = false;
		/// the curve used to animate the intensity value on
		[Tooltip("the curve used to animate the intensity value on")]
		public AnimationCurve ShakeIntensity = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("the value to remap the curve's 0 to")]
		public float RemapIntensityZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("the value to remap the curve's 1 to")]
		public float RemapIntensityOne = 1000f;

		#if MM_HDRP
		protected Volume _volume;
		protected MotionBlur _motionBlur;
		protected float _initialIntensity;
		protected float _originalShakeDuration;
		protected AnimationCurve _originalShakeIntensity;
		protected float _originalRemapIntensityZero;
		protected float _originalRemapIntensityOne;
		protected bool _originalRelativeIntensity;

		/// <summary>
		/// On init we initialize our values
		/// </summary>
		protected override void Initialization()
		{
			base.Initialization();
			_volume = this.gameObject.GetComponent<Volume>();
			_volume.profile.TryGet(out _motionBlur);
		}

		/// <summary>
		/// Shakes values over time
		/// </summary>
		protected override void Shake()
		{
			float newValue = ShakeFloat(ShakeIntensity, RemapIntensityZero, RemapIntensityOne, RelativeIntensity, _initialIntensity);
			_motionBlur.intensity.Override(newValue);
		}

		/// <summary>
		/// Collects initial values on the target
		/// </summary>
		protected override void GrabInitialValues()
		{
			_initialIntensity = _motionBlur.intensity.value;
		}

		/// <summary>
		/// When we get the appropriate event, we trigger a shake
		/// </summary>
		/// <param name="intensity"></param>
		/// <param name="duration"></param>
		/// <param name="amplitude"></param>
		/// <param name="relativeIntensity"></param>
		/// <param name="attenuation"></param>
		/// <param name="channel"></param>
		public virtual void OnMotionBlurShakeEvent(AnimationCurve intensity, float duration, float remapMin, float remapMax, bool relativeIntensity = false,
			float attenuation = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, 
			bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false)
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
				_originalShakeDuration = ShakeDuration;
				_originalShakeIntensity = ShakeIntensity;
				_originalRemapIntensityZero = RemapIntensityZero;
				_originalRemapIntensityOne = RemapIntensityOne;
				_originalRelativeIntensity = RelativeIntensity;
			}

			if (!OnlyUseShakerValues)
			{
				TimescaleMode = timescaleMode;
				ShakeDuration = duration;
				ShakeIntensity = intensity;
				RemapIntensityZero = remapMin * attenuation;
				RemapIntensityOne = remapMax * attenuation;
				RelativeIntensity = relativeIntensity;
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
			_motionBlur.intensity.Override(_initialIntensity);
		}

		/// <summary>
		/// Resets the shaker's values
		/// </summary>
		protected override void ResetShakerValues()
		{
			base.ResetShakerValues();
			ShakeDuration = _originalShakeDuration;
			ShakeIntensity = _originalShakeIntensity;
			RemapIntensityZero = _originalRemapIntensityZero;
			RemapIntensityOne = _originalRemapIntensityOne;
			RelativeIntensity = _originalRelativeIntensity;
		}

		/// <summary>
		/// Starts listening for events
		/// </summary>
		public override void StartListening()
		{
			base.StartListening();
			MMMotionBlurShakeEvent_HDRP.Register(OnMotionBlurShakeEvent);
		}

		/// <summary>
		/// Stops listening for events
		/// </summary>
		public override void StopListening()
		{
			base.StopListening();
			MMMotionBlurShakeEvent_HDRP.Unregister(OnMotionBlurShakeEvent);
		}
		#endif
	}

	/// <summary>
	/// An event used to trigger vignette shakes
	/// </summary>
	public struct MMMotionBlurShakeEvent_HDRP
	{
		static private event Delegate OnEvent;
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void RuntimeInitialization() { OnEvent = null; }
		static public void Register(Delegate callback) { OnEvent += callback; }
		static public void Unregister(Delegate callback) { OnEvent -= callback; }
		
		public delegate void Delegate(AnimationCurve intensity, float duration, float remapMin, float remapMax, bool relativeIntensity = false,
			float attenuation = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, 
			bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false);
		
		static public void Trigger(AnimationCurve intensity, float duration, float remapMin, float remapMax, bool relativeIntensity = false,
			float attenuation = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true,
			bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false)
		{
			OnEvent?.Invoke(intensity, duration, remapMin, remapMax, relativeIntensity, attenuation, channelData, 
				resetShakerValuesAfterShake, resetTargetValuesAfterShake, forwardDirection, timescaleMode, stop, restore);
		}
	}
}