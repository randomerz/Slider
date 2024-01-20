using System;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback allows you to hold a reference, that can then be used by other feedbacks to automatically set their target.
	/// It doesn't do anything when played. 
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback allows you to hold a reference, that can then be used by other feedbacks to automatically set their target. It doesn't do anything when played.")]
	[FeedbackPath("Feedbacks/MMF Reference Holder")]
	public class MMF_ReferenceHolder : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.FeedbacksColor; } }
		public override string RequiredTargetText => GameObjectReference != null ? GameObjectReference.name : "";  
		#endif
		/// the duration of this feedback is 0
		public override float FeedbackDuration => 0f;
		public override bool DisplayFullHeaderColor => true;

		[MMFInspectorGroup("References", true, 37, true)]
		/// the game object to set as the target (or on which to look for a specific component as a target) of all feedbacks that may look at this reference holder for a target
		[Tooltip("the game object to set as the target (or on which to look for a specific component as a target) of all feedbacks that may look at this reference holder for a target")] 
		public GameObject GameObjectReference;
		/// whether or not to force this reference holder on all compatible feedbacks in the MMF Player's list
		[Tooltip("whether or not to force this reference holder on all compatible feedbacks in the MMF Player's list")] 
		public bool ForceReferenceOnAll = false;
		
		/// <summary>
		/// On init we force our reference on all feedbacks if needed
		/// </summary>
		/// <param name="owner"></param>
		protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);
			if (ForceReferenceOnAll)
			{
				for (int index = 0; index < Owner.FeedbacksList.Count; index++)
				{
					if (Owner.FeedbacksList[index].HasAutomatedTargetAcquisition)
					{
						Owner.FeedbacksList[index].SetIndexInFeedbacksList(index);
						Owner.FeedbacksList[index].ForcedReferenceHolder = this;
						Owner.FeedbacksList[index].ForceAutomateTargetAcquisition();
					}
				}
			}
		}

		/// <summary>
		/// On Play we do nothing
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			return;
		}
	}
}