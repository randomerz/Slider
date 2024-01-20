using UnityEngine;
#if MM_TEXTMESHPRO
using TMPro;
#endif

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback will let you change the text of a target TMP text component
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback will let you change the text of a target TMP text component")]
	[FeedbackPath("TextMesh Pro/TMP Text")]
	public class MMFeedbackTMPText : MMFeedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.TMPColor; } }
		#endif

		#if MM_TEXTMESHPRO
		[Header("TextMesh Pro")]
		/// the target TMP_Text component we want to change the text on
		[Tooltip("the target TMP_Text component we want to change the text on")]
		public TMP_Text TargetTMPText;
		#endif
		
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
			
			#if MM_TEXTMESHPRO
			if (TargetTMPText == null)
			{
				return;
			}
			TargetTMPText.text = NewText;
			#endif
		}
	}
}