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
	/// Add this class to a Camera with a HDRP color adjustments post processing and it'll be able to "shake" its values by getting events
	/// </summary>
	#if MM_HDRP
	[RequireComponent(typeof(Volume))]
	#endif
	[AddComponentMenu("More Mountains/Feedbacks/Shakers/PostProcessing/MMChannelMixerShaker_HDRP")]
	public class MMChannelMixerShaker_HDRP : MMShaker
	{
		/// whether or not to add to the initial value
		public bool RelativeValues = true;

		[MMInspectorGroup("Red", true, 42)]
		/// the curve used to animate the red value on
		[Tooltip("the curve used to animate the red value on")]
		public AnimationCurve ShakeRed = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("the value to remap the curve's 0 to")]
		[Range(-200f, 200f)]
		public float RemapRedZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("the value to remap the curve's 1 to")]
		[Range(-200f, 200f)]
		public float RemapRedOne = 200f;

		[MMInspectorGroup("Green", true, 43)]
		/// the curve used to animate the green value on
		[Tooltip("the curve used to animate the green value on")]
		public AnimationCurve ShakeGreen = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("the value to remap the curve's 0 to")]
		[Range(-200f, 200f)]
		public float RemapGreenZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("the value to remap the curve's 1 to")]
		[Range(-200f, 200f)]
		public float RemapGreenOne = 200f;

		[MMInspectorGroup("Blue", true, 44)]
		/// the curve used to animate the blue value on
		[Tooltip("the curve used to animate the blue value on")]
		public AnimationCurve ShakeBlue = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("the value to remap the curve's 0 to")]
		[Range(-200f, 200f)]
		public float RemapBlueZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("the value to remap the curve's 1 to")]
		[Range(-200f, 200f)]
		public float RemapBlueOne = 200f;
        
		#if MM_HDRP
		protected Volume _volume;
		protected ChannelMixer _channelMixer;
		protected float _initialRed;
		protected float _initialGreen;
		protected float _initialBlue;
		protected float _initialContrast;
		protected Color _initialColorFilterColor;
		protected float _originalShakeDuration;
		protected bool _originalRelativeValues;
	        
		protected AnimationCurve _originalShakeRed;
		protected float _originalRemapRedZero;
		protected float _originalRemapRedOne;
		protected AnimationCurve _originalShakeGreen;
		protected float _originalRemapGreenZero;
		protected float _originalRemapGreenOne;
		protected AnimationCurve _originalShakeBlue;
		protected float _originalRemapBlueZero;
		protected float _originalRemapBlueOne;

		/// <summary>
		/// On init we initialize our values
		/// </summary>
		protected override void Initialization()
		{
			base.Initialization();
			_volume = this.gameObject.GetComponent<Volume>();
			_volume.profile.TryGet(out _channelMixer);
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
			float newRed = ShakeFloat(ShakeRed, RemapRedZero, RemapRedOne, RelativeValues, _initialRed);
			_channelMixer.redOutRedIn.Override(newRed);
			float newGreen = ShakeFloat(ShakeGreen, RemapGreenZero, RemapGreenOne, RelativeValues, _initialGreen);
			_channelMixer.greenOutGreenIn.Override(newGreen);
			float newBlue = ShakeFloat(ShakeBlue, RemapBlueZero, RemapBlueOne, RelativeValues, _initialBlue);
			_channelMixer.blueOutBlueIn.Override(newBlue);
		}

		/// <summary>
		/// Collects initial values on the target
		/// </summary>
		protected override void GrabInitialValues()
		{
			_initialRed = _channelMixer.redOutRedIn.value;
			_initialGreen = _channelMixer.greenOutGreenIn.value;
			_initialBlue = _channelMixer.blueOutBlueIn.value;
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
		public virtual void OnMMChannelMixerShakeEvent(AnimationCurve shakeRed, float remapRedZero, float remapRedOne,
			AnimationCurve shakeGreen, float remapGreenZero, float remapGreenOne,
			AnimationCurve shakeBlue, float remapBlueZero, float remapBlueOne,
			float duration, bool relativeValues = false,
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
				_originalRelativeValues = RelativeValues;
				_originalShakeRed = ShakeRed;
				_originalRemapRedZero = RemapRedZero;
				_originalRemapRedOne = RemapRedOne;
				_originalShakeGreen = ShakeGreen;
				_originalRemapGreenZero = RemapGreenZero;
				_originalRemapGreenOne = RemapGreenOne;
				_originalShakeBlue = ShakeBlue;
				_originalRemapBlueZero = RemapBlueZero;
				_originalRemapBlueOne = RemapBlueOne;
			}

			if (!OnlyUseShakerValues)
			{
				TimescaleMode = timescaleMode;
				ShakeDuration = duration;
				RelativeValues = relativeValues;
				ShakeRed = shakeRed;
				RemapRedZero = remapRedZero;
				RemapRedOne = remapRedOne;
				ShakeGreen = shakeGreen;
				RemapGreenZero = remapGreenZero;
				RemapGreenOne = remapGreenOne;
				ShakeBlue = shakeBlue;
				RemapBlueZero = remapBlueZero;
				RemapBlueOne = remapBlueOne;
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
			_channelMixer.redOutRedIn.Override(_initialRed);
			_channelMixer.greenOutGreenIn.Override(_initialGreen);
			_channelMixer.blueOutBlueIn.Override(_initialBlue);
		}

		/// <summary>
		/// Resets the shaker's values
		/// </summary>
		protected override void ResetShakerValues()
		{
			base.ResetShakerValues();
			ShakeDuration = _originalShakeDuration;
			RelativeValues = _originalRelativeValues;
			ShakeRed = _originalShakeRed;
			RemapRedZero = _originalRemapRedZero;
			RemapRedOne = _originalRemapRedOne;
			ShakeGreen = _originalShakeGreen;
			RemapGreenZero = _originalRemapGreenZero;
			RemapGreenOne = _originalRemapGreenOne;
			ShakeBlue = _originalShakeBlue;
			RemapBlueZero = _originalRemapBlueZero;
			RemapBlueOne = _originalRemapBlueOne;
		}

		/// <summary>
		/// Starts listening for events
		/// </summary>
		public override void StartListening()
		{
			base.StartListening();
			MMChannelMixerShakeEvent_HDRP.Register(OnMMChannelMixerShakeEvent);
		}

		/// <summary>
		/// Stops listening for events
		/// </summary>
		public override void StopListening()
		{
			base.StopListening();
			MMChannelMixerShakeEvent_HDRP.Unregister(OnMMChannelMixerShakeEvent);
		}
		#endif
	}

	/// <summary>
	/// An event used to trigger vignette shakes
	/// </summary>
	public struct MMChannelMixerShakeEvent_HDRP
	{
		static private event Delegate OnEvent;
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void RuntimeInitialization() { OnEvent = null; }
		static public void Register(Delegate callback) { OnEvent += callback; }
		static public void Unregister(Delegate callback) { OnEvent -= callback; }
		
		public delegate void Delegate(
			AnimationCurve shakeRed, float remapRedZero, float remapRedOne,
			AnimationCurve shakeGreen, float remapGreenZero, float remapGreenOne,
			AnimationCurve shakeBlue, float remapBlueZero, float remapBlueOne,
			float duration, bool relativeValues = false,
			float attenuation = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, 
			bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false);

		static public void Trigger(
			AnimationCurve shakeRed, float remapRedZero, float remapRedOne,
			AnimationCurve shakeGreen, float remapGreenZero, float remapGreenOne,
			AnimationCurve shakeBlue, float remapBlueZero, float remapBlueOne,
			float duration, bool relativeValues = false,
			float attenuation = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, 
			bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false)
		{
			OnEvent?.Invoke(shakeRed, remapRedZero, remapRedOne,
				shakeGreen, remapGreenZero, remapGreenOne,
				shakeBlue, remapBlueZero, remapBlueOne,
				duration, relativeValues, attenuation, channelData, resetShakerValuesAfterShake, resetTargetValuesAfterShake, forwardDirection, timescaleMode, stop, restore);
		}
	}
}