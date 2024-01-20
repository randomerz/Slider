using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback lets you control the contents of a target Text over time
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback lets you control the contents of a target Text over time.")]
	[FeedbackPath("UI/Text")]
	public class MMFeedbackText : MMFeedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		public enum ColorModes { Instant, Gradient, Interpolate }

		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.UIColor; } }
		#endif

		[Header("Target")]
		/// the Text component to control
		[Tooltip(" Text component to control")]
		public Text TargetText;

        
		/// the new text to replace the old one with
		[Tooltip("the new text to replace the old one with")]
		[TextArea]
		public string NewText = "Hello World";

		/// <summary>
		/// On play we change the text of our target TMPText
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			if (TargetText == null)
			{
				return;
			}

			TargetText.text = NewText;
		}
	}
}