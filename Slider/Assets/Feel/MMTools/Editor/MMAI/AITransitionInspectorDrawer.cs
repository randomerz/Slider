using UnityEngine;
using UnityEditor;
using System.Collections;

namespace MoreMountains.Tools
{
	[CustomPropertyDrawer(typeof(AITransition))]
	public class AITransitionPropertyInspector : PropertyDrawer
	{
		const float LineHeight = 16f;

        
		#if  UNITY_EDITOR
        
		/// <summary>
		/// Draws a Transition inspector, a transition is one or more action(s), one or more decision(s) and associated true/false states
		/// </summary>
		/// <param name="rect"></param>
		/// <param name="prop"></param>
		/// <param name="label"></param>
		public override void OnGUI(Rect rect, SerializedProperty prop, GUIContent label)
		{
			Rect position = rect;
			foreach (SerializedProperty a in prop)
			{
				var height = Mathf.Max(LineHeight, EditorGUI.GetPropertyHeight(a));
				position.height = height;

				if(a.name == "Decision")
				{
					// draw the decision dropdown
					DrawSelectionDropdown(position, prop);

					// draw the base decision field
					position.y += height;
					EditorGUI.PropertyField(position, a, new GUIContent(a.name));
					position.y += height;

					/*var @object = a.objectReferenceValue;
					AIDecision @typedObject = @object as AIDecision;
					if (@typedObject != null && !string.IsNullOrEmpty(@typedObject.Label))
					{
					    EditorGUI.LabelField(position, "Label", @typedObject.Label);
					    position.y += height;
					}
					else
					{
					    EditorGUIUtility.GetControlID(FocusType.Passive);
					}*/
				}
				else
				{
					EditorGUI.PropertyField(position, a, new GUIContent(a.name));
					position.y += height;
				}
			}
		}
        
		#endif

		/// <summary>
		/// Draws a selector letting the user pick any decision associated with the AIBrain this transition is on
		/// </summary>
		/// <param name="position"></param>
		/// <param name="prop"></param>
		protected virtual void DrawSelectionDropdown(Rect position, SerializedProperty prop)
		{
			AIDecision thisDecision = prop.objectReferenceValue as AIDecision;
			AIDecision[] decisions = (prop.serializedObject.targetObject as AIBrain).GetAttachedDecisions();
			int selected = 0;
			int i = 1;
			string[] options = new string[decisions.Length + 1];
			options[0] = "None";
			foreach (AIDecision decision in decisions)
			{
				string name = string.IsNullOrEmpty(decision.Label) ? decision.GetType().Name : decision.Label;
				options[i] = i.ToString() + " - " + name;
				if (decision == thisDecision)
				{
					selected = i;
				}
				i++;
			}

			EditorGUI.BeginChangeCheck();
			selected = EditorGUI.Popup(position, selected, options);
			if (EditorGUI.EndChangeCheck())
			{
				prop.objectReferenceValue = (selected == 0) ? null : decisions[selected - 1];
				prop.serializedObject.ApplyModifiedProperties();
				EditorUtility.SetDirty(prop.serializedObject.targetObject);
			}
		}

		/// <summary>
		/// Determines the height of the transition property
		/// </summary>
		/// <param name="property"></param>
		/// <param name="label"></param>
		/// <returns></returns>
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			float height = 0;
			foreach (SerializedProperty a in property)
			{
				var h = Mathf.Max(LineHeight, EditorGUI.GetPropertyHeight(a));
				if(a.name == "Decision")
				{
					height += h * 2;

					/*var @object = a.objectReferenceValue;
					AIDecision @typedObject = @object as AIDecision;
					if (@typedObject != null && !string.IsNullOrEmpty(@typedObject.Label))
					{
					    height += h;
					}*/
				}
				else
				{
					height += h;
				}
			}
			return height;
		}
	}
}