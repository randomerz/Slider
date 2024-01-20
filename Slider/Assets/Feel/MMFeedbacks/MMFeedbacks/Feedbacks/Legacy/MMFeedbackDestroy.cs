using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback allows you to destroy a target gameobject, either via Destroy, DestroyImmediate, or SetActive:False
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback allows you to destroy a target gameobject, either via Destroy, DestroyImmediate, or SetActive:False")]
	[FeedbackPath("GameObject/Destroy")]
	public class MMFeedbackDestroy : MMFeedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.GameObjectColor; } }
		#endif

		/// the possible ways to destroy an object
		public enum Modes { Destroy, DestroyImmediate, Disable }

		[Header("Destroy")]
		/// the gameobject we want to change the active state of
		[Tooltip("the gameobject we want to change the active state of")]
		public GameObject TargetGameObject;
		/// the selected destruction mode 
		[Tooltip("the selected destruction mode")]
		public Modes Mode;

		/// <summary>
		/// On Play we change the state of our Behaviour if needed
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized || (TargetGameObject == null))
			{
				return;
			}
			ProceedWithDestruction(TargetGameObject);
		}
        
		/// <summary>
		/// Changes the status of the Behaviour
		/// </summary>
		/// <param name="state"></param>
		protected virtual void ProceedWithDestruction(GameObject go)
		{
			switch (Mode)
			{
				case Modes.Destroy:
					Destroy(go);
					break;
				case Modes.DestroyImmediate:
					DestroyImmediate(go);
					break;
				case Modes.Disable:
					go.SetActive(false);
					break;
			}
		}
	}
}