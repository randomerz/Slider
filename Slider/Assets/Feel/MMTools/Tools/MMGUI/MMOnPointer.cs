using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace MoreMountains.Tools
{
	/// <summary>
	/// A simple helper class you can use to trigger methods on Unity's pointer events
	/// Typically used on a UI Image
	/// </summary>
	public class MMOnPointer : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerUpHandler, IPointerExitHandler, IPointerClickHandler
	{
		[Header("Pointer movement")]
		/// an event to trigger when the pointer enters the associated game object
		[Tooltip("an event to trigger when the pointer enters the associated game object")]
		public UnityEvent PointerEnter;
		/// an event to trigger when the pointer exits the associated game object
		[Tooltip("an event to trigger when the pointer exits the associated game object")]
		public UnityEvent PointerExit;
		
		[Header("Clicks")]
		/// an event to trigger when the pointer is pressed down on the associated game object
		[Tooltip("an event to trigger when the pointer is pressed down on the associated game object")]
		public UnityEvent PointerDown;
		/// an event to trigger when the pointer is pressed up on the associated game object
		[Tooltip("an event to trigger when the pointer is pressed up on the associated game object")]
		public UnityEvent PointerUp;
		/// an event to trigger when the pointer is clicked on the associated game object
		[Tooltip("an event to trigger when the pointer is clicked on the associated game object")]
		public UnityEvent PointerClick;
		
		/// <summary>
		/// IPointerEnterHandler implementation
		/// </summary>
		/// <param name="eventData"></param>
		public void OnPointerEnter(PointerEventData eventData)
		{
			PointerEnter?.Invoke();
		}

		/// <summary>
		/// IPointerExitHandler implementation
		/// </summary>
		/// <param name="eventData"></param>
		public void OnPointerExit(PointerEventData eventData)
		{
			PointerExit?.Invoke();
		}
		
		/// <summary>
		/// IPointerDownHandler implementation
		/// </summary>
		/// <param name="eventData"></param>
		public void OnPointerDown(PointerEventData eventData)
		{
			PointerDown?.Invoke();
		}

		/// <summary>
		/// IPointerUpHandler implementation
		/// </summary>
		/// <param name="eventData"></param>
		public void OnPointerUp(PointerEventData eventData)
		{
			PointerUp?.Invoke();
		}

		/// <summary>
		/// IPointerClickHandler implementation
		/// </summary>
		/// <param name="eventData"></param>
		public void OnPointerClick(PointerEventData eventData)
		{
			PointerClick?.Invoke();
		}
	}
}
