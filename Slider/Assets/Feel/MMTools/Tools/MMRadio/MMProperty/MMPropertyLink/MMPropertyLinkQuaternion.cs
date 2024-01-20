using UnityEngine;
using System;

namespace MoreMountains.Tools
{
	/// <summary>
	/// Quaternion property setter
	/// </summary>
	public class MMPropertyLinkQuaternion : MMPropertyLink
	{
		public Func<Quaternion> GetQuaternionDelegate;
		public Action<Quaternion> SetQuaternionDelegate;

		protected Quaternion _initialValue = Quaternion.identity;
		protected Quaternion _newValue;

		/// <summary>
		/// On init we grab our initial initialization
		/// </summary>
		/// <param name="property"></param>
		public override void Initialization(MMProperty property)
		{
			base.Initialization(property);
			_initialValue = (Quaternion)GetPropertyValue(property);
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
					GetQuaternionDelegate = (Func<Quaternion>)Delegate.CreateDelegate(typeof(Func<Quaternion>),
						firstArgument,
						property.MemberPropertyInfo.GetGetMethod());
				}

				if (property.MemberPropertyInfo.GetSetMethod() != null)
				{
					SetQuaternionDelegate = (Action<Quaternion>)Delegate.CreateDelegate(typeof(Action<Quaternion>),
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
			SetValueOptimized(property, (Quaternion)newValue);
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
			float axisValue = 0f;
			Quaternion propertyQuaternion = GetValueOptimized(property);
            
			switch (emitter.Vector3Option)
			{
				case MMPropertyEmitter.Vector3Options.X:
					axisValue = propertyQuaternion.eulerAngles.x;
					break;
				case MMPropertyEmitter.Vector3Options.Y:
					axisValue = propertyQuaternion.eulerAngles.y;
					break;
				case MMPropertyEmitter.Vector3Options.Z:
					axisValue = propertyQuaternion.eulerAngles.z;
					break;
			}
			axisValue = MMMaths.Clamp(axisValue, emitter.QuaternionRemapMinToZero, emitter.QuaternionRemapMaxToOne, emitter.ClampMin, emitter.ClampMax);

			float returnValue = MMMaths.Remap(axisValue, emitter.QuaternionRemapMinToZero, emitter.QuaternionRemapMaxToOne, 0f, 1f);

			emitter.Level = returnValue;
			return returnValue;
		}

		/// <summary>
		/// Sets the level, based on remap zero and remap one, angles in degree
		/// </summary>
		/// <param name="receiver"></param>
		/// <param name="property"></param>
		/// <param name="level"></param>
		public override void SetLevel(MMPropertyReceiver receiver, MMProperty property, float level)
		{
			base.SetLevel(receiver, property, level);

			_newValue = (receiver.RelativeValue) ? _initialValue : Quaternion.identity;

			if (receiver.ModifyX)
			{
				float newX = MMMaths.Remap(level, 0f, 1f, receiver.QuaternionRemapZero.x, receiver.QuaternionRemapOne.x);
				_newValue = _newValue * Quaternion.AngleAxis(newX, Vector3.right);
			}

			if (receiver.ModifyY)
			{
				float newY = MMMaths.Remap(level, 0f, 1f, receiver.QuaternionRemapZero.y, receiver.QuaternionRemapOne.y);
				_newValue = _newValue * Quaternion.AngleAxis(newY, Vector3.up);
			}

			if (receiver.ModifyZ)
			{
				float newZ = MMMaths.Remap(level, 0f, 1f, receiver.QuaternionRemapZero.z, receiver.QuaternionRemapOne.z);
				_newValue = _newValue * Quaternion.AngleAxis(newZ, Vector3.forward);
			}

			SetValueOptimized(property, _newValue);
		}

		/// <summary>
		/// Gets either the cached value or the raw value
		/// </summary>
		/// <param name="property"></param>
		/// <returns></returns>
		protected virtual Quaternion GetValueOptimized(MMProperty property)
		{
			return _getterSetterInitialized ? GetQuaternionDelegate() : (Quaternion)GetPropertyValue(property);
		}

		/// <summary>
		/// Sets either the cached value or the raw value
		/// </summary>
		/// <param name="property"></param>
		/// <param name="newValue"></param>
		protected virtual void SetValueOptimized(MMProperty property, Quaternion newValue)
		{
			if (_getterSetterInitialized)
			{
				SetQuaternionDelegate(_newValue);
			}
			else
			{
				SetPropertyValue(property, _newValue);
			}
		}
	}
}