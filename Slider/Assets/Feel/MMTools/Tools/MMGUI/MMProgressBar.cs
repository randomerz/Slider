using UnityEngine;
using UnityEngine.UI;
using System.Collections;
#if MM_TEXTMESHPRO
using TMPro;
#endif
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace MoreMountains.Tools
{
	/// <summary>
	/// Add this bar to an object and link it to a bar (possibly the same object the script is on), and you'll be able to resize the bar object based on a current value, located between a min and max value.
	/// See the HealthBar.cs script for a use case
	/// </summary>
	[MMRequiresConstantRepaint]
	[AddComponentMenu("More Mountains/Tools/GUI/MMProgressBar")]
	public class MMProgressBar : MMMonoBehaviour
	{
		public enum MMProgressBarStates {Idle, Decreasing, Increasing, InDecreasingDelay, InIncreasingDelay }
		/// the possible fill modes 
		public enum FillModes { LocalScale, FillAmount, Width, Height, Anchor }
		/// the possible directions for the fill (for local scale and fill amount only)
		public enum BarDirections { LeftToRight, RightToLeft, UpToDown, DownToUp }
		/// the possible timescales the bar can work on
		public enum TimeScales { UnscaledTime, Time }
		/// the possible ways to animate the bar fill
		public enum BarFillModes { SpeedBased, FixedDuration }
        
		[MMInspectorGroup("Bindings", true, 10)]
		/// optional - the ID of the player associated to this bar
		[Tooltip("optional - the ID of the player associated to this bar")]
		public string PlayerID;
		/// the main, foreground bar
		[Tooltip("the main, foreground bar")]
		public Transform ForegroundBar;
		/// the delayed bar that will show when moving from a value to a new, lower value
		[Tooltip("the delayed bar that will show when moving from a value to a new, lower value")]
		[FormerlySerializedAs("DelayedBar")] 
		public Transform DelayedBarDecreasing;
		/// the delayed bar that will show when moving from a value to a new, higher value
		[Tooltip("the delayed bar that will show when moving from a value to a new, higher value")]
		public Transform DelayedBarIncreasing;
        
		[MMInspectorGroup("Fill Settings", true, 11)]
		/// the local scale or fillamount value to reach when the value associated to the bar is at 0%
		[FormerlySerializedAs("StartValue")] 
		[Range(0f,1f)]
		[Tooltip("the local scale or fillamount value to reach when the value associated to the bar is at 0%")]
		public float MinimumBarFillValue = 0f;
		/// the local scale or fillamount value to reach when the bar is full
		[FormerlySerializedAs("EndValue")] 
		[Range(0f,1f)]
		[Tooltip("the local scale or fillamount value to reach when the bar is full")]
		public float MaximumBarFillValue = 1f;
		/// whether or not to initialize the value of the bar on start
		[Tooltip("whether or not to initialize the value of the bar on start")]
		public bool SetInitialFillValueOnStart = false;
		/// the initial value of the bar
		[MMCondition("SetInitialFillValueOnStart", true)]
		[Range(0f,1f)]
		[Tooltip("the initial value of the bar")]
		public float InitialFillValue = 0f;
		/// the direction this bar moves to
		[Tooltip("the direction this bar moves to")]
		public BarDirections BarDirection = BarDirections.LeftToRight;
		/// the foreground bar's fill mode
		[Tooltip("the foreground bar's fill mode")]
		public FillModes FillMode = FillModes.LocalScale;
		/// defines whether the bar will work on scaled or unscaled time (whether or not it'll keep moving if time is slowed down for example)
		[Tooltip("defines whether the bar will work on scaled or unscaled time (whether or not it'll keep moving if time is slowed down for example)")]
		public TimeScales TimeScale = TimeScales.UnscaledTime;
		/// the selected fill animation mode
		[Tooltip("the selected fill animation mode")]
		public BarFillModes BarFillMode = BarFillModes.SpeedBased;

		[MMInspectorGroup("Foreground Bar Settings", true, 12)]
		/// whether or not the foreground bar should lerp
		[Tooltip("whether or not the foreground bar should lerp")]
		public bool LerpForegroundBar = true;
		/// the speed at which to lerp the foreground bar
		[Tooltip("the speed at which to lerp the foreground bar")]
		[MMCondition("LerpForegroundBar", true)]
		public float LerpForegroundBarSpeedDecreasing = 15f;
		/// the speed at which to lerp the foreground bar if value is increasing
		[Tooltip("the speed at which to lerp the foreground bar if value is increasing")]
		[FormerlySerializedAs("LerpForegroundBarSpeed")]
		[MMCondition("LerpForegroundBar", true)]
		public float LerpForegroundBarSpeedIncreasing = 15f;
		/// the speed at which to lerp the foreground bar if speed is decreasing
		[Tooltip("the speed at which to lerp the foreground bar if speed is decreasing")]
		[MMCondition("LerpForegroundBar", true)]
		public float LerpForegroundBarDurationDecreasing = 0.2f;
		/// the duration each update of the foreground bar should take (only if in fixed duration bar fill mode)
		[Tooltip("the duration each update of the foreground bar should take (only if in fixed duration bar fill mode)")]
		[MMCondition("LerpForegroundBar", true)]
		public float LerpForegroundBarDurationIncreasing = 0.2f;
		/// the curve to use when animating the foreground bar fill decreasing
		[Tooltip("the curve to use when animating the foreground bar fill decreasing")]
		[MMCondition("LerpForegroundBar", true)]
		public AnimationCurve LerpForegroundBarCurveDecreasing = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
		/// the curve to use when animating the foreground bar fill increasing
		[Tooltip("the curve to use when animating the foreground bar fill increasing")]
		[MMCondition("LerpForegroundBar", true)]
		public AnimationCurve LerpForegroundBarCurveIncreasing = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

		[MMInspectorGroup("Delayed Bar Decreasing", true, 13)]
		
		/// the delay before the delayed bar moves (in seconds)
		[Tooltip("the delay before the delayed bar moves (in seconds)")]
		[FormerlySerializedAs("Delay")] 
		public float DecreasingDelay = 1f;
		/// whether or not the delayed bar's animation should lerp
		[Tooltip("whether or not the delayed bar's animation should lerp")]
		[FormerlySerializedAs("LerpDelayedBar")] 
		public bool LerpDecreasingDelayedBar = true;
		/// the speed at which to lerp the delayed bar
		[Tooltip("the speed at which to lerp the delayed bar")]
		[FormerlySerializedAs("LerpDelayedBarSpeed")] 
		[MMCondition("LerpDecreasingDelayedBar", true)]
		public float LerpDecreasingDelayedBarSpeed = 15f;
		/// the duration each update of the foreground bar should take (only if in fixed duration bar fill mode)
		[Tooltip("the duration each update of the foreground bar should take (only if in fixed duration bar fill mode)")]
		[FormerlySerializedAs("LerpDelayedBarDuration")] 
		[MMCondition("LerpDecreasingDelayedBar", true)]
		public float LerpDecreasingDelayedBarDuration = 0.2f;
		/// the curve to use when animating the delayed bar fill
		[Tooltip("the curve to use when animating the delayed bar fill")]
		[FormerlySerializedAs("LerpDelayedBarCurve")] 
		[MMCondition("LerpDecreasingDelayedBar", true)]
		public AnimationCurve LerpDecreasingDelayedBarCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

		[MMInspectorGroup("Delayed Bar Increasing", true, 18)]
		
		/// the delay before the delayed bar moves (in seconds)
		[Tooltip("the delay before the delayed bar moves (in seconds)")]
		public float IncreasingDelay = 1f;
		/// whether or not the delayed bar's animation should lerp
		[Tooltip("whether or not the delayed bar's animation should lerp")]
		public bool LerpIncreasingDelayedBar = true;
		/// the speed at which to lerp the delayed bar
		[Tooltip("the speed at which to lerp the delayed bar")]
		[MMCondition("LerpIncreasingDelayedBar", true)]
		public float LerpIncreasingDelayedBarSpeed = 15f;
		/// the duration each update of the foreground bar should take (only if in fixed duration bar fill mode)
		[Tooltip("the duration each update of the foreground bar should take (only if in fixed duration bar fill mode)")]
		[MMCondition("LerpIncreasingDelayedBar", true)]
		public float LerpIncreasingDelayedBarDuration = 0.2f;
		/// the curve to use when animating the delayed bar fill
		[Tooltip("the curve to use when animating the delayed bar fill")]
		[MMCondition("LerpIncreasingDelayedBar", true)]
		public AnimationCurve LerpIncreasingDelayedBarCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

		[MMInspectorGroup("Bump", true, 14)]
		/// whether or not the bar should "bump" when changing value
		[Tooltip("whether or not the bar should 'bump' when changing value")]
		public bool BumpScaleOnChange = true;
		/// whether or not the bar should bump when its value increases
		[Tooltip("whether or not the bar should bump when its value increases")]
		public bool BumpOnIncrease = false;
		/// whether or not the bar should bump when its value decreases
		[Tooltip("whether or not the bar should bump when its value decreases")]
		public bool BumpOnDecrease = false;
		/// the duration of the bump animation
		[Tooltip("the duration of the bump animation")]
		public float BumpDuration = 0.2f;
		/// whether or not the bar should flash when bumping
		[Tooltip("whether or not the bar should flash when bumping")]
		public bool ChangeColorWhenBumping = true;
		/// whether or not to store the initial bar color before a bump
		[Tooltip("whether or not to store the initial bar color before a bump")]
		public bool StoreBarColorOnPlay = true;
		/// the color to apply to the bar when bumping
		[Tooltip("the color to apply to the bar when bumping")]
		[MMCondition("ChangeColorWhenBumping", true)]
		public Color BumpColor = Color.white;
		/// the curve to map the bump animation on
		[Tooltip("the curve to map the bump animation on")]
		[FormerlySerializedAs("BumpAnimationCurve")]
		public AnimationCurve BumpScaleAnimationCurve = new AnimationCurve(new Keyframe(1, 1), new Keyframe(0.3f, 1.05f), new Keyframe(1, 1));
		/// the curve to map the bump animation color animation on
		[Tooltip("the curve to map the bump animation color animation on")]
		public AnimationCurve BumpColorAnimationCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0));
		/// if this is true, the BumpIntensityMultiplier curve will be evaluated to apply a multiplier to the bump intensity 
		[Tooltip("if this is true, the BumpIntensityMultiplier curve will be evaluated to apply a multiplier to the bump intensity")]
		public bool ApplyBumpIntensityMultiplier = false;
		/// the curve to map the bump's intensity on. x is the delta of the bump, y is the associated multiplier
		[Tooltip("the curve to map the bump's intensity on. x is the normalized delta of the bump (from -1:-100% to 1:100%), y is the associated multiplier")]
		[MMCondition("ApplyBumpIntensityMultiplier", true)]
		public AnimationCurve BumpIntensityMultiplier = new AnimationCurve(new Keyframe(-1, 1), new Keyframe(1, 1));
		/// whether or not the bar is bumping right now
		public bool Bumping { get; protected set; }

		[MMInspectorGroup("Events", true, 16)] 
        
		/// an event to trigger every time the bar bumps
		[Tooltip("an event to trigger every time the bar bumps")]
		public UnityEvent OnBump;
		/// an event to trigger every time the bar bumps, with its bump intensity (based on BumpDeltaMultiplier) in parameter
		[Tooltip("an event to trigger every time the bar bumps, with its bump intensity (based on BumpDeltaMultiplier) in parameter")]
		public UnityEvent<float> OnBumpIntensity;
		/// an event to trigger every time the bar starts decreasing
		[Tooltip("an event to trigger every time the bar starts decreasing")]
		public UnityEvent OnBarMovementDecreasingStart;
		/// an event to trigger every time the bar stops decreasing
		[Tooltip("an event to trigger every time the bar stops decreasing")]
		public UnityEvent OnBarMovementDecreasingStop;
		/// an event to trigger every time the bar starts increasing
		[Tooltip("an event to trigger every time the bar starts increasing")]
		public UnityEvent OnBarMovementIncreasingStart;
		/// an event to trigger every time the bar stops increasing
		[Tooltip("an event to trigger every time the bar stops increasing")]
		public UnityEvent OnBarMovementIncreasingStop;

		[MMInspectorGroup("Text", true, 20)] 
		/// a Text object to update with the bar's value
		[Tooltip("a Text object to update with the bar's value")]
		public Text PercentageText;
		#if MM_TEXTMESHPRO
		/// a TMPro text object to update with the bar's value
		[Tooltip("a TMPro text object to update with the bar's value")]
		public TMP_Text PercentageTextMeshPro;
		#endif

		/// a prefix to always add to the bar's value display
		[Tooltip("a prefix to always add to the bar's value display")]
		public string TextPrefix;
		/// a suffix to always add to the bar's value display
		[Tooltip("a suffix to always add to the bar's value display")]
		public string TextSuffix;
		/// a value multiplier to always apply to the bar's value when displaying it
		[Tooltip("a value multiplier to always apply to the bar's value when displaying it")]
		public float TextValueMultiplier = 1f;
		/// the format in which the text should display
		[Tooltip("the format in which the text should display")]
		public string TextFormat = "{000}";
		/// whether or not to display the total after the current value 
		[Tooltip("whether or not to display the total after the current value")]
		public bool DisplayTotal = false;
		/// if DisplayTotal is true, the separator to put between the current value and the total
		[Tooltip("if DisplayTotal is true, the separator to put between the current value and the total")]
		[MMCondition("DisplayTotal", true)]
		public string TotalSeparator = " / ";

		[MMInspectorGroup("Debug", true, 15)]
		/// the value the bar will move to if you press the DebugSet button
		[Tooltip("the value the bar will move to if you press the DebugSet button")]
		[Range(0f, 1f)] 
		public float DebugNewTargetValue;

		[MMInspectorButton("DebugUpdateBar")]
		public bool DebugUpdateBarButton;
		[MMInspectorButton("DebugSetBar")]
		public bool DebugSetBarButton;
		[MMInspectorButton("Bump")]
		public bool TestBumpButton;
		[MMInspectorButton("Plus10Percent")]
		public bool Plus10PercentButton;
		[MMInspectorButton("Minus10Percent")]
		public bool Minus10PercentButton;
        
		[MMInspectorGroup("Debug Read Only", true, 19)]
		/// the current progress of the bar, ideally read only
		[Tooltip("the current progress of the bar, ideally read only")]
		[Range(0f,1f)]
		public float BarProgress;
		/// the current progress of the bar, ideally read only
		[Tooltip("the current progress of the bar, ideally read only")]
		[Range(0f,1f)]
		public float BarTarget;
		/// the current progress of the delayed bar increasing
		[Tooltip("the current progress of the delayed bar increasing")]
		[Range(0f,1f)]
		public float DelayedBarIncreasingProgress;
		/// the current progress of the delayed bar decreasing
		[Tooltip("the current progress of the delayed bar decreasing")]
		[Range(0f,1f)]
		public float DelayedBarDecreasingProgress;

		protected bool _initialized;
		protected Vector2 _initialBarSize;
		protected Color _initialColor;
		protected Vector3 _initialScale;
		protected Image _foregroundImage;
		protected Image _delayedDecreasingImage;
		protected Image _delayedIncreasingImage;
		protected Vector3 _targetLocalScale = Vector3.one;
		protected float _newPercent;
		protected float _percentLastTimeBarWasUpdated;
		protected float _lastUpdateTimestamp;
		protected float _time;
		protected float _deltaTime;
		protected int _direction;
		protected Coroutine _coroutine;
		protected bool _coroutineShouldRun = false;
		protected bool _isDelayedBarIncreasingNotNull;
		protected bool _isDelayedBarDecreasingNotNull;
		protected bool _actualUpdate;
		protected Vector2 _anchorVector;
		protected float _delayedBarDecreasingProgress;
		protected float _delayedBarIncreasingProgress;
		protected MMProgressBarStates CurrentState = MMProgressBarStates.Idle;
		protected string _updatedText;
		protected string _totalText;
		protected bool _isForegroundBarNotNull;
		protected bool _isForegroundImageNotNull;
		protected bool _isPercentageTextNotNull;
		protected bool _isPercentageTextMeshProNotNull;

		#region PUBLIC_API
        
		/// <summary>
		/// Updates the bar's values, using a normalized value
		/// </summary>
		/// <param name="normalizedValue"></param>
		public virtual void UpdateBar01(float normalizedValue) 
		{
			UpdateBar(Mathf.Clamp01(normalizedValue), 0f, 1f);
		}
        
		/// <summary>
		/// Updates the bar's values based on the specified parameters
		/// </summary>
		/// <param name="currentValue">Current value.</param>
		/// <param name="minValue">Minimum value.</param>
		/// <param name="maxValue">Max value.</param>
		public virtual void UpdateBar(float currentValue,float minValue,float maxValue) 
		{
			if (!_initialized)
			{
				Initialization();
			}

			if (StoreBarColorOnPlay)
			{
				StoreInitialColor();	
			}

			if (!this.gameObject.activeInHierarchy)
			{
				this.gameObject.SetActive(true);    
			}
            
			_newPercent = MMMaths.Remap(currentValue, minValue, maxValue, MinimumBarFillValue, MaximumBarFillValue);
	        
			_actualUpdate = (BarTarget != _newPercent);
	        
			if (!_actualUpdate)
			{
				return;
			}
	        
			if (CurrentState != MMProgressBarStates.Idle)
			{
				if ((CurrentState == MMProgressBarStates.Decreasing) ||
				    (CurrentState == MMProgressBarStates.InDecreasingDelay))
				{
					if (_newPercent >= BarTarget)
					{
						StopCoroutine(_coroutine);
						SetBar01(BarTarget);
					}
				}
				if ((CurrentState == MMProgressBarStates.Increasing) ||
				    (CurrentState == MMProgressBarStates.InIncreasingDelay))
				{
					if (_newPercent <= BarTarget)
					{
						StopCoroutine(_coroutine);
						SetBar01(BarTarget);
					}
				}
			}
	        
			_percentLastTimeBarWasUpdated = BarProgress;
			_delayedBarDecreasingProgress = DelayedBarDecreasingProgress;
			_delayedBarIncreasingProgress = DelayedBarIncreasingProgress;
	        
			BarTarget = _newPercent;
			
			if ((_newPercent != _percentLastTimeBarWasUpdated) && !Bumping)
			{
				Bump();
			}

			DetermineDeltaTime();
			_lastUpdateTimestamp = _time;
	        
			DetermineDirection();
			if (_direction < 0)
			{
				OnBarMovementDecreasingStart?.Invoke();
			}
			else
			{
				OnBarMovementIncreasingStart?.Invoke();
			}
		        
			if (_coroutine != null)
			{
				StopCoroutine(_coroutine);
			}
			_coroutineShouldRun = true;     
		    

			if (this.gameObject.activeInHierarchy)
			{
				_coroutine = StartCoroutine(UpdateBarsCo());
			}
			else
			{
				SetBar(currentValue, minValue, maxValue);
			}

			UpdateText();
		}

		/// <summary>
		/// Sets the bar value to the one specified 
		/// </summary>
		/// <param name="currentValue"></param>
		/// <param name="minValue"></param>
		/// <param name="maxValue"></param>
		public virtual void SetBar(float currentValue, float minValue, float maxValue)
		{
			float newPercent = MMMaths.Remap(currentValue, minValue, maxValue, 0f, 1f);
			SetBar01(newPercent);
		}

		/// <summary>
		/// Sets the bar value to the normalized value set in parameter
		/// </summary>
		/// <param name="newPercent"></param>
		public virtual void SetBar01(float newPercent)
		{
			if (!_initialized)
			{
				Initialization();
			}

			newPercent = MMMaths.Remap(newPercent, 0f, 1f, MinimumBarFillValue, MaximumBarFillValue);
			BarProgress = newPercent;
			DelayedBarDecreasingProgress = newPercent;
			DelayedBarIncreasingProgress = newPercent;
			//_newPercent = newPercent;
			BarTarget = newPercent;
			_percentLastTimeBarWasUpdated = newPercent;
			_delayedBarDecreasingProgress = DelayedBarDecreasingProgress;
			_delayedBarIncreasingProgress = DelayedBarIncreasingProgress;
			SetBarInternal(newPercent, ForegroundBar, _foregroundImage, _initialBarSize);
			SetBarInternal(newPercent, DelayedBarDecreasing, _delayedDecreasingImage, _initialBarSize);
			SetBarInternal(newPercent, DelayedBarIncreasing, _delayedIncreasingImage, _initialBarSize);
			UpdateText();
			_coroutineShouldRun = false;
			CurrentState = MMProgressBarStates.Idle;
		}
        
		#endregion PUBLIC_API

		#region START
        
		/// <summary>
		/// On start we store our image component
		/// </summary>
		protected virtual void Start()
		{
			if (!_initialized)
			{
				Initialization();
			}
		}

		protected virtual void OnEnable()
		{
			if (!_initialized)
			{
				return;
			}

			StoreInitialColor();
		}

		public virtual void Initialization()
		{
			_isForegroundBarNotNull = ForegroundBar != null;
			_isDelayedBarDecreasingNotNull = DelayedBarDecreasing != null;
			_isDelayedBarIncreasingNotNull = DelayedBarIncreasing != null;
			_isPercentageTextNotNull = PercentageText != null;
			#if MM_TEXTMESHPRO
			_isPercentageTextMeshProNotNull = PercentageTextMeshPro != null;
			#endif
			_initialScale = this.transform.localScale;

			if (_isForegroundBarNotNull)
			{
				_foregroundImage = ForegroundBar.GetComponent<Image>();
				_isForegroundImageNotNull = _foregroundImage != null;
				_initialBarSize = _foregroundImage.rectTransform.sizeDelta;
			}
			if (_isDelayedBarDecreasingNotNull)
			{
				_delayedDecreasingImage = DelayedBarDecreasing.GetComponent<Image>();
			}
			if (_isDelayedBarIncreasingNotNull)
			{
				_delayedIncreasingImage = DelayedBarIncreasing.GetComponent<Image>();
			}
			_initialized = true;

			StoreInitialColor();

			_percentLastTimeBarWasUpdated = BarProgress;

			if (SetInitialFillValueOnStart)
			{
				SetBar01(InitialFillValue);
			}
		}

		protected virtual void StoreInitialColor()
		{
			if (!Bumping && _isForegroundImageNotNull)
			{
				_initialColor = _foregroundImage.color;
			}
		}
        
		#endregion START

		#region TESTS

		/// <summary>
		/// This test method, called via the inspector button of the same name, lets you test what happens when you update the bar to a certain value
		/// </summary>
		protected virtual void DebugUpdateBar()
		{
			this.UpdateBar01(DebugNewTargetValue);
		}
        
		/// <summary>
		/// Test method
		/// </summary>
		protected virtual void DebugSetBar()
		{
			this.SetBar01(DebugNewTargetValue);
		}

		/// <summary>
		/// Test method - increases the bar's current value by 10%
		/// </summary>
		public virtual void Plus10Percent()
		{
			float newProgress = BarTarget + 0.1f;
			newProgress = Mathf.Clamp(newProgress, 0f, 1f);
			UpdateBar01(newProgress);
		}
        
		/// <summary>
		/// Test method - decreases the bar's current value by 10%
		/// </summary>
		public virtual void Minus10Percent()
		{
			float newProgress = BarTarget - 0.1f;
			newProgress = Mathf.Clamp(newProgress, 0f, 1f);
			UpdateBar01(newProgress);
		}

		/// <summary>
		/// Test method - increases the bar's current value by 20%
		/// </summary>
		public virtual void Plus20Percent()
		{
			float newProgress = BarTarget + 0.2f;
			newProgress = Mathf.Clamp(newProgress, 0f, 1f);
			UpdateBar01(newProgress);
		}
        
		/// <summary>
		/// Test method - decreases the bar's current value by 20%
		/// </summary>
		public virtual void Minus20Percent()
		{
			float newProgress = BarTarget - 0.2f;
			newProgress = Mathf.Clamp(newProgress, 0f, 1f);
			UpdateBar01(newProgress);
		}


		#endregion TESTS

		protected virtual void UpdateText()
		{
			_updatedText = TextPrefix + (BarTarget * TextValueMultiplier).ToString(TextFormat);
			if (DisplayTotal)
			{
				_updatedText += TotalSeparator + (TextValueMultiplier).ToString(TextFormat);
			}
			_updatedText += TextSuffix;
			if (_isPercentageTextNotNull)
			{
				PercentageText.text = _updatedText;
			}
			#if MM_TEXTMESHPRO
			if (_isPercentageTextMeshProNotNull)
			{
				PercentageTextMeshPro.text = _updatedText;
			}
			#endif
		}
        
		/// <summary>
		/// On Update we update our bars
		/// </summary>
		protected virtual IEnumerator UpdateBarsCo()
		{
			while (_coroutineShouldRun)
			{
				DetermineDeltaTime();
				DetermineDirection();
				UpdateBars();
				yield return null;
			}

			CurrentState = MMProgressBarStates.Idle;
			yield break;
		}
		
		protected virtual void DetermineDeltaTime()
		{
			_deltaTime = (TimeScale == TimeScales.Time) ? Time.deltaTime : Time.unscaledDeltaTime;
			_time = (TimeScale == TimeScales.Time) ? Time.time : Time.unscaledTime;
		}

		protected virtual void DetermineDirection()
		{
			_direction = (_newPercent > _percentLastTimeBarWasUpdated) ? 1 : -1;
		}

		/// <summary>
		/// Updates the foreground bar's scale
		/// </summary>
		protected virtual void UpdateBars()
		{
			float newFill;
			float newFillDelayed;
			float t1, t2 = 0f;
			
			// if the value is decreasing
			if (_direction < 0)
			{
				newFill = ComputeNewFill(LerpForegroundBar, LerpForegroundBarSpeedDecreasing, LerpForegroundBarDurationDecreasing, LerpForegroundBarCurveDecreasing, 0f, _percentLastTimeBarWasUpdated, out t1);
				SetBarInternal(newFill, ForegroundBar, _foregroundImage, _initialBarSize);
				SetBarInternal(newFill, DelayedBarIncreasing, _delayedIncreasingImage, _initialBarSize);

				BarProgress = newFill;
				DelayedBarIncreasingProgress = newFill;

				CurrentState = MMProgressBarStates.Decreasing;
				
				if (_time - _lastUpdateTimestamp > DecreasingDelay)
				{
					newFillDelayed = ComputeNewFill(LerpDecreasingDelayedBar, LerpDecreasingDelayedBarSpeed, LerpDecreasingDelayedBarDuration, LerpDecreasingDelayedBarCurve, DecreasingDelay,_delayedBarDecreasingProgress, out t2);
					SetBarInternal(newFillDelayed, DelayedBarDecreasing, _delayedDecreasingImage, _initialBarSize);

					DelayedBarDecreasingProgress = newFillDelayed;
					CurrentState = MMProgressBarStates.InDecreasingDelay;
				}
			}
			else // if the value is increasing
			{
				newFill = ComputeNewFill(LerpForegroundBar, LerpForegroundBarSpeedIncreasing, LerpForegroundBarDurationIncreasing, LerpForegroundBarCurveIncreasing, 0f, _delayedBarIncreasingProgress, out t1);
				SetBarInternal(newFill, DelayedBarIncreasing, _delayedIncreasingImage, _initialBarSize);
				
				DelayedBarIncreasingProgress = newFill;
				CurrentState = MMProgressBarStates.Increasing;

				if (DelayedBarIncreasing == null)
				{
					newFill = ComputeNewFill(LerpForegroundBar, LerpForegroundBarSpeedIncreasing, LerpForegroundBarDurationIncreasing, LerpForegroundBarCurveIncreasing, 0f, _percentLastTimeBarWasUpdated, out t2);
					SetBarInternal(newFill, DelayedBarDecreasing, _delayedDecreasingImage, _initialBarSize);
					SetBarInternal(newFill, ForegroundBar, _foregroundImage, _initialBarSize);
					
					BarProgress = newFill;	
					DelayedBarDecreasingProgress = newFill;
					CurrentState = MMProgressBarStates.InDecreasingDelay;
				}
				else
				{
					if (_time - _lastUpdateTimestamp > IncreasingDelay)
					{
						newFillDelayed = ComputeNewFill(LerpIncreasingDelayedBar, LerpForegroundBarSpeedIncreasing, LerpForegroundBarDurationIncreasing, LerpForegroundBarCurveIncreasing, IncreasingDelay, _delayedBarDecreasingProgress, out t2);
					
						SetBarInternal(newFillDelayed, DelayedBarDecreasing, _delayedDecreasingImage, _initialBarSize);
						SetBarInternal(newFillDelayed, ForegroundBar, _foregroundImage, _initialBarSize);
					
						BarProgress = newFillDelayed;	
						DelayedBarDecreasingProgress = newFillDelayed;
						CurrentState = MMProgressBarStates.InDecreasingDelay;
					}
				}
			}
			
			if ((t1 >= 1f) && (t2 >= 1f))
			{
				_coroutineShouldRun = false;
				if (_direction > 0)
				{
					OnBarMovementIncreasingStop?.Invoke();
				}
				else
				{
					OnBarMovementDecreasingStop?.Invoke();
				}
			}
		}

		protected virtual float ComputeNewFill(bool lerpBar, float barSpeed, float barDuration, AnimationCurve barCurve, float delay, float lastPercent, out float t)
		{
			float newFill = 0f;
			t = 0f;
			if (lerpBar)
			{
				float delta = 0f;
				float timeSpent = _time - _lastUpdateTimestamp - delay;
				float speed = barSpeed;
				if (speed == 0f) { speed = 1f; }
				
				float duration = (BarFillMode == BarFillModes.FixedDuration) ? barDuration : (Mathf.Abs(_newPercent - lastPercent)) / speed;
				
				delta = MMMaths.Remap(timeSpent, 0f, duration, 0f, 1f);
				delta = Mathf.Clamp(delta, 0f, 1f);
				t = delta;
				if (t < 1f)
				{
					delta = barCurve.Evaluate(delta);
					newFill = Mathf.LerpUnclamped(lastPercent, _newPercent, delta);	
				}
				else
				{
					newFill = _newPercent;
				}
			}
			else
			{
				newFill = _newPercent;
			}

			newFill = Mathf.Clamp( newFill, 0f, 1f);

			return newFill;
		}

		protected virtual void SetBarInternal(float newAmount, Transform bar, Image image, Vector2 initialSize)
		{
			if (bar == null)
			{
				return;
			}
			
			switch (FillMode)
			{
				case FillModes.LocalScale:
					_targetLocalScale = Vector3.one;
					switch (BarDirection)
					{
						case BarDirections.LeftToRight:
							_targetLocalScale.x = newAmount;
							break;
						case BarDirections.RightToLeft:
							_targetLocalScale.x = 1f - newAmount;
							break;
						case BarDirections.DownToUp:
							_targetLocalScale.y = newAmount;
							break;
						case BarDirections.UpToDown:
							_targetLocalScale.y = 1f - newAmount;
							break;
					}

					bar.localScale = _targetLocalScale;
					break;

				case FillModes.Width:
					if (image == null)
					{
						return;
					}
					float newSizeX = MMMaths.Remap(newAmount, 0f, 1f, 0, initialSize.x);
					image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newSizeX);
					break;

				case FillModes.Height:
					if (image == null)
					{
						return;
					}
					float newSizeY = MMMaths.Remap(newAmount, 0f, 1f, 0, initialSize.y);
					image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newSizeY);
					break;

				case FillModes.FillAmount:
					if (image == null)
					{
						return;
					}
					image.fillAmount = newAmount;
					break;
				case FillModes.Anchor:
					if (image == null)
					{
						return;
					}
					switch (BarDirection)
					{
						case BarDirections.LeftToRight:
							_anchorVector.x = 0f;
							_anchorVector.y = 0f;
							image.rectTransform.anchorMin = _anchorVector;
							_anchorVector.x = newAmount;
							_anchorVector.y = 1f;
							image.rectTransform.anchorMax = _anchorVector;
							break;
						case BarDirections.RightToLeft:
							_anchorVector.x = newAmount;
							_anchorVector.y = 0f;
							image.rectTransform.anchorMin = _anchorVector;
							_anchorVector.x = 1f;
							_anchorVector.y = 1f;
							image.rectTransform.anchorMax = _anchorVector;
							break;
						case BarDirections.DownToUp:
							_anchorVector.x = 0f;
							_anchorVector.y = 0f;
							image.rectTransform.anchorMin = _anchorVector;
							_anchorVector.x = 1f;
							_anchorVector.y = newAmount;
							image.rectTransform.anchorMax = _anchorVector;
							break;
						case BarDirections.UpToDown:
							_anchorVector.x = 0f;
							_anchorVector.y = newAmount;
							image.rectTransform.anchorMin = _anchorVector;
							_anchorVector.x = 1f;
							_anchorVector.y = 1f;
							image.rectTransform.anchorMax = _anchorVector;
							break;
					}
					break;
			}
		}

		#region  Bump

		/// <summary>
		/// Triggers a camera bump
		/// </summary>
		public virtual void Bump()
		{
			float delta = _newPercent - _percentLastTimeBarWasUpdated;
			float intensityMultiplier = BumpIntensityMultiplier.Evaluate(delta);
			
			bool shouldBump = false;

			if (!_initialized)
			{
				return;
			}
			
			DetermineDirection();
			
			if (BumpOnIncrease && (_direction > 0))
			{
				shouldBump = true;
			}
			
			if (BumpOnDecrease && (_direction < 0))
			{
				shouldBump = true;
			}
			
			if (BumpScaleOnChange)
			{
				shouldBump = true;
			}

			if (!shouldBump)
			{
				return;
			}
			
			if (this.gameObject.activeInHierarchy)
			{
				StartCoroutine(BumpCoroutine(intensityMultiplier));
			}

			OnBump?.Invoke();
			OnBumpIntensity?.Invoke(ApplyBumpIntensityMultiplier ? intensityMultiplier : 1f);
		}

		/// <summary>
		/// A coroutine that (usually quickly) changes the scale of the bar 
		/// </summary>
		/// <returns>The coroutine.</returns>
		protected virtual IEnumerator BumpCoroutine(float intensityMultiplier)
		{
			float journey = 0f;

			Bumping = true;

			while (journey <= BumpDuration)
			{
				journey = journey + _deltaTime;
				float percent = Mathf.Clamp01(journey / BumpDuration);

				float curvePercent = BumpScaleAnimationCurve.Evaluate(percent);

				if (ApplyBumpIntensityMultiplier)
				{
					float multiplier = Mathf.Abs(1f - curvePercent) * intensityMultiplier;
					curvePercent = 1 + multiplier;	
				}

				float colorCurvePercent = BumpColorAnimationCurve.Evaluate(percent);
				this.transform.localScale = curvePercent * _initialScale;

				if (ChangeColorWhenBumping && _isForegroundImageNotNull)
				{
					_foregroundImage.color = Color.Lerp(_initialColor, BumpColor, colorCurvePercent);
				}
				yield return null;
			}
			if (ChangeColorWhenBumping && _isForegroundImageNotNull)
			{
				_foregroundImage.color = _initialColor;
			}
			Bumping = false;
			yield return null;
		}

		#endregion Bump

		#region ShowHide

		/// <summary>
		/// A simple method you can call to show the bar (set active true)
		/// </summary>
		public virtual void ShowBar()
		{
			this.gameObject.SetActive(true);
		}

		/// <summary>
		/// Hides (SetActive false) the progress bar object, after an optional delay
		/// </summary>
		/// <param name="delay"></param>
		public virtual void HideBar(float delay)
		{
			if (delay <= 0)
			{
				this.gameObject.SetActive(false);
			}
			else if (this.gameObject.activeInHierarchy)
			{
				StartCoroutine(HideBarCo(delay));
			}
		}

		/// <summary>
		/// An internal coroutine used to handle the disabling of the progress bar after a delay
		/// </summary>
		/// <param name="delay"></param>
		/// <returns></returns>
		protected virtual IEnumerator HideBarCo(float delay)
		{
			yield return MMCoroutine.WaitFor(delay);
			this.gameObject.SetActive(false);
		}

		#endregion ShowHide
		
	}
}