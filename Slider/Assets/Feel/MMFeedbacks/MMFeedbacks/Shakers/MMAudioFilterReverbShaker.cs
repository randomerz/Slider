using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// Add this to an audio reverb filter to shake its values remapped along a curve 
	/// </summary>
	[AddComponentMenu("More Mountains/Feedbacks/Shakers/Audio/MMAudioFilterReverbShaker")]
	[RequireComponent(typeof(AudioReverbFilter))]
	public class MMAudioFilterReverbShaker : MMShaker
	{
		[MMInspectorGroup("Reverb", true, 55)]
		/// whether or not to add to the initial value
		[Tooltip("whether or not to add to the initial value")]
		public bool RelativeReverb = false;
		/// the curve used to animate the intensity value on
		[Tooltip("the curve used to animate the intensity value on")]
		public AnimationCurve ShakeReverb = new AnimationCurve(new Keyframe(0, 0f), new Keyframe(0.5f, 1f), new Keyframe(1, 0f));
		/// the value to remap the curve's 0 to
		[Tooltip("the value to remap the curve's 0 to")]
		[Range(-10000f, 2000f)]
		public float RemapReverbZero = -10000f;
		/// the value to remap the curve's 1 to
		[Tooltip("the value to remap the curve's 1 to")]
		[Range(-10000f, 2000f)]
		public float RemapReverbOne = 2000f;

		/// the audio source to pilot
		protected AudioReverbFilter _targetAudioReverbFilter;
		protected float _initialReverb;
		protected float _originalShakeDuration;
		protected bool _originalRelativeReverb;
		protected AnimationCurve _originalShakeReverb;
		protected float _originalRemapReverbZero;
		protected float _originalRemapReverbOne;

		/// <summary>
		/// On init we initialize our values
		/// </summary>
		protected override void Initialization()
		{
			base.Initialization();
			_targetAudioReverbFilter = this.gameObject.GetComponent<AudioReverbFilter>();
		}

		/// <summary>
		/// When that shaker gets added, we initialize its shake duration
		/// </summary>
		protected virtual void Reset()
		{
			ShakeDuration = 2f;
		}

		/// <summary>
		/// Shakes values over time
		/// </summary>
		protected override void Shake()
		{
			float newReverbLevel = ShakeFloat(ShakeReverb, RemapReverbZero, RemapReverbOne, RelativeReverb, _initialReverb);
			_targetAudioReverbFilter.reverbLevel = newReverbLevel;
		}

		/// <summary>
		/// Collects initial values on the target
		/// </summary>
		protected override void GrabInitialValues()
		{
			_initialReverb = _targetAudioReverbFilter.reverbLevel;
		}

		/// <summary>
		/// When we get the appropriate event, we trigger a shake
		/// </summary>
		/// <param name="reverbCurve"></param>
		/// <param name="duration"></param>
		/// <param name="amplitude"></param>
		/// <param name="relativeReverb"></param>
		/// <param name="feedbacksIntensity"></param>
		/// <param name="channel"></param>
		public virtual void OnMMAudioFilterReverbShakeEvent(AnimationCurve reverbCurve, float duration, float remapMin, float remapMax, bool relativeReverb = false,
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
				_originalShakeReverb = ShakeReverb;
				_originalRemapReverbZero = RemapReverbZero;
				_originalRemapReverbOne = RemapReverbOne;
				_originalRelativeReverb = RelativeReverb;
			}
			
			if (restore)
			{
				ResetTargetValues();
				return;
			}

			if (!OnlyUseShakerValues)
			{
				TimescaleMode = timescaleMode;
				ShakeDuration = duration;
				ShakeReverb = reverbCurve;
				RemapReverbZero = remapMin * feedbacksIntensity;
				RemapReverbOne = remapMax * feedbacksIntensity;
				RelativeReverb = relativeReverb;
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
			_targetAudioReverbFilter.reverbLevel = _initialReverb;
		}

		/// <summary>
		/// Resets the shaker's values
		/// </summary>
		protected override void ResetShakerValues()
		{
			base.ResetShakerValues();
			ShakeDuration = _originalShakeDuration;
			ShakeReverb = _originalShakeReverb;
			RemapReverbZero = _originalRemapReverbZero;
			RemapReverbOne = _originalRemapReverbOne;
			RelativeReverb = _originalRelativeReverb;
		}

		/// <summary>
		/// Starts listening for events
		/// </summary>
		public override void StartListening()
		{
			base.StartListening();
			MMAudioFilterReverbShakeEvent.Register(OnMMAudioFilterReverbShakeEvent);
		}

		/// <summary>
		/// Stops listening for events
		/// </summary>
		public override void StopListening()
		{
			base.StopListening();
			MMAudioFilterReverbShakeEvent.Unregister(OnMMAudioFilterReverbShakeEvent);
		}
	}

	/// <summary>
	/// An event used to trigger vignette shakes
	/// </summary>
	public struct MMAudioFilterReverbShakeEvent
	{
		static private event Delegate OnEvent;
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void RuntimeInitialization() { OnEvent = null; }
		static public void Register(Delegate callback) { OnEvent += callback; }
		static public void Unregister(Delegate callback) { OnEvent -= callback; }

		public delegate void Delegate(AnimationCurve reverbCurve, float duration, float remapMin, float remapMax, bool relativeReverb = false,
			float feedbacksIntensity = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, 
			bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false);

		static public void Trigger(AnimationCurve reverbCurve, float duration, float remapMin, float remapMax, bool relativeReverb = false,
			float feedbacksIntensity = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, 
			bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false)
		{
			OnEvent?.Invoke(reverbCurve, duration, remapMin, remapMax, relativeReverb,
				feedbacksIntensity, channelData, resetShakerValuesAfterShake, resetTargetValuesAfterShake, forwardDirection, timescaleMode, stop, restore);
		}
	}
}