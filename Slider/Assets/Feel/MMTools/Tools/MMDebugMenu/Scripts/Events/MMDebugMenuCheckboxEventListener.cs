using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MoreMountains.Tools
{
	[Serializable]
	public class MMDCheckboxPressedEvent : UnityEvent<bool> { }
	[Serializable]
	public class MMDCheckboxTrueEvent : UnityEvent { }
	[Serializable]
	public class MMDCheckboxFalseEvent : UnityEvent { }

	/// <summary>
	/// A class used to listen to events from a MMDebugMenu's checkbox
	/// </summary>
	public class MMDebugMenuCheckboxEventListener : MonoBehaviour
	{
		[Header("Events")]
		/// the name of the event to listen to
		public string CheckboxEventName = "CheckboxEventName";
		/// an event fired when the checkbox gets pressed
		public MMDCheckboxPressedEvent MMDPressedEvent;
		/// an event fired when the checkbox is pressed and becomes true/checked
		public MMDCheckboxTrueEvent MMDTrueEvent;
		/// an event fired when the checkbox is pressed and becomes false/unchecked
		public MMDCheckboxFalseEvent MMDFalseEvent;

		[Header("Test")]
		public bool TestValue = true;
		[MMInspectorButton("TestSetValue")]
		public bool TestSetValueButton;

		/// <summary>
		/// This test methods will send a set event to all checkboxes bound to the CheckboxEventName
		/// </summary>
		protected virtual void TestSetValue()
		{
			MMDebugMenuCheckboxEvent.Trigger(CheckboxEventName, TestValue, MMDebugMenuCheckboxEvent.EventModes.SetCheckbox);
		}

		/// <summary>
		/// When get a checkbox event, we invoke our events if needed
		/// </summary>
		/// <param name="checkboxNameEvent"></param>
		/// <param name="value"></param>
		protected virtual void OnMMDebugMenuCheckboxEvent(string checkboxNameEvent, bool value, MMDebugMenuCheckboxEvent.EventModes eventMode)
		{
			if ((eventMode == MMDebugMenuCheckboxEvent.EventModes.FromCheckbox) && (checkboxNameEvent == CheckboxEventName))
			{
				if (MMDPressedEvent != null)
				{
					MMDPressedEvent.Invoke(value);
				}

				if (value)
				{
					if (MMDTrueEvent != null)
					{
						MMDTrueEvent.Invoke();
					}
				}
				else
				{
					if (MMDFalseEvent != null)
					{
						MMDFalseEvent.Invoke();
					}
				}
			}
		}

		/// <summary>
		/// Starts listening for events
		/// </summary>
		public virtual void OnEnable()
		{
			MMDebugMenuCheckboxEvent.Register(OnMMDebugMenuCheckboxEvent);
		}

		/// <summary>
		/// Stops listening for events
		/// </summary>
		public virtual void OnDisable()
		{
			MMDebugMenuCheckboxEvent.Unregister(OnMMDebugMenuCheckboxEvent);
		}
	}
}