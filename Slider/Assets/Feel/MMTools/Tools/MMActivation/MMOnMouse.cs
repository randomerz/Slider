using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MoreMountains.Tools
{
	/// <summary>
	/// Attach this class to a collider and it'll let you trigger events when the user clicks/drags/enters/etc that collider
	/// </summary>
	public class MMOnMouse : MonoBehaviour
	{
		/// OnMouseDown is called when the user has pressed the mouse button while over the Collider.
		[Tooltip("OnMouseDown is called when the user has pressed the mouse button while over the Collider.")]
		public UnityEvent OnMouseDownEvent;
		/// OnMouseDrag is called when the user has clicked on a Collider and is still holding down the mouse.
		[Tooltip("OnMouseDrag is called when the user has clicked on a Collider and is still holding down the mouse.")]
		public UnityEvent OnMouseDragEvent;
		/// Called when the mouse enters the Collider.
		[Tooltip("Called when the mouse enters the Collider.")]
		public UnityEvent OnMouseEnterEvent;
		/// Called when the mouse is not any longer over the Collider.
		[Tooltip("Called when the mouse is not any longer over the Collider.")]
		public UnityEvent OnMouseExitEvent;
		/// Called every frame while the mouse is over the Collider.
		[Tooltip("Called every frame while the mouse is over the Collider.")]
		public UnityEvent OnMouseOverEvent;
		/// OnMouseUp is called when the user has released the mouse button.
		[Tooltip("OnMouseUp is called when the user has released the mouse button.")]
		public UnityEvent OnMouseUpEvent;
		/// OnMouseUpAsButton is only called when the mouse is released over the same Collider as it was pressed.
		[Tooltip("OnMouseUpAsButton is only called when the mouse is released over the same Collider as it was pressed.")]
		public UnityEvent OnMouseUpAsButtonEvent;

		protected virtual void OnMouseDown()
		{
			OnMouseDownEvent.Invoke();
		}
		
		protected virtual void OnMouseDrag()
		{
			OnMouseDragEvent.Invoke();
		}
		
		protected virtual void OnMouseEnter()
		{
			OnMouseEnterEvent.Invoke();
		}
		
		protected virtual void OnMouseExit()
		{
			OnMouseExitEvent.Invoke();
		}
		
		protected virtual void OnMouseOver()
		{
			OnMouseOverEvent.Invoke();
		}
		
		protected virtual void OnMouseUp()
		{
			OnMouseUpEvent.Invoke();
		}
		
		protected virtual void OnMouseUpAsButton()
		{
			OnMouseUpAsButtonEvent.Invoke();
		}
		
	}	
}