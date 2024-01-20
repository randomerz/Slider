using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
#if MM_URP
using UnityEngine.Rendering.Universal;
#endif
using MoreMountains.Feedbacks;
using MoreMountains.Tools;

namespace MoreMountains.FeedbacksForThirdParty
{
	/// <summary>
	/// Add this class to a Camera with a white balance post processing and it'll be able to "shake" its values by getting events
	/// </summary>
	#if MM_URP
	[RequireComponent(typeof(Volume))]
	#endif
	[AddComponentMenu("More Mountains/Feedbacks/Shakers/PostProcessing/MMWhiteBalanceShaker_URP")]
	public class MMWhiteBalanceShaker_URP : MMShaker
	{
		/// whether or not to add to the initial value
		[Tooltip("whether or not to add to the initial value")]
		public bool RelativeValues = true;

		[MMInspectorGroup("Temperature", true, 55)]
		/// the curve used to animate the temperature value on
		[Tooltip("the curve used to animate the temperature value on")]
		public AnimationCurve ShakeTemperature = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("the value to remap the curve's 0 to")]
		[Range(-100f, 100f)]
		public float RemapTemperatureZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("the value to remap the curve's 1 to")]
		[Range(-100f, 100f)]
		public float RemapTemperatureOne = 100f;

		[MMInspectorGroup("Tint", true, 56)]
		/// the curve used to animate the tint value on
		[Tooltip("the curve used to animate the tint value on")]
		public AnimationCurve ShakeTint = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("the value to remap the curve's 0 to")]
		[Range(-100f, 100f)]
		public float RemapTintZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("the value to remap the curve's 1 to")]
		[Range(-100f, 100f)]
		public float RemapTintOne = 100f;

		#if MM_URP
		protected Volume _volume;
		protected WhiteBalance _whiteBalance;
		protected float _initialTemperature;
		protected float _initialTint;
		protected float _originalShakeDuration;
		protected bool _originalRelativeValues;
		protected AnimationCurve _originalShakeTemperature;
		protected float _originalRemapTemperatureZero;
		protected float _originalRemapTemperatureOne;
		protected AnimationCurve _originalShakeTint;
		protected float _originalRemapTintZero;
		protected float _originalRemapTintOne;

		/// <summary>
		/// On init we initialize our values
		/// </summary>
		protected override void Initialization()
		{
			base.Initialization();
			_volume = this.gameObject.GetComponent<Volume>();
			_volume.profile.TryGet(out _whiteBalance);
		}

		/// <summary>
		/// Shakes values over time
		/// </summary>
		protected override void Shake()
		{
			float newTemperature = ShakeFloat(ShakeTemperature, RemapTemperatureZero, RemapTemperatureOne, RelativeValues, _initialTemperature);
			_whiteBalance.temperature.Override(newTemperature);
			float newTint = ShakeFloat(ShakeTint, RemapTintZero, RemapTintOne, RelativeValues, _initialTint);
			_whiteBalance.tint.Override(newTint);
		}

		/// <summary>
		/// Collects initial values on the target
		/// </summary>
		protected override void GrabInitialValues()
		{
			_initialTemperature = _whiteBalance.temperature.value;
			_initialTint = _whiteBalance.tint.value;
		}

		/// <summary>
		/// When we get the appropriate event, we trigger a shake
		/// </summary>
		/// <param name="temperature"></param>
		/// <param name="duration"></param>
		/// <param name="amplitude"></param>
		/// <param name="relativeValues"></param>
		/// <param name="attenuation"></param>
		/// <param name="channel"></param>
		public virtual void OnWhiteBalanceShakeEvent(AnimationCurve temperature, float duration, float remapTemperatureMin, float remapTemperatureMax,
			AnimationCurve tint, float remapTintMin, float remapTintMax, bool relativeValues = false,
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
				_originalShakeTemperature = ShakeTemperature;
				_originalRemapTemperatureZero = RemapTemperatureZero;
				_originalRemapTemperatureOne = RemapTemperatureOne;
				_originalRelativeValues = RelativeValues;
				_originalShakeTint = ShakeTint;
				_originalRemapTintZero = RemapTintZero;
				_originalRemapTintOne = RemapTintOne;
			}

			if (!OnlyUseShakerValues)
			{
				TimescaleMode = timescaleMode;
				ShakeDuration = duration;
				ShakeTemperature = temperature;
				RemapTemperatureZero = remapTemperatureMin * attenuation;
				RemapTemperatureOne = remapTemperatureMax * attenuation;
				RelativeValues = relativeValues;
				ShakeTint = tint;
				RemapTintZero = remapTintMin;
				RemapTintOne = remapTintMax;
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
			_whiteBalance.temperature.Override(_initialTemperature);
			_whiteBalance.tint.Override(_initialTint);
		}

		/// <summary>
		/// Resets the shaker's values
		/// </summary>
		protected override void ResetShakerValues()
		{
			base.ResetShakerValues();
			ShakeDuration = _originalShakeDuration;
			ShakeTemperature = _originalShakeTemperature;
			RemapTemperatureZero = _originalRemapTemperatureZero;
			RemapTemperatureOne = _originalRemapTemperatureOne;
			RelativeValues = _originalRelativeValues;
			ShakeTint = _originalShakeTint;
			RemapTintZero = _originalRemapTintZero;
			RemapTintOne = _originalRemapTintOne;
		}

		/// <summary>
		/// Starts listening for events
		/// </summary>
		public override void StartListening()
		{
			base.StartListening();
			MMWhiteBalanceShakeEvent_URP.Register(OnWhiteBalanceShakeEvent);
		}

		/// <summary>
		/// Stops listening for events
		/// </summary>
		public override void StopListening()
		{
			base.StopListening();
			MMWhiteBalanceShakeEvent_URP.Unregister(OnWhiteBalanceShakeEvent);
		}
		#endif
	}

	/// <summary>
	/// An event used to trigger vignette shakes
	/// </summary>
	public struct MMWhiteBalanceShakeEvent_URP
	{
		static private event Delegate OnEvent;
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void RuntimeInitialization() { OnEvent = null; }
		static public void Register(Delegate callback) { OnEvent += callback; }
		static public void Unregister(Delegate callback) { OnEvent -= callback; }

		public delegate void Delegate(AnimationCurve temperature, float duration, float remapTemperatureMin, float remapTemperatureMax,
			AnimationCurve tint, float remapTintMin, float remapTintMax, bool relativeValues = false,
			float attenuation = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, 
			bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false);

		static public void Trigger(AnimationCurve temperature, float duration, float remapTemperatureMin, float remapTemperatureMax,
			AnimationCurve tint, float remapTintMin, float remapTintMax, bool relativeValues = false,
			float attenuation = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, 
			bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false)
		{
			OnEvent?.Invoke(temperature, duration, remapTemperatureMin, remapTemperatureMax,
				tint, remapTintMin, remapTintMax, relativeValues,
				attenuation, channelData, resetShakerValuesAfterShake, resetTargetValuesAfterShake, forwardDirection, timescaleMode, stop, restore);
		}
	}
}