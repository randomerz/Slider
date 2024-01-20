using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback will let you turn the BlocksRaycast parameter of a target CanvasGroup on or off on play
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback will let you turn the BlocksRaycast parameter of a target CanvasGroup on or off on play")]
	[FeedbackPath("UI/CanvasGroup BlocksRaycasts")]
	public class MMFeedbackCanvasGroupBlocksRaycasts : MMFeedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.UIColor; } }
		#endif
        
		[Header("Canvas Group")]
		/// the target canvas group we want to control the BlocksRaycasts parameter on 
		[Tooltip("the target canvas group we want to control the BlocksRaycasts parameter on")]
		public CanvasGroup TargetCanvasGroup;
		/// if this is true, on play, the target canvas group will block raycasts, if false it won't
		[Tooltip("if this is true, on play, the target canvas group will block raycasts, if false it won't")]
		public bool ShouldBlockRaycasts = true;
        
		/// <summary>
		/// On play we turn raycast block on or off
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			if (TargetCanvasGroup == null)
			{
				return;
			}

			TargetCanvasGroup.blocksRaycasts = ShouldBlockRaycasts;
		}
	}
}