using UnityEngine;
#if MM_POSTPROCESSING
using UnityEngine.Rendering.PostProcessing;
#endif
using MoreMountains.Feedbacks;
using MoreMountains.Tools;

namespace MoreMountains.FeedbacksForThirdParty
{
	/// <summary>
	/// Add this class to a Camera with a vignette post processing and it'll be able to "shake" its values by getting events
	/// </summary>
	[AddComponentMenu("More Mountains/Feedbacks/Shakers/PostProcessing/MMVignetteShaker")]
	#if MM_POSTPROCESSING
	[RequireComponent(typeof(PostProcessVolume))]
	#endif
	public class MMVignetteShaker : MMShaker
	{
		[MMInspectorGroup("Vignette Intensity", true, 53)]
		/// whether or not to add to the initial value
		[Tooltip("whether or not to add to the initial value")]
		public bool RelativeIntensity = true;
		/// the curve used to animate the intensity value on
		[Tooltip("the curve used to animate the intensity value on")]
		public AnimationCurve ShakeIntensity = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("the value to remap the curve's 0 to")]
		[Range(0f, 1f)]
		public float RemapIntensityZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("the value to remap the curve's 1 to")]
		[Range(0f, 1f)]
		public float RemapIntensityOne = 0.1f;

		[MMInspectorGroup("Vignette Color", true, 51)]
		/// whether or not to also animate  the vignette's color
		[Tooltip("whether or not to also animate the vignette's color")]
		public bool InterpolateColor = false;
		/// the curve to animate the color on
		[Tooltip("the curve to animate the color on")]
		public AnimationCurve ColorCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.05f, 1f), new Keyframe(0.95f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("the value to remap the curve's 0 to")]
		[Range(0, 1)]
		public float RemapColorZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("the value to remap the curve's 1 to")]
		[Range(0f, 1f)]
		public float RemapColorOne = 1f;
		/// the color to lerp towards
		[Tooltip("the color to lerp towards")]
		public Color TargetColor = Color.red;


		#if MM_POSTPROCESSING
		protected PostProcessVolume _volume;
		protected Vignette _vignette;
		protected float _initialIntensity;
		protected float _originalShakeDuration;
		protected AnimationCurve _originalShakeIntensity;
		protected float _originalRemapIntensityZero;
		protected float _originalRemapIntensityOne;
		protected bool _originalRelativeIntensity;

		protected bool _originalInterpolateColor;
		protected AnimationCurve _originalColorCurve;
		protected float _originalRemapColorZero;
		protected float _originalRemapColorOne;
		protected Color _originalTargetColor;
		protected Color _initialColor;

		/// <summary>
		/// On init we initialize our values
		/// </summary>
		protected override void Initialization()
		{
			base.Initialization();
			_volume = this.gameObject.GetComponent<PostProcessVolume>();
			_volume.profile.TryGetSettings(out _vignette);
			_initialColor = _vignette.color;
		}

		public virtual void SetVignette(float newValue)
		{
			_vignette.intensity.Override(newValue);
		}

		/// <summary>
		/// Shakes values over time
		/// </summary>
		protected override void Shake()
		{
			float newValue = ShakeFloat(ShakeIntensity, RemapIntensityZero, RemapIntensityOne, RelativeIntensity, _initialIntensity);
			_vignette.intensity.Override(newValue);

			if (InterpolateColor)
			{
				float newColorValue = ShakeFloat(ColorCurve, RemapColorZero, RemapColorOne, RelativeIntensity, 0);
				_vignette.color.Override(Color.Lerp(_initialColor, TargetColor, newColorValue));	
			}
		}

		/// <summary>
		/// Collects initial values on the target
		/// </summary>
		protected override void GrabInitialValues()
		{
			_initialIntensity = _vignette.intensity;
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
		public virtual void OnVignetteShakeEvent(AnimationCurve intensity, float duration, float remapMin, float remapMax, bool relativeIntensity = false,
			float feedbacksIntensity = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, 
			bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false, 
			bool interpolateColor = false, AnimationCurve colorCurve = null, float remapColorZero = 0f, float remapColorOne = 1f, Color targetColor = default(Color))
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
				_originalInterpolateColor = InterpolateColor;
				_originalColorCurve = ColorCurve;
				_originalRemapColorZero = RemapColorZero;
				_originalRemapColorOne = RemapColorOne;
				_originalTargetColor = TargetColor;
			}

			if (!OnlyUseShakerValues)
			{
				TimescaleMode = timescaleMode;
				ShakeDuration = duration;
				ShakeIntensity = intensity;
				RemapIntensityZero = remapMin * feedbacksIntensity;
				RemapIntensityOne = remapMax * feedbacksIntensity;
				RelativeIntensity = relativeIntensity;
				ForwardDirection = forwardDirection;
				InterpolateColor = interpolateColor;
				ColorCurve = colorCurve;
				RemapColorZero = remapColorZero;
				RemapColorOne = remapColorOne;
				TargetColor = targetColor;
			}

			Play();
		}

		/// <summary>
		/// Resets the target's values
		/// </summary>
		protected override void ResetTargetValues()
		{
			base.ResetTargetValues();
			_vignette.intensity.Override(_initialIntensity);
			_vignette.color.Override(_initialColor);
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
			InterpolateColor = _originalInterpolateColor;
			ColorCurve = _originalColorCurve;
			RemapColorZero = _originalRemapColorZero;
			RemapColorOne = _originalRemapColorOne;
			TargetColor = _originalTargetColor;
		}

		/// <summary>
		/// Starts listening for events
		/// </summary>
		public override void StartListening()
		{
			base.StartListening();
			MMVignetteShakeEvent.Register(OnVignetteShakeEvent);
		}

		/// <summary>
		/// Stops listening for events
		/// </summary>
		public override void StopListening()
		{
			base.StopListening();
			MMVignetteShakeEvent.Unregister(OnVignetteShakeEvent);
		}
		#endif
	}

	/// <summary>
	/// An event used to trigger vignette shakes
	/// </summary>
	public struct MMVignetteShakeEvent
	{
		static private event Delegate OnEvent;
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void RuntimeInitialization() { OnEvent = null; }
		static public void Register(Delegate callback) { OnEvent += callback; }
		static public void Unregister(Delegate callback) { OnEvent -= callback; }

		public delegate void Delegate(AnimationCurve intensity, float duration, float remapMin, float remapMax, bool relativeIntensity = false,
			float feedbacksIntensity = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, 
			bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false, 
			bool interpolateColor = false, AnimationCurve colorCurve = null, float remapColorZero = 0f, float remapColorOne = 1f, Color targetColor = default(Color));
		
		static public void Trigger(AnimationCurve intensity, float duration, float remapMin, float remapMax, bool relativeIntensity = false,
			float feedbacksIntensity = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, 
			bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false, 
			bool interpolateColor = false, AnimationCurve colorCurve = null, float remapColorZero = 0f, float remapColorOne = 1f, Color targetColor = default(Color))
		{
			OnEvent?.Invoke(intensity, duration, remapMin, remapMax, relativeIntensity, feedbacksIntensity, channelData, resetShakerValuesAfterShake, 
				resetTargetValuesAfterShake, forwardDirection, timescaleMode, stop, restore,
				interpolateColor, colorCurve, remapColorZero, remapColorOne, targetColor);
		}
	}
}