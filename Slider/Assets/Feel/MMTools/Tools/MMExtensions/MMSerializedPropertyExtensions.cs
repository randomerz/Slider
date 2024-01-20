using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
 
namespace MoreMountains.Tools 
{
	/// <summary>
	/// Serialized property extensions
	/// </summary>
	public static class MMSerializedPropertyExtensions 
	{
		#if UNITY_EDITOR
		/// <summary>
		/// Returns the object value of a target serialized property
		/// </summary>
		/// <param name="property"></param>
		/// <returns></returns>
		public static object MMGetObjectValue(this SerializedProperty property)
		{
			if (property == null)
			{
				return null;
			}

			string propertyPath = property.propertyPath.Replace(".Array.data[", "[");
			object targetObject = property.serializedObject.targetObject;
			var elements = propertyPath.Split('.');
			foreach (var element in elements)
			{
				if (!element.Contains("["))
				{
					targetObject = GetPropertyValue(targetObject, element);
				}
				else
				{
					string elementName = element.Substring(0, element.IndexOf("["));
					int elementIndex = System.Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
					targetObject = GetPropertyValue(targetObject, elementName, elementIndex);
				}
			}
			return targetObject;
		}
         
		private static object GetPropertyValue(object source, string propertyName)
		{
			if (source == null)
			{
				return null;
			}
                 
			Type propertyType = source.GetType();

			while (propertyType != null)
			{
				FieldInfo fieldInfo = propertyType.GetField(propertyName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
				if (fieldInfo != null)
				{
					return fieldInfo.GetValue(source);
				}
				PropertyInfo propertyInfo = propertyType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase | BindingFlags.Instance);
				if (propertyInfo != null)
				{
					return propertyInfo.GetValue(source, null);
				}
				propertyType = propertyType.BaseType;
			}
			return null;
		}

		private static object GetPropertyValue(object source, string propertyName, int index)
		{
			var enumerable = GetPropertyValue(source, propertyName) as System.Collections.IEnumerable;
			if (enumerable == null)
			{
				return null;
			}
			var enumerator = enumerable.GetEnumerator();
			for (int i = 0; i <= index; i++)
			{
				if (!enumerator.MoveNext())
				{
					return null;
				}
			}
			return enumerator.Current;
		}
		#endif
	}
}