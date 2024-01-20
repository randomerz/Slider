using UnityEngine;
using UnityEngine.EventSystems;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MoreMountains.Tools
{
	/// <summary>
	/// Add this component to a UI rectangle and it'll act as a detection zone for a joystick.
	/// Note that this component extends the MMTouchJoystick class so you don't need to add another joystick to it. It's both the detection zone and the stick itself.
	/// </summary>
	[AddComponentMenu("More Mountains/Tools/Controls/MMTouchRepositionableJoystick")]
	public class MMTouchRepositionableJoystick : MMTouchJoystick, IPointerDownHandler
	{
		[MMInspectorGroup("Repositionable Joystick", true, 22)]
		/// the canvas group to use as the joystick's knob
		[Tooltip("the canvas group to use as the joystick's knob")]
		public CanvasGroup KnobCanvasGroup;
		/// the canvas group to use as the joystick's background
		[Tooltip("the canvas group to use as the joystick's background")]
		public CanvasGroup BackgroundCanvasGroup;
		/// if this is true, the joystick won't be able to travel beyond the bounds of the top level canvas
		[Tooltip("if this is true, the joystick won't be able to travel beyond the bounds of the top level canvas")]
		public bool ConstrainToInitialRectangle = true;
		/// if this is true, the joystick will return back to its initial position when released
		[Tooltip("if this is true, the joystick will return back to its initial position when released")]
		public bool ResetPositionToInitialOnRelease = false;

		protected Vector3 _initialPosition;
		protected Vector3 _newPosition;
		protected CanvasGroup _knobCanvasGroup;
		protected RectTransform _rectTransform;

		/// <summary>
		/// On Start, we instantiate our joystick's image if there's one
		/// </summary>
		protected override void Start()
		{
			base.Start();

			// we store the detection zone's initial position
			_rectTransform = GetComponent<RectTransform>();
			_initialPosition = BackgroundCanvasGroup.GetComponent<RectTransform>().position;
		}

		/// <summary>
		/// On init we set our knob transform
		/// </summary>
		public override void Initialize()
		{
			base.Initialize();
			SetKnobTransform(KnobCanvasGroup.transform);
			_canvasGroup = KnobCanvasGroup;
			_initialOpacity = _canvasGroup.alpha;
		}

		/// <summary>
		/// When the zone is pressed, we move our joystick accordingly
		/// </summary>
		/// <param name="data">Data.</param>
		public override void OnPointerDown(PointerEventData data)
		{
			base.OnPointerDown(data);
			
			// if we're in "screen space - camera" render mode
			if (ParentCanvasRenderMode == RenderMode.ScreenSpaceCamera)
			{
				_newPosition = TargetCamera.ScreenToWorldPoint(data.position);
			}
			// otherwise
			else
			{
				_newPosition = data.position;
			}
			_newPosition.z = this.transform.position.z;
			
			if (!WithinBounds())
			{
				return;
			}

			// we define a new neutral position
			BackgroundCanvasGroup.transform.position = _newPosition;
			SetNeutralPosition(_newPosition);
			_knobTransform.position = _newPosition;
		}

		/// <summary>
		/// Returns true if the joystick's new position is within the bounds of the top level canvas
		/// </summary>
		/// <returns></returns>
		protected virtual bool WithinBounds()
		{
			if (!ConstrainToInitialRectangle)
			{
				return true;
			}
			return RectTransformUtility.RectangleContainsScreenPoint(_rectTransform, _newPosition);
		}

		/// <summary>
		/// When the player lets go of the stick, we restore our stick's position if needed
		/// </summary>
		/// <param name="eventData">Event data.</param>
		public override void OnPointerUp(PointerEventData eventData)
		{
			base.OnPointerUp(eventData);

			if (ResetPositionToInitialOnRelease)
			{
				BackgroundCanvasGroup.transform.position = _initialPosition;
				_knobTransform.position = _initialPosition;
			}
		}
		
		
		#if UNITY_EDITOR
		/// <summary>
		/// Draws gizmos if needed
		/// </summary>
		protected override void OnDrawGizmos()
		{
			if (!DrawGizmos)
			{
				return;
			}

			Handles.color = MMColors.Orange;
			if (!Application.isPlaying)
			{
				if (KnobCanvasGroup != null)
				{
					Handles.DrawWireDisc(KnobCanvasGroup.transform.position, Vector3.forward, ComputedMaxRange);	
				}
				else
				{
					Handles.DrawWireDisc(this.transform.position, Vector3.forward, ComputedMaxRange);	
				}
			}
			else
			{
				Handles.DrawWireDisc(_neutralPosition, Vector3.forward, ComputedMaxRange);
			}
		}
		#endif
	}
}