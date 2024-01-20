using UnityEngine;
#if MM_POSTPROCESSING
using UnityEngine.Rendering.PostProcessing;
#endif
using MoreMountains.Feedbacks;
using MoreMountains.Tools;

namespace MoreMountains.FeedbacksForThirdParty
{
	/// <summary>
	/// Add this class to a Camera with a color grading post processing and it'll be able to "shake" its values by getting events
	/// </summary>
	[AddComponentMenu("More Mountains/Feedbacks/Shakers/PostProcessing/MMColorGradingShaker")]
	#if MM_POSTPROCESSING
	[RequireComponent(typeof(PostProcessVolume))]
	#endif
	public class MMColorGradingShaker : MMShaker
	{
		/// whether or not to add to the initial value
		public bool RelativeValues = true;

		[MMInspectorGroup("Post Exposure", true, 40)]
		/// the curve used to animate the focus distance value on
		[Tooltip("the curve used to animate the focus distance value on")]
		public AnimationCurve ShakePostExposure = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("the value to remap the curve's 0 to")]
		public float RemapPostExposureZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("the value to remap the curve's 1 to")]
		public float RemapPostExposureOne = 1f;

		[MMInspectorGroup("Hue Shift", true, 49)]
		/// the curve used to animate the aperture value on
		[Tooltip("the curve used to animate the aperture value on")]
		public AnimationCurve ShakeHueShift = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("the value to remap the curve's 0 to")]
		[Range(-180f, 180f)]
		public float RemapHueShiftZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("the value to remap the curve's 1 to")]
		[Range(-180f, 180f)]
		public float RemapHueShiftOne = 180f;

		[MMInspectorGroup("Saturation", true, 48)]
		/// the curve used to animate the focal length value on
		[Tooltip("the curve used to animate the focal length value on")]
		public AnimationCurve ShakeSaturation = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("the value to remap the curve's 0 to")]
		[Range(-100f, 100f)]
		public float RemapSaturationZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("the value to remap the curve's 1 to")]
		[Range(-100f, 100f)]
		public float RemapSaturationOne = 100f;

		[MMInspectorGroup("Contrast", true, 47)]
		/// the curve used to animate the focal length value on
		[Tooltip("the curve used to animate the focal length value on")]
		public AnimationCurve ShakeContrast = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("the value to remap the curve's 0 to")]
		[Range(-100f, 100f)]
		public float RemapContrastZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("the value to remap the curve's 1 to")]
		[Range(-100f, 100f)]
		public float RemapContrastOne = 100f;
        
		#if MM_POSTPROCESSING
		protected PostProcessVolume _volume;
		protected ColorGrading _colorGrading;
		protected float _initialPostExposure;
		protected float _initialHueShift;
		protected float _initialSaturation;
		protected float _initialContrast;
		protected float _originalShakeDuration;
		protected bool _originalRelativeValues;
		protected AnimationCurve _originalShakePostExposure;
		protected float _originalRemapPostExposureZero;
		protected float _originalRemapPostExposureOne;
		protected AnimationCurve _originalShakeHueShift;
		protected float _originalRemapHueShiftZero;
		protected float _originalRemapHueShiftOne;
		protected AnimationCurve _originalShakeSaturation;
		protected float _originalRemapSaturationZero;
		protected float _originalRemapSaturationOne;
		protected AnimationCurve _originalShakeContrast;
		protected float _originalRemapContrastZero;
		protected float _originalRemapContrastOne;

		/// <summary>
		/// On init we initialize our values
		/// </summary>
		protected override void Initialization()
		{
			base.Initialization();
			_volume = this.gameObject.GetComponent<PostProcessVolume>();
			_volume.profile.TryGetSettings(out _colorGrading);
		}

		/// <summary>
		/// When that shaker gets added, we initialize its shake duration
		/// </summary>
		protected virtual void Reset()
		{
			ShakeDuration = 0.8f;
		}

		/// <summary>
		/// Shakes values over time
		/// </summary>
		protected override void Shake()
		{
			float newPostExposure = ShakeFloat(ShakePostExposure, RemapPostExposureZero, RemapPostExposureOne, RelativeValues, _initialPostExposure);
			_colorGrading.postExposure.Override(newPostExposure);
			float newHueShift = ShakeFloat(ShakeHueShift, RemapHueShiftZero, RemapHueShiftOne, RelativeValues, _initialHueShift);
			_colorGrading.hueShift.Override(newHueShift);
			float newSaturation = ShakeFloat(ShakeSaturation, RemapSaturationZero, RemapSaturationOne, RelativeValues, _initialSaturation);
			_colorGrading.saturation.Override(newSaturation);
			float newContrast = ShakeFloat(ShakeContrast, RemapContrastZero, RemapContrastOne, RelativeValues, _initialContrast);
			_colorGrading.contrast.Override(newContrast);
		}

		/// <summary>
		/// Collects initial values on the target
		/// </summary>
		protected override void GrabInitialValues()
		{
			_initialPostExposure = _colorGrading.postExposure;
			_initialHueShift = _colorGrading.hueShift;
			_initialSaturation = _colorGrading.saturation;
			_initialContrast = _colorGrading.contrast;
		}

		/// <summary>
		/// When we get the appropriate event, we trigger a shake
		/// </summary>
		/// <param name="intensity"></param>
		/// <param name="duration"></param>
		/// <param name="amplitude"></param>
		/// <param name="relativeIntensity"></param>
		/// <param name="feedbacksIntensity"></param>
		/// <param name="channel"></param>
		public virtual void OnMMColorGradingShakeEvent(AnimationCurve shakePostExposure, float remapPostExposureZero, float remapPostExposureOne,
			AnimationCurve shakeHueShift, float remapHueShiftZero, float remapHueShiftOne,
			AnimationCurve shakeSaturation, float remapSaturationZero, float remapSaturationOne,
			AnimationCurve shakeContrast, float remapContrastZero, float remapContrastOne,
			float duration, bool relativeValues = false,
			float feedbacksIntensity = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, 
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
				_originalRelativeValues = RelativeValues;
				_originalShakePostExposure = ShakePostExposure;
				_originalRemapPostExposureZero = RemapPostExposureZero;
				_originalRemapPostExposureOne = RemapPostExposureOne;
				_originalShakeHueShift = ShakeHueShift;
				_originalRemapHueShiftZero = RemapHueShiftZero;
				_originalRemapHueShiftOne = RemapHueShiftOne;
				_originalShakeSaturation = ShakeSaturation;
				_originalRemapSaturationZero = RemapSaturationZero;
				_originalRemapSaturationOne = RemapSaturationOne;
				_originalShakeContrast = ShakeContrast;
				_originalRemapContrastZero = RemapContrastZero;
				_originalRemapContrastOne = RemapContrastOne;
			}

			if (!OnlyUseShakerValues)
			{
				TimescaleMode = timescaleMode;
				ShakeDuration = duration;
				RelativeValues = relativeValues;
				ShakePostExposure = shakePostExposure;
				RemapPostExposureZero = remapPostExposureZero;
				RemapPostExposureOne = remapPostExposureOne;
				ShakeHueShift = shakeHueShift;
				RemapHueShiftZero = remapHueShiftZero;
				RemapHueShiftOne = remapHueShiftOne;
				ShakeSaturation = shakeSaturation;
				RemapSaturationZero = remapSaturationZero;
				RemapSaturationOne = remapSaturationOne;
				ShakeContrast = shakeContrast;
				RemapContrastZero = remapContrastZero;
				RemapContrastOne = remapContrastOne;
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
			_colorGrading.postExposure.Override(_initialPostExposure);
			_colorGrading.hueShift.Override(_initialHueShift);
			_colorGrading.saturation.Override(_initialSaturation);
			_colorGrading.contrast.Override(_initialContrast);
		}

		/// <summary>
		/// Resets the shaker's values
		/// </summary>
		protected override void ResetShakerValues()
		{
			base.ResetShakerValues();
			ShakeDuration = _originalShakeDuration;
			RelativeValues = _originalRelativeValues;
			ShakePostExposure = _originalShakePostExposure;
			RemapPostExposureZero = _originalRemapPostExposureZero;
			RemapPostExposureOne = _originalRemapPostExposureOne;
			ShakeHueShift = _originalShakeHueShift;
			RemapHueShiftZero = _originalRemapHueShiftZero;
			RemapHueShiftOne = _originalRemapHueShiftOne;
			ShakeSaturation = _originalShakeSaturation;
			RemapSaturationZero = _originalRemapSaturationZero;
			RemapSaturationOne = _originalRemapSaturationOne;
			ShakeContrast = _originalShakeContrast;
			RemapContrastZero = _originalRemapContrastZero;
			RemapContrastOne = _originalRemapContrastOne;
		}

		/// <summary>
		/// Starts listening for events
		/// </summary>
		public override void StartListening()
		{
			base.StartListening();
			MMColorGradingShakeEvent.Register(OnMMColorGradingShakeEvent);
		}

		/// <summary>
		/// Stops listening for events
		/// </summary>
		public override void StopListening()
		{
			base.StopListening();
			MMColorGradingShakeEvent.Unregister(OnMMColorGradingShakeEvent);
		}
		#endif
	}

	/// <summary>
	/// An event used to trigger vignette shakes
	/// </summary>
	public struct MMColorGradingShakeEvent
	{
		static private event Delegate OnEvent;
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void RuntimeInitialization() { OnEvent = null; }
		static public void Register(Delegate callback) { OnEvent += callback; }
		static public void Unregister(Delegate callback) { OnEvent -= callback; }
		
		public delegate void Delegate(AnimationCurve shakePostExposure, float remapPostExposureZero, float remapPostExposureOne,
			AnimationCurve shakeHueShift, float remapHueShiftZero, float remapHueShiftOne,
			AnimationCurve shakeSaturation, float remapSaturationZero, float remapSaturationOne,
			AnimationCurve shakeContrast, float remapContrastZero, float remapContrastOne,
			float duration, bool relativeValues = false,
			float feedbacksIntensity = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, 
			bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false);

		static public void Trigger(AnimationCurve shakePostExposure, float remapPostExposureZero, float remapPostExposureOne,
			AnimationCurve shakeHueShift, float remapHueShiftZero, float remapHueShiftOne,
			AnimationCurve shakeSaturation, float remapSaturationZero, float remapSaturationOne,
			AnimationCurve shakeContrast, float remapContrastZero, float remapContrastOne,
			float duration, bool relativeValues = false,
			float feedbacksIntensity = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, 
			bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false)
		{
			OnEvent?.Invoke(shakePostExposure, remapPostExposureZero, remapPostExposureOne,
				shakeHueShift, remapHueShiftZero, remapHueShiftOne,
				shakeSaturation, remapSaturationZero, remapSaturationOne,
				shakeContrast, remapContrastZero, remapContrastOne,
				duration, relativeValues, feedbacksIntensity, channelData, resetShakerValuesAfterShake, resetTargetValuesAfterShake, forwardDirection, timescaleMode, stop, restore);
		}
	}
}