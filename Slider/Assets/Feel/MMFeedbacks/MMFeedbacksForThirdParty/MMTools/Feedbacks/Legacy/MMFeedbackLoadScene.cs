using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using UnityEngine.SceneManagement;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback will request the load of a new scene, using the method of your choice
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback will request the load of a new scene, using the method of your choice")]
	[FeedbackPath("Scene/Load Scene")]
	public class MMFeedbackLoadScene : MMFeedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.SceneColor; } }
		#endif

		/// the possible ways to load a new scene :
		/// - direct : uses Unity's SceneManager API
		/// - MMSceneLoadingManager : the simple, original MM way of loading scenes
		/// - MMAdditiveSceneLoadingManager : a more advanced way of loading scenes, with (way) more options
		public enum LoadingModes { Direct, MMSceneLoadingManager, MMAdditiveSceneLoadingManager }

		[Header("Scene Names")]
		/// the name of the loading screen scene to use
		[Tooltip("the name of the loading screen scene to use - HAS TO BE ADDED TO YOUR BUILD SETTINGS")]
		public string LoadingSceneName = "MMAdditiveLoadingScreen";
		/// the name of the destination scene
		[Tooltip("the name of the destination scene - HAS TO BE ADDED TO YOUR BUILD SETTINGS")]
		public string DestinationSceneName = "";

		[Header("Mode")] 
		/// the loading mode to use
		[Tooltip("the loading mode to use to load the destination scene : " +
		         "- direct : uses Unity's SceneManager API" +
		         "- MMSceneLoadingManager : the simple, original MM way of loading scenes" +
		         "- MMAdditiveSceneLoadingManager : a more advanced way of loading scenes, with (way) more options")]
		public LoadingModes LoadingMode = LoadingModes.MMAdditiveSceneLoadingManager;
        
		[Header("Loading Scene Manager")]
		/// the priority to use when loading the new scenes
		[Tooltip("the priority to use when loading the new scenes")]
		public ThreadPriority Priority = ThreadPriority.High;
		/// whether or not to interpolate progress (slower, but usually looks better and smoother)
		[Tooltip("whether or not to interpolate progress (slower, but usually looks better and smoother)")]
		public bool InterpolateProgress = true;
		/// whether or not to perform extra checks to make sure the loading screen and destination scene are in the build settings
		[Tooltip("whether or not to perform extra checks to make sure the loading screen and destination scene are in the build settings")]
		public bool SecureLoad = true;

		[Header("Loading Scene Delays")]
		/// a delay (in seconds) to apply before the first fade plays
		[Tooltip("a delay (in seconds) to apply before the first fade plays")]
		public float BeforeEntryFadeDelay = 0f;
		/// the duration (in seconds) of the entry fade
		[Tooltip("the duration (in seconds) of the entry fade")]
		public float EntryFadeDuration = 0.2f;
		/// a delay (in seconds) to apply after the first fade plays
		[Tooltip("a delay (in seconds) to apply after the first fade plays")]
		public float AfterEntryFadeDelay = 0f;
		/// a delay (in seconds) to apply before the exit fade plays
		[Tooltip("a delay (in seconds) to apply before the exit fade plays")]
		public float BeforeExitFadeDelay = 0f;
		/// the duration (in seconds) of the exit fade
		[Tooltip("the duration (in seconds) of the exit fade")]
		public float ExitFadeDuration = 0.2f;
        
		[Header("Transitions")]
		/// the speed at which the progress bar should move if interpolated
		[Tooltip("the speed at which the progress bar should move if interpolated")]
		public float ProgressInterpolationSpeed = 5f;
		/// the order in which to play fades (really depends on the type of fader you have in your loading screen
		[Tooltip("the order in which to play fades (really depends on the type of fader you have in your loading screen")]
		public MMAdditiveSceneLoadingManager.FadeModes FadeMode = MMAdditiveSceneLoadingManager.FadeModes.FadeInThenOut;
		/// the tween to use on the entry fade
		[Tooltip("the tween to use on the entry fade")]
		public MMTweenType EntryFadeTween = new MMTweenType(new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1)));
		/// the tween to use on the exit fade
		[Tooltip("the tween to use on the exit fade")]
		public MMTweenType ExitFadeTween = new MMTweenType(new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1)));

		/// <summary>
		/// On play, we request a load of the destination scene using hte specified method
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			switch (LoadingMode)
			{
				case LoadingModes.Direct:
					SceneManager.LoadScene(DestinationSceneName);
					break;
				case LoadingModes.MMSceneLoadingManager:
					MMSceneLoadingManager.LoadScene(DestinationSceneName, LoadingSceneName);
					break;
				case LoadingModes.MMAdditiveSceneLoadingManager:
					MMAdditiveSceneLoadingManager.LoadScene(DestinationSceneName, LoadingSceneName, 
						Priority, SecureLoad, InterpolateProgress, 
						BeforeEntryFadeDelay, EntryFadeDuration,
						AfterEntryFadeDelay,
						BeforeExitFadeDelay, ExitFadeDuration,
						EntryFadeTween, ExitFadeTween,
						ProgressInterpolationSpeed, FadeMode);
					break;
			}
		}
	}
}