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
	public class MMF_ImageRaycastTarget : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.UIColor; } }
		public override bool EvaluateRequiresSetup() { return (TargetImage == null); }
		public override string RequiredTargetText { get { return TargetImage != null ? TargetImage.name : "";  } }
		public override string RequiresSetupText { get { return "This feedback requires that a TargetImage be set to be able to work properly. You can set one below."; } }
		#endif
		public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => TargetImage = FindAutomatedTarget<Image>();
        
		[MMFInspectorGroup("Image", true, 12, true)]
		/// the target Image we want to control the RaycastTarget parameter on
		[Tooltip("the target Image we want to control the RaycastTarget parameter on")]
		public Image TargetImage;
		/// if this is true, when played, the target image will become a raycast target
		[Tooltip("if this is true, when played, the target image will become a raycast target")]
		public bool ShouldBeRaycastTarget = true;

		protected bool _initialState;
		
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

			_initialState = TargetImage.raycastTarget;
			TargetImage.raycastTarget = NormalPlayDirection ? ShouldBeRaycastTarget : !ShouldBeRaycastTarget;
		}
		
		/// <summary>
		/// On restore, we restore our initial state
		/// </summary>
		protected override void CustomRestoreInitialValues()
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			TargetImage.raycastTarget = _initialState;
		}
	}
}