using UnityEngine;
using System;

namespace MoreMountains.Tools
{
	/// <summary>
	/// Vector3 property setter
	/// </summary>
	public class MMPropertyLinkVector3 : MMPropertyLink
	{
		public Func<Vector3> GetVector3Delegate;
		public Action<Vector3> SetVector3Delegate;

		protected Vector3 _initialValue;
		protected Vector3 _newValue;
		protected Vector3 _vector3;
        
		/// <summary>
		/// On init we grab our initial value
		/// </summary>
		/// <param name="property"></param>
		public override void Initialization(MMProperty property)
		{
			base.Initialization(property);
			_initialValue = (Vector3)GetPropertyValue(property);
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
					GetVector3Delegate = (Func<Vector3>)Delegate.CreateDelegate(typeof(Func<Vector3>),
						firstArgument,
						property.MemberPropertyInfo.GetGetMethod());
				}
				if (property.MemberPropertyInfo.GetSetMethod() != null)
				{
					SetVector3Delegate = (Action<Vector3>)Delegate.CreateDelegate(typeof(Action<Vector3>),
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
			SetValueOptimized(property, (Vector3)newValue);
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
			_vector3 = GetValueOptimized(property);

			float newValue = 0f;

			switch (emitter.Vector3Option)
			{
				case MMPropertyEmitter.Vector3Options.X:
					newValue = _vector3.x;
					break;
				case MMPropertyEmitter.Vector3Options.Y:
					newValue = _vector3.y;
					break;
				case MMPropertyEmitter.Vector3Options.Z:
					newValue = _vector3.z;
					break;
			}

			float returnValue = newValue;
			returnValue = MMMaths.Clamp(returnValue, emitter.FloatRemapMinToZero, emitter.FloatRemapMaxToOne, emitter.ClampMin, emitter.ClampMax);
			returnValue = MMMaths.Remap(returnValue, emitter.FloatRemapMinToZero, emitter.FloatRemapMaxToOne, 0f, 1f);

			emitter.Level = returnValue;
			return returnValue;
		}

		/// <summary>
		/// Sets the level
		/// </summary>
		/// <param name="receiver"></param>
		/// <param name="property"></param>
		/// <param name="level"></param>
		public override void SetLevel(MMPropertyReceiver receiver, MMProperty property, float level)
		{
			base.SetLevel(receiver, property, level);

			_newValue.x = receiver.ModifyX ? MMMaths.Remap(level, 0f, 1f, receiver.Vector3RemapZero.x, receiver.Vector3RemapOne.x) : 0f;
			_newValue.y = receiver.ModifyY ? MMMaths.Remap(level, 0f, 1f, receiver.Vector3RemapZero.y, receiver.Vector3RemapOne.y) : 0f;
			_newValue.z = receiver.ModifyZ ? MMMaths.Remap(level, 0f, 1f, receiver.Vector3RemapZero.z, receiver.Vector3RemapOne.z) : 0f;

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
		protected virtual Vector3 GetValueOptimized(MMProperty property)
		{
			return _getterSetterInitialized ? GetVector3Delegate() : (Vector3)GetPropertyValue(property);
		}

		/// <summary>
		/// Sets either the cached value or the raw value
		/// </summary>
		/// <param name="property"></param>
		/// <param name="newValue"></param>
		protected virtual void SetValueOptimized(MMProperty property, Vector3 newValue)
		{
			if (_getterSetterInitialized)
			{
				SetVector3Delegate(_newValue);
			}
			else
			{
				SetPropertyValue(property, _newValue);
			}
		}
	}
}