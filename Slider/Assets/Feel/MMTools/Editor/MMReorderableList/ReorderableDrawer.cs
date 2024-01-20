using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MoreMountains.Tools
{
	[CustomPropertyDrawer(typeof(MMReorderableAttributeAttribute))]
	public class ReorderableDrawer : PropertyDrawer {

		private static Dictionary<int, MMReorderableList> lists = new Dictionary<int, MMReorderableList>();

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {

			MMReorderableList list = GetList(property, attribute as MMReorderableAttributeAttribute);

			return list != null ? list.GetHeight() : EditorGUIUtility.singleLineHeight;
		}		
		
		#if  UNITY_EDITOR
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

			MMReorderableList list = GetList(property, attribute as MMReorderableAttributeAttribute);

			if (list != null) {

				list.DoList(EditorGUI.IndentedRect(position), label);
			}
			else {

				GUI.Label(position, "Array must extend from ReorderableArray", EditorStyles.label);
			}
		}
		#endif

		public static int GetListId(SerializedProperty property) {

			if (property != null) {

				int h1 = property.serializedObject.targetObject.GetHashCode();
				int h2 = property.propertyPath.GetHashCode();

				return (((h1 << 5) + h1) ^ h2);
			}

			return 0;
		}

		public static MMReorderableList GetList(SerializedProperty property) {

			return GetList(property, null, GetListId(property));
		}

		public static MMReorderableList GetList(SerializedProperty property, MMReorderableAttributeAttribute attrib) {

			return GetList(property, attrib, GetListId(property));
		}

		public static MMReorderableList GetList(SerializedProperty property, int id) {

			return GetList(property, null, id);
		}

		public static MMReorderableList GetList(SerializedProperty property, MMReorderableAttributeAttribute attrib, int id) {

			if (property == null) {

				return null;
			}

			MMReorderableList list = null;
			SerializedProperty array = property.FindPropertyRelative("array");

			if (array != null && array.isArray) {

				if (!lists.TryGetValue(id, out list)) {

					if (attrib != null) {

						Texture icon = !string.IsNullOrEmpty(attrib.elementIconPath) ? AssetDatabase.GetCachedIcon(attrib.elementIconPath) : null;

						MMReorderableList.ElementDisplayType displayType = attrib.singleLine ? MMReorderableList.ElementDisplayType.SingleLine : MMReorderableList.ElementDisplayType.Auto;

						list = new MMReorderableList(array, attrib.add, attrib.remove, attrib.draggable, displayType, attrib.elementNameProperty, attrib.elementNameOverride, icon);
					}
					else {

						list = new MMReorderableList(array, true, true, true);
					}

					lists.Add(id, list);
				}
				else {

					list.List = array;
				}
			}

			return list;
		}
	}
}