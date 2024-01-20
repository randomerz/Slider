using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MoreMountains.Tools
{
	[Serializable]
	public class MMDSliderValueChangedEvent : UnityEvent<float> { }

	/// <summary>
	/// A class used to listen to slider events from a MMDebugMenu
	/// </summary>
	public class MMDebugMenuSliderEventListener : MonoBehaviour
	{
		[Header("Events")]
		/// the name of the slider event to listen to
		public string SliderEventName = "SliderEventName";
		/// an event fired when the slider's value changes
		public MMDSliderValueChangedEvent MMDValueChangedEvent;

		[Header("Test")]
		[Range(0f, 1f)]
		public float TestValue = 1f;
		[MMInspectorButton("TestSetValue")]
		public bool TestSetValueButton;

		/// <summary>
		/// This test methods will send a set event to all sliders bound to the SliderEventName
		/// </summary>
		protected virtual void TestSetValue()
		{
			MMDebugMenuSliderEvent.Trigger(SliderEventName, TestValue, MMDebugMenuSliderEvent.EventModes.SetSlider);
		}

		/// <summary>
		/// When we get a slider event, we trigger an event if needed 
		/// </summary>
		/// <param name="sliderEventName"></param>
		/// <param name="value"></param>
		protected virtual void OnMMDebugMenuSliderEvent(string sliderEventName, float value, MMDebugMenuSliderEvent.EventModes eventMode)
		{
			if ( (eventMode == MMDebugMenuSliderEvent.EventModes.FromSlider) 
			     && (sliderEventName == SliderEventName))
			{
				if (MMDValueChangedEvent != null)
				{
					MMDValueChangedEvent.Invoke(value);
				}
			}
		}

		/// <summary>
		/// Starts listening for events
		/// </summary>
		public virtual void OnEnable()
		{
			MMDebugMenuSliderEvent.Register(OnMMDebugMenuSliderEvent);
		}

		/// <summary>
		/// Stops listening for events
		/// </summary>
		public virtual void OnDisable()
		{
			MMDebugMenuSliderEvent.Unregister(OnMMDebugMenuSliderEvent);
		}
	}
}