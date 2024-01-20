using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;

namespace MoreMountains.Tools
{
	public class MMInspectorGroupData
	{
		public bool GroupIsOpen;
		public MMInspectorGroupAttribute GroupAttribute;
		public List<SerializedProperty> PropertiesList = new List<SerializedProperty>();
		public HashSet<string> GroupHashSet = new HashSet<string>();
		public Color GroupColor;

		public void ClearGroup()
		{
			GroupAttribute = null;
			GroupHashSet.Clear();
			PropertiesList.Clear();
		}
	}

	/// <summary>
	/// A generic drawer for all MMMonoBehaviour, handles both the Group and RequiresConstantRepaint attributes
	/// </summary>
	[CanEditMultipleObjects]
	[CustomEditor(typeof(MMMonoBehaviour), true, isFallback = true)]
	public class MMMonoBehaviourDrawer : UnityEditor.Editor
	{
		public bool DrawerInitialized;
		public List<SerializedProperty> PropertiesList = new List<SerializedProperty>();
		public Dictionary<string, MMInspectorGroupData> GroupData = new Dictionary<string, MMInspectorGroupData>();
        
		private string[] _mmHiddenPropertiesToHide;
		private bool _hasMMHiddenProperties = false;
		private bool _requiresConstantRepaint;
		protected bool _shouldDrawBase = true;

		public override bool RequiresConstantRepaint()
		{
			return _requiresConstantRepaint;
		}

		protected virtual void OnEnable()
		{
			DrawerInitialized = false;
			if (!target || !serializedObject.targetObject)
			{
				return;
			}
			_requiresConstantRepaint = serializedObject.targetObject.GetType().GetCustomAttribute<MMRequiresConstantRepaintAttribute>() != null;
            
			MMHiddenPropertiesAttribute[] hiddenProperties = (MMHiddenPropertiesAttribute[])target.GetType().GetCustomAttributes(typeof(MMHiddenPropertiesAttribute), false);
			if (hiddenProperties != null && hiddenProperties.Length > 0 && hiddenProperties[0].PropertiesNames != null)
			{
				_mmHiddenPropertiesToHide = hiddenProperties[0].PropertiesNames;
				_hasMMHiddenProperties = true;
			}
		}

		protected virtual void OnDisable()
		{
			if (target == null)
			{
				return;    
			}
			foreach (KeyValuePair<string, MMInspectorGroupData> groupData in GroupData)
			{
				EditorPrefs.SetBool(string.Format($"{groupData.Value.GroupAttribute.GroupName}{groupData.Value.PropertiesList[0].name}{target.GetInstanceID()}"), groupData.Value.GroupIsOpen);
				groupData.Value.ClearGroup();
			}
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			Initialization();
			DrawBase();
			DrawScriptBox();
			DrawContainer();
			DrawContents();

			serializedObject.ApplyModifiedProperties();            
		}

		protected virtual void Initialization()
		{
			if (DrawerInitialized)
			{
				return;
			}

			List<FieldInfo> fieldInfoList;
			MMInspectorGroupAttribute previousGroupAttribute = default;
			int fieldInfoLength = MMMonoBehaviourFieldInfo.GetFieldInfo(target, out fieldInfoList);

			for (int i = 0; i < fieldInfoLength; i++)
			{
				MMInspectorGroupAttribute group = Attribute.GetCustomAttribute(fieldInfoList[i], typeof(MMInspectorGroupAttribute)) as MMInspectorGroupAttribute;
				MMInspectorGroupData groupData;
				if (group == null)
				{
					if (previousGroupAttribute != null && previousGroupAttribute.GroupAllFieldsUntilNextGroupAttribute)
					{
						_shouldDrawBase = false;
						if (!GroupData.TryGetValue(previousGroupAttribute.GroupName, out groupData))
						{
							GroupData.Add(previousGroupAttribute.GroupName, new MMInspectorGroupData
							{
								GroupAttribute = previousGroupAttribute,
								GroupHashSet = new HashSet<string> { fieldInfoList[i].Name },
								GroupColor = MMColors.GetColorAt(previousGroupAttribute.GroupColorIndex)
							});
						}
						else
						{
							groupData.GroupColor = MMColors.GetColorAt(previousGroupAttribute.GroupColorIndex);
							groupData.GroupHashSet.Add(fieldInfoList[i].Name);
						}
					}

					continue;
				}
                
				previousGroupAttribute = group;

				if (!GroupData.TryGetValue(group.GroupName, out groupData))
				{
					bool groupIsOpen = EditorPrefs.GetBool(string.Format($"{group.GroupName}{fieldInfoList[i].Name}{target.GetInstanceID()}"), false);
					GroupData.Add(group.GroupName, new MMInspectorGroupData
					{
						GroupAttribute = group,
						GroupColor = MMColors.GetColorAt(previousGroupAttribute.GroupColorIndex),
						GroupHashSet = new HashSet<string> { fieldInfoList[i].Name }, GroupIsOpen = groupIsOpen });
				}
				else
				{
					groupData.GroupHashSet.Add(fieldInfoList[i].Name);
					groupData.GroupColor = MMColors.GetColorAt(previousGroupAttribute.GroupColorIndex);
				}
			}

			SerializedProperty iterator = serializedObject.GetIterator();

			if (iterator.NextVisible(true))
			{
				do
				{
					FillPropertiesList(iterator);
				} while (iterator.NextVisible(false));
			}

			DrawerInitialized = true;
		}

		protected virtual void DrawBase()
		{
			if (_shouldDrawBase)
			{
				DrawDefaultInspector();
				return;
			}
		}

		protected virtual void DrawScriptBox()
		{
			if (PropertiesList.Count == 0)
			{
				return;
			}

			using (new EditorGUI.DisabledScope("m_Script" == PropertiesList[0].propertyPath))
			{
				EditorGUILayout.PropertyField(PropertiesList[0], true);
			}
		}

		protected virtual void DrawContainer()
		{
			if (PropertiesList.Count == 0)
			{
				return;
			}

			foreach (KeyValuePair<string, MMInspectorGroupData> pair in GroupData)
			{
				this.DrawVerticalLayout(() => DrawGroup(pair.Value), MMMonoBehaviourDrawerStyle.ContainerStyle);
				EditorGUI.indentLevel = 0;
			}
		}

		protected virtual void DrawContents()
		{
			if (PropertiesList.Count == 0)
			{
				return;
			}

			EditorGUILayout.Space();
			for (int i = 1; i < PropertiesList.Count; i++)
			{
				if (_hasMMHiddenProperties && (!_mmHiddenPropertiesToHide.Contains(PropertiesList[i].name)))
				{
					EditorGUILayout.PropertyField(PropertiesList[i], true);
				}
			}
		}

		protected virtual void DrawGroup(MMInspectorGroupData groupData)
		{
			Rect verticalGroup = EditorGUILayout.BeginVertical();

			var leftBorderRect = new Rect(verticalGroup.xMin + 5, verticalGroup.yMin - 10, 3f, verticalGroup.height + 20);
			leftBorderRect.xMin = 15f;
			leftBorderRect.xMax = 18f;
			EditorGUI.DrawRect(leftBorderRect, groupData.GroupColor); 

			groupData.GroupIsOpen = EditorGUILayout.Foldout(groupData.GroupIsOpen, groupData.GroupAttribute.GroupName, true, MMMonoBehaviourDrawerStyle.GroupStyle);

			if (groupData.GroupIsOpen)
			{
				EditorGUI.indentLevel = 0;

				for (int i = 0; i < groupData.PropertiesList.Count; i++)
				{
					this.DrawVerticalLayout(() => DrawChild(i), MMMonoBehaviourDrawerStyle.BoxChildStyle);
				}
			}

			EditorGUILayout.EndVertical();

			void DrawChild(int i)
			{
				if ((_hasMMHiddenProperties) && (_mmHiddenPropertiesToHide.Contains(groupData.PropertiesList[i].name)))
				{
					return;
				}
				EditorGUILayout.PropertyField(groupData.PropertiesList[i], new GUIContent(ObjectNames.NicifyVariableName(groupData.PropertiesList[i].name), tooltip:groupData.PropertiesList[i].tooltip), true);
			}
		}
        
		public void FillPropertiesList(SerializedProperty serializedProperty)
		{
			bool shouldClose = false;

			foreach (KeyValuePair<string, MMInspectorGroupData> pair in GroupData)
			{
				if (pair.Value.GroupHashSet.Contains(serializedProperty.name))
				{
					SerializedProperty property = serializedProperty.Copy();
					shouldClose = true;
					pair.Value.PropertiesList.Add(property);
					break;
				}
			}

			if (!shouldClose)
			{
				SerializedProperty property = serializedProperty.Copy();
				PropertiesList.Add(property);
			}
		}
	}    
}