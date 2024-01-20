using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
	/// <summary>
	/// A helper class that will hash a animation parameter and update it on demand
	/// </summary>
	[AddComponentMenu("More Mountains/Tools/Animation/MMAnimationParameter")]
	public class MMAnimationParameter : MonoBehaviour
	{
		/// the name of the animation parameter to hash
		public string ParameterName;
		/// the animator to update
		public Animator TargetAnimator;

		protected int _parameter;

		/// <summary>
		/// On awake we initialize our class
		/// </summary>
		protected virtual void Awake()
		{
			Initialization();
		}

		/// <summary>
		/// Hashes the parameter name into an int
		/// </summary>
		protected virtual void Initialization()
		{
			_parameter = Animator.StringToHash(ParameterName);
		}

		/// <summary>
		/// Sets the trigger of the specified name
		/// </summary>
		public virtual void SetTrigger()
		{
			TargetAnimator.SetTrigger(_parameter);
		}

		/// <summary>
		/// Sets the int of the specified name to the specified value
		/// </summary>
		public virtual void SetInt(int value)
		{
			TargetAnimator.SetInteger(_parameter, value);
		}
        
		/// <summary>
		/// Sets the float of the specified name to the specified value
		/// </summary>
		public virtual void SetFloat(float value)
		{
			TargetAnimator.SetFloat(_parameter, value);
		}
        
		/// <summary>
		/// Sets the bool of the specified name to the specified value
		/// </summary>
		public virtual void SetBool(bool value)
		{
			TargetAnimator.SetBool(_parameter, value);
		}
	}
}