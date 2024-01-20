using UnityEngine;
using System.Collections;
using System;
using MoreMountains.Tools;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Events;

namespace MoreMountains.Tools
{	
	/// <summary>
	/// A component to handle switches 
	/// </summary>
	public class MMDebugMenuSwitch : MMTouchButton 
	{
		[Header("Switch")]
		/// a SpriteReplace to represent the switch knob
		public MMDebugMenuSpriteReplace SwitchKnob;
		/// the possible states of the switch   
		[MMReadOnly]
		public bool SwitchState;
		/// the state the switch should start in
		public bool InitialState = false;

		[Header("Binding")]
		/// the methods to call when the switch is turned on
		public UnityEvent OnSwitchOn;
		/// the methods to call when the switch is turned off
		public UnityEvent OnSwitchOff;
		/// the methods to call when the switch is turned off
		public UnityEvent<bool> OnSwitchToggle;

		/// <summary>
		/// On init, we set our current switch state
		/// </summary>
		protected override void Initialization()
		{
			base.Initialization ();
			SwitchState = InitialState;
			InitializeState ();

			SwitchKnob.Initialization();
			if (InitialState)
			{
				SwitchKnob.SwitchToOnSprite();
			}
			else
			{
				SwitchKnob.SwitchToOffSprite();
			}
		}

		public virtual void InitializeState()
		{
			/*if (CurrentSwitchState == SwitchStates.Left)
			{
				_animator?.Play ("RollLeft");
			}
			else
			{
				_animator?.Play ("RollRight");
			}*/
		}

		public virtual void SetTrue()
		{
			SwitchState = true;
			if (_animator != null)
			{
				_animator.SetTrigger("Right");
			}
			SwitchKnob.SwitchToOnSprite();
			if (OnSwitchOn != null)
			{
				OnSwitchOn.Invoke();
			}
		}

		public virtual void SetFalse()
		{
			SwitchState = false;
			if (_animator != null)
			{
				_animator.SetTrigger("Left");
			}
			SwitchKnob.SwitchToOffSprite();
			if (OnSwitchOff != null)
			{
				OnSwitchOff.Invoke();
			}
		}

		/// <summary>
		/// Use this method to go from one state to the other
		/// </summary>
		public virtual void ToggleState()
		{
			if (SwitchState == false)
			{
				SetTrue();
			}
			else
			{
				SetFalse();	
			}
			OnSwitchToggle?.Invoke(SwitchState);
		}		
	}
}