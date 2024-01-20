using System;
using UnityEngine;

namespace MoreMountains.Tools
{
	/// <summary>
	/// A class, meant to be extended, used to control a MMProperty and get/set its value
	/// </summary>
	public abstract class MMPropertyLink
	{
		protected bool _getterSetterInitialized = false;
        
		/// <summary>
		/// Initialization method
		/// </summary>
		/// <param name="property"></param>
		public virtual void Initialization(MMProperty property) 
		{
			CreateGettersAndSetters(property);
		}

		/// <summary>
		/// A method used to cache getter and setter for properties, not fields (sadly)
		/// </summary>
		/// <param name="property"></param>
		public virtual void CreateGettersAndSetters(MMProperty property)
		{

		}

		/// <summary>
		/// Gets the "level" of the property, a normalized float value, caching the operation if possible
		/// </summary>
		/// <param name="emitter"></param>
		/// <param name="property"></param>
		/// <returns></returns>
		public virtual float GetLevel(MMPropertyEmitter emitter, MMProperty property)
		{
			return 0f;
		}

		/// <summary>
		/// Sets the property's level, float normalized, caching the operation if possible
		/// </summary>
		/// <param name="receiver"></param>
		/// <param name="property"></param>
		/// <param name="level"></param>
		public virtual void SetLevel(MMPropertyReceiver receiver, MMProperty property, float level)
		{
			receiver.Level = level;
		}

		/// <summary>
		/// Gets the raw value of the property, a normalized float value, caching the operation if possible
		/// </summary>
		/// <param name="emitter"></param>
		/// <param name="property"></param>
		/// <returns></returns>
		public virtual object GetValue(MMPropertyEmitter emitter, MMProperty property)
		{
			return 0f;
		}

		/// <summary>
		/// Sets the raw property value, float normalized, caching the operation if possible
		/// </summary>
		/// <param name="receiver"></param>
		/// <param name="property"></param>
		/// <param name="level"></param>
		public virtual void SetValue(MMPropertyReceiver receiver, MMProperty property, object newValue)
		{

		}

		/// <summary>
		/// Returns the value of the selected property
		/// </summary>
		/// <returns></returns>
		public virtual object GetPropertyValue(MMProperty property)
		{
			object target = (property.TargetScriptableObject == null) ? (object)property.TargetComponent : (object)property.TargetScriptableObject;

			if (property.MemberType == MMProperty.MemberTypes.Property)
			{
				return property.MemberPropertyInfo.GetValue(target);
			}
			else if (property.MemberType == MMProperty.MemberTypes.Field)
			{
				return property.MemberFieldInfo.GetValue(target);
			}
			return 0f;
		}

		/// <summary>
		/// Sets the value of the selected property
		/// </summary>
		/// <param name="newValue"></param>
		protected virtual void SetPropertyValue(MMProperty property, object newValue)
		{
			object target = (property.TargetScriptableObject == null) ? (object)property.TargetComponent : (object)property.TargetScriptableObject;

			if (property.MemberType == MMProperty.MemberTypes.Property)
			{
				property.MemberPropertyInfo.SetValue(target, newValue);
			}
			else if (property.MemberType == MMProperty.MemberTypes.Field)
			{
				property.MemberFieldInfo.SetValue(target, newValue);
			}
		}
	}
}