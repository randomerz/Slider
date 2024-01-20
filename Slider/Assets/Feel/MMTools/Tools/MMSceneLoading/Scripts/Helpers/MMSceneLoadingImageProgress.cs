using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MoreMountains.Tools
{	
	/// <summary>
	/// A very simple class, meant to be used within a MMSceneLoading screen, to update the fill amount of an Image
	/// based on loading progress
	/// </summary>
	public class MMSceneLoadingImageProgress : MonoBehaviour
	{
		protected Image _image;

		/// <summary>
		/// On Awake we store our Image
		/// </summary>
		protected virtual void Awake()
		{
			_image = this.gameObject.GetComponent<Image>();
		}
        
		/// <summary>
		/// Meant to be called by the MMSceneLoadingManager, turns the progress of a load into fill amount
		/// </summary>
		/// <param name="newValue"></param>
		public virtual void SetProgress(float newValue)
		{
			_image.fillAmount = newValue;
		}
	}
}