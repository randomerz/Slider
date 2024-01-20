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
	/// Add this class to a Camera with a URP vignette post processing and it'll be able to "shake" its values by getting events
	/// </summary>
	#if MM_URP
	[RequireComponent(typeof(Volume))]
	#endif
	[AddComponentMenu("More Mountains/Feedbacks/Shakers/PostProcessing/MMPaniniProjectionShaker_URP")]
	public class MMPaniniProjectionShaker_URP : MMShaker
	{
		[MMInspectorGroup("Distance", true, 62)]
		/// whether or not to add to the initial value
		[Tooltip("whether or not to add to the initial value")]
		public bool RelativeDistance = false;
		/// the curve used to animate the distance value on
		[Tooltip("the curve used to animate the distance value on")]
		public AnimationCurve ShakeDistance = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("the value to remap the curve's 0 to")]
		[Range(0f, 1f)]
		public float RemapDistanceZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("the value to remap the curve's 1 to")]
		[Range(0f, 1f)]
		public float RemapDistanceOne = 1f;

		#if MM_URP
		protected Volume _volume;
		protected PaniniProjection _paniniProjection;
		protected float _initialDistance;
		protected float _originalShakeDuration;
		protected AnimationCurve _originalShakeDistance;
		protected float _originalRemapDistanceZero;
		protected float _originalRemapDistanceOne;
		protected bool _originalRelativeDistance;

		/// <summary>
		/// On init we initialize our values
		/// </summary>
		protected override void Initialization()
		{
			base.Initialization();
			_volume = this.gameObject.GetComponent<Volume>();
			_volume.profile.TryGet(out _paniniProjection);
		}

		/// <summary>
		/// Shakes values over time
		/// </summary>
		protected override void Shake()
		{
			float newValue = ShakeFloat(ShakeDistance, RemapDistanceZero, RemapDistanceOne, RelativeDistance, _initialDistance);
			_paniniProjection.distance.Override(newValue);
		}

		/// <summary>
		/// Collects initial values on the target
		/// </summary>
		protected override void GrabInitialValues()
		{
			_initialDistance = _paniniProjection.distance.value;
		}

		/// <summary>
		/// When we get the appropriate event, we trigger a shake
		/// </summary>
		/// <param name="distance"></param>
		/// <param name="duration"></param>
		/// <param name="amplitude"></param>
		/// <param name="relativeDistance"></param>
		/// <param name="attenuation"></param>
		/// <param name="channel"></param>
		public virtual void OnPaniniProjectionShakeEvent(AnimationCurve distance, float duration, float remapMin, float remapMax, bool relativeDistance = false,
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
				_originalShakeDistance = ShakeDistance;
				_originalRemapDistanceZero = RemapDistanceZero;
				_originalRemapDistanceOne = RemapDistanceOne;
				_originalRelativeDistance = RelativeDistance;
			}

			if (!OnlyUseShakerValues)
			{
				TimescaleMode = timescaleMode;
				ShakeDuration = duration;
				ShakeDistance = distance;
				RemapDistanceZero = remapMin * attenuation;
				RemapDistanceOne = remapMax * attenuation;
				RelativeDistance = relativeDistance;
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
			_paniniProjection.distance.Override(_initialDistance);
		}

		/// <summary>
		/// Resets the shaker's values
		/// </summary>
		protected override void ResetShakerValues()
		{
			base.ResetShakerValues();
			ShakeDuration = _originalShakeDuration;
			ShakeDistance = _originalShakeDistance;
			RemapDistanceZero = _originalRemapDistanceZero;
			RemapDistanceOne = _originalRemapDistanceOne;
			RelativeDistance = _originalRelativeDistance;
		}

		/// <summary>
		/// Starts listening for events
		/// </summary>
		public override void StartListening()
		{
			base.StartListening();
			MMPaniniProjectionShakeEvent_URP.Register(OnPaniniProjectionShakeEvent);
		}

		/// <summary>
		/// Stops listening for events
		/// </summary>
		public override void StopListening()
		{
			base.StopListening();
			MMPaniniProjectionShakeEvent_URP.Unregister(OnPaniniProjectionShakeEvent);
		}
		#endif
	}

	/// <summary>
	/// An event used to trigger vignette shakes
	/// </summary>
	public struct MMPaniniProjectionShakeEvent_URP
	{
		static private event Delegate OnEvent;
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void RuntimeInitialization() { OnEvent = null; }
		static public void Register(Delegate callback) { OnEvent += callback; }
		static public void Unregister(Delegate callback) { OnEvent -= callback; }

		public delegate void Delegate(AnimationCurve distance, float duration, float remapMin, float remapMax, bool relativeDistance = false,
			float attenuation = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, 
			bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false);
		
		static public void Trigger(AnimationCurve distance, float duration, float remapMin, float remapMax, bool relativeDistance = false,
			float attenuation = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, 
			bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false)
		{
			OnEvent?.Invoke(distance, duration, remapMin, remapMax, relativeDistance, attenuation, channelData, 
				resetShakerValuesAfterShake, resetTargetValuesAfterShake, forwardDirection, timescaleMode, stop, restore);
		}
	}
}