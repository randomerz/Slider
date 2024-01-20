using UnityEngine;
using System;

namespace MoreMountains.Tools
{
	/// <summary>
	/// Bool property setter
	/// </summary>
	public class MMPropertyLinkBool : MMPropertyLink
	{
		public Func<bool> GetBoolDelegate;
		public Action<bool> SetBoolDelegate;

		protected bool _initialValue;
		protected bool _newValue;

		/// <summary>
		/// On init we grab our initial value
		/// </summary>
		/// <param name="property"></param>
		public override void Initialization(MMProperty property)
		{
			base.Initialization(property);
			_initialValue = (bool)GetPropertyValue(property);
		}

		/// <summary>
		/// Creates cached getter and setters for properties
		/// </summary>
		/// <param name="property"></param>
		public override void CreateGettersAndSetters(MMProperty property)
		{
			base.CreateGettersAndSetters(property);
			if (property.MemberType == MMProperty.MemberTypes.Property)
			{
				object firstArgument = (property.TargetScriptableObject == null) ? (object)property.TargetComponent : (object)property.TargetScriptableObject;

				if (property.MemberPropertyInfo.GetGetMethod() != null)
				{
					GetBoolDelegate = (Func<bool>)Delegate.CreateDelegate(typeof(Func<bool>),
						firstArgument,
						property.MemberPropertyInfo.GetGetMethod());
				}
				if (property.MemberPropertyInfo.GetSetMethod() != null)
				{
					SetBoolDelegate = (Action<bool>)Delegate.CreateDelegate(typeof(Action<bool>),
						firstArgument,
						property.MemberPropertyInfo.GetSetMethod());
				}
				_getterSetterInitialized = true;
			}
		}

		/// <summary>
		/// Gets the raw value of the property, a normalized float value, caching the operation if possible
		/// </summary>
		/// <param name="emitter"></param>
		/// <param name="property"></param>
		/// <returns></returns>
		public override object GetValue(MMPropertyEmitter emitter, MMProperty property)
		{
			return GetValueOptimized(property);
		}

		/// <summary>
		/// Sets the raw property value, float normalized, caching the operation if possible
		/// </summary>
		/// <param name="receiver"></param>
		/// <param name="property"></param>
		/// <param name="level"></param>
		public override void SetValue(MMPropertyReceiver receiver, MMProperty property, object newValue)
		{
			SetValueOptimized(property, (bool)newValue);
		}

		/// <summary>
		/// Returns this property link's level between 0 and 1
		/// </summary>
		/// <param name="receiver"></param>
		/// <param name="property"></param>
		/// <param name="level"></param>
		/// <returns></returns>
		public override float GetLevel(MMPropertyEmitter emitter, MMProperty property)
		{
			bool boolValue = GetValueOptimized(property);
			float returnValue = (boolValue == true) ? emitter.BoolRemapTrue : emitter.BoolRemapFalse;
			emitter.Level = returnValue;
			return returnValue;
		}

		/// <summary>
		/// Set the level (more than the link's Threshold > true, less > false)
		/// </summary>
		/// <param name="receiver"></param>
		/// <param name="property"></param>
		/// <param name="level"></param>
		public override void SetLevel(MMPropertyReceiver receiver, MMProperty property, float level)
		{
			base.SetLevel(receiver, property, level);
			_newValue = (level > receiver.Threshold) ? receiver.BoolRemapOne : receiver.BoolRemapZero;            
			SetValueOptimized(property, _newValue);
		}

		/// <summary>
		/// Gets either the cached value or the raw value
		/// </summary>
		/// <param name="property"></param>
		/// <returns></returns>
		protected virtual bool GetValueOptimized(MMProperty property)
		{
			return _getterSetterInitialized ? GetBoolDelegate() : (bool)GetPropertyValue(property);
		}

		/// <summary>
		/// Sets either the cached value or the raw value
		/// </summary>
		/// <param name="property"></param>
		/// <param name="newValue"></param>
		protected virtual void SetValueOptimized(MMProperty property, bool newValue)
		{
			if (_getterSetterInitialized)
			{
				SetBoolDelegate(_newValue);
			}
			else
			{
				SetPropertyValue(property, _newValue);
			}
		}
	}
}