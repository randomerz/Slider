using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
	/// <summary>
	/// This component lets you very easily have one property drive the value of another property.
	/// To do so, drag the object with the property you want to "read" from into the Emitter Property slot, then select the component the property is on, and finally the property itself.
	/// Then drag the object with the property you want to "write" to into the ReceiverProperty slot, and pick the property you want to drive with the emitter's value.
	/// </summary>
	public class MMEmmiterReceiver : MonoBehaviour
	{
		[MMInformation(
			"This component lets you very easily have one property drive the value of another property. " +
			"To do so, drag the object with the property you want to 'read' from into the Emitter Property slot, then select the component the property is on, and finally the property itself." + 
			"Then drag the object with the property you want to 'write' to into the ReceiverProperty slot, and pick the property you want to drive with the emitter's value.",
			MoreMountains.Tools.MMInformationAttribute.InformationType.Info, false)]
		public bool Emitting = true;
		
		[Header("Emitter")]
		/// the property whose value you want to read and to have drive the ReceiverProperty's value
		[Tooltip("the property whose value you want to read and to have drive the ReceiverProperty's value")]
		public MMPropertyEmitter EmitterProperty;
		
		[Header("Receiver")]
		/// the property whose value you want to be driven by the EmitterProperty's value
		[Tooltip("the property whose value you want to be driven by the EmitterProperty's value")]
		public MMPropertyReceiver ReceiverProperty;

		/// a delegate to handle value changes
		public delegate void OnValueChangeDelegate();
		/// what to do on value change
		public OnValueChangeDelegate OnValueChange;
		
		protected float _levelLastFrame;
		
		/// <summary>
		/// On Awake we initialize both properties
		/// </summary>
		protected virtual void Awake()
		{
			EmitterProperty.Initialization(EmitterProperty.TargetComponent.gameObject);
			ReceiverProperty.Initialization(ReceiverProperty.TargetComponent.gameObject);
		}
		
		/// <summary>
		/// On Update we emit our value to our receiver
		/// </summary>
		protected virtual void Update()
		{
			EmitValue();
		}

		/// <summary>
		/// If needed, reads the current level of the emitter and sets it to the receiver
		/// </summary>
		protected virtual void EmitValue()
		{
			if (!Emitting)
			{
				return;
			}
			
			float level = EmitterProperty.GetLevel();

			if (level != _levelLastFrame)
			{
				// we trigger a value change event
				OnValueChange?.Invoke();

				ReceiverProperty?.SetLevel(level);
			}           

			_levelLastFrame = level;
		}
	}	
}