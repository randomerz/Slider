using UnityEngine;
using UnityEditor;
using System.Collections;

namespace MoreMountains.Tools
{
	[CustomPropertyDrawer(typeof(AIAction))]
	public class AIActionPropertyInspector : PropertyDrawer
	{
		const float LineHeight = 16f;

		#if  UNITY_EDITOR

		/// <summary>
		/// Draws 
		/// </summary>
		/// <param name="rect"></param>
		/// <param name="prop"></param>
		/// <param name="label"></param>
		public override void OnGUI(Rect rect, SerializedProperty prop, GUIContent label)
		{
			// determines the height of the Action property
			var height = Mathf.Max(LineHeight, EditorGUI.GetPropertyHeight(prop));
			Rect position = rect;
			position.height = height;

			// draws the dropdown
			DrawSelectionDropdown(position, prop);

			// draws the base field
			position.y += height;
			EditorGUI.PropertyField(position, prop);
		}
        
		#endif

		/// <summary>
		/// Draws a selector letting the user pick any action associated with the AIBrain this action is on
		/// </summary>
		/// <param name="position"></param>
		/// <param name="prop"></param>
		protected virtual void DrawSelectionDropdown(Rect position, SerializedProperty prop)
		{
			AIAction thisAction = prop.objectReferenceValue as AIAction;
			AIAction[] actions = (prop.serializedObject.targetObject as AIBrain).GetAttachedActions();
			int selected = 0;
			int i = 1;
			string[] options = new string[actions.Length + 1];
			options[0] = "None";
			foreach (AIAction action in actions)
			{
				string name = string.IsNullOrEmpty(action.Label) ? action.GetType().Name : action.Label;
				options[i] = i.ToString() + " - " + name;
				if (action == thisAction)
				{
					selected = i;
				}
				i++;
			}

			EditorGUI.BeginChangeCheck();
			selected = EditorGUI.Popup(position, selected, options);
			if (EditorGUI.EndChangeCheck())
			{
				prop.objectReferenceValue = (selected == 0) ? null : actions[selected - 1];
				prop.serializedObject.ApplyModifiedProperties();
				EditorUtility.SetDirty(prop.serializedObject.targetObject);
			}
		}

		/// <summary>
		/// Returns the height of the full property
		/// </summary>
		/// <param name="property"></param>
		/// <param name="label"></param>
		/// <returns></returns>
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			var h = Mathf.Max(LineHeight, EditorGUI.GetPropertyHeight(property));
			float height = h * 2; // 2 lines, one for the dropdown, one for the property field
			return height;
		}
	}
}