using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

namespace MoreMountains.Tools
{	
	/// <summary>
	/// The possible directions a swipe can have
	/// </summary>
	public enum MMPossibleSwipeDirections { Up, Down, Left, Right }


	[System.Serializable]
	public class SwipeEvent : UnityEvent<MMSwipeEvent> {}

	/// <summary>
	/// An event usually triggered when a swipe happens. It contains the swipe "base" direction, and detailed information if needed (angle, length, origin and destination
	/// </summary>
	public struct MMSwipeEvent
	{
		public MMPossibleSwipeDirections SwipeDirection;
		public float SwipeAngle;
		public float SwipeLength;
		public Vector2 SwipeOrigin;
		public Vector2 SwipeDestination;
		public float SwipeDuration;

		/// <summary>
		/// Initializes a new instance of the <see cref="MoreMountains.Tools.MMSwipeEvent"/> struct.
		/// </summary>
		/// <param name="direction">Direction.</param>
		/// <param name="angle">Angle.</param>
		/// <param name="length">Length.</param>
		/// <param name="origin">Origin.</param>
		/// <param name="destination">Destination.</param>
		public MMSwipeEvent(MMPossibleSwipeDirections direction, float angle, float length, Vector2 origin, Vector2 destination, float swipeDuration)
		{
			SwipeDirection = direction;
			SwipeAngle = angle;
			SwipeLength = length;
			SwipeOrigin = origin;
			SwipeDestination = destination;
			SwipeDuration = swipeDuration;
		}

		static MMSwipeEvent e;
		public static void Trigger(MMPossibleSwipeDirections direction, float angle, float length, Vector2 origin, Vector2 destination, float swipeDuration)
		{
			e.SwipeDirection = direction;
			e.SwipeAngle = angle;
			e.SwipeLength = length;
			e.SwipeOrigin = origin;
			e.SwipeDestination = destination;
			e.SwipeDuration = swipeDuration;
			MMEventManager.TriggerEvent(e);
		}
	}

	/// <summary>
	/// Add a swipe manager to your scene, and it'll trigger MMSwipeEvents everytime a swipe happens. From its inspector you can determine the minimal length of a swipe. Shorter swipes will be ignored
	/// </summary>
	[RequireComponent(typeof(RectTransform))]
	[AddComponentMenu("More Mountains/Tools/Controls/MMSwipeZone")]
	public class MMSwipeZone : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IPointerEnterHandler
	{
		/// the minimal length of a swipe
		[Tooltip("the minimal length of a swipe")]
		public float MinimalSwipeLength = 50f;
		/// the maximum press length of a swipe
		[Tooltip("the maximum press length of a swipe")]
		public float MaximumPressLength = 10f;

		/// The method(s) to call when the zone is swiped
		[Tooltip("The method(s) to call when the zone is swiped")]
		public SwipeEvent ZoneSwiped;
		/// The method(s) to call while the zone is being pressed
		[Tooltip("The method(s) to call while the zone is being pressed")]
		public UnityEvent ZonePressed;

		[Header("Mouse Mode")]
		[MMInformation("If you set this to true, you'll need to actually press the button for it to be triggered, otherwise a simple hover will trigger it (better for touch input).", MMInformationAttribute.InformationType.Info,false)]
		/// If you set this to true, you'll need to actually press the button for it to be triggered, otherwise a simple hover will trigger it (better for touch input).
		[Tooltip("If you set this to true, you'll need to actually press the button for it to be triggered, otherwise a simple hover will trigger it (better for touch input).")]
		public bool MouseMode = false;

		protected Vector2 _firstTouchPosition;
		protected float _angle;
		protected float _length;
		protected Vector2 _destination;
		protected Vector2 _deltaSwipe;
		protected MMPossibleSwipeDirections _swipeDirection;
		protected float _lastPointerUpAt = 0f;
		protected float _swipeStartedAt = 0f;
		protected float _swipeEndedAt = 0f;

		/// <summary>
		/// Invokes a swipe event with the correct properties
		/// </summary>
		protected virtual void Swipe()
		{
			float duration = _swipeEndedAt - _swipeStartedAt;
			MMSwipeEvent swipeEvent = new MMSwipeEvent (_swipeDirection, _angle, _length, _firstTouchPosition, _destination, duration);
			MMEventManager.TriggerEvent(swipeEvent);
			if (ZoneSwiped != null)
			{
				ZoneSwiped.Invoke (swipeEvent);
			}
		}

		/// <summary>
		/// Raises the press event
		/// </summary>
		protected virtual void Press()
		{
			if (ZonePressed != null)
			{
				ZonePressed.Invoke ();
			}
		}

		/// <summary>
		/// Triggers the bound pointer down action
		/// </summary>
		public virtual void OnPointerDown(PointerEventData data)
		{
			#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
			_firstTouchPosition = Mouse.current.position.ReadValue();
			#else
			_firstTouchPosition = Input.mousePosition;
			#endif
			_swipeStartedAt = Time.unscaledTime;
		}

		/// <summary>
		/// Triggers the bound pointer up action
		/// </summary>
		public virtual void OnPointerUp(PointerEventData data)
		{
			if (Time.frameCount == _lastPointerUpAt)
			{
				return;
			}

			#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            _destination = Mouse.current.position.ReadValue();
			#else
			_destination = Input.mousePosition;
			#endif
			_deltaSwipe = _destination - _firstTouchPosition;
			_length = _deltaSwipe.magnitude;

			// if the swipe has been long enough
			if (_length > MinimalSwipeLength)
			{
				_angle = MMMaths.AngleBetween (_deltaSwipe, Vector2.right);
				_swipeDirection = AngleToSwipeDirection (_angle);
				_swipeEndedAt = Time.unscaledTime;
				Swipe ();
			}

			// if it's just a press
			if (_deltaSwipe.magnitude < MaximumPressLength)
			{
				Press ();
			}

			_lastPointerUpAt = Time.frameCount;
		}

		/// <summary>
		/// Triggers the bound pointer enter action when touch enters zone
		/// </summary>
		public virtual void OnPointerEnter(PointerEventData data)
		{
			if (!MouseMode)
			{
				OnPointerDown (data);
			}
		}

		/// <summary>
		/// Triggers the bound pointer exit action when touch is out of zone
		/// </summary>
		public virtual void OnPointerExit(PointerEventData data)
		{
			if (!MouseMode)
			{
				OnPointerUp(data);	
			}
		}

		/// <summary>
		/// Determines a MMPossibleSwipeDirection out of an angle in degrees. 
		/// </summary>
		/// <returns>The to swipe direction.</returns>
		/// <param name="angle">Angle in degrees.</param>
		protected virtual MMPossibleSwipeDirections AngleToSwipeDirection(float angle)
		{
			if ((angle < 45) || (angle >= 315))
			{
				return MMPossibleSwipeDirections.Right;
			}
			if ((angle >= 45) && (angle < 135))
			{
				return MMPossibleSwipeDirections.Up;
			}
			if ((angle >= 135) && (angle < 225))
			{
				return MMPossibleSwipeDirections.Left;
			}
			if ((angle >= 225) && (angle < 315))
			{
				return MMPossibleSwipeDirections.Down;
			}
			return MMPossibleSwipeDirections.Right;
		}
	}
}