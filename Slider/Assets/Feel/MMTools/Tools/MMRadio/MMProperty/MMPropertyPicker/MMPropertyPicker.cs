using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;
using UnityEngine.Events;

namespace MoreMountains.Tools
{
	/// <summary>
	/// A class used to pick a property on a target object / component / scriptable object
	/// </summary>
	[Serializable]
	public class MMPropertyPicker
	{
		/// the target object to look for a property on
		public UnityEngine.Object TargetObject;
		/// the component to look for a property on | storage only, not displayed in the inspector
		public Component TargetComponent;
		/// the component to look for a property on | storage only, not displayed in the inspector
		public ScriptableObject TargetScriptableObject;
		/// the name of the property to link to
		public string TargetPropertyName;
		/// whether or not this property has been found
		public bool PropertyFound { get; protected set; }
        
		protected MMProperty _targetMMProperty;
		protected bool _initialized = false;
		protected MMPropertyLink _propertySetter;

		/// <summary>
		/// When the property picker gets initialized, it grabs the stored property or field 
		/// and initializes a MMProperty and MMPropertyLink
		/// </summary>
		/// <param name="source"></param>
		public virtual void Initialization(GameObject source)
		{
			if ((TargetComponent == null) && (TargetScriptableObject == null))
			{
				PropertyFound = false;
				return;
			}
            
			_targetMMProperty = MMProperty.FindProperty(TargetPropertyName, TargetComponent, source, TargetScriptableObject);

			if (_targetMMProperty == null)
			{
				PropertyFound = false;
				return;
			}

			if ((_targetMMProperty.TargetComponent == null) && (_targetMMProperty.TargetScriptableObject == null))
			{
				PropertyFound = false;
				return;
			}
			if ((_targetMMProperty.MemberPropertyInfo == null) && (_targetMMProperty.MemberFieldInfo == null))
			{
				PropertyFound = false;
				return;
			}
			PropertyFound = true;
			_initialized = true;

			// if succession because pattern matching isn't supported before C# 7
			if (_targetMMProperty.PropertyType == typeof(string))
			{
				_propertySetter = new MMPropertyLinkString();
				_propertySetter.Initialization(_targetMMProperty);
				return;
			}
			if (_targetMMProperty.PropertyType == typeof(float))
			{
				_propertySetter = new MMPropertyLinkFloat();
				_propertySetter.Initialization(_targetMMProperty);
				return;
			}
			if (_targetMMProperty.PropertyType == typeof(Vector2))
			{
				_propertySetter = new MMPropertyLinkVector2();
				_propertySetter.Initialization(_targetMMProperty);
				return;
			}
			if (_targetMMProperty.PropertyType == typeof(Vector3))
			{
				_propertySetter = new MMPropertyLinkVector3();
				_propertySetter.Initialization(_targetMMProperty);
				return;
			}
			if (_targetMMProperty.PropertyType == typeof(Vector4))
			{
				_propertySetter = new MMPropertyLinkVector4();
				_propertySetter.Initialization(_targetMMProperty);
				return;
			}
			if (_targetMMProperty.PropertyType == typeof(Quaternion))
			{
				_propertySetter = new MMPropertyLinkQuaternion();
				_propertySetter.Initialization(_targetMMProperty);
				return;
			}
			if (_targetMMProperty.PropertyType == typeof(int))
			{
				_propertySetter = new MMPropertyLinkInt();
				_propertySetter.Initialization(_targetMMProperty);
				return;
			}
			if (_targetMMProperty.PropertyType == typeof(bool))
			{
				_propertySetter = new MMPropertyLinkBool();
				_propertySetter.Initialization(_targetMMProperty);
				return;
			}
			if (_targetMMProperty.PropertyType == typeof(Color))
			{
				_propertySetter = new MMPropertyLinkColor();
				_propertySetter.Initialization(_targetMMProperty);
				return;
			}
		}

		/// <summary>
		/// Returns the raw value of the target property
		/// </summary>
		/// <returns></returns>
		public virtual object GetRawValue()
		{
			return _propertySetter.GetPropertyValue(_targetMMProperty);
		}
	}
}