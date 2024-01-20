using UnityEngine;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// A feedback used to change the parent of a transform
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback lets you change the parent of a transform.")]
	[FeedbackPath("Transform/Set Parent")]
	public class MMF_SetParent : MMF_Feedback 
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
        
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.TransformColor; } }
		public override bool EvaluateRequiresSetup() { return (ObjectToParent == null); }
		public override string RequiredTargetText { get { return ObjectToParent != null ? ObjectToParent.name : "";  } }
		public override string RequiresSetupText { get { return "This feedback requires an ObjectToParent, that will be reparented to NewParent"; } } 
		#endif
		
		public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => ObjectToParent = FindAutomatedTarget<Transform>(); 

		[MMFInspectorGroup("Parenting", true, 12, true)]
		/// the object we want to change the parent of
		[Tooltip("the object we want to change the parent of")]
		public Transform ObjectToParent;
		/// the object ObjectToParent should now be parented to after playing this feedback
		[Tooltip("the object ObjectToParent should now be parented to after playing this feedback")]
		public Transform NewParent;
		/// if true, the parent-relative position, scale and rotation are modified such that the object keeps the same world space position, rotation and scale as before
		[Tooltip("if true, the parent-relative position, scale and rotation are modified such that the object keeps the same world space position, rotation and scale as before")]
		public bool WorldPositionStays = true;

		/// <summary>
		/// On Play, changes the parent of the target transform
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			if (ObjectToParent == null)
			{
				Debug.LogWarning("No object to parent was set for " + Owner.name);
				return;
			}
			ObjectToParent.SetParent(NewParent, WorldPositionStays);
		}
	}
}