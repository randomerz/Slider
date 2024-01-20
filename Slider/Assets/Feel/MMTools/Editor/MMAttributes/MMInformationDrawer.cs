#if UNITY_EDITOR

using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;

namespace MoreMountains.Tools
{	
	[CustomPropertyDrawer (typeof(MMInformationAttribute))]
	/// <summary>
	/// This class allows the display of a message box (warning, info, error...) next to a property (before or after)
	/// </summary>
	public class MMInformationAttributeDrawer : PropertyDrawer 
	{
		// determines the space after the help box, the space before the text box, and the width of the help box icon
		const int spaceBeforeTheTextBox = 5;
		const int spaceAfterTheTextBox = 10;
		const int iconWidth = 55;

		MMInformationAttribute informationAttribute { get { return ((MMInformationAttribute)attribute); } }

        
		#if  UNITY_EDITOR
		/// <summary>
		/// OnGUI, displays the property and the textbox in the specified order
		/// </summary>
		/// <param name="rect">Rect.</param>
		/// <param name="prop">Property.</param>
		/// <param name="label">Label.</param>
		public override void OnGUI (Rect rect, SerializedProperty prop, GUIContent label) 
		{
			if (HelpEnabled())
			{
				EditorStyles.helpBox.richText=true ;	

				if (!informationAttribute.MessageAfterProperty)
				{
					// we position the message before the property
					rect.height = DetermineTextboxHeight(informationAttribute.Message);
					EditorGUI.HelpBox (rect, informationAttribute.Message, informationAttribute.Type);

					rect.y += rect.height + spaceBeforeTheTextBox;
					EditorGUI.PropertyField(rect, prop, label, true);	
				}
				else
				{
					// we position the property first, then the message
					rect.height = GetPropertyHeight(prop,label); 
					EditorGUI.PropertyField(rect, prop, label, true);	

					rect.height = DetermineTextboxHeight(informationAttribute.Message);
					// we add the complete property height (property + helpbox, as overridden in this very script), and substract both to get just the property
					rect.y += GetPropertyHeight(prop,label) - DetermineTextboxHeight(informationAttribute.Message) - spaceAfterTheTextBox;
					EditorGUI.HelpBox (rect, informationAttribute.Message, informationAttribute.Type);
				}
  
			}
			else
			{
				EditorGUI.PropertyField(rect, prop, label, true);	  
			}
		}
		#endif

		/// <summary>
		/// Returns the complete height of the whole block (property + help text)
		/// </summary>
		/// <returns>The block height.</returns>
		/// <param name="property">Property.</param>
		/// <param name="label">Label.</param>
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			if (HelpEnabled())
			{
				return EditorGUI.GetPropertyHeight(property) + DetermineTextboxHeight(informationAttribute.Message) + spaceAfterTheTextBox + spaceBeforeTheTextBox;
			}
			else
			{
				return EditorGUI.GetPropertyHeight(property);
			}
		}

		/// <summary>
		/// Checks the editor prefs to see if help is enabled or not
		/// </summary>
		/// <returns><c>true</c>, if enabled was helped, <c>false</c> otherwise.</returns>
		protected virtual bool HelpEnabled()
		{
			bool helpEnabled = false;
			if (EditorPrefs.HasKey("MMShowHelpInInspectors"))
			{
				if (EditorPrefs.GetBool("MMShowHelpInInspectors"))
				{
					helpEnabled = true;
				}
			}
			return helpEnabled;
		}

		/// <summary>
		/// Determines the height of the textbox.
		/// </summary>
		/// <returns>The textbox height.</returns>
		/// <param name="message">Message.</param>
		protected virtual float DetermineTextboxHeight(string message)
		{
			GUIStyle style = new GUIStyle(EditorStyles.helpBox);
			style.richText=true;

			float newHeight = style.CalcHeight(new GUIContent(message),EditorGUIUtility.currentViewWidth - iconWidth);
			return newHeight;
		}
	}
}

#endif