using UnityEngine;
using System.Collections;

namespace MoreMountains.Tools
{
	/// <summary>
	/// An event used to move filters on and off a camera
	/// </summary>
	public struct MMPostProcessingMovingFilterEvent
	{
		static private event Delegate OnEvent;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void RuntimeInitialization()
		{
			OnEvent = null;
		}

		static public void Register(Delegate callback)
		{
			OnEvent += callback;
		}

		static public void Unregister(Delegate callback)
		{
			OnEvent -= callback;
		}

		public delegate void Delegate(MMTweenType curve, bool active, bool toggle, float duration, int channel = 0,
			bool stop = false, bool restore = false);

		static public void Trigger(MMTweenType curve, bool active, bool toggle, float duration, int channel = 0,
			bool stop = false, bool restore = false)
		{
			OnEvent?.Invoke(curve, active, toggle, duration, channel, stop, restore);
		}
	}

	/// <summary>
	/// 
	/// This class lets you create moving filters, very much like the old gelatin camera filters, that will move to connect to your camera
	/// Typically a moving filter should be made of a MMPostProcessingMovingFilter component, 
	/// a PostProcessing volume, and a BoxCollider (recommended size is 1,1,1 if you want to use the default offset)
	/// The filter will move on the y axis.
	/// 
	/// Use : 
	/// MMPostProcessingMovingFilterEvent.Trigger(MMTween.MMTweenCurve.EaseInOutCubic, TrueOrFalse, Duration, ChannelID);
	/// 
	/// </summary>
	[AddComponentMenu("More Mountains/Tools/Camera/MMPostProcessingMovingFilter")]
	public class MMPostProcessingMovingFilter : MonoBehaviour
	{
		public enum TimeScales
		{
			Unscaled,
			Scaled
		}

		[Header("Settings")]
		/// the channel ID for this filter. Any event with a different channel ID will be ignored
		public int Channel = 0;

		/// whether this should use scaled or unscaled time
		public TimeScales TimeScale = TimeScales.Unscaled;

		/// the curve to use for this movement
		public MMTweenType Curve = new MMTweenType(MMTween.MMTweenCurve.EaseInCubic);

		/// whether the filter is active at start or not
		public bool Active = false;

		[MMVector("On", "Off")]
		/// the vertical offsets to apply when the filter is on or off
		public Vector2 FilterOffset = new Vector2(0f, 5f);

		/// whether or not to add the initial position
		public bool AddToInitialPosition = true;

		[Header("Tests")]
		/// the duration to apply to the test methods
		public float TestDuration = 0.5f;

		/// a test button to toggle the filter on or off
		[MMInspectorButton("PostProcessingToggle")]
		public bool PostProcessingToggleButton;

		/// a test button to turn the filter off
		[MMInspectorButton("PostProcessingTriggerOff")]
		public bool PostProcessingTriggerOffButton;

		/// a test button to turn the filter on
		[MMInspectorButton("PostProcessingTriggerOn")]
		public bool PostProcessingTriggerOnButton;

		protected bool _lastReachedState = false;
		protected float _duration = 2f;
		protected float _lastMovementStartedAt = 0f;
		protected Vector3 _initialPosition;
		protected Vector3 _positionToRestore;
		protected Vector3 _newPosition;

		/// <summary>
		/// On Start we initialize our filter
		/// </summary>
		protected virtual void Start()
		{
			Initialization();
		}

		/// <summary>
		/// Sets the filter at the right initial position
		/// </summary>
		protected virtual void Initialization()
		{
			_lastMovementStartedAt = 0f;

			_positionToRestore = this.transform.localPosition;
			
			if (AddToInitialPosition)
			{
				_initialPosition = this.transform.localPosition;
			}
			else
			{
				_initialPosition = Vector3.zero;
			}

			_newPosition = _initialPosition;
			_newPosition.y = Active ? _initialPosition.y + FilterOffset.x : _initialPosition.y + FilterOffset.y;
			this.transform.localPosition = _newPosition;
			_lastReachedState = Active;
		}

		/// <summary>
		/// On update we move if needed
		/// </summary>
		protected virtual void Update()
		{
			// if we're already at destination, we do nothing and exit
			if (_lastReachedState == Active)
			{
				return;
			}

			MoveTowardsCurrentTarget();
		}

		/// <summary>
		/// Moves the filter towards its current target position
		/// </summary>
		protected virtual void MoveTowardsCurrentTarget()
		{
			if (_newPosition != this.transform.localPosition)
			{
				this.transform.localPosition = _newPosition;
			}

			float originY = Active ? _initialPosition.y + FilterOffset.y : _initialPosition.y + FilterOffset.x;
			float targetY = Active ? _initialPosition.y + FilterOffset.x : _initialPosition.y + FilterOffset.y;
			float currentTime = (TimeScale == TimeScales.Unscaled) ? Time.unscaledTime : Time.time;

			_newPosition = this.transform.localPosition;
			_newPosition.y =
				MMTween.Tween(currentTime - _lastMovementStartedAt, 0f, _duration, originY, targetY, Curve);

			if (currentTime - _lastMovementStartedAt > _duration)
			{
				_newPosition.y = targetY;
				this.transform.localPosition = _newPosition;
				_lastReachedState = Active;
			}
		}

		public virtual void RestoreInitialPosition()
		{
			this.transform.localPosition = _positionToRestore;
		}

		/// <summary>
		/// if we get a PostProcessingTriggerEvent
		/// </summary>
		/// <param name="curve"></param>
		/// <param name="active"></param>
		/// <param name="duration"></param>
		/// <param name="channel"></param>
		public virtual void OnMMPostProcessingMovingFilterEvent(MMTweenType curve, bool active, bool toggle,
			float duration, int channel = 0, bool stop = false, bool restore = false)
		{
			if ((channel != Channel) && (channel != -1) && (Channel != -1))
			{
				return;
			}

			if (stop)
			{
				_lastReachedState = Active;
				return;
			}

			if (restore)
			{
				RestoreInitialPosition();
				return;
			}

			Curve = curve;
			_duration = duration;

			if (toggle)
			{
				Active = !Active;
			}
			else
			{
				Active = active;
			}

			float currentTime = (TimeScale == TimeScales.Unscaled) ? Time.unscaledTime : Time.time;
			_lastMovementStartedAt = currentTime;
		}

		/// <summary>
		/// On enable, we start listening to MMPostProcessingTriggerEvents
		/// </summary>
		protected virtual void OnEnable()
		{
			MMPostProcessingMovingFilterEvent.Register(OnMMPostProcessingMovingFilterEvent);
		}

		/// <summary>
		/// On disable, we stop listening to MMPostProcessingTriggerEvents
		/// </summary>
		protected virtual void OnDisable()
		{
			MMPostProcessingMovingFilterEvent.Unregister(OnMMPostProcessingMovingFilterEvent);
		}

		// TEST METHODS --------------------------------------------------------------------------------------------------------------------

		/// <summary>
		/// Toggles the post processing effect on or off
		/// </summary>
		protected virtual void PostProcessingToggle()
		{
			MMPostProcessingMovingFilterEvent.Trigger(new MMTweenType(MMTween.MMTweenCurve.EaseInOutCubic), false, true,
				TestDuration, 0);
		}

		/// <summary>
		/// Turns the post processing effect off
		/// </summary>
		protected virtual void PostProcessingTriggerOff()
		{
			MMPostProcessingMovingFilterEvent.Trigger(new MMTweenType(MMTween.MMTweenCurve.EaseInOutCubic), false,
				false, TestDuration, 0);
		}

		/// <summary>
		/// Turns the post processing effect on
		/// </summary>
		protected virtual void PostProcessingTriggerOn()
		{
			MMPostProcessingMovingFilterEvent.Trigger(new MMTweenType(MMTween.MMTweenCurve.EaseInOutCubic), true, false,
				TestDuration, 0);
		}
	}
}