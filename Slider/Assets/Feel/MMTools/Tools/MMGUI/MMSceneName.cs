using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MoreMountains.Tools
{
	/// <summary>
	/// This component, when added on a Text component, will display the name of the level
	/// </summary>
	public class MMSceneName : MonoBehaviour
	{
		protected Text _text;

		/// <summary>
		/// On Awake, stores the Text component
		/// </summary>
		protected virtual void Awake()
		{
			_text = this.gameObject.GetComponent<Text>();
		}

		/// <summary>
		/// On Start, sets the level name
		/// </summary>
		protected virtual void Start()
		{
			SetLevelNameText();
		}

		/// <summary>
		/// Assigns the level name to the Text
		/// </summary>
		public virtual void SetLevelNameText()
		{
			if (_text != null)
			{
				_text.text = SceneManager.GetActiveScene().name;
			}
		}
	}
}