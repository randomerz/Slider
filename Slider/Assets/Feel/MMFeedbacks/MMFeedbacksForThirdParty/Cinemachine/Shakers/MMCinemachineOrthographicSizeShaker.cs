using UnityEngine;
#if MM_CINEMACHINE
using Cinemachine;
#endif
using MoreMountains.Feedbacks;
using MoreMountains.Tools;

namespace MoreMountains.FeedbacksForThirdParty
{
	/// <summary>
	/// Add this to a Cinemachine virtual camera and it'll let you control its orthographic size over time, can be piloted by a MMFeedbackCameraOrthographicSize
	/// </summary>
	[AddComponentMenu("More Mountains/Feedbacks/Shakers/Cinemachine/MMCinemachineOrthographicSizeShaker")]
	#if MM_CINEMACHINE
	[RequireComponent(typeof(CinemachineVirtualCamera))]
	#endif
	public class MMCinemachineOrthographicSizeShaker : MMShaker
	{
		[MMInspectorGroup("Orthographic Size", true, 43)]
		/// whether or not to add to the initial value
		[Tooltip("whether or not to add to the initial value")]
		public bool RelativeOrthographicSize = false;
		/// the curve used to animate the intensity value on
		[Tooltip("the curve used to animate the intensity value on")]
		public AnimationCurve ShakeOrthographicSize = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("the value to remap the curve's 0 to")]
		public float RemapOrthographicSizeZero = 5f;
		/// the value to remap the curve's 1 to
		[Tooltip("the value to remap the curve's 1 to")]
		public float RemapOrthographicSizeOne = 10f;

		#if MM_CINEMACHINE
		protected CinemachineVirtualCamera _targetCamera;
		protected float _initialOrthographicSize;
		protected float _originalShakeDuration;
		protected bool _originalRelativeOrthographicSize;
		protected AnimationCurve _originalShakeOrthographicSize;
		protected float _originalRemapOrthographicSizeZero;
		protected float _originalRemapOrthographicSizeOne;

		/// <summary>
		/// On init we initialize our values
		/// </summary>
		protected override void Initialization()
		{
			base.Initialization();
			_targetCamera = this.gameObject.GetComponent<CinemachineVirtualCamera>();
		}

		/// <summary>
		/// When that shaker gets added, we initialize its shake duration
		/// </summary>
		protected virtual void Reset()
		{
			ShakeDuration = 0.5f;
		}

		/// <summary>
		/// Shakes values over time
		/// </summary>
		protected override void Shake()
		{
			float newOrthographicSize = ShakeFloat(ShakeOrthographicSize, RemapOrthographicSizeZero, RemapOrthographicSizeOne, RelativeOrthographicSize, _initialOrthographicSize);
			_targetCamera.m_Lens.OrthographicSize = newOrthographicSize;
		}

		/// <summary>
		/// Collects initial values on the target
		/// </summary>
		protected override void GrabInitialValues()
		{
			_initialOrthographicSize = _targetCamera.m_Lens.OrthographicSize;
		}

		/// <summary>
		/// When we get the appropriate event, we trigger a shake
		/// </summary>
		/// <param name="distortionCurve"></param>
		/// <param name="duration"></param>
		/// <param name="amplitude"></param>
		/// <param name="relativeDistortion"></param>
		/// <param name="feedbacksIntensity"></param>
		/// <param name="channel"></param>
		public virtual void OnMMCameraOrthographicSizeShakeEvent(AnimationCurve distortionCurve, float duration, float remapMin, float remapMax, bool relativeDistortion = false,
			float feedbacksIntensity = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, bool forwardDirection = true, 
			TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false)
		{
			if (!CheckEventAllowed(channelData))
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
            
			if (!Interruptible && Shaking)
			{
				return;
			}
            
			_resetShakerValuesAfterShake = resetShakerValuesAfterShake;
			_resetTargetValuesAfterShake = resetTargetValuesAfterShake;

			if (resetShakerValuesAfterShake)
			{
				_originalShakeDuration = ShakeDuration;
				_originalShakeOrthographicSize = ShakeOrthographicSize;
				_originalRemapOrthographicSizeZero = RemapOrthographicSizeZero;
				_originalRemapOrthographicSizeOne = RemapOrthographicSizeOne;
				_originalRelativeOrthographicSize = RelativeOrthographicSize;
			}

			if (!OnlyUseShakerValues)
			{
				TimescaleMode = timescaleMode;
				ShakeDuration = duration;
				ShakeOrthographicSize = distortionCurve;
				RemapOrthographicSizeZero = remapMin * feedbacksIntensity;
				RemapOrthographicSizeOne = remapMax * feedbacksIntensity;
				RelativeOrthographicSize = relativeDistortion;
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
			_targetCamera.m_Lens.OrthographicSize = _initialOrthographicSize;
		}

		/// <summary>
		/// Resets the shaker's values
		/// </summary>
		protected override void ResetShakerValues()
		{
			base.ResetShakerValues();
			ShakeDuration = _originalShakeDuration;
			ShakeOrthographicSize = _originalShakeOrthographicSize;
			RemapOrthographicSizeZero = _originalRemapOrthographicSizeZero;
			RemapOrthographicSizeOne = _originalRemapOrthographicSizeOne;
			RelativeOrthographicSize = _originalRelativeOrthographicSize;
		}

		/// <summary>
		/// Starts listening for events
		/// </summary>
		public override void StartListening()
		{
			base.StartListening();
			MMCameraOrthographicSizeShakeEvent.Register(OnMMCameraOrthographicSizeShakeEvent);
		}

		/// <summary>
		/// Stops listening for events
		/// </summary>
		public override void StopListening()
		{
			base.StopListening();
			MMCameraOrthographicSizeShakeEvent.Unregister(OnMMCameraOrthographicSizeShakeEvent);
		}
		#endif
	}
}