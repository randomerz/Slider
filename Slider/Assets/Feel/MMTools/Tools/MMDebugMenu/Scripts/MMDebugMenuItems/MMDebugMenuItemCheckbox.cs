using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MoreMountains.Tools
{
	/// <summary>
	/// A class used to bind a checkbox to a MMDebugMenu
	/// </summary>
	public class MMDebugMenuItemCheckbox : MonoBehaviour
	{
		[Header("Bindings")]
		/// the switch used to display the checkbox
		public MMDebugMenuSwitch Switch;
		/// the text used to display the checkbox's text
		public Text SwitchText;
		/// the name of the checkbox event
		public string CheckboxEventName = "Checkbox";

		protected bool _valueSetThisFrame = false;
		protected bool _listening = false;

		/// <summary>
		/// Triggers an event when the checkbox gets pressed
		/// </summary>
		public virtual void TriggerCheckboxEvent()
		{
			if (_valueSetThisFrame)
			{
				_valueSetThisFrame = false;
				return;
			}
			MMDebugMenuCheckboxEvent.Trigger(CheckboxEventName, Switch.SwitchState, MMDebugMenuCheckboxEvent.EventModes.FromCheckbox);
		}

		/// <summary>
		/// Triggers an event when the checkbox gets checked and becomes true
		/// </summary>
		public virtual void TriggerCheckboxEventTrue()
		{
			if (_valueSetThisFrame)
			{
				_valueSetThisFrame = false;
				return;
			}
			MMDebugMenuCheckboxEvent.Trigger(CheckboxEventName, true, MMDebugMenuCheckboxEvent.EventModes.FromCheckbox);
		}

		/// <summary>
		/// Triggers an event when the checkbox gets unchecked and becomes false
		/// </summary>
		public virtual void TriggerCheckboxEventFalse()
		{
			if (_valueSetThisFrame)
			{
				_valueSetThisFrame = false;
				return;
			}
			MMDebugMenuCheckboxEvent.Trigger(CheckboxEventName, false, MMDebugMenuCheckboxEvent.EventModes.FromCheckbox);
		}

		protected virtual void OnMMDebugMenuCheckboxEvent(string checkboxEventName, bool value, MMDebugMenuCheckboxEvent.EventModes eventMode)
		{
			if ((eventMode == MMDebugMenuCheckboxEvent.EventModes.SetCheckbox)
			    && (checkboxEventName == CheckboxEventName))
			{
				_valueSetThisFrame = true;
				if (value)
				{
					Switch.SetTrue();
				}
				else
				{
					Switch.SetFalse();
				}
			}
		}

		/// <summary>
		/// Starts listening for events
		/// </summary>
		public virtual void OnEnable()
		{
			if (!_listening)
			{
				_listening = true;
				MMDebugMenuCheckboxEvent.Register(OnMMDebugMenuCheckboxEvent);
			}            
		}

		/// <summary>
		/// Stops listening for events
		/// </summary>
		public virtual void OnDestroy()
		{
			_listening = false;
			MMDebugMenuCheckboxEvent.Unregister(OnMMDebugMenuCheckboxEvent);
		}
	}
}