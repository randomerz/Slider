using System.Collections;
using System.Collections.Generic;
using System.Net.Mime;
using UnityEngine;
using UnityEngine.UI;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback will let you control the RaycastTarget parameter of a target image, turning it on or off on play
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback will let you control the RaycastTarget parameter of a target image, turning it on or off on play")]
	[FeedbackPath("UI/Image RaycastTarget")]
	public class MMFeedbackImageRaycastTarget : MMFeedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.UIColor; } }
		#endif
        
		[Header("Image")]
		/// the target Image we want to control the RaycastTarget parameter on
		[Tooltip("the target Image we want to control the RaycastTarget parameter on")]
		public Image TargetImage;
		/// if this is true, when played, the target image will become a raycast target
		[Tooltip("if this is true, when played, the target image will become a raycast target")]
		public bool ShouldBeRaycastTarget = true;
        
		/// <summary>
		/// On play we turn raycastTarget on or off
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			if (TargetImage == null)
			{
				return;
			}

			TargetImage.raycastTarget = NormalPlayDirection ? ShouldBeRaycastTarget : !ShouldBeRaycastTarget;
		}
	}
}