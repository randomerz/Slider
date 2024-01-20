using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback doesn't do anything by default, it's just meant as a comment, you can store text in it for future reference, maybe to remember how you setup a particular MMFeedbacks. Optionally it can also output that comment to the console on Play.
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback doesn't do anything by default, it's just meant as a comment, you can store text in it for future reference, maybe to remember how you setup a particular MMFeedbacks. Optionally it can also output that comment to the console on Play.")]
	[FeedbackPath("Debug/Comment")]
	public class MMFeedbackDebugComment : MMFeedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.DebugColor; } }
		#endif
     
		/// the comment / note associated to this feedback 
		[Tooltip("the comment / note associated to this feedback")]
		[TextArea(10,30)] 
		public string Comment;

		/// if this is true, the comment will be output to the console on Play 
		[Tooltip("if this is true, the comment will be output to the console on Play")]
		public bool LogComment = false;
		/// the color of the message when in DebugLogTime mode
		[Tooltip("the color of the message when in DebugLogTime mode")]
		[MMCondition("LogComment", true)]
		public Color DebugColor = Color.gray;
        
		/// <summary>
		/// On Play we output our message to the console if needed
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized || !LogComment)
			{
				return;
			}
            
			Debug.Log(Comment);
		}
	}
}