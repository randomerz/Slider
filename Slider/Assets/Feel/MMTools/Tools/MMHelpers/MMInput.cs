using UnityEngine;
using System.Collections;

namespace MoreMountains.Tools
{	
	/// <summary>
	/// Input helpers
	/// </summary>

	public class MMInput : MonoBehaviour 
	{
		/// <summary>
		/// All possible states for a button. Can be used in a state machine.
		/// </summary>
		public enum ButtonStates { Off, ButtonDown, ButtonPressed, ButtonUp }

		public enum AxisTypes { Positive, Negative }

		/// <summary>
		/// Takes an axis and returns a ButtonState depending on whether the axis is pressed or not (useful for xbox triggers for example), and when you need to use an axis/trigger as a binary thing
		/// </summary>
		/// <returns>The axis as button.</returns>
		/// <param name="axisName">Axis name.</param>
		/// <param name="threshold">Threshold value below which the button is off or released.</param>
		/// <param name="currentState">Current state of the axis.</param>
		public static ButtonStates ProcessAxisAsButton (string axisName, float threshold, ButtonStates currentState, AxisTypes AxisType = AxisTypes.Positive)
		{
			float axisValue = Input.GetAxis (axisName);
			ButtonStates returnState;

			bool comparison = (AxisType == AxisTypes.Positive) ? (axisValue < threshold) : (axisValue > threshold);
			
			if (comparison)
			{
				if (currentState == ButtonStates.ButtonPressed)
				{
					returnState = ButtonStates.ButtonUp;
				}
				else
				{
					returnState = ButtonStates.Off;
				}
			}
			else
			{
				if (currentState == ButtonStates.Off)
				{
					returnState = ButtonStates.ButtonDown;
				}
				else
				{
					returnState = ButtonStates.ButtonPressed;
				}
			}
			return returnState;
		}

		/// <summary>
		/// IM button, short for InputManager button, a class used to handle button states, whether mobile or actual keys
		/// </summary>
		public class IMButton
		{
			/// a state machine used to store button states
			public MMStateMachine<MMInput.ButtonStates> State {get;protected set;}
			/// the unique ID of this button
			public string ButtonID;

			public delegate void ButtonDownMethodDelegate();
			public delegate void ButtonPressedMethodDelegate();
			public delegate void ButtonUpMethodDelegate();

			public ButtonDownMethodDelegate ButtonDownMethod;
			public ButtonPressedMethodDelegate ButtonPressedMethod;
			public ButtonUpMethodDelegate ButtonUpMethod;

			/// returns the time (in unscaled seconds) since the last time the button was pressed down
			public float TimeSinceLastButtonDown { get { return Time.unscaledTime - _lastButtonDownAt; } }
			/// returns the time (in unscaled seconds) since the last time the button was released
			public float TimeSinceLastButtonUp { get { return Time.unscaledTime - _lastButtonUpAt; } }
			/// returns true if this button was pressed down within the time (in unscaled seconds) passed in parameters
			public bool ButtonDownRecently(float time) { return (TimeSinceLastButtonDown <= time); }
			/// returns true if this button was released within the time (in unscaled seconds) passed in parameters
			public bool ButtonUpRecently(float time) { return (TimeSinceLastButtonUp <= time); }

			protected float _lastButtonDownAt;
			protected float _lastButtonUpAt;

			/// <summary>
			/// Constructor
			/// </summary>
			/// <param name="playerID"></param>
			/// <param name="buttonID"></param>
			/// <param name="btnDown"></param>
			/// <param name="btnPressed"></param>
			/// <param name="btnUp"></param>
			public IMButton(string playerID, string buttonID, ButtonDownMethodDelegate btnDown = null, ButtonPressedMethodDelegate btnPressed = null, ButtonUpMethodDelegate btnUp = null) 
			{
				ButtonID = playerID + "_" + buttonID;
				ButtonDownMethod = btnDown;
				ButtonUpMethod = btnUp;
				ButtonPressedMethod = btnPressed;
				State = new MMStateMachine<MMInput.ButtonStates> (null, false);
				State.ChangeState (MMInput.ButtonStates.Off);
			}
			
			/// <summary>
			/// Returns true if the button is currently pressed
			/// </summary>
			public virtual bool IsPressed => State.CurrentState == MMInput.ButtonStates.ButtonPressed;
			
			/// <summary>
			/// Returns true if the button is down this frame 
			/// </summary>
			public virtual bool IsDown => State.CurrentState == MMInput.ButtonStates.ButtonDown;
			
			/// <summary>
			/// Returns true if the button is up this frame
			/// </summary>
			public virtual bool IsUp => State.CurrentState == MMInput.ButtonStates.ButtonUp;
			
			/// <summary>
			/// Returns true if the button is neither pressed, down or up this frame
			/// </summary>
			public virtual bool IsOff => State.CurrentState == MMInput.ButtonStates.Off;

			/// <summary>
			/// Presses the button for the first time, putting it in ButtonDown state
			/// </summary>
			public virtual void TriggerButtonDown()
			{
				_lastButtonDownAt = Time.unscaledTime;
				if (ButtonDownMethod == null)
				{
					State.ChangeState(MMInput.ButtonStates.ButtonDown);
				}
				else
				{
					ButtonDownMethod();
				}
			}

			/// <summary>
			/// Puts the button in the Pressed state, potentially bypassing the Down state
			/// </summary>
			public virtual void TriggerButtonPressed()
			{
				if (ButtonPressedMethod == null)
				{
					State.ChangeState(MMInput.ButtonStates.ButtonPressed);
				}
				else
				{
					ButtonPressedMethod();
				}
			}

			/// <summary>
			/// Puts the button in the Up state
			/// </summary>
			public virtual void TriggerButtonUp()
			{
				_lastButtonUpAt = Time.unscaledTime;
				if (ButtonUpMethod == null)
				{
					State.ChangeState(MMInput.ButtonStates.ButtonUp);
				}
				else
				{
					ButtonUpMethod();
				}
			}
		}
	}


}