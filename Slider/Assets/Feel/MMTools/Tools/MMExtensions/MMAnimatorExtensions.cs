using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace MoreMountains.Tools
{
	/// <summary>
	/// Animator extensions
	/// </summary>
	public static class MMAnimatorExtensions
	{
		/// <summary>
		/// Determines if an animator contains a certain parameter, based on a type and a name
		/// </summary>
		/// <returns><c>true</c> if has parameter of type the specified self name type; otherwise, <c>false</c>.</returns>
		/// <param name="self">Self.</param>
		/// <param name="name">Name.</param>
		/// <param name="type">Type.</param>
		public static bool MMHasParameterOfType(this Animator self, string name, AnimatorControllerParameterType type)
		{
			if (string.IsNullOrEmpty(name)) { return false; }
			AnimatorControllerParameter[] parameters = self.parameters;
			foreach (AnimatorControllerParameter currParam in parameters)
			{
				if (currParam.type == type && currParam.name == name)
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Adds an animator parameter name to a parameter list if that parameter exists.
		/// </summary>
		/// <param name="animator"></param>
		/// <param name="parameterName"></param>
		/// <param name="parameter"></param>
		/// <param name="type"></param>
		/// <param name="parameterList"></param>
		public static void AddAnimatorParameterIfExists(Animator animator, string parameterName, out int parameter, AnimatorControllerParameterType type, HashSet<int> parameterList)
		{
			if (string.IsNullOrEmpty(parameterName))
			{
				parameter = -1;
				return;
			}

			parameter = Animator.StringToHash(parameterName);

			if (animator.MMHasParameterOfType(parameterName, type))
			{
				parameterList.Add(parameter);
			}
		}

		/// <summary>
		/// Adds an animator parameter name to a parameter list if that parameter exists.
		/// </summary>
		/// <param name="animator"></param>
		/// <param name="parameterName"></param>
		/// <param name="type"></param>
		/// <param name="parameterList"></param>
		public static void AddAnimatorParameterIfExists(Animator animator, string parameterName, AnimatorControllerParameterType type, HashSet<string> parameterList)
		{
			if (animator.MMHasParameterOfType(parameterName, type))
			{
				parameterList.Add(parameterName);
			}
		}
        
		// SIMPLE METHODS -------------------------------------------------------------------------------------------------------------------------------------------------------------

		#region SimpleMethods
        
		// <summary>
		/// Updates the animator bool.
		/// </summary>
		/// <param name="animator">Animator.</param>
		/// <param name="parameterName">Parameter name.</param>
		/// <param name="value">If set to <c>true</c> value.</param>
		public static void UpdateAnimatorBool(Animator animator, string parameterName, bool value)
		{
			animator.SetBool(parameterName, value);
		}

		/// <summary>
		/// Updates the animator integer.
		/// </summary>
		/// <param name="animator">Animator.</param>
		/// <param name="parameter">Parameter name.</param>
		/// <param name="value">Value.</param>
		public static void UpdateAnimatorInteger(Animator animator, string parameterName, int value)
		{
			animator.SetInteger(parameterName, value);
		}

		/// <summary>
		/// Updates the animator's float 
		/// </summary>
		/// <param name="animator"></param>
		/// <param name="parameterName"></param>
		/// <param name="value"></param>
		public static void UpdateAnimatorFloat(Animator animator, string parameterName, float value, bool performSanityCheck = true)
		{
			animator.SetFloat(parameterName, value);
		}
        
		#endregion

		// INT PARAMETER METHODS -------------------------------------------------------------------------------------------------------------------------------------------------------------

		// <summary>
		/// Updates the animator bool.
		/// </summary>
		/// <param name="animator">Animator.</param>
		/// <param name="parameterName">Parameter name.</param>
		/// <param name="value">If set to <c>true</c> value.</param>
		public static bool UpdateAnimatorBool(Animator animator, int parameter, bool value, HashSet<int> parameterList, bool performSanityCheck = true)
		{
			if (performSanityCheck && !parameterList.Contains(parameter))
			{
				return false;
			}
			animator.SetBool(parameter, value);
			return true;
		}

		/// <summary>
		/// Sets an animator's trigger of the int parameter specified
		/// </summary>
		/// <param name="animator"></param>
		/// <param name="parameter"></param>
		/// <param name="parameterList"></param>
		public static bool UpdateAnimatorTrigger(Animator animator, int parameter, HashSet<int> parameterList, bool performSanityCheck = true)
		{
			if (performSanityCheck && !parameterList.Contains(parameter))
			{
				return false;
			}
			animator.SetTrigger(parameter);
			return true;
		}

		/// <summary>
		/// Triggers an animator trigger.
		/// </summary>
		/// <param name="animator">Animator.</param>
		/// <param name="parameter">Parameter name.</param>
		/// <param name="value">If set to <c>true</c> value.</param>
		public static bool SetAnimatorTrigger(Animator animator, int parameter, HashSet<int> parameterList, bool performSanityCheck = true)
		{
			if (performSanityCheck && !parameterList.Contains(parameter))
			{
				return false;
			}
			animator.SetTrigger(parameter);
			return true;
		}

		/// <summary>
		/// Updates the animator float.
		/// </summary>
		/// <param name="animator">Animator.</param>
		/// <param name="parameter">Parameter name.</param>
		/// <param name="value">Value.</param>
		public static bool UpdateAnimatorFloat(Animator animator, int parameter, float value, HashSet<int> parameterList, bool performSanityCheck = true)
		{
			if (performSanityCheck && !parameterList.Contains(parameter))
			{
				return false;
			}
			animator.SetFloat(parameter, value);
			return true;
		}

		/// <summary>
		/// Updates the animator integer.
		/// </summary>
		/// <param name="animator">Animator.</param>
		/// <param name="parameter">Parameter name.</param>
		/// <param name="value">Value.</param>
		public static bool UpdateAnimatorInteger(Animator animator, int parameter, int value, HashSet<int> parameterList, bool performSanityCheck = true)
		{
			if (performSanityCheck && !parameterList.Contains(parameter))
			{
				return false;
			}
			animator.SetInteger(parameter, value);
			return true;
		}

        
        
		// STRING PARAMETER METHODS -------------------------------------------------------------------------------------------------------------------------------------------------------------

		#region StringParameterMethods

		// <summary>
		/// Updates the animator bool.
		/// </summary>
		/// <param name="animator">Animator.</param>
		/// <param name="parameterName">Parameter name.</param>
		/// <param name="value">If set to <c>true</c> value.</param>
		public static void UpdateAnimatorBool(Animator animator, string parameterName, bool value, HashSet<string> parameterList, bool performSanityCheck = true)
		{
			if (parameterList.Contains(parameterName))
			{
				animator.SetBool(parameterName, value);
			}
		}

		/// <summary>
		/// Sets an animator's trigger of the string parameter name specified
		/// </summary>
		/// <param name="animator"></param>
		/// <param name="parameterName"></param>
		/// <param name="parameterList"></param>
		public static void UpdateAnimatorTrigger(Animator animator, string parameterName, HashSet<string> parameterList, bool performSanityCheck = true)
		{
			if (parameterList.Contains(parameterName))
			{
				animator.SetTrigger(parameterName);
			}
		}

		/// <summary>
		/// Triggers an animator trigger.
		/// </summary>
		/// <param name="animator">Animator.</param>
		/// <param name="parameterName">Parameter name.</param>
		/// <param name="value">If set to <c>true</c> value.</param>
		public static void SetAnimatorTrigger(Animator animator, string parameterName, HashSet<string> parameterList, bool performSanityCheck = true)
		{
			if (parameterList.Contains(parameterName))
			{
				animator.SetTrigger(parameterName);
			}
		}

		/// <summary>
		/// Updates the animator float.
		/// </summary>
		/// <param name="animator">Animator.</param>
		/// <param name="parameterName">Parameter name.</param>
		/// <param name="value">Value.</param>
		public static void UpdateAnimatorFloat(Animator animator, string parameterName, float value, HashSet<string> parameterList, bool performSanityCheck = true)
		{
			if (parameterList.Contains(parameterName))
			{
				animator.SetFloat(parameterName, value);
			}
		}

		/// <summary>
		/// Updates the animator integer.
		/// </summary>
		/// <param name="animator">Animator.</param>
		/// <param name="parameterName">Parameter name.</param>
		/// <param name="value">Value.</param>
		public static void UpdateAnimatorInteger(Animator animator, string parameterName, int value, HashSet<string> parameterList, bool performSanityCheck = true)
		{
			if (parameterList.Contains(parameterName))
			{
				animator.SetInteger(parameterName, value);
			}
		}

		// <summary>
		/// Updates the animator bool after checking the parameter's existence.
		/// </summary>
		/// <param name="animator">Animator.</param>
		/// <param name="parameterName">Parameter name.</param>
		/// <param name="value">If set to <c>true</c> value.</param>
		public static void UpdateAnimatorBoolIfExists(Animator animator, string parameterName, bool value, bool performSanityCheck = true)
		{
			if (animator.MMHasParameterOfType(parameterName, AnimatorControllerParameterType.Bool))
			{
				animator.SetBool(parameterName, value);
			}
		}

		/// <summary>
		/// Updates an animator trigger if it exists
		/// </summary>
		/// <param name="animator"></param>
		/// <param name="parameterName"></param>
		public static void UpdateAnimatorTriggerIfExists(Animator animator, string parameterName, bool performSanityCheck = true)
		{
			if (animator.MMHasParameterOfType(parameterName, AnimatorControllerParameterType.Trigger))
			{
				animator.SetTrigger(parameterName);
			}
		}

		/// <summary>
		/// Triggers an animator trigger after checking for the parameter's existence.
		/// </summary>
		/// <param name="animator">Animator.</param>
		/// <param name="parameterName">Parameter name.</param>
		/// <param name="value">If set to <c>true</c> value.</param>
		public static void SetAnimatorTriggerIfExists(Animator animator, string parameterName, bool performSanityCheck = true)
		{
			if (animator.MMHasParameterOfType(parameterName, AnimatorControllerParameterType.Trigger))
			{
				animator.SetTrigger(parameterName);
			}
		}

		/// <summary>
		/// Updates the animator float after checking for the parameter's existence.
		/// </summary>
		/// <param name="animator">Animator.</param>
		/// <param name="parameterName">Parameter name.</param>
		/// <param name="value">Value.</param>
		public static void UpdateAnimatorFloatIfExists(Animator animator, string parameterName, float value, bool performSanityCheck = true)
		{
			if (animator.MMHasParameterOfType(parameterName, AnimatorControllerParameterType.Float))
			{
				animator.SetFloat(parameterName, value);
			}
		}

		/// <summary>
		/// Updates the animator integer after checking for the parameter's existence.
		/// </summary>
		/// <param name="animator">Animator.</param>
		/// <param name="parameterName">Parameter name.</param>
		/// <param name="value">Value.</param>
		public static void UpdateAnimatorIntegerIfExists(Animator animator, string parameterName, int value, bool performSanityCheck = true)
		{
			if (animator.MMHasParameterOfType(parameterName, AnimatorControllerParameterType.Int))
			{
				animator.SetInteger(parameterName, value);
			}
		}

		#endregion
        
        
	}
}