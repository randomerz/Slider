using MoreMountains.FeedbacksForThirdParty;
using MoreMountains.Tools;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// Add this class to an empty object in your scene and it will prevent any unchecked feedback in its inspector from playing
	/// </summary>
	public partial class MMFeedbacksAuthorizations : MMMonoBehaviour
	{
		[MMInspectorGroup("Animation", true, 16)] [MMInspectorButton("ToggleAnimation")]
		public bool ToggleAnimationButton;

		public bool AnimationParameter = true;
		public bool AnimatorSpeed = true;

		[MMInspectorGroup("Audio", true, 17)] [MMInspectorButton("ToggleAudio")]
		public bool ToggleAudioButton;

		public bool AudioFilterDistortion = true;
		public bool AudioFilterEcho = true;
		public bool AudioFilterHighPass = true;
		public bool AudioFilterLowPass = true;
		public bool AudioFilterReverb = true;
		public bool AudioMixerSnapshotTransition = true;
		public bool AudioSource = true;
		public bool AudioSourcePitch = true;
		public bool AudioSourceStereoPan = true;
		public bool AudioSourceVolume = true;
		public bool MMPlaylist = true;
		public bool MMSoundManagerAllSoundsControl = true;
		public bool MMSoundManagerSaveAndLoad = true;
		public bool MMSoundManagerSound = true;
		public bool MMSoundManagerSoundControl = true;
		public bool MMSoundManagerSoundFade = true;
		public bool MMSoundManagerTrackControl = true;
		public bool MMSoundManagerTrackFade = true;
		public bool Sound = true;

		[MMInspectorGroup("Camera", true, 18)] [MMInspectorButton("ToggleCamera")]
		public bool ToggleCameraButton;

		public bool CameraShake = true;
		public bool CameraZoom = true;
		#if MM_CINEMACHINE
		public bool CinemachineImpulse = true;
		public bool CinemachineImpulseClear = true;
		public bool CinemachineImpulseSource = true;
		public bool CinemachineTransition = true;
		#endif
		public bool ClippingPlanes = true;
		public bool Fade = true;
		public bool FieldOfView = true;
		public bool Flash = true;
		public bool OrthographicSize = true;

		[MMInspectorGroup("Debug", true, 19)] [MMInspectorButton("ToggleDebug")]
		public bool ToggleDebugButton;

		public bool Comment = true;
		public bool Log = true;

		[MMInspectorGroup("Events", true, 20)] [MMInspectorButton("ToggleEvents")]
		public bool ToggleEventsButton;

		public bool MMGameEvent = true;
		public bool UnityEvents = true;

		[MMInspectorGroup("GameObject", true, 47)] [MMInspectorButton("ToggleGameObject")]
		public bool ToggleGameObjectButton;

		public bool Broadcast = true;
		public bool Collider = true;
		public bool Collider2D = true;
		public bool DestroyTargetObject = true;
		public bool EnableBehaviour = true;
		public bool FloatController = true;
		public bool InstantiateObject = true;
		public bool MMRadioSignal = true;
		public bool Rigidbody = true;
		public bool Rigidbody2D = true;
		public bool SetActive = true;

		
		#if MOREMOUNTAINS_NICEVIBRATIONS_INSTALLED
		[MMInspectorGroup("Haptics", true, 22)] [MMInspectorButton("ToggleHaptics")]
		public bool ToggleHapticsButton;

		public bool HapticClip = true;
		public bool HapticContinuous = true;
		public bool HapticControl = true;
		public bool HapticEmphasis = true;
		public bool HapticPreset = true;
		#endif

		[MMInspectorGroup("Light", true, 23)] [MMInspectorButton("ToggleLight")]
		public bool ToggleLightButton;

		public bool Light = true;

		[MMInspectorGroup("Loop", true, 24)] [MMInspectorButton("ToggleLoop")]
		public bool ToggleLoopButton;

		public bool Looper = true;
		public bool LooperStart = true;

		[MMInspectorGroup("Particles", true, 25)] [MMInspectorButton("ToggleParticles")]
		public bool ToggleParticlesButton;

		public bool ParticlesInstantiation = true;
		public bool ParticlesPlay = true;

		[MMInspectorGroup("Pause", true, 26)] [MMInspectorButton("TogglePause")]
		public bool TogglePauseButton;

		public bool HoldingPause = true;
		public bool Pause = true;

		[MMInspectorGroup("Post Process", true, 27)] [MMInspectorButton("TogglePostProcess")]
		public bool TogglePostProcessButton;

		public bool Bloom = true;
		public bool ChromaticAberration = true;
		public bool ColorGrading = true;
		public bool DepthOfField = true;
		public bool GlobalPPVolumeAutoBlend = true;
		public bool LensDistortion = true;
		public bool PPMovingFilter = true;
		public bool Vignette = true;

		[MMInspectorGroup("Flicker", true, 28)] [MMInspectorButton("ToggleFlicker")]
		public bool ToggleFlickerButton;

		public bool Flicker = true;
		public bool Fog = true;
		public bool Material = true;
		public bool MMBlink = true;
		public bool ShaderGlobal = true;
		public bool ShaderController = true;
		public bool Skybox = true;
		public bool SpriteRenderer = true;
		public bool TextureOffset = true;
		public bool TextureScale = true;

		[MMInspectorGroup("Scene", true, 29)] [MMInspectorButton("ToggleScene")]
		public bool ToggleSceneButton;

		public bool LoadScene = true;
		public bool UnloadScene = true;

		[MMInspectorGroup("Time", true, 31)] [MMInspectorButton("ToggleTime")]
		public bool ToggleTimeButton;

		public bool FreezeFrame = true;
		public bool TimescaleModifier = true;

		[MMInspectorGroup("Transform", true, 32)] [MMInspectorButton("ToggleTransform")]
		public bool ToggleTransformButton;

		public bool Destination = true;
		public bool Position = true;
		public bool PositionShake = true;
		public bool RotatePositionAround = true;
		public bool Rotation = true;
		public bool RotationShake = true;
		public bool Scale = true;
		public bool ScaleShake = true;
		public bool SquashAndStretch = true;
		public bool Wiggle = true;

		[MMInspectorGroup("UI", true, 33)] [MMInspectorButton("ToggleUI")]
		public bool ToggleUiButton;

		public bool CanvasGroup = true;
		public bool CanvasGroupBlocksRaycasts = true;
		public bool FloatingText = true;
		public bool Graphic = true;
		public bool GraphicCrossFade = true;
		public bool Image = true;
		public bool ImageAlpha = true;
		public bool ImageFill = true;
		public bool ImageRaycastTarget = true;
		public bool ImageTextureOffset = true;
		public bool ImageTextureScale = true;
		public bool RectTransformAnchor = true;
		public bool RectTransformOffset = true;
		public bool RectTransformPivot = true;
		public bool RectTransformSizeDelta = true;
		public bool Text = true;
		public bool TextColor = true;
		public bool TextFontSize = true;
		public bool VideoPlayer = true;
		
		[MMInspectorGroup("TextMesh Pro", true, 30)] [MMInspectorButton("ToggleTextMeshPro")]
		public bool ToggleTextMeshProButton;

		#if MM_TEXTMESHPRO
		public bool TMPAlpha = true;
		public bool TMPCharacterSpacing = true;
		public bool TMPColor = true;
		public bool TMPCountTo = true;
		public bool TMPDilate = true;
		public bool TMPFontSize = true;
		public bool TMPLineSpacing = true;
		public bool TMPOutlineColor = true;
		public bool TMPOutlineWidth = true;
		public bool TMPParagraphSpacing = true;
		public bool TMPSoftness = true;
		public bool TMPText = true;
		public bool TMPTextReveal = true;
		public bool TMPWordSpacing = true;
		#endif
		
		#region ToggleMethods
		
		private void ToggleAnimation()
		{
			AnimationParameter = !AnimationParameter;
			AnimatorSpeed = !AnimatorSpeed;
		}

		private void ToggleAudio()
		{
			AudioFilterDistortion = !AudioFilterDistortion;
			AudioFilterEcho = !AudioFilterEcho;
			AudioFilterHighPass = !AudioFilterHighPass;
			AudioFilterLowPass = !AudioFilterLowPass;
			AudioFilterReverb = !AudioFilterReverb;
			AudioMixerSnapshotTransition = !AudioMixerSnapshotTransition;
			AudioSource = !AudioSource;
			AudioSourcePitch = !AudioSourcePitch;
			AudioSourceStereoPan = !AudioSourceStereoPan;
			AudioSourceVolume = !AudioSourceVolume;
			MMPlaylist = !MMPlaylist;
			MMSoundManagerAllSoundsControl = !MMSoundManagerAllSoundsControl;
			MMSoundManagerSaveAndLoad = !MMSoundManagerSaveAndLoad;
			MMSoundManagerSound = !MMSoundManagerSound;
			MMSoundManagerSoundControl = !MMSoundManagerSoundControl;
			MMSoundManagerSoundFade = !MMSoundManagerSoundFade;
			MMSoundManagerTrackControl = !MMSoundManagerTrackControl;
			MMSoundManagerTrackFade = !MMSoundManagerTrackFade;
			Sound = !Sound;
		}

		private void ToggleCamera()
		{
			CameraShake = !CameraShake;
			CameraZoom = !CameraZoom;
			#if MM_CINEMACHINE
			CinemachineImpulse = !CinemachineImpulse;
			CinemachineImpulseClear = !CinemachineImpulseClear;
			CinemachineImpulseSource = !CinemachineImpulseSource;
			CinemachineTransition = !CinemachineTransition;
			#endif
			ClippingPlanes = !ClippingPlanes;
			Fade = !Fade;
			FieldOfView = !FieldOfView;
			Flash = !Flash;
			OrthographicSize = !OrthographicSize;
		}

		private void ToggleDebug()
		{
			Comment = !Comment;
			Log = !Log;
		}

		private void ToggleEvents()
		{
			MMGameEvent = !MMGameEvent;
			UnityEvents = !UnityEvents;
		}

		private void ToggleGameObject()
		{
			Broadcast = !Broadcast;
			Collider = !Collider;
			Collider2D = !Collider2D;
			DestroyTargetObject = !DestroyTargetObject;
			EnableBehaviour = !EnableBehaviour;
			FloatController = !FloatController;
			InstantiateObject = !InstantiateObject;
			MMRadioSignal = !MMRadioSignal;
			Rigidbody = !Rigidbody;
			Rigidbody2D = !Rigidbody2D;
			SetActive = !SetActive;
		}
		
		#if MOREMOUNTAINS_NICEVIBRATIONS_INSTALLED
		private void ToggleHaptics()
		{
			HapticClip = !HapticClip;
			HapticContinuous = !HapticContinuous;
			HapticControl = !HapticControl;
			HapticEmphasis = !HapticEmphasis;
			HapticPreset = !HapticPreset;
		}
		#endif

		private void ToggleLight()
		{
			Light = !Light;
		}

		private void ToggleLoop()
		{
			Looper = !Looper;
			LooperStart = !LooperStart;
		}

		private void ToggleParticles()
		{
			ParticlesInstantiation = !ParticlesInstantiation;
			ParticlesPlay = !ParticlesPlay;
		}

		private void TogglePause()
		{
			HoldingPause = !HoldingPause;
			Pause = !Pause;
		}

		#if MM_POSTPROCESSING
		private void TogglePostProcess()
		{
			Bloom = !Bloom;
			ChromaticAberration = !ChromaticAberration;
			ColorGrading = !ColorGrading;
			DepthOfField = !DepthOfField;
			GlobalPPVolumeAutoBlend = !GlobalPPVolumeAutoBlend;
			LensDistortion = !LensDistortion;
			PPMovingFilter = !PPMovingFilter;
			Vignette = !Vignette;
		}
		#endif

		private void ToggleFlicker()
		{
			Flicker = !Flicker;
			Fog = !Fog;
			Material = !Material;
			MMBlink = !MMBlink;
			ShaderGlobal = !ShaderGlobal;
			ShaderController = !ShaderController;
			Skybox = !Skybox;
			SpriteRenderer = !SpriteRenderer;
			TextureOffset = !TextureOffset;
			TextureScale = !TextureScale;
		}

		private void ToggleScene()
		{
			LoadScene = !LoadScene;
			UnloadScene = !UnloadScene;
		}

		private void ToggleTime()
		{
			FreezeFrame = !FreezeFrame;
			TimescaleModifier = !TimescaleModifier;
		}

		private void ToggleTransform()
		{
			Destination = !Destination;
			Position = !Position;
			PositionShake = !PositionShake;
			RotatePositionAround = !RotatePositionAround;
			Rotation = !Rotation;
			RotationShake = !RotationShake;
			Scale = !Scale;
			ScaleShake = !ScaleShake;
			SquashAndStretch = !SquashAndStretch;
			Wiggle = !Wiggle;
		}

		private void ToggleUI()
		{
			CanvasGroup = !CanvasGroup;
			CanvasGroupBlocksRaycasts = !CanvasGroupBlocksRaycasts;
			FloatingText = !FloatingText;
			Graphic = !Graphic;
			GraphicCrossFade = !GraphicCrossFade;
			Image = !Image;
			ImageAlpha = !ImageAlpha;
			ImageFill = !ImageFill;
			ImageRaycastTarget = !ImageRaycastTarget;
			ImageTextureOffset = !ImageTextureOffset;
			ImageTextureScale = !ImageTextureScale;
			RectTransformAnchor = !RectTransformAnchor;
			RectTransformOffset = !RectTransformOffset;
			RectTransformPivot = !RectTransformPivot;
			RectTransformSizeDelta = !RectTransformSizeDelta;
			Text = !Text;
			TextColor = !TextColor;
			TextFontSize = !TextFontSize;
			VideoPlayer = !VideoPlayer;
		}
		
		#if MM_TEXTMESHPRO
		private void ToggleTextMeshPro()
		{
			TMPAlpha = !TMPAlpha;
			TMPCharacterSpacing = !TMPCharacterSpacing;
			TMPColor = !TMPColor;
			TMPCountTo = !TMPCountTo;
			TMPDilate = !TMPDilate;
			TMPFontSize = !TMPFontSize;
			TMPLineSpacing = !TMPLineSpacing;
			TMPOutlineColor = !TMPOutlineColor;
			TMPOutlineWidth = !TMPOutlineWidth;
			TMPParagraphSpacing = !TMPParagraphSpacing;
			TMPSoftness = !TMPSoftness;
			TMPText = !TMPText;
			TMPTextReveal = !TMPTextReveal;
			TMPWordSpacing = !TMPWordSpacing;
		}
		#endif
		
		#endregion

		private void Start()
		{
			MMF_Animation.FeedbackTypeAuthorized = AnimationParameter;
			MMFeedbackAnimation.FeedbackTypeAuthorized = AnimationParameter;
			  
			MMF_AnimatorSpeed.FeedbackTypeAuthorized = AnimatorSpeed;
			MMFeedbackAnimatorSpeed.FeedbackTypeAuthorized = AnimatorSpeed;
			  
			MMF_AudioFilterDistortion.FeedbackTypeAuthorized = AudioFilterDistortion;
			MMFeedbackAudioFilterDistortion.FeedbackTypeAuthorized = AudioFilterDistortion;
			  
			MMF_AudioFilterEcho.FeedbackTypeAuthorized = AudioFilterEcho;
			MMFeedbackAudioFilterEcho.FeedbackTypeAuthorized = AudioFilterEcho;
			  
			MMF_AudioFilterHighPass.FeedbackTypeAuthorized = AudioFilterHighPass;
			MMFeedbackAudioFilterHighPass.FeedbackTypeAuthorized = AudioFilterHighPass;
			  
			MMF_AudioFilterLowPass.FeedbackTypeAuthorized = AudioFilterLowPass;
			MMFeedbackAudioFilterLowPass.FeedbackTypeAuthorized = AudioFilterLowPass;
			  
			MMF_AudioFilterReverb.FeedbackTypeAuthorized = AudioFilterReverb;
			MMFeedbackAudioFilterReverb.FeedbackTypeAuthorized = AudioFilterReverb;
			  
			MMF_AudioMixerSnapshotTransition.FeedbackTypeAuthorized = AudioMixerSnapshotTransition;
			MMFeedbackAudioMixerSnapshotTransition.FeedbackTypeAuthorized = AudioMixerSnapshotTransition;
			  
			MMF_AudioSource.FeedbackTypeAuthorized = AudioSource;
			MMFeedbackAudioSource.FeedbackTypeAuthorized = AudioSource;
			  
			MMF_AudioSourcePitch.FeedbackTypeAuthorized = AudioSourcePitch;
			MMFeedbackAudioSourcePitch.FeedbackTypeAuthorized = AudioSourcePitch;
			  
			MMF_AudioSourceStereoPan.FeedbackTypeAuthorized = AudioSourceStereoPan;
			MMFeedbackAudioSourceStereoPan.FeedbackTypeAuthorized = AudioSourceStereoPan;
			  
			MMF_AudioSourceVolume.FeedbackTypeAuthorized = AudioSourceVolume;
			MMFeedbackAudioSourceVolume.FeedbackTypeAuthorized = AudioSourceVolume;
		  
			MMF_Playlist.FeedbackTypeAuthorized = MMPlaylist;
			MMFeedbackPlaylist.FeedbackTypeAuthorized = MMPlaylist;
			  
			MMF_MMSoundManagerAllSoundsControl.FeedbackTypeAuthorized = MMSoundManagerAllSoundsControl;
			MMFeedbackMMSoundManagerAllSoundsControl.FeedbackTypeAuthorized = MMSoundManagerAllSoundsControl;
		  
			MMF_MMSoundManagerSaveLoad.FeedbackTypeAuthorized = MMSoundManagerSaveAndLoad;
			MMFeedbackMMSoundManagerSaveLoad.FeedbackTypeAuthorized = MMSoundManagerSaveAndLoad;
			  
			MMF_MMSoundManagerSound.FeedbackTypeAuthorized = MMSoundManagerSound;
			MMFeedbackMMSoundManagerSound.FeedbackTypeAuthorized = MMSoundManagerSound;
			  
			MMF_MMSoundManagerSoundControl.FeedbackTypeAuthorized = MMSoundManagerSoundControl;
			MMFeedbackMMSoundManagerSoundControl.FeedbackTypeAuthorized = MMSoundManagerSoundControl;
			  
			MMF_MMSoundManagerSoundFade.FeedbackTypeAuthorized = MMSoundManagerSoundFade;
			MMFeedbackMMSoundManagerSoundFade.FeedbackTypeAuthorized = MMSoundManagerSoundFade;
			  
			MMF_MMSoundManagerTrackControl.FeedbackTypeAuthorized = MMSoundManagerTrackControl;
			MMFeedbackMMSoundManagerTrackControl.FeedbackTypeAuthorized = MMSoundManagerTrackControl;
			  
			MMF_MMSoundManagerTrackFade.FeedbackTypeAuthorized = MMSoundManagerTrackFade;
			MMFeedbackMMSoundManagerTrackFade.FeedbackTypeAuthorized = MMSoundManagerTrackFade;
			  
			MMF_Sound.FeedbackTypeAuthorized = Sound;
			MMFeedbackSound.FeedbackTypeAuthorized = Sound;
			  
			MMF_CameraShake.FeedbackTypeAuthorized = CameraShake;
			MMFeedbackCameraShake.FeedbackTypeAuthorized = CameraShake;
			  
			MMF_CameraZoom.FeedbackTypeAuthorized = CameraZoom;
			MMFeedbackCameraZoom.FeedbackTypeAuthorized = CameraZoom;
		  
			#if MM_CINEMACHINE
			MMF_CinemachineImpulse.FeedbackTypeAuthorized = CinemachineImpulse;
			MMFeedbackCinemachineImpulse.FeedbackTypeAuthorized = CinemachineImpulse;
			  
			MMF_CinemachineImpulseClear.FeedbackTypeAuthorized = CinemachineImpulseClear;
			MMFeedbackCinemachineImpulseClear.FeedbackTypeAuthorized = CinemachineImpulseClear;
			  
			MMF_CinemachineImpulseSource.FeedbackTypeAuthorized = CinemachineImpulseSource;
			  
			MMF_CinemachineTransition.FeedbackTypeAuthorized = CinemachineTransition;
			MMFeedbackCinemachineTransition.FeedbackTypeAuthorized = CinemachineTransition;
			#endif
		  
			MMF_CameraClippingPlanes.FeedbackTypeAuthorized = ClippingPlanes;
			MMFeedbackCameraClippingPlanes.FeedbackTypeAuthorized = ClippingPlanes;
			  
			MMF_Fade.FeedbackTypeAuthorized = Fade;
			MMFeedbackFade.FeedbackTypeAuthorized = Fade;
		  
			MMF_CameraFieldOfView.FeedbackTypeAuthorized = FieldOfView;
			MMFeedbackCameraFieldOfView.FeedbackTypeAuthorized = FieldOfView;
			  
			MMF_Flash.FeedbackTypeAuthorized = Flash;
			MMFeedbackFlash.FeedbackTypeAuthorized = Flash;
		  
			MMF_CameraOrthographicSize.FeedbackTypeAuthorized = OrthographicSize;
			MMFeedbackCameraOrthographicSize.FeedbackTypeAuthorized = OrthographicSize;
			  
			MMF_DebugComment.FeedbackTypeAuthorized = Comment;
			MMFeedbackDebugComment.FeedbackTypeAuthorized = Comment;
			  
			MMF_DebugLog.FeedbackTypeAuthorized = Log;
			MMFeedbackDebugLog.FeedbackTypeAuthorized = Log;
			  
			MMF_MMGameEvent.FeedbackTypeAuthorized = MMGameEvent;
			MMFeedbackMMGameEvent.FeedbackTypeAuthorized = MMGameEvent;
		  
			MMF_Events.FeedbackTypeAuthorized = UnityEvents;
			MMFeedbackEvents.FeedbackTypeAuthorized = UnityEvents;
		  
			MMF_Broadcast.FeedbackTypeAuthorized = Broadcast;
			MMFeedbackBroadcast.FeedbackTypeAuthorized = Broadcast;
		  
			MMF_Collider.FeedbackTypeAuthorized = Collider;
			MMFeedbackCollider.FeedbackTypeAuthorized = Collider;
			  
			MMF_Collider2D.FeedbackTypeAuthorized = Collider2D;
			MMFeedbackCollider2D.FeedbackTypeAuthorized = Collider2D;
			  
			MMF_Destroy.FeedbackTypeAuthorized = DestroyTargetObject;
			MMFeedbackDestroy.FeedbackTypeAuthorized = DestroyTargetObject;
			  
			MMF_Enable.FeedbackTypeAuthorized = EnableBehaviour;
			MMFeedbackEnable.FeedbackTypeAuthorized = EnableBehaviour;
			  
			MMF_FloatController.FeedbackTypeAuthorized = FloatController;
			MMFeedbackFloatController.FeedbackTypeAuthorized = FloatController;
			  
			MMF_InstantiateObject.FeedbackTypeAuthorized = InstantiateObject;
			MMFeedbackInstantiateObject.FeedbackTypeAuthorized = InstantiateObject;
		  
			MMF_RadioSignal.FeedbackTypeAuthorized = MMRadioSignal;
			MMFeedbackRadioSignal.FeedbackTypeAuthorized = MMRadioSignal;
		  
			MMF_Rigidbody.FeedbackTypeAuthorized = Rigidbody;
			MMFeedbackRigidbody.FeedbackTypeAuthorized = Rigidbody;
			  
			MMF_Rigidbody2D.FeedbackTypeAuthorized = Rigidbody2D;
			MMFeedbackRigidbody2D.FeedbackTypeAuthorized = Rigidbody2D;
			  
			MMF_SetActive.FeedbackTypeAuthorized = SetActive;
			MMFeedbackSetActive.FeedbackTypeAuthorized = SetActive;
		  
			#if MOREMOUNTAINS_NICEVIBRATIONS_INSTALLED
			MMF_Haptics.FeedbackTypeAuthorized = HapticClip;
			MMFeedbackHaptics.FeedbackTypeAuthorized = HapticClip;

			MMF_NVContinuous.FeedbackTypeAuthorized = HapticContinuous;
			MMFeedbackNVContinuous.FeedbackTypeAuthorized = HapticContinuous;
			  
			MMF_NVControl.FeedbackTypeAuthorized = HapticControl;
			MMFeedbackNVControl.FeedbackTypeAuthorized = HapticControl;
			  
			MMF_NVEmphasis.FeedbackTypeAuthorized = HapticEmphasis;
			MMFeedbackNVEmphasis.FeedbackTypeAuthorized = HapticEmphasis;
			  
			MMF_NVPreset.FeedbackTypeAuthorized = HapticPreset;
			MMFeedbackNVPreset.FeedbackTypeAuthorized = HapticPreset;
			#endif
  
			MMF_Light.FeedbackTypeAuthorized = Light;
			MMFeedbackLight.FeedbackTypeAuthorized = Light;
		  
			MMF_Looper.FeedbackTypeAuthorized = Looper;
			MMFeedbackLooper.FeedbackTypeAuthorized = Looper;
			  
			MMF_LooperStart.FeedbackTypeAuthorized = LooperStart;
			MMFeedbackLooperStart.FeedbackTypeAuthorized = LooperStart;
			  
			MMF_ParticlesInstantiation.FeedbackTypeAuthorized = ParticlesInstantiation;
			MMFeedbackParticlesInstantiation.FeedbackTypeAuthorized = ParticlesInstantiation;
			  
			MMF_Particles.FeedbackTypeAuthorized = ParticlesPlay;
			MMFeedbackParticles.FeedbackTypeAuthorized = ParticlesPlay;
			  
			MMF_HoldingPause.FeedbackTypeAuthorized = HoldingPause;
			MMFeedbackHoldingPause.FeedbackTypeAuthorized = HoldingPause;
		  
			MMF_Pause.FeedbackTypeAuthorized = Pause;
			MMFeedbackPause.FeedbackTypeAuthorized = Pause;

			MMF_Flicker.FeedbackTypeAuthorized = Flicker;
			MMFeedbackFlicker.FeedbackTypeAuthorized = Flicker;
			  
			MMF_Fog.FeedbackTypeAuthorized = Fog;
			MMFeedbackFog.FeedbackTypeAuthorized = Fog;
			  
			MMF_Material.FeedbackTypeAuthorized = Material;
			MMFeedbackMaterial.FeedbackTypeAuthorized = Material;
		  
			MMF_Blink.FeedbackTypeAuthorized = MMBlink;
			MMFeedbackBlink.FeedbackTypeAuthorized = MMBlink;
			  
			MMF_ShaderGlobal.FeedbackTypeAuthorized = ShaderGlobal;
			MMFeedbackShaderGlobal.FeedbackTypeAuthorized = ShaderGlobal;
			  
			MMF_ShaderController.FeedbackTypeAuthorized = ShaderController;
			MMFeedbackShaderController.FeedbackTypeAuthorized = ShaderController;
			  
			MMF_Skybox.FeedbackTypeAuthorized = Skybox;
			MMFeedbackSkybox.FeedbackTypeAuthorized = Skybox;
			  
			MMF_SpriteRenderer.FeedbackTypeAuthorized = SpriteRenderer;
			MMFeedbackSpriteRenderer.FeedbackTypeAuthorized = SpriteRenderer;
			  
			MMF_TextureOffset.FeedbackTypeAuthorized = TextureOffset;
			MMFeedbackTextureOffset.FeedbackTypeAuthorized = TextureOffset;
			  
			MMF_TextureScale.FeedbackTypeAuthorized = TextureScale;
			MMFeedbackTextureScale.FeedbackTypeAuthorized = TextureScale;
			  
			MMF_LoadScene.FeedbackTypeAuthorized = LoadScene;
			MMFeedbackLoadScene.FeedbackTypeAuthorized = LoadScene;
		  
			MMF_UnloadScene.FeedbackTypeAuthorized = UnloadScene;
			MMFeedbackUnloadScene.FeedbackTypeAuthorized = UnloadScene;
		  
			MMF_FreezeFrame.FeedbackTypeAuthorized = FreezeFrame;
			MMFeedbackFreezeFrame.FeedbackTypeAuthorized = FreezeFrame;
			  
			MMF_TimescaleModifier.FeedbackTypeAuthorized = TimescaleModifier;
			MMFeedbackTimescaleModifier.FeedbackTypeAuthorized = TimescaleModifier;
			  
			MMF_DestinationTransform.FeedbackTypeAuthorized = Destination;
			MMFeedbackDestinationTransform.FeedbackTypeAuthorized = Destination;
			  
			MMF_Position.FeedbackTypeAuthorized = Position;
			MMFeedbackPosition.FeedbackTypeAuthorized = Position;
			  
			MMF_PositionShake.FeedbackTypeAuthorized = PositionShake;
			  
			MMF_RotatePositionAround.FeedbackTypeAuthorized = RotatePositionAround;
			  
			MMF_Rotation.FeedbackTypeAuthorized = Rotation;
			MMFeedbackRotation.FeedbackTypeAuthorized = Rotation;
			  
			MMF_RotationShake.FeedbackTypeAuthorized = RotationShake;
			  
			MMF_Scale.FeedbackTypeAuthorized = Scale;
			MMFeedbackScale.FeedbackTypeAuthorized = Scale;
			  
			MMF_ScaleShake.FeedbackTypeAuthorized = ScaleShake;
			  
			MMF_SquashAndStretch.FeedbackTypeAuthorized = SquashAndStretch;
			MMFeedbackSquashAndStretch.FeedbackTypeAuthorized = SquashAndStretch;
			  
			MMF_Wiggle.FeedbackTypeAuthorized = Wiggle;
			MMFeedbackWiggle.FeedbackTypeAuthorized = Wiggle;
		  
			MMF_CanvasGroup.FeedbackTypeAuthorized = CanvasGroup;
			MMFeedbackCanvasGroup.FeedbackTypeAuthorized = CanvasGroup;
		  
			MMF_CanvasGroupBlocksRaycasts.FeedbackTypeAuthorized = CanvasGroupBlocksRaycasts;
			MMFeedbackCanvasGroupBlocksRaycasts.FeedbackTypeAuthorized = CanvasGroupBlocksRaycasts;
			  
			MMF_FloatingText.FeedbackTypeAuthorized = FloatingText;
			MMFeedbackFloatingText.FeedbackTypeAuthorized = FloatingText;
			  
			MMF_Graphic.FeedbackTypeAuthorized = Graphic;
			  
			MMF_GraphicCrossFade.FeedbackTypeAuthorized = GraphicCrossFade;
			  
			MMF_Image.FeedbackTypeAuthorized = Image;
			MMFeedbackImage.FeedbackTypeAuthorized = Image;
			  
			MMF_ImageAlpha.FeedbackTypeAuthorized = ImageAlpha;
			MMFeedbackImageAlpha.FeedbackTypeAuthorized = ImageAlpha;
			  
			MMF_ImageFill.FeedbackTypeAuthorized = ImageFill;
			  
			MMF_ImageRaycastTarget.FeedbackTypeAuthorized = ImageRaycastTarget;
			MMFeedbackImageRaycastTarget.FeedbackTypeAuthorized = ImageRaycastTarget;
			  
			MMF_ImageTextureOffset.FeedbackTypeAuthorized = ImageTextureOffset;
			  
			MMF_ImageTextureScale.FeedbackTypeAuthorized = ImageTextureScale;
			
			MMF_RectTransformAnchor.FeedbackTypeAuthorized = RectTransformAnchor;
			MMFeedbackRectTransformAnchor.FeedbackTypeAuthorized = RectTransformAnchor;
		  
			MMF_RectTransformOffset.FeedbackTypeAuthorized = RectTransformOffset;
			MMFeedbackRectTransformOffset.FeedbackTypeAuthorized = RectTransformOffset;
		  
			MMF_RectTransformPivot.FeedbackTypeAuthorized = RectTransformPivot;
			MMFeedbackRectTransformPivot.FeedbackTypeAuthorized = RectTransformPivot;
		  
			MMF_RectTransformSizeDelta.FeedbackTypeAuthorized = RectTransformSizeDelta;
			MMFeedbackRectTransformSizeDelta.FeedbackTypeAuthorized = RectTransformSizeDelta;
		  
			MMF_Text.FeedbackTypeAuthorized = Text;
			MMFeedbackText.FeedbackTypeAuthorized = Text;
			  
			MMF_TextColor.FeedbackTypeAuthorized = TextColor;
			MMFeedbackTextColor.FeedbackTypeAuthorized = TextColor;
		  
			MMF_TextFontSize.FeedbackTypeAuthorized = TextFontSize;
			MMFeedbackTextFontSize.FeedbackTypeAuthorized = TextFontSize;
		  
			MMF_VideoPlayer.FeedbackTypeAuthorized = VideoPlayer;
			MMFeedbackVideoPlayer.FeedbackTypeAuthorized = VideoPlayer;
			
			#if MM_POSTPROCESSING
			MMF_Bloom.FeedbackTypeAuthorized = Bloom;
			MMFeedbackBloom.FeedbackTypeAuthorized = Bloom;
			  
			MMF_ChromaticAberration.FeedbackTypeAuthorized = ChromaticAberration;
			MMFeedbackChromaticAberration.FeedbackTypeAuthorized = ChromaticAberration;
			  
			MMF_ColorGrading.FeedbackTypeAuthorized = ColorGrading;
			MMFeedbackColorGrading.FeedbackTypeAuthorized = ColorGrading;
			  
			MMF_DepthOfField.FeedbackTypeAuthorized = DepthOfField;
			MMFeedbackDepthOfField.FeedbackTypeAuthorized = DepthOfField;
		  
			MMF_GlobalPPVolumeAutoBlend.FeedbackTypeAuthorized = GlobalPPVolumeAutoBlend;
			MMFeedbackGlobalPPVolumeAutoBlend.FeedbackTypeAuthorized = GlobalPPVolumeAutoBlend;
			  
			MMF_LensDistortion.FeedbackTypeAuthorized = LensDistortion;
			MMFeedbackLensDistortion.FeedbackTypeAuthorized = LensDistortion;
			  
			MMF_Vignette.FeedbackTypeAuthorized = Vignette;
			MMFeedbackVignette.FeedbackTypeAuthorized = Vignette;
			  
			MMF_PPMovingFilter.FeedbackTypeAuthorized = PPMovingFilter;
			MMFeedbackPPMovingFilter.FeedbackTypeAuthorized = PPMovingFilter;
			#endif
			
			#if MM_HDRP
			
			MMF_Bloom_HDRP.FeedbackTypeAuthorized = Bloom;
			MMF_ChromaticAberration_HDRP.FeedbackTypeAuthorized = ChromaticAberration;
			MMF_LensDistortion_HDRP.FeedbackTypeAuthorized = LensDistortion;
			MMF_ColorAdjustments_HDRP.FeedbackTypeAuthorized = ColorGrading;
			MMF_LensDistortion_HDRP.FeedbackTypeAuthorized = LensDistortion;
			MMF_Vignette_HDRP.FeedbackTypeAuthorized = Vignette;
			
			#endif
			
			#if MM_URP
			
			MMF_Bloom_URP.FeedbackTypeAuthorized = Bloom;
			MMF_ChromaticAberration_URP.FeedbackTypeAuthorized = ChromaticAberration;
			MMF_LensDistortion_URP.FeedbackTypeAuthorized = LensDistortion;
			MMF_ColorAdjustments_URP.FeedbackTypeAuthorized = ColorGrading;
			MMF_LensDistortion_URP.FeedbackTypeAuthorized = LensDistortion;
			MMF_Vignette_URP.FeedbackTypeAuthorized = Vignette;
			
			#endif
			
			#if MM_TEXTMESHPRO
			MMF_TMPAlpha.FeedbackTypeAuthorized = TMPAlpha;
			MMFeedbackTMPAlpha.FeedbackTypeAuthorized = TMPAlpha;
			  
			MMF_TMPCharacterSpacing.FeedbackTypeAuthorized = TMPCharacterSpacing;
			MMFeedbackTMPCharacterSpacing.FeedbackTypeAuthorized = TMPCharacterSpacing;
			  
			MMF_TMPColor.FeedbackTypeAuthorized = TMPColor;
			MMFeedbackTMPColor.FeedbackTypeAuthorized = TMPColor;
			  
			MMF_TMPCountTo.FeedbackTypeAuthorized = TMPCountTo;
			  
			MMF_TMPDilate.FeedbackTypeAuthorized = TMPDilate;
			MMFeedbackTMPDilate.FeedbackTypeAuthorized = TMPDilate;
			  
			MMF_TMPFontSize.FeedbackTypeAuthorized = TMPFontSize;
			MMFeedbackTMPFontSize.FeedbackTypeAuthorized = TMPFontSize;
			  
			MMF_TMPLineSpacing.FeedbackTypeAuthorized = TMPLineSpacing;
			MMFeedbackTMPLineSpacing.FeedbackTypeAuthorized = TMPLineSpacing;
			  
			MMF_TMPOutlineColor.FeedbackTypeAuthorized = TMPOutlineColor;
			MMFeedbackTMPOutlineColor.FeedbackTypeAuthorized = TMPOutlineColor;
			  
			MMF_TMPOutlineWidth.FeedbackTypeAuthorized = TMPOutlineWidth;
			MMFeedbackTMPOutlineWidth.FeedbackTypeAuthorized = TMPOutlineWidth;
			  
			MMF_TMPParagraphSpacing.FeedbackTypeAuthorized = TMPParagraphSpacing;
			MMFeedbackTMPParagraphSpacing.FeedbackTypeAuthorized = TMPParagraphSpacing;
			  
			MMF_TMPSoftness.FeedbackTypeAuthorized = TMPSoftness;
			MMFeedbackTMPSoftness.FeedbackTypeAuthorized = TMPSoftness;
		  
			MMF_TMPText.FeedbackTypeAuthorized = TMPText;
			MMFeedbackTMPText.FeedbackTypeAuthorized = TMPText;
			  
			MMF_TMPTextReveal.FeedbackTypeAuthorized = TMPTextReveal;
			MMFeedbackTMPTextReveal.FeedbackTypeAuthorized = TMPTextReveal;
			#endif
		}
	}

}