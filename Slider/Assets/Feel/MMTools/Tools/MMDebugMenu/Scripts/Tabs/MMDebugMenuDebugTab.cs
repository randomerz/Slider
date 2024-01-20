using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MoreMountains.Tools
{
	/// <summary>
	/// A class used to handle the display of a debug log tab in a MMDebugMenu
	/// </summary>
	public class MMDebugMenuDebugTab : MonoBehaviour
	{
		/// the scrollrect where the log will be displayed
		public ScrollRect DebugScrollRect;
		/// the text container
		public Text DebugText;
		/// the prompt input
		public InputField CommandPrompt;
		/// a decorative prompt character
		public Text CommandPromptCharacter;
		/// whether or not the touch screen is visible
		public bool TouchScreenVisible = false;
		protected TouchScreenKeyboard _touchScreenKeyboard;
		protected RectTransform _rectTransform;
		protected float _mobileMenuOffset = -1000f;
		protected bool _touchScreenVisibleLastFrame;

		/// <summary>
		/// On awake we prepare our prompt listener
		/// </summary>
		protected virtual void Awake()
		{
			MMDebug.MMDebugLogEvent.Register(OnMMDebugLogEvent);
			DebugText.text = "";
			_rectTransform = this.gameObject.GetComponent<RectTransform>();

			CommandPrompt.onEndEdit.AddListener(val =>
			{
				CommandPrompt.text = "";
				if (val != "")
				{
					MMDebug.DebugLogCommand(val);
				}                
			});
		}

		/// <summary>
		/// if the mobile touchscreen is open, we move away
		/// </summary>
		protected virtual void Update()
		{
			TouchScreenVisible = TouchScreenKeyboard.visible;

			if (TouchScreenVisible)
			{
				_rectTransform.MMSetBottom(650f);
			}
			else
			{
				_rectTransform.MMSetBottom(0f);
			}
		}

		/// <summary>
		/// on late update we scroll to the bottom if needed
		/// </summary>
		protected virtual void LateUpdate()
		{
			if (_touchScreenVisibleLastFrame != TouchScreenVisible)
			{
				StartCoroutine(ScrollToLogBottomCo());
			}
			_touchScreenVisibleLastFrame = TouchScreenVisible;
		}

		/// <summary>
		/// Scrolls to the bottom on enable
		/// </summary>
		protected virtual void OnEnable()
		{
			StartCoroutine(ScrollToLogBottomCo());
		}

		/// <summary>
		/// when we get a new log event, we update our text and scroll to the bottom
		/// </summary>
		/// <param name="item"></param>
		protected virtual void OnMMDebugLogEvent(MMDebug.DebugLogItem item)
		{
			DebugText.text = MMDebug.LogHistoryText;
			if (this.gameObject.activeInHierarchy)
			{
				StartCoroutine(ScrollToLogBottomCo());
			}            
		}

		/// <summary>
		/// A coroutine used to scroll to the bottom
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator ScrollToLogBottomCo()
		{
			yield return new WaitForEndOfFrame();
			DebugScrollRect.normalizedPosition = Vector2.zero;
			CommandPrompt.ActivateInputField();
			CommandPrompt.Select();
		}

		/// <summary>
		/// Stops listening for events
		/// </summary>
		public virtual void OnDestroy()
		{
			MMDebug.MMDebugLogEvent.Unregister(OnMMDebugLogEvent);
		}
	}
}