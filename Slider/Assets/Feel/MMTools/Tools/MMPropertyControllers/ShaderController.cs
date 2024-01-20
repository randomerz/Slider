using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using UnityEngine.UI;

namespace MoreMountains.Tools
{

	/// <summary>
	/// A class used to control a float in any other class, over time
	/// To use it, simply drag a monobehaviour in its target field, pick a control mode (ping pong or random), and tweak the settings
	/// </summary>
	[MMRequiresConstantRepaint]
	[AddComponentMenu("More Mountains/Tools/Property Controllers/ShaderController")]
	public class ShaderController : MMMonoBehaviour
	{
		/// the possible types of targets
		public enum TargetTypes { Renderer, Image, RawImage, Text }
		/// the possible types of properties
		public enum PropertyTypes { Bool, Float, Int, Vector, Keyword, Color }
		/// the possible control modes
		public enum ControlModes { PingPong, Random, OneTime, AudioAnalyzer, ToDestination, Driven, Loop }
		/// the possible color modes on which to interpolate colors
		public enum ColorModes { TwoColors, ColorRamp }

		[Header("Target")]
		/// the type of renderer to pilot
		[Tooltip("the type of renderer to pilot")]
		public TargetTypes TargetType = TargetTypes.Renderer;
		/// the renderer with the shader you want to control
		[Tooltip("the renderer with the shader you want to control")]
		[MMEnumCondition("TargetType",(int)TargetTypes.Renderer)]
		public Renderer TargetRenderer;
		/// the ID of the material in the Materials array on the target renderer (usually 0)
		[Tooltip("the ID of the material in the Materials array on the target renderer (usually 0)")]
		[MMEnumCondition("TargetType", (int)TargetTypes.Renderer)]
		public int TargetMaterialID = 0;
		/// the Image with the shader you want to control
		[Tooltip("the Image with the shader you want to control")]
		[MMEnumCondition("TargetType", (int)TargetTypes.Image)]
		public Image TargetImage;
		/// if this is true, the 'materialForRendering' for this Image will be used, instead of the regular material
		[Tooltip("if this is true, the 'materialForRendering' for this Image will be used, instead of the regular material")]
		[MMEnumCondition("TargetType", (int)TargetTypes.Image)]
		public bool UseMaterialForRendering = false;
		/// the RawImage with the shader you want to control
		[Tooltip("the RawImage with the shader you want to control")]
		[MMEnumCondition("TargetType", (int)TargetTypes.RawImage)]
		public RawImage TargetRawImage;
		/// the Text with the shader you want to control
		[Tooltip("the Text with the shader you want to control")]
		[MMEnumCondition("TargetType", (int)TargetTypes.Text)]
		public Text TargetText;
		/// if this is true, material will be cached on Start
		[Tooltip("if this is true, material will be cached on Start")]
		public bool CacheMaterial = true;
		/// if this is true, an instance of the material will be created on start so that this controller only affects its target
		[Tooltip("if this is true, an instance of the material will be created on start so that this controller only affects its target")]
		public bool CreateMaterialInstance = false;
		/// the EXACT name of the property to affect
		[Tooltip("the EXACT name of the property to affect")]
		public string TargetPropertyName;
		/// the type of the property to affect
		[Tooltip("the type of the property to affect")]
		public PropertyTypes PropertyType = PropertyTypes.Float;
		/// whether or not to affect its x component
		[Tooltip("whether or not to affect its x component")]
		[MMEnumCondition("PropertyType", (int)PropertyTypes.Vector)]
		public bool X;
		/// whether or not to affect its y component
		[Tooltip("whether or not to affect its y component")]
		[MMEnumCondition("PropertyType", (int)PropertyTypes.Vector)]
		public bool Y;
		/// whether or not to affect its z component
		[Tooltip("whether or not to affect its z component")]
		[MMEnumCondition("PropertyType", (int)PropertyTypes.Vector)]
		public bool Z;
		/// whether or not to affect its w component
		[Tooltip("whether or not to affect its w component")]
		[MMEnumCondition("PropertyType", (int)PropertyTypes.Vector)]
		public bool W;

		[Header("Color")]
		/// whether to move from a color to another, or to evalute colors on a ramp
		[Tooltip("whether to move from a color to another, or to evalute colors on a ramp")]
		public ColorModes ColorMode = ColorModes.TwoColors;
		/// the ramp along which to lerp when in ramp color mode
		[Tooltip("the ramp along which to lerp when in ramp color mode")]
		[GradientUsage(true)]
		public Gradient ColorRamp;
		/// the color to lerp from	
		[Tooltip("the color to lerp from")]
		[ColorUsage(true, true)]
		public Color FromColor = Color.black;
		/// the color to lerp to	
		[Tooltip("the color to lerp to")]
		[ColorUsage(true, true)]
		public Color ToColor = Color.white;

		[Header("Global Settings")]
		/// the control mode (ping pong or random)
		[Tooltip("the control mode (ping pong or random)")]
		public ControlModes ControlMode;
		/// whether or not the updated value should be added to the initial one
		[Tooltip("whether or not the updated value should be added to the initial one")]
		public bool AddToInitialValue = false;
		/// whether or not to use unscaled time
		[Tooltip("whether or not to use unscaled time")]
		public bool UseUnscaledTime = true;
		/// whether or not you want to revert to the InitialValue after the control ends
		[Tooltip("whether or not you want to revert to the InitialValue after the control ends")]
		public bool RevertToInitialValueAfterEnd = true;
		/// if this is true, this component will use material property blocks instead of working on an instance of the material.
		[Tooltip("if this is true, this component will use material property blocks instead of working on an instance of the material.")] 
		[MMEnumCondition("TargetType", (int)TargetTypes.Renderer)]
		public bool UseMaterialPropertyBlocks = false;
		/// if using material property blocks on a sprite renderer, you'll want to make sure the sprite texture gets passed to the block when updating it. For that, you need to specify your sprite's material's shader's texture property name. If you're not working with a sprite renderer, you can safely ignore this.
		[Tooltip("if using material property blocks on a sprite renderer, you'll want to make sure the sprite texture gets passed to the block when updating it. For that, you need to specify your sprite's material's shader's texture property name. If you're not working with a sprite renderer, you can safely ignore this.")]
		[MMCondition("UseMaterialPropertyBlocks", true)]
		public string SpriteRendererTextureProperty = "_MainTex";
		/// whether or not to perform extra safety checks (safer, more costly)
		[Tooltip("whether or not to perform extra safety checks (safer, more costly)")]
		public bool SafeMode = false;

		[Header("Ping Pong")]
		/// the curve to apply to the tween
		[Tooltip("the curve to apply to the tween")]
		public MMTweenType Curve;
		/// the minimum value for the ping pong
		[Tooltip("the minimum value for the ping pong")]
		public float MinValue = 0f;
		/// the maximum value for the ping pong
		[Tooltip("the maximum value for the ping pong")]
		public float MaxValue = 5f;
		/// the duration of one ping (or pong)
		[Tooltip("the duration of one ping (or pong)")]
		public float Duration = 1f;
		/// the duration of the pause between two ping (or pongs) (in seconds)
		[Tooltip("the duration of the pause between two ping (or pongs) (in seconds)")]
		public float PingPongPauseDuration = 1f;

		[Header("Loop")]
		/// the curve to apply to the tween
		[Tooltip("the curve to apply to the tween")]
		public MMTweenType LoopCurve;
		/// the start value for the loop tween
		[Tooltip("the start value for the loop tween")]
		public float LoopStartValue = 0f;
		/// the end value for the loop tween
		[Tooltip("the end value for the loop tween")]
		public float LoopEndValue = 5f;
		/// the duration of one loop
		[Tooltip("the duration of one loop")]
		public float LoopDuration = 1f;
		/// the duration of the pause between two loops (in seconds)
		[Tooltip("the duration of the pause between two loops (in seconds)")]
		public float LoopPauseDuration = 1f;

		[Header("Driven")]
		/// the value that will be applied to the controlled float in driven mode 
		[Tooltip("the value that will be applied to the controlled float in driven mode")]
		public float DrivenLevel = 0f;

		[Header("Random")]
		/// the noise amplitude
		[Tooltip("the noise amplitude")]
		[MMVector("Min", "Max")]
		public Vector2 Amplitude = new Vector2(0f,5f);
		/// the noise frequency
		[Tooltip("the noise frequency")]
		[MMVector("Min", "Max")]
		public Vector2 Frequency = new Vector2(1f, 1f);
		/// the noise shift
		[Tooltip("the noise shift")]
		[MMVector("Min", "Max")]
		public Vector2 Shift = new Vector2(0f, 1f);

		/// if this is true, will let you remap the noise value (without amplitude) to the bounds you've specified
		[Tooltip("if this is true, will let you remap the noise value (without amplitude) to the bounds you've specified")]
		public bool RemapNoiseValues = false;
		/// the value to which to remap the random's zero bound
		[Tooltip("the value to which to remap the random's zero bound")]
		[MMCondition("RemapNoiseValues", true)]
		public float RemapNoiseZero = 0f;
		/// the value to which to remap the random's one bound
		[Tooltip("the value to which to remap the random's one bound")]
		[MMCondition("RemapNoiseValues", true)]
		public float RemapNoiseOne = 1f;
        
		[Header("OneTime")]
		/// the duration of the One Time shake
		[Tooltip("the duration of the One Time shake")]
		public float OneTimeDuration = 1f;
		/// the amplitude of the One Time shake (this will be multiplied by the curve's height)
		[Tooltip("the amplitude of the One Time shake (this will be multiplied by the curve's height)")]
		public float OneTimeAmplitude = 1f;
		/// the low value to remap the normalized curve value to 
		[Tooltip("the low value to remap the normalized curve value to")]
		public float OneTimeRemapMin = 0f;
		/// the high value to remap the normalized curve value to 
		[Tooltip("the high value to remap the normalized curve value to")]
		public float OneTimeRemapMax = 1f;
		/// the curve to apply to the one time shake
		[Tooltip("the curve to apply to the one time shake")]
		public AnimationCurve OneTimeCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		[MMInspectorButton("OneTime")]
		/// a test button for the one time shake
		[Tooltip("a test button for the one time shake")]
		public bool OneTimeButton;
		/// whether or not this controller should go back to sleep after a OneTime
		[Tooltip("whether or not this controller should go back to sleep after a OneTime")]
		public bool DisableAfterOneTime = false;
		/// whether or not this controller should go back to sleep after a OneTime
		[Tooltip("whether or not this controller should go back to sleep after a OneTime")]
		public bool DisableGameObjectAfterOneTime = false;

		[Header("AudioAnalyzer")]
		/// the bound audio analyzer used to drive this controller
		[Tooltip("the bound audio analyzer used to drive this controller")]
		public MMAudioAnalyzer AudioAnalyzer;
		/// the ID of the selected beat on the analyzer
		[Tooltip("the ID of the selected beat on the analyzer")]
		public int BeatID;
		/// the multiplier to apply to the value out of the analyzer
		[Tooltip("the multiplier to apply to the value out of the analyzer")]
		public float AudioAnalyzerMultiplier = 1f;
		/// the offset to apply to the value out of the analyzer
		[Tooltip("the offset to apply to the value out of the analyzer")]
		public float AudioAnalyzerOffset = 0f;
		/// the speed at which to lerp the value
		[Tooltip("the speed at which to lerp the value")]
		public float AudioAnalyzerLerp = 60f;

		[Header("ToDestination")]
		/// the value to go to when in ToDestination mode
		[Tooltip("the value to go to when in ToDestination mode")]
		public float ToDestinationValue = 1f;
		/// the duration of the ToDestination tween
		[Tooltip("the duration of the ToDestination tween")]
		public float ToDestinationDuration = 1f;
		/// the curve to use to tween to the ToDestination value
		[Tooltip("the curve to use to tween to the ToDestination value")]
		public AnimationCurve ToDestinationCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 0.6f), new Keyframe(1f, 1f));
		/// a test button for the one time shake
		[Tooltip("a test button for the one time shake")]
		[MMInspectorButton("ToDestination")]
		public bool ToDestinationButton;
		/// whether or not this controller should go back to sleep after a OneTime
		[Tooltip("whether or not this controller should go back to sleep after a OneTime")]
		public bool DisableAfterToDestination = false;

		[Header("Debug")]
		/// the initial value of the controlled float
		[Tooltip("the initial value of the controlled float")]
		[MMReadOnly]
		public float InitialValue;
		/// the current value of the controlled float
		[Tooltip("the current value of the controlled float")]
		[MMReadOnly]
		public float CurrentValue;
		/// the current value of the controlled float, normalized
		[Tooltip("the current value of the controlled float, normalized")]
		[MMReadOnly]
		public float CurrentValueNormalized = 0f;
		/// the current value of the controlled float	
		[Tooltip("the current value of the controlled float")]
		[MMReadOnly]
		public Color InitialColor;

		/// the ID of the property
		[Tooltip("the ID of the property")]
		[MMReadOnly]
		public int PropertyID;
		/// whether or not the property got found
		[Tooltip("whether or not the property got found")]
		[MMReadOnly]
		public bool PropertyFound = false;
		/// the target material
		[Tooltip("the target material")]
		[MMReadOnly]
		public Material TargetMaterial;

		/// internal use only
		[HideInInspector]
		public float PingPong;
		/// internal use only
		[HideInInspector]
		public float LoopTime;
        
		protected float _randomAmplitude;
		protected float _randomFrequency;
		protected float _randomShift;
		protected float _elapsedTime = 0f;
		protected bool _shaking = false;
		protected float _startedTimestamp = 0f;
		protected float _remappedTimeSinceStart = 0f;
		protected Color _currentColor;
		protected Vector4 _vectorValue;
		protected float _pingPongDirection = 1f;
		protected float _lastPingPongPauseAt = 0f;
		protected float _lastLoopPauseAt = 0f;
		protected float _initialValue = 0f;
		protected Color _fromColorStorage;
		protected bool _activeLastFrame = false;
		protected MaterialPropertyBlock _propertyBlock;
		protected SpriteRenderer _spriteRenderer;
		protected Texture2D _spriteRendererTexture;
		protected bool SpriteRendererIsNull;

		/// <summary>
		/// Finds an attribute (property or field) on the target object
		/// </summary>
		/// <param name="propertyName"></param>
		/// <returns></returns>
		public virtual bool FindShaderProperty(string propertyName)
		{
			if (TargetType == TargetTypes.Renderer)
			{
				if (CreateMaterialInstance)
				{
					TargetRenderer.materials[TargetMaterialID] = new Material(TargetRenderer.materials[TargetMaterialID]);
				}
				TargetMaterial = UseMaterialPropertyBlocks ? TargetRenderer.sharedMaterials[TargetMaterialID] : TargetRenderer.materials[TargetMaterialID];
			}
			else if (TargetType == TargetTypes.Image)
			{
				if (CreateMaterialInstance)
				{
					TargetImage.material = new Material(TargetImage.material);
				}
				TargetMaterial = TargetImage.material;
			}
			else if (TargetType == TargetTypes.RawImage)
			{
				if (CreateMaterialInstance)
				{
					TargetRawImage.material = new Material(TargetRawImage.material);
				}
				TargetMaterial = TargetRawImage.material;
			}
			else if (TargetType == TargetTypes.Text)
			{
				if (CreateMaterialInstance)
				{
					TargetText.material = new Material(TargetText.material);
				}
				TargetMaterial = TargetText.material;
			}

			if (PropertyType == PropertyTypes.Keyword)
			{
				PropertyFound = true;
				return true;
			}
			if (TargetMaterial.HasProperty(propertyName))
			{                
				PropertyID = Shader.PropertyToID(propertyName);
				PropertyFound = true;
				return true;
			}
			return false;
		}

		/// <summary>
		/// On start we initialize our controller
		/// </summary>
		protected virtual void Awake()
		{
			Initialization();
		}

		/// <summary>
		/// On enable, grabs the initial value
		/// </summary>
		protected virtual void OnEnable()
		{
			InitialValue = GetInitialValue();
			if (PropertyType == PropertyTypes.Color)
			{
				InitialColor = TargetMaterial.GetColor(PropertyID);
			}
		}

		/// <summary>
		/// Returns true if the renderer is null, false otherwise
		/// </summary>
		/// <returns></returns>
		protected virtual bool RendererIsNull()
		{
			if ((TargetType == TargetTypes.Renderer) && (TargetRenderer == null))
			{
				return true;
			}
			if ((TargetType == TargetTypes.Image) && (TargetImage == null))
			{
				return true;
			}
			if ((TargetType == TargetTypes.RawImage) && (TargetRawImage == null))
			{
				return true;
			}
			if ((TargetType == TargetTypes.Text) && (TargetText == null))
			{
				return true;
			}
			return false;
		}

		/// <summary>
		/// Grabs the target property and initializes stuff
		/// </summary>
		public virtual void Initialization()
		{
			if (RendererIsNull() || (string.IsNullOrEmpty(TargetPropertyName)))
			{
				return;
			}
			if (TargetType != TargetTypes.Renderer)
			{
				UseMaterialPropertyBlocks = false;
			}

			StoreSpriteRenderer();
            
			PropertyFound = FindShaderProperty(TargetPropertyName);
			if (!PropertyFound)
			{
				return;
			}

			_elapsedTime = 0f;
			_randomAmplitude = Random.Range(Amplitude.x, Amplitude.y);
			_randomFrequency = Random.Range(Frequency.x, Frequency.y);
			_randomShift = Random.Range(Shift.x, Shift.y);
            
			if ((TargetType == TargetTypes.Renderer) && UseMaterialPropertyBlocks)
			{
				_propertyBlock = new MaterialPropertyBlock();
				TargetRenderer.GetPropertyBlock(_propertyBlock, TargetMaterialID);
			}

			InitialValue = GetInitialValue();
			if (PropertyType == PropertyTypes.Color)
			{
				InitialColor = TargetMaterial.GetColor(PropertyID);
			}
                
			_shaking = false;
			if (ControlMode == ControlModes.OneTime)
			{
				this.enabled = false;
			}
			StoreSpriteRendererTexture();
		}

		/// <summary>
		/// Stores the sprite renderer and a test for it
		/// </summary>
		public virtual void StoreSpriteRenderer()
		{
			_spriteRenderer = (TargetRenderer != null) ? TargetRenderer.GetComponent<SpriteRenderer>() : null;
			SpriteRendererIsNull = _spriteRenderer == null;
		}

		/// <summary>
		/// Stores the SpriteRenderer's texture if found
		/// </summary>
		public virtual void StoreSpriteRendererTexture()
		{
			if (SpriteRendererIsNull)
			{
				return;
			}
			_spriteRendererTexture = _spriteRenderer.sprite.texture;
		}

		/// <summary>
		/// Sets the texture associated with the sprite renderer to the specified block
		/// </summary>
		/// <param name="block"></param>
		protected virtual void SetStoredSpriteRendererTexture(MaterialPropertyBlock block)
		{
			if (SpriteRendererIsNull)
			{
				return;
			}
			block.SetTexture(SpriteRendererTextureProperty, _spriteRendererTexture);
		}

		/// <summary>
		/// Sets the level to the value passed in parameters
		/// </summary>
		/// <param name="level"></param>
		public virtual void SetDrivenLevelAbsolute(float level)
		{
			DrivenLevel = level;
		}

		/// <summary>
		/// Sets the level to the remapped value passed in parameters
		/// </summary>
		/// <param name="normalizedLevel"></param>
		/// <param name="remapZero"></param>
		/// <param name="remapOne"></param>
		public virtual void SetDrivenLevelNormalized(float normalizedLevel, float remapZero, float remapOne)
		{
			DrivenLevel = MMMaths.Remap(normalizedLevel, 0f, 1f, remapZero, remapOne);
		}

		/// <summary>
		/// Triggers a one time shake of the shader controller
		/// </summary>
		public virtual void OneTime()
		{
			if (!CacheMaterial)
			{
				Initialization();
			}

			if (RendererIsNull() || (!PropertyFound))
			{
				return;
			}
			else
			{
				this.gameObject.SetActive(true);
				this.enabled = true;
				ControlMode = ControlModes.OneTime;
				_startedTimestamp = GetTime();
				_shaking = true;
			}
		}

		/// <summary>
		/// Triggers a one time shake of the controller to a specified destination value
		/// </summary>
		public virtual void ToDestination()
		{
			if (!CacheMaterial)
			{
				Initialization();
			}
			if (RendererIsNull() || (!PropertyFound))
			{
				return;
			}
			else
			{
				this.enabled = true;
				if (PropertyType == PropertyTypes.Color)
				{
					_fromColorStorage = FromColor;
					FromColor = TargetMaterial.GetColor(PropertyID);
				}                
				ControlMode = ControlModes.ToDestination;
				_startedTimestamp = GetTime();
				_shaking = true;
				_initialValue = GetInitialValue();
			}
		}

		/// <summary>
		/// Use this method to change the FromColor value
		/// </summary>
		/// <param name="newColor"></param>
		public void SetFromColor(Color newColor) { FromColor = newColor; }

		/// <summary>
		/// Use this method to change the ToColor value
		/// </summary>
		/// <param name="newColor"></param>
		public void SetToColor(Color newColor) { ToColor = newColor; }

		/// <summary>
		/// Use this method to change the OneTimeRemapMin value
		/// </summary>
		/// <param name="newValue"></param>
		public virtual void SetRemapOneTimeMin(float newValue) { OneTimeRemapMin = newValue; }

		/// <summary>
		/// Use this method to change the OneTimeRemapMax value
		/// </summary>
		/// <param name="newValue"></param>
		public virtual void SetRemapOneTimeMax(float newValue) { OneTimeRemapMax = newValue; }

		/// <summary>
		/// Use this method to change the ToDestinationValue 
		/// </summary>
		/// <param name="newValue"></param>
		public virtual void SetToDestinationValue(float newValue) { ToDestinationValue = newValue; }

		/// <summary>
		/// Returns the relevant delta time
		/// </summary>
		/// <returns></returns>
		protected float GetDeltaTime()
		{
			return UseUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
		}

		/// <summary>
		/// Returns the relevant time
		/// </summary>
		/// <returns></returns>
		protected float GetTime()
		{
			return UseUnscaledTime ? Time.unscaledTime : Time.time;
		}

		/// <summary>
		/// On Update, we move our value based on the defined settings
		/// </summary>
		protected virtual void Update()
		{
			UpdateValue();
		}

		protected virtual void OnDisable()
		{
			if (RevertToInitialValueAfterEnd)
			{
				CurrentValue = InitialValue;
				_currentColor = InitialColor;
				SetValue(CurrentValue);
			}
		}

		/// <summary>
		/// Updates the value over time based on the selected options
		/// </summary>
		protected virtual void UpdateValue()
		{
			if (SafeMode)
			{
				if (RendererIsNull() || (!PropertyFound))
				{
					return;
				}    
			}

			switch (ControlMode)
			{
				case ControlModes.PingPong:
					if (GetTime() - _lastPingPongPauseAt < PingPongPauseDuration)
					{
						return;
					}
					PingPong += GetDeltaTime() * _pingPongDirection;
					if (PingPong < 0f)
					{
						PingPong = 0f;
						_pingPongDirection = -_pingPongDirection;
						_lastPingPongPauseAt = GetTime();
					}

					if (PingPong > Duration)
					{
						PingPong = Duration;
						_pingPongDirection = -_pingPongDirection;
						_lastPingPongPauseAt = GetTime();
					}
					CurrentValue = MMTween.Tween(PingPong, 0f, Duration, MinValue, MaxValue, Curve);
					CurrentValueNormalized = MMMaths.Remap(CurrentValue, MinValue, MaxValue, 0f, 1f);
					break;
				case ControlModes.Loop:
					if (GetTime() - _lastLoopPauseAt < LoopPauseDuration)
					{
						return;
					}
					LoopTime += GetDeltaTime();
					if (LoopTime > LoopDuration)
					{
						LoopTime = 0f;
						_lastLoopPauseAt = GetTime();
					}
					CurrentValue = MMTween.Tween(LoopTime, 0f, LoopDuration, LoopStartValue, LoopEndValue, LoopCurve);
					CurrentValueNormalized = MMMaths.Remap(CurrentValue, LoopStartValue, LoopEndValue, 0f, 1f);
					break;
				case ControlModes.Random:
					_elapsedTime += GetDeltaTime();
					CurrentValueNormalized = Mathf.PerlinNoise(_randomFrequency * _elapsedTime, _randomShift); 
					if (RemapNoiseValues)
					{
						CurrentValue = CurrentValueNormalized;
						CurrentValue = MMMaths.Remap(CurrentValue, 0f, 1f, RemapNoiseZero, RemapNoiseOne);
					}
					else
					{
						CurrentValue = (CurrentValueNormalized * 2.0f - 1.0f) * _randomAmplitude;
					}
					break;
				case ControlModes.OneTime:
					if (!_shaking)
					{
						return;
					}
					_remappedTimeSinceStart = MMMaths.Remap(GetTime() - _startedTimestamp, 0f, OneTimeDuration, 0f, 1f);
					CurrentValueNormalized = OneTimeCurve.Evaluate(_remappedTimeSinceStart);
					CurrentValue = MMMaths.Remap(CurrentValueNormalized, 0f, 1f, OneTimeRemapMin, OneTimeRemapMax);
					CurrentValue *= OneTimeAmplitude;
					break;
				case ControlModes.AudioAnalyzer:
					CurrentValue = Mathf.Lerp(CurrentValue, AudioAnalyzer.Beats[BeatID].CurrentValue * AudioAnalyzerMultiplier + AudioAnalyzerOffset, AudioAnalyzerLerp * GetDeltaTime());
					CurrentValueNormalized = Mathf.Clamp(AudioAnalyzer.Beats[BeatID].CurrentValue, 0f, 1f);
					break;
				case ControlModes.Driven:
					CurrentValue = DrivenLevel;
					CurrentValueNormalized = Mathf.Clamp(CurrentValue, 0f, 1f);
					break;
				case ControlModes.ToDestination:
					if (!_shaking)
					{
						return;
					}
					_remappedTimeSinceStart = MMMaths.Remap(GetTime() - _startedTimestamp, 0f, ToDestinationDuration, 0f, 1f);
					float time = ToDestinationCurve.Evaluate(_remappedTimeSinceStart);
					CurrentValue = Mathf.LerpUnclamped(_initialValue, ToDestinationValue, time);
					CurrentValueNormalized = MMMaths.Remap(CurrentValue, _initialValue, ToDestinationValue, 0f, 1f);
					break;
			}

			if (PropertyType == PropertyTypes.Color)
			{
				if (ColorMode == ColorModes.TwoColors)
				{
					_currentColor = Color.Lerp(FromColor, ToColor, CurrentValue);	
				}
				else
				{
					_currentColor = ColorRamp.Evaluate(CurrentValue);
				}
			}

			if (AddToInitialValue)
			{
				CurrentValue += InitialValue;
			}

			if ((ControlMode == ControlModes.OneTime) && _shaking && (GetTime() - _startedTimestamp > OneTimeDuration))
			{
				_shaking = false;
				if (RevertToInitialValueAfterEnd)
				{
					CurrentValue = InitialValue;
					if (PropertyType == PropertyTypes.Color)
					{
						_currentColor = InitialColor;
					}
				}
				else
				{
					CurrentValue = OneTimeCurve.Evaluate(1f);
					CurrentValue = MMMaths.Remap(CurrentValue, 0f, 1f, OneTimeRemapMin, OneTimeRemapMax);
					CurrentValue *= OneTimeAmplitude;
				}
				SetValue(CurrentValue);
				if (DisableAfterOneTime)
				{
					this.enabled = false;
				}     
				if (DisableGameObjectAfterOneTime)
				{
					this.gameObject.SetActive(false);
				}
				return;
			}

			if ((ControlMode == ControlModes.ToDestination) && _shaking && (GetTime() - _startedTimestamp > ToDestinationDuration))
			{
				_shaking = false;
				FromColor = _fromColorStorage;
				if (RevertToInitialValueAfterEnd)
				{
					CurrentValue = InitialValue;
					if (PropertyType == PropertyTypes.Color)
					{
						_currentColor = InitialColor;
					}
				}
				else
				{
					CurrentValue = ToDestinationValue;
				}
				SetValue(CurrentValue);
				if (DisableAfterToDestination)
				{
					this.enabled = false;
				}
				return;
			}

			SetValue(CurrentValue);
		}

		/// <summary>
		/// Grabs and stores the initial value
		/// </summary>
		protected virtual float GetInitialValue()
		{
			if (TargetMaterial == null)
			{
				Debug.LogWarning("Material is null", this);
				return 0f;
			}

			switch (PropertyType)
			{
				case PropertyTypes.Bool:
					return TargetMaterial.GetInt(PropertyID);                    

				case PropertyTypes.Int:
					return TargetMaterial.GetInt(PropertyID);

				case PropertyTypes.Float:
					return TargetMaterial.GetFloat(PropertyID);

				case PropertyTypes.Vector:
					return TargetMaterial.GetVector(PropertyID).x;                    

				case PropertyTypes.Keyword:
					return TargetMaterial.IsKeywordEnabled(TargetPropertyName) ? 1f : 0f;

				case PropertyTypes.Color:
					if (ControlMode != ControlModes.ToDestination)
					{
						InitialColor = TargetMaterial.GetColor(PropertyID);
					}                    
					return 0f;

				default:
					return 0f;
			}
		}

		/// <summary>
		/// Sets the value in the shader
		/// </summary>
		/// <param name="newValue"></param>
		protected virtual void SetValue(float newValue)
		{
			if (TargetType == TargetTypes.Image && UseMaterialForRendering)
			{
				if (SafeMode)
				{
					if (TargetImage == null)
					{
						return;    
					}
				}
				TargetMaterial = TargetImage.materialForRendering;
			}

			switch (PropertyType)
			{
				case PropertyTypes.Bool:
					newValue = (newValue > 0f) ? 1f : 0f;
					int newBool = Mathf.RoundToInt(newValue);
					if (UseMaterialPropertyBlocks)
					{
						if (TargetRenderer == null) { return; }
						TargetRenderer.GetPropertyBlock(_propertyBlock, TargetMaterialID);
						StoreSpriteRendererTexture();
						_propertyBlock.SetInt(PropertyID, newBool);
						SetStoredSpriteRendererTexture(_propertyBlock);
						TargetRenderer.SetPropertyBlock(_propertyBlock, TargetMaterialID);    
					}
					else
					{
						TargetMaterial.SetInt(PropertyID, newBool);    
					}
					break;

				case PropertyTypes.Keyword:
					newValue = (newValue > 0f) ? 1f : 0f;
					if (newValue == 0f)
					{
						TargetMaterial.DisableKeyword(TargetPropertyName);
					}
					else
					{
						TargetMaterial.EnableKeyword(TargetPropertyName);
					}
					break;

				case PropertyTypes.Int:
					int newInt = Mathf.RoundToInt(newValue);
					if (UseMaterialPropertyBlocks)
					{
						if (TargetRenderer == null) { return; }
						TargetRenderer.GetPropertyBlock(_propertyBlock, TargetMaterialID);
						StoreSpriteRendererTexture();
						_propertyBlock.SetInt(PropertyID, newInt);
						SetStoredSpriteRendererTexture(_propertyBlock);
						TargetRenderer.SetPropertyBlock(_propertyBlock, TargetMaterialID);    
					}
					else
					{
						TargetMaterial.SetInt(PropertyID, newInt);    
					}
					break;

				case PropertyTypes.Float:
					if (UseMaterialPropertyBlocks)
					{
						if (TargetRenderer == null) { return; }
						TargetRenderer.GetPropertyBlock(_propertyBlock, TargetMaterialID);
						StoreSpriteRendererTexture();
						_propertyBlock.SetFloat(PropertyID, newValue);
						SetStoredSpriteRendererTexture(_propertyBlock);
						TargetRenderer.SetPropertyBlock(_propertyBlock, TargetMaterialID);    
					}
					else
					{
						TargetMaterial.SetFloat(PropertyID, newValue);
					}
					break;

				case PropertyTypes.Vector:
					_vectorValue = TargetMaterial.GetVector(PropertyID);
					if (X)
					{
						_vectorValue.x = newValue;
					}
					if (Y)
					{
						_vectorValue.y = newValue;
					}
					if (Z)
					{
						_vectorValue.z = newValue;
					}
					if (W)
					{
						_vectorValue.w = newValue;
					}
					if (UseMaterialPropertyBlocks)
					{
						if (TargetRenderer == null) { return; }
						TargetRenderer.GetPropertyBlock(_propertyBlock, TargetMaterialID);
						_propertyBlock.SetVector(PropertyID, _vectorValue);
						SetStoredSpriteRendererTexture(_propertyBlock);
						TargetRenderer.SetPropertyBlock(_propertyBlock, TargetMaterialID);    
					}
					else
					{
						TargetMaterial.SetVector(PropertyID, _vectorValue);
					}
					break;
                    
				case PropertyTypes.Color:
					if (UseMaterialPropertyBlocks)
					{
						if (TargetRenderer == null) { return; }
						TargetRenderer.GetPropertyBlock(_propertyBlock, TargetMaterialID);
						StoreSpriteRendererTexture();
						_propertyBlock.SetColor(PropertyID, _currentColor);
						SetStoredSpriteRendererTexture(_propertyBlock);
						TargetRenderer.SetPropertyBlock(_propertyBlock, TargetMaterialID);    
					}
					else
					{
						TargetMaterial.SetColor(PropertyID, _currentColor);
					}
					break;
			}
		}

		/// <summary>
		/// Interrupts any tween in progress, and disables itself
		/// </summary>
		public virtual void Stop()
		{
			_shaking = false;
			this.enabled = false;
		}

		public virtual void RestoreInitialValues()
		{
			_currentColor = InitialColor;
			SetValue(InitialValue);
		}
	}
}