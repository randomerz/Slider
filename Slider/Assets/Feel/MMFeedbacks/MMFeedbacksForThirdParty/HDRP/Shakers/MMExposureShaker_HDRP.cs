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
	/// Add this class to a Camera with a HDRP exposure post processing and it'll be able to "shake" its values by getting events
	/// </summary>
	#if MM_HDRP
	[RequireComponent(typeof(Volume))]
	#endif
	[AddComponentMenu("More Mountains/Feedbacks/Shakers/PostProcessing/MMExposureShaker_HDRP")]
	public class MMExposureShaker_HDRP : MMShaker
	{
		[MMInspectorGroup("Exposure Intensity", true, 46)]
		/// whether or not to add to the initial value
		[Tooltip("whether or not to add to the initial value")]
		public bool RelativeIntensity = false;
		/// the curve used to animate the intensity value on
		[Tooltip("the curve used to animate the intensity value on")]
		public AnimationCurve ShakeFixedExposure = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("the value to remap the curve's 0 to")]
		public float RemapFixedExposureZero = 8.5f;
		/// the value to remap the curve's 1 to
		[Tooltip("the value to remap the curve's 1 to")]
		public float RemapFixedExposureOne = 6f;

		#if MM_HDRP
		protected Volume _volume;
		protected Exposure _exposure;
		protected float _initialFixedExposure;
		protected float _originalShakeDuration;
		protected AnimationCurve _originalShakeFixedExposure;
		protected float _originalRemapFixedExposureZero;
		protected float _originalRemapFixedExposureOne;
		protected bool _originalRelativeFixedExposure;

		/// <summary>
		/// On init we initialize our values
		/// </summary>
		protected override void Initialization()
		{
			base.Initialization();
			_volume = this.gameObject.GetComponent<Volume>();
			_volume.profile.TryGet(out _exposure);
		}

		/// <summary>
		/// Shakes values over time
		/// </summary>
		protected override void Shake()
		{
			float newValue = ShakeFloat(ShakeFixedExposure, RemapFixedExposureZero, RemapFixedExposureOne, RelativeIntensity, _initialFixedExposure);
			_exposure.fixedExposure.Override(newValue);
		}

		/// <summary>
		/// Collects initial values on the target
		/// </summary>
		protected override void GrabInitialValues()
		{
			_initialFixedExposure = _exposure.fixedExposure.value;
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
		public virtual void OnExposureShakeEvent(AnimationCurve intensity, float duration, float remapMin, float remapMax, bool relativeIntensity = false,
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
				_originalShakeFixedExposure = ShakeFixedExposure;
				_originalRemapFixedExposureZero = RemapFixedExposureZero;
				_originalRemapFixedExposureOne = RemapFixedExposureOne;
				_originalRelativeFixedExposure = RelativeIntensity;
			}

			if (!OnlyUseShakerValues)
			{
				TimescaleMode = timescaleMode;
				ShakeDuration = duration;
				ShakeFixedExposure = intensity;
				RemapFixedExposureZero = remapMin * attenuation;
				RemapFixedExposureOne = remapMax * attenuation;
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
			_exposure.fixedExposure.Override(_initialFixedExposure);
		}

		/// <summary>
		/// Resets the shaker's values
		/// </summary>
		protected override void ResetShakerValues()
		{
			base.ResetShakerValues();
			ShakeDuration = _originalShakeDuration;
			ShakeFixedExposure = _originalShakeFixedExposure;
			RemapFixedExposureZero = _originalRemapFixedExposureZero;
			RemapFixedExposureOne = _originalRemapFixedExposureOne;
			RelativeIntensity = _originalRelativeFixedExposure;
		}

		/// <summary>
		/// Starts listening for events
		/// </summary>
		public override void StartListening()
		{
			base.StartListening();
			MMExposureShakeEvent_HDRP.Register(OnExposureShakeEvent);
		}

		/// <summary>
		/// Stops listening for events
		/// </summary>
		public override void StopListening()
		{
			base.StopListening();
			MMExposureShakeEvent_HDRP.Unregister(OnExposureShakeEvent);
		}
		#endif
	}

	/// <summary>
	/// An event used to trigger exposure shakes
	/// </summary>
	public struct MMExposureShakeEvent_HDRP
	{
		static private event Delegate OnEvent;
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void RuntimeInitialization() { OnEvent = null; }
		static public void Register(Delegate callback) { OnEvent += callback; }
		static public void Unregister(Delegate callback) { OnEvent -= callback; }
		
		public delegate void Delegate(AnimationCurve fixedExposure, float duration, float remapMin, float remapMax, bool relativeFixedExposure = false,
			float attenuation = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, 
			bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false);
		
		static public void Trigger(AnimationCurve fixedExposure, float duration, float remapMin, float remapMax, bool relativeFixedExposure = false,
			float attenuation = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, 
			bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false)
		{
			OnEvent?.Invoke(fixedExposure, duration, remapMin, remapMax, relativeFixedExposure, attenuation, channelData, resetShakerValuesAfterShake, 
				resetTargetValuesAfterShake, forwardDirection, timescaleMode, stop, restore);
		}
	}
}