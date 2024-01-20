using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System;
using System.Linq;

namespace MoreMountains.Tools
{
	[CustomPropertyDrawer(typeof(MMPropertyPicker), true)]
	public class MMPropertyPickerDrawer : PropertyDrawer
	{
		public class PropertyPickerViewData
		{
			public UnityEngine.Object _TargetObject;
			public GameObject _TargetGameObject;

			public const int _lineHeight = 20;
			public const int _lineMargin = 2;

			public int _selectedComponentIndex = 0;
			public int _selectedPropertyIndex = 0;

			public const string _undefinedComponentString = "<Undefined Component>";
			public const string _undefinedPropertyString = "<Undefined Property>";

			public bool _initialized = false;

			public string[] _componentNames;
			public List<Component> _componentList;

			public string[] _propertiesNames;
			public List<string> _propertiesList;
			public Type _propertyType = null;

			public int _numberOfLines = 0;
			public Color _progressBarBackground = new Color(0, 0, 0, 0.5f);

			public Type[] _authorizedTypes;
			public bool _targetIsScriptableObject;
		}
		
		private Dictionary<string, PropertyPickerViewData> _propertyPickerViewData = new Dictionary<string, PropertyPickerViewData>();
		
		

		/// <summary>
		/// Defines the height of the drawer
		/// </summary>
		/// <param name="property"></param>
		/// <param name="label"></param>
		/// <returns></returns>
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			if (!_propertyPickerViewData.TryGetValue(property.propertyPath, out var viewData))
			{
				viewData = new PropertyPickerViewData();
				_propertyPickerViewData[property.propertyPath] = viewData;
			}
			
			Initialization(property, viewData);

			viewData._numberOfLines = 2;

			if (viewData._TargetObject != null)
			{
				viewData._numberOfLines = 3;
				if (viewData._selectedComponentIndex != 0)
				{
					viewData._numberOfLines = 4;
				}
			}

			if (viewData._targetIsScriptableObject)
			{
				viewData._numberOfLines = 4;
			}

			return PropertyPickerViewData._lineHeight * viewData._numberOfLines + PropertyPickerViewData._lineMargin * viewData._numberOfLines - 1 + AdditionalHeight(viewData);
		}

		public virtual float AdditionalHeight(PropertyPickerViewData viewData)
		{
			return 0f;
		}

		/// <summary>
		/// Initializes the dropdowns
		/// </summary>
		/// <param name="property"></param>
		protected virtual void Initialization(SerializedProperty property, PropertyPickerViewData viewData)
		{
			
			if (viewData._initialized)
			{
				return;
			}

			FillAuthorizedTypes(viewData);

			FillComponentsList(property, viewData);
			FillPropertyList(property, viewData);

			GetComponentIndex(property, viewData);
			GetPropertyIndex(property, viewData);

			viewData._propertyType = GetPropertyType(property, viewData);

			viewData._initialized = true;
		}

		protected static bool AuthorizedType(Type[] typeArray, Type checkedType)
		{
			foreach (Type t in typeArray)
			{
				if (t == checkedType)
				{
					return true;
				}
			}
			return false;
		}

		protected virtual void FillAuthorizedTypes(PropertyPickerViewData viewData)
		{
			viewData._authorizedTypes = new Type[]
			{
				typeof(String),
				typeof(float),
				typeof(Vector2),
				typeof(Vector3),
				typeof(Vector4),
				typeof(Quaternion),
				typeof(int),
				typeof(bool),
				typeof(Color)
			};
		}

		#if  UNITY_EDITOR
		/// <summary>
		/// Draws the inspector
		/// </summary>
		/// <param name="position"></param>
		/// <param name="property"></param>
		/// <param name="label"></param>
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			if (!_propertyPickerViewData.TryGetValue(property.propertyPath, out var viewData))
			{
				viewData = new PropertyPickerViewData();
				_propertyPickerViewData[property.propertyPath] = viewData;
			}
			
			Initialization(property, viewData);
			
			// rectangles
			Rect targetLabelRect = new Rect(position.x, position.y, position.width, PropertyPickerViewData._lineHeight);
			Rect targetObjectRect = new Rect(position.x, position.y + (PropertyPickerViewData._lineHeight + PropertyPickerViewData._lineMargin), position.width, PropertyPickerViewData._lineHeight);
			Rect targetComponentRect = new Rect(position.x, position.y + (PropertyPickerViewData._lineHeight + PropertyPickerViewData._lineMargin) * 2, position.width, PropertyPickerViewData._lineHeight);
			Rect targetPropertyRect = new Rect(position.x, position.y + (PropertyPickerViewData._lineHeight + PropertyPickerViewData._lineMargin) * 3, position.width, PropertyPickerViewData._lineHeight);

			EditorGUI.BeginProperty(position, label, property);

			EditorGUI.LabelField(targetLabelRect, new GUIContent(property.name));

			EditorGUI.indentLevel++;

			// displays the target object selector
            
			// property.serializedObject.Update(); // removed to prevent blocking upper parts of the inspector

			EditorGUI.BeginChangeCheck();

			EditorGUI.PropertyField(targetObjectRect, property.FindPropertyRelative("TargetObject"), new GUIContent("Target Object"), true);
			if (EditorGUI.EndChangeCheck())
			{
				property.serializedObject.ApplyModifiedProperties();
				viewData._TargetObject = property.FindPropertyRelative("TargetObject").objectReferenceValue as UnityEngine.Object;
				FillComponentsList(property, viewData);
				viewData._selectedComponentIndex = 0;
				viewData._selectedPropertyIndex = 0;
				SetTargetComponent(property, viewData);
				if (viewData._targetIsScriptableObject)
				{
					FillPropertyList(property, viewData);
				}
			}

			// displays a label for scriptable objects
			if (viewData._targetIsScriptableObject)
			{
				EditorGUI.LabelField(targetComponentRect, "Type", "Scriptable Object");
			}

			// displays the component dropdown for gameobjects
			if ((viewData._componentNames != null) && (viewData._componentNames.Length > 0))
			{
				EditorGUI.BeginChangeCheck();
				viewData._selectedComponentIndex = EditorGUI.Popup(targetComponentRect, "Component", viewData._selectedComponentIndex, viewData._componentNames);
				if (EditorGUI.EndChangeCheck())
				{
					SetTargetComponent(property, viewData);
					viewData._selectedPropertyIndex = 0;
					FillPropertyList(property, viewData);
				}
			}

			// displays the properties dropdown
			if (((viewData._selectedComponentIndex != 0) || viewData._targetIsScriptableObject) && (viewData._propertiesNames != null) && (viewData._propertiesNames.Length > 0))
			{
				EditorGUI.BeginChangeCheck();
				viewData._selectedPropertyIndex = EditorGUI.Popup(targetPropertyRect, "Property", viewData._selectedPropertyIndex, viewData._propertiesNames);
				if (EditorGUI.EndChangeCheck())
				{
					SetTargetProperty(property, viewData);
				}
			}

			DisplayAdditionalProperties(position, property, label, viewData);

			EditorGUI.indentLevel--;

			EditorGUI.EndProperty();
		}
		#endif

		protected virtual void DisplayAdditionalProperties(Rect position, SerializedProperty property, GUIContent label, PropertyPickerViewData viewData)
		{

		}

		protected virtual void DrawLevelProgressBar(Rect position, float level, Color frontColor, Color negativeColor, PropertyPickerViewData viewData)
		{
			Rect levelLabelRect = new Rect(position.x, position.y + (PropertyPickerViewData._lineHeight + PropertyPickerViewData._lineMargin) * (viewData._numberOfLines - 1), position.width, PropertyPickerViewData._lineHeight);
			Rect levelValueRect = new Rect(position.x - 15 + EditorGUIUtility.labelWidth + 4, position.y + (PropertyPickerViewData._lineHeight + PropertyPickerViewData._lineMargin) * (viewData._numberOfLines - 1), position.width, PropertyPickerViewData._lineHeight);

			float progressX = position.x - 5 + EditorGUIUtility.labelWidth + 60;
			float progressY = position.y + (PropertyPickerViewData._lineHeight + PropertyPickerViewData._lineMargin) * (viewData._numberOfLines - 1) + 6;
			float progressHeight = 10f;
			float fullProgressWidth = position.width - EditorGUIUtility.labelWidth - 60 + 5;

			bool negative = false;
			float displayLevel = level;
			if (level < 0f)
			{
				negative = true;
				level = -level;
			}

			float progressLevel = Mathf.Clamp01(level);
			Rect levelProgressBg = new Rect(progressX, progressY, fullProgressWidth, progressHeight);
			float progressWidth = MMMaths.Remap(progressLevel, 0f, 1f, 0f, fullProgressWidth);
			Rect levelProgressFront = new Rect(progressX, progressY, progressWidth, progressHeight);

			EditorGUI.LabelField(levelLabelRect, new GUIContent("Level"));
			EditorGUI.LabelField(levelValueRect, new GUIContent(displayLevel.ToString("F4")));
			EditorGUI.DrawRect(levelProgressBg, viewData._progressBarBackground);
			if (negative)
			{
				EditorGUI.DrawRect(levelProgressFront, negativeColor);
			}
			else
			{
				EditorGUI.DrawRect(levelProgressFront, frontColor);
			}            
		}

		/// <summary>
		/// Fills a list of all the components on the target object
		/// </summary>
		/// <param name="property"></param>
		protected virtual void FillComponentsList(SerializedProperty property, PropertyPickerViewData viewData)
		{
			viewData._TargetObject = property.FindPropertyRelative("TargetObject").objectReferenceValue as UnityEngine.Object;
			viewData._TargetGameObject = property.FindPropertyRelative("TargetObject").objectReferenceValue as GameObject;
			
			viewData._targetIsScriptableObject = false;
			if (property.FindPropertyRelative("TargetObject").objectReferenceValue is ScriptableObject)
			{
				viewData._targetIsScriptableObject = true;
			}

			if (viewData._TargetGameObject == null)
			{
				viewData._componentNames = null;
				return;
			}

			// we create a list of components and an array of names
			viewData._componentList = new List<Component>();
			viewData._componentNames = new string[0];

			// we create a temp list to fill our array with
			List<string> tempComponentsNameList = new List<string>();
			tempComponentsNameList.Add(PropertyPickerViewData._undefinedComponentString);
			viewData._componentList.Add(null);

			// we add all components to the list
			Component[] components = viewData._TargetGameObject.GetComponents(typeof(Component));
			foreach (Component component in components)
			{
				viewData._componentList.Add(component);
				tempComponentsNameList.Add(component.GetType().Name);
			}
			viewData._componentNames = tempComponentsNameList.ToArray();
		}

		/// <summary>
		/// Fills a list of all properties and fields on the target component
		/// </summary>
		/// <param name="property"></param>
		protected virtual void FillPropertyList(SerializedProperty property, PropertyPickerViewData viewData)
		{
			if (viewData._TargetObject == null)
			{
				return;
			}

			if ((property.FindPropertyRelative("TargetComponent").objectReferenceValue == null)
			    && !viewData._targetIsScriptableObject)
			{
				return;
			}

			// we create a list of components and an array of names
			viewData._propertiesNames = Array.Empty<string>();
			viewData._propertiesList = new List<string>();

			// we create a temp list to fill our array with
			List<string> tempPropertiesList = new List<string>();
			tempPropertiesList.Add(PropertyPickerViewData._undefinedPropertyString);
			viewData._propertiesList.Add("");

			if (!viewData._targetIsScriptableObject)
			{
				// Find all fields
				var fieldsList = property.FindPropertyRelative("TargetComponent").objectReferenceValue.GetType()
					.GetFields(BindingFlags.Public | BindingFlags.Instance)
					.Where(field =>
						(AuthorizedType(viewData._authorizedTypes, field.FieldType))
					)
					.OrderBy(prop => prop.FieldType.Name).ToList();

				foreach (FieldInfo thisFieldInfo in fieldsList)
				{
					string newEntry = thisFieldInfo.Name + " [Field - " + thisFieldInfo.FieldType.Name + "]";
					tempPropertiesList.Add(newEntry);
					viewData._propertiesList.Add(thisFieldInfo.Name);
				}

				// finds all properties
				var propertiesList = property.FindPropertyRelative("TargetComponent").objectReferenceValue.GetType()
					.GetProperties(BindingFlags.Public | BindingFlags.Instance)
					.Where(prop =>
						(AuthorizedType(viewData._authorizedTypes, prop.PropertyType))
					)
					.OrderBy(prop => prop.PropertyType.Name).ToList();

				foreach (PropertyInfo foundProperty in propertiesList)
				{
					string newEntry = foundProperty.Name + " [Property - " + foundProperty.PropertyType.Name + "]";
					tempPropertiesList.Add(newEntry);
					viewData._propertiesList.Add(foundProperty.Name);
				}
			}
			else
			{
				// if this is a scriptable object
				// finds all fields
				var fieldsList = property.FindPropertyRelative("TargetObject").objectReferenceValue.GetType()
					.GetFields(BindingFlags.Public | BindingFlags.Instance)
					.Where(field =>
						(AuthorizedType(viewData._authorizedTypes, field.FieldType))
					)
					.OrderBy(prop => prop.FieldType.Name).ToList();

				foreach (FieldInfo thisFieldInfo in fieldsList)
				{
					string newEntry = thisFieldInfo.Name + " [Field - " + thisFieldInfo.FieldType.Name + "]";
					tempPropertiesList.Add(newEntry);
					viewData._propertiesList.Add(thisFieldInfo.Name);
				}

				// finds all properties
				var propertiesList = property.FindPropertyRelative("TargetObject").objectReferenceValue.GetType()
					.GetProperties(BindingFlags.Public | BindingFlags.Instance)
					.Where(prop =>
						(AuthorizedType(viewData._authorizedTypes, prop.PropertyType))
					)
					.OrderBy(prop => prop.PropertyType.Name).ToList();

				foreach (PropertyInfo foundProperty in propertiesList)
				{
					string newEntry = foundProperty.Name + " [Property - " + foundProperty.PropertyType.Name + "]";
					tempPropertiesList.Add(newEntry);
					viewData._propertiesList.Add(foundProperty.Name);
				}
			}

			viewData._propertiesNames = tempPropertiesList.ToArray();
		}

		/// <summary>
		/// Sets the target property
		/// </summary>
		/// <param name="property"></param>
		protected virtual void SetTargetProperty(SerializedProperty property, PropertyPickerViewData viewData)
		{
			if (viewData._selectedPropertyIndex > 0)
			{
				property.serializedObject.Update();
				property.FindPropertyRelative("TargetPropertyName").stringValue = viewData._propertiesList[viewData._selectedPropertyIndex];
				property.serializedObject.ApplyModifiedProperties();
				viewData._propertyType = GetPropertyType(property, viewData);
			}
			else
			{
				property.serializedObject.Update();
				property.FindPropertyRelative("TargetPropertyName").stringValue = "";
				property.serializedObject.ApplyModifiedProperties();
				viewData._selectedPropertyIndex = 0;
				viewData._propertyType = null;
			}
		}

		/// <summary>
		/// Sets the target component
		/// </summary>
		/// <param name="property"></param>
		protected virtual void SetTargetComponent(SerializedProperty property, PropertyPickerViewData viewData)
		{
			if (viewData._targetIsScriptableObject)
			{
				property.serializedObject.Update();
				property.FindPropertyRelative("TargetScriptableObject").objectReferenceValue = property.FindPropertyRelative("TargetObject").objectReferenceValue as ScriptableObject;
				property.FindPropertyRelative("TargetComponent").objectReferenceValue = null;
				property.serializedObject.ApplyModifiedProperties();
				return;
			}

			if (viewData._selectedComponentIndex > 0)
			{
				property.serializedObject.Update();
				property.FindPropertyRelative("TargetComponent").objectReferenceValue = viewData._componentList[viewData._selectedComponentIndex];
				property.FindPropertyRelative("TargetScriptableObject").objectReferenceValue = null;
				property.serializedObject.ApplyModifiedProperties();
			}
			else
			{
				property.serializedObject.Update();
				property.FindPropertyRelative("TargetComponent").objectReferenceValue = null;
				property.FindPropertyRelative("TargetPropertyName").stringValue = "";
				property.FindPropertyRelative("TargetScriptableObject").objectReferenceValue = null;
				viewData._selectedComponentIndex = 0;
				viewData._selectedPropertyIndex = 0;
				property.serializedObject.ApplyModifiedProperties();
			}
		}

		/// <summary>
		/// Gets the component index
		/// </summary>
		/// <param name="property"></param>
		protected virtual void GetComponentIndex(SerializedProperty property, PropertyPickerViewData viewData)
		{
			int index = 0;
			bool found = false;

			Component targetComponent = property.FindPropertyRelative("TargetComponent").objectReferenceValue as Component;

			if ((viewData._componentList == null) || (viewData._componentList.Count == 0))
			{
				viewData._selectedComponentIndex = 0;
				return;
			}

			foreach (Component component in viewData._componentList)
			{
				if (component == targetComponent)
				{
					viewData._selectedComponentIndex = index;
					found = true;
				}
				index++;
			}
			if (!found)
			{
				viewData._selectedComponentIndex = 0;
			}
		}

		/// <summary>
		/// Gets the property index
		/// </summary>
		/// <param name="property"></param>
		protected virtual void GetPropertyIndex(SerializedProperty property, PropertyPickerViewData viewData)
		{
			int index = 0;
			bool found = false;

			Component targetComponent = property.FindPropertyRelative("TargetComponent").objectReferenceValue as Component;
			ScriptableObject targetScriptable = property.FindPropertyRelative("TargetScriptableObject").objectReferenceValue as ScriptableObject;

			if ((targetComponent == null) && (targetScriptable == null))
			{
				return;
			}

			string targetProperty = property.FindPropertyRelative("TargetPropertyName").stringValue;

			if ((viewData._propertiesList == null) || (viewData._propertiesList.Count == 0))
			{
				viewData._selectedPropertyIndex = 0;
				return;
			}

			foreach (string prop in viewData._propertiesList)
			{
				if (prop == targetProperty)
				{
					viewData._selectedPropertyIndex = index;
					found = true;
				}
				index++;
			}
			if (!found)
			{
				viewData._selectedPropertyIndex = 0;
			}

		}

		protected virtual Type GetPropertyType(SerializedProperty property, PropertyPickerViewData viewData)
		{
			if (viewData._selectedPropertyIndex == 0)
			{
				return null;
			}

			MMProperty tempProperty;

			tempProperty = MMProperty.FindProperty(viewData._propertiesList[viewData._selectedPropertyIndex], property.FindPropertyRelative("TargetComponent").objectReferenceValue as Component, null, property.FindPropertyRelative("TargetObject").objectReferenceValue as ScriptableObject);
                        
			if (tempProperty != null)
			{
				return tempProperty.PropertyType;
			}
			else
			{
				return null;
			}
		}

	}
}