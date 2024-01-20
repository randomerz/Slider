using UnityEngine;
using System;

namespace MoreMountains.Tools
{
	/// <summary>
	/// Int property setter
	/// </summary>
	public class MMPropertyLinkInt : MMPropertyLink
	{
		public Func<int> GetIntDelegate;
		public Action<int> SetIntDelegate;

		protected int _initialValue;
		protected int _newValue;

		/// <summary>
		/// On init we grab our initial value
		/// </summary>
		/// <param name="property"></param>
		public override void Initialization(MMProperty property)
		{
			base.Initialization(property);
			_initialValue = (int)GetPropertyValue(property);
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
					GetIntDelegate = (Func<int>)Delegate.CreateDelegate(typeof(Func<int>),
						firstArgument,
						property.MemberPropertyInfo.GetGetMethod());
				}

				if (property.MemberPropertyInfo.GetSetMethod() != null)
				{
					SetIntDelegate = (Action<int>)Delegate.CreateDelegate(typeof(Action<int>),
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
			SetValueOptimized(property, (int)newValue);
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
			float returnValue = GetValueOptimized(property);

			returnValue = MMMaths.Clamp(returnValue, emitter.IntRemapMinToZero, emitter.IntRemapMaxToOne, emitter.ClampMin, emitter.ClampMax);
			returnValue = MMMaths.Remap(returnValue, emitter.IntRemapMinToZero, emitter.IntRemapMaxToOne, 0f, 1f);

			emitter.Level = returnValue;
			return returnValue;
		}

		/// <summary>
		/// Sets the specified level
		/// </summary>
		/// <param name="receiver"></param>
		/// <param name="property"></param>
		/// <param name="level"></param>
		public override void SetLevel(MMPropertyReceiver receiver, MMProperty property, float level)
		{
			base.SetLevel(receiver, property, level);

			_newValue = (int)MMMaths.Remap(level, 0f, 1f, receiver.IntRemapZero, receiver.IntRemapOne);

			if (receiver.RelativeValue)
			{
				_newValue = _initialValue + _newValue;
			}

			SetValueOptimized(property, _newValue);
		}

		/// <summary>
		/// Gets either the cached value or the raw value
		/// </summary>
		/// <param name="property"></param>
		/// <returns></returns>
		protected virtual int GetValueOptimized(MMProperty property)
		{
			return _getterSetterInitialized ? GetIntDelegate() : (int)GetPropertyValue(property);
		}

		/// <summary>
		/// Sets either the cached value or the raw value
		/// </summary>
		/// <param name="property"></param>
		/// <param name="newValue"></param>
		protected virtual void SetValueOptimized(MMProperty property, int newValue)
		{
			if (_getterSetterInitialized)
			{
				SetIntDelegate(_newValue);
			}
			else
			{
				SetPropertyValue(property, _newValue);
			}
		}
	}
}