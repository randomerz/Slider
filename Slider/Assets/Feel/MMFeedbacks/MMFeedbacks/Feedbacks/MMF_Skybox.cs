using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback will let you change the scene's skybox on play, replacing it with another one, either a specific one, or one picked at random among multiple skyboxes.
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback will let you change the scene's skybox on play, replacing it with another one, either a specific one, or one picked at random among multiple skyboxes.")]
	[FeedbackPath("Renderer/Skybox")]
	public class MMF_Skybox : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.RendererColor; } }
		#endif
        
		/// whether skyboxes are selected at random or not
		public enum Modes { Single, Random }

		[MMFInspectorGroup("Skybox", true, 65)]
		/// the selected mode 
		public Modes Mode = Modes.Single;
		/// the skybox to assign when in Single mode
		public Material SingleSkybox;
		/// the skyboxes to pick from when in Random mode
		public Material[] RandomSkyboxes;

		protected Material _initialSkybox;
        
		/// <summary>
		/// On play, we set the scene's skybox to a new one
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			_initialSkybox = RenderSettings.skybox;
            
			if (Mode == Modes.Single)
			{
				RenderSettings.skybox = SingleSkybox;
			}
			else if (Mode == Modes.Random)
			{
				RenderSettings.skybox = RandomSkyboxes[Random.Range(0, RandomSkyboxes.Length)];
			}
		}
		
		/// <summary>
		/// On restore, we put our object back at its initial position
		/// </summary>
		protected override void CustomRestoreInitialValues()
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			RenderSettings.skybox = _initialSkybox;
		}
	}    
}