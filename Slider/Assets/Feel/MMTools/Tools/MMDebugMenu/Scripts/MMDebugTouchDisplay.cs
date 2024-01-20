using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
	/// <summary>
	/// Add this class to a canvas and it'll automatically reposition TouchPrefabs at the position of touches
	/// You can set a higher TouchProvision if your game gets more than the default number (6) simultaneous touches
	/// Disable/enable this mono for it to stop/work
	/// </summary>
	public class MMDebugTouchDisplay : MonoBehaviour
	{
		[Header("Bindings")]
		/// the canvas to display the TouchPrefabs on
		public Canvas TargetCanvas;

		[Header("Touches")]
		/// the prefabs to instantiate to signify the position of the touches
		public RectTransform TouchPrefab;
		/// the amount of these prefabs to pool and provision
		public int TouchProvision = 6;

		protected List<RectTransform> _touchDisplays;

		/// <summary>
		/// On Start we initialize our pool
		/// </summary>
		protected virtual void Start()
		{
			Initialization();
		}

		/// <summary>
		/// Creates the pool of prefabs
		/// </summary>
		protected virtual void Initialization()
		{
			_touchDisplays = new List<RectTransform>();

			for (int i = 0; i < TouchProvision; i++)
			{
				RectTransform touchDisplay = Instantiate(TouchPrefab);
				touchDisplay.transform.SetParent(TargetCanvas.transform);
				touchDisplay.name = "MMDebugTouchDisplay_" + i;
				touchDisplay.gameObject.SetActive(false);
				_touchDisplays.Add(touchDisplay);
			}

			this.enabled = false;
		}

		/// <summary>
		/// On update we detect touches and move our prefabs at their position
		/// </summary>
		protected virtual void Update()
		{
			DisableAllDisplays();
			DetectTouches();
		}

		/// <summary>
		/// Acts on all touches
		/// </summary>
		protected virtual void DetectTouches()
		{
			for (int i = 0; i < Input.touchCount; ++i)
			{
				_touchDisplays[i].gameObject.SetActive(true);
				_touchDisplays[i].position = Input.GetTouch(i).position;
			}
		}

		/// <summary>
		/// Disables all touch prefabs
		/// </summary>
		protected virtual void DisableAllDisplays()
		{
			foreach(RectTransform display in _touchDisplays)
			{
				display.gameObject.SetActive(false);
			}
		}

		/// <summary>
		/// When this mono gets disabled we turn all our prefabs off
		/// </summary>
		protected virtual void OnDisable()
		{
			DisableAllDisplays();
		}
	}
}