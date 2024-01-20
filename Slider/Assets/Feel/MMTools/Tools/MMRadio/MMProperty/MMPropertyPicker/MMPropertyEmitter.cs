using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;
using UnityEngine.Events;

namespace MoreMountains.Tools
{
	/// <summary>
	/// A class used to pick a property, and remap its value for emission/broadcast
	/// </summary>
	[Serializable]
	public class MMPropertyEmitter : MMPropertyPicker 
	{
		/// the min value to clamp this property value to
		public bool ClampMin = true;
		/// the max value to clamp this property value to
		public bool ClampMax = true;

		// vectors ----------------------------------------------------------------------------------------------------------------------
		/// the possible axis to look for on a Vector2
		public enum Vector2Options { X, Y }
		/// the possible axis to look for on a Vector3
		public enum Vector3Options { X, Y, Z }
		/// the possible axis to look for on a Vector4
		public enum Vector4Options { X, Y, Z, W }
		/// the selected axis on Vector2
		public Vector2Options Vector2Option;
		/// the selected axis on Vector3
		public Vector3Options Vector3Option;
		/// the selected axis on Vector4
		public Vector4Options Vector4Option;

		// bool  ----------------------------------------------------------------------------------------------------------------------
		/// what to remap a false value to
		public float BoolRemapFalse = 0f;
		/// what to remap a true value to
		public float BoolRemapTrue = 1f;

		// int  ----------------------------------------------------------------------------------------------------------------------
		/// what to remap the int min to
		public int IntRemapMinToZero = 0;
		/// what to remap the int max to
		public int IntRemapMaxToOne = 1;

		// float  ----------------------------------------------------------------------------------------------------------------------
		/// what to remap the float min to
		public float FloatRemapMinToZero = 0f;
		/// what to remap the float max to
		public float FloatRemapMaxToOne = 1f;

		// quaternion ----------------------------------------------------------------------------------------------------------------------
		/// what to remap the quaternion min to
		public float QuaternionRemapMinToZero = 0f;
		/// what to remap the quaternion max to
		public float QuaternionRemapMaxToOne = 360f;
		/// this property's current level
		public float Level = 0f;
        
		/// <summary>
		/// Gets this property's level
		/// </summary>
		/// <returns></returns>
		public virtual float GetLevel()
		{
			return _propertySetter.GetLevel(this, _targetMMProperty);
		}
	}
}