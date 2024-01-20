using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;
using UnityEngine.Events;

namespace MoreMountains.Tools 
{
	/// <summary>
	/// A class used to pick a property and modify its value
	/// </summary>
	[Serializable]
	public class MMPropertyReceiver : MMPropertyPicker
	{
		/// values will only be modified if this is true
		public bool ShouldModifyValue = true;
        
		/// whether or not to add to this property's initial value
		public bool RelativeValue = true;

		// vectors ----------------------------------------------------------------------------------------------------------------------
		/// whether or not to modify the X value of this vector
		public bool ModifyX = true;
		/// whether or not to modify the Y value of this vector
		public bool ModifyY = true;
		/// whether or not to modify the Z value of this vector
		public bool ModifyZ = true;
		/// whether or not to modify the W value of this vector
		public bool ModifyW = true;

		// bool & string ----------------------------------------------------------------------------------------------------------------------
		/// the threshold after which the float level should make this bool false or true
		public float Threshold = 0.5f;

		// bool  ----------------------------------------------------------------------------------------------------------------------
		/// the state to remap a float's zero to
		public bool BoolRemapZero = false;
		/// the state to remap a float's one to
		public bool BoolRemapOne = true;

		// string ----------------------------------------------------------------------------------------------------------------------
		/// the string to remap a float's zero to
		public string StringRemapZero = "Zero";
		/// the string to remap a float's zero to
		public string StringRemapOne = "One";

		// int  ----------------------------------------------------------------------------------------------------------------------
		/// the int value to remap the level's zero to
		public int IntRemapZero = 0;
		/// the int value to remap the level's 1 to
		public int IntRemapOne = 1;

		// float  ----------------------------------------------------------------------------------------------------------------------
		/// the float value to remap the level's 0 to
		public float FloatRemapZero = 0f;
		/// the float value to remap the level's 1 to
		public float FloatRemapOne = 1f;

		// vector2  ----------------------------------------------------------------------------------------------------------------------
		/// the vector2 value to remap the level's 0 to
		public Vector2 Vector2RemapZero = Vector2.zero;
		/// the vector2 value to remap the level's 1 to
		public Vector2 Vector2RemapOne = Vector2.one;

		// vector3  ----------------------------------------------------------------------------------------------------------------------
		/// the vector3 value to remap the level's 0 to
		public Vector3 Vector3RemapZero = Vector3.zero;
		/// the vector3 value to remap the level's 1 to
		public Vector3 Vector3RemapOne = Vector3.one;

		// vector4 ----------------------------------------------------------------------------------------------------------------------
		/// the vector4 value to remap the level's 0 to
		public Vector4 Vector4RemapZero = Vector4.zero;
		/// the vector4 value to remap the level's 1 to
		public Vector4 Vector4RemapOne = Vector4.one;

		// quaternion ----------------------------------------------------------------------------------------------------------------------
		/// the quaternion value to remap the level's 0 to
		public Vector3 QuaternionRemapZero = Vector3.zero;
		/// the quaternion value to remap the level's 1 to
		public Vector3 QuaternionRemapOne = new Vector3(180f, 180f, 180f);

		// color  ----------------------------------------------------------------------------------------------------------------------
		/// the color value to remap the level's 0 to
		[ColorUsage(true, true)]
		public Color ColorRemapZero = Color.white;
		/// the color value to remap the level's 1 to
		[ColorUsage(true, true)]
		public Color ColorRemapOne = Color.black;

		/// the current level 
		public float Level = 0f;

		/// <summary>
		/// Sets the level
		/// </summary>
		/// <param name="newLevel"></param>
		public virtual void SetLevel(float newLevel)
		{
			if (!PropertyFound)
			{
				return;
			}

			if (!ShouldModifyValue)
			{
				return;
			}
            
			_propertySetter.SetLevel(this, _targetMMProperty, newLevel);
		}       
	}
}