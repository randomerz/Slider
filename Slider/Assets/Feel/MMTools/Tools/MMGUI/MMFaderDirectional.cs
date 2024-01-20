using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
using MoreMountains.Tools;

namespace MoreMountains.Tools
{
	/// <summary>
	/// The Fader class can be put on an Image, and it'll intercept MMFadeEvents and turn itself on or off accordingly.
	/// This specific fader will move from left to right, right to left, top to bottom or bottom to top
	/// </summary>
	[RequireComponent(typeof(CanvasGroup))]
	[AddComponentMenu("More Mountains/Tools/GUI/MMFaderDirectional")]
	public class MMFaderDirectional : MonoBehaviour, MMEventListener<MMFadeEvent>, MMEventListener<MMFadeInEvent>, MMEventListener<MMFadeOutEvent>, MMEventListener<MMFadeStopEvent>
	{
		/// the possible directions this fader can move in
		public enum Directions { TopToBottom, LeftToRight, RightToLeft, BottomToTop }

		[Header("Identification")]
		/// the ID for this fader (0 is default), set more IDs if you need more than one fader
		[Tooltip("the ID for this fader (0 is default), set more IDs if you need more than one fader")]
		public int ID;

		[Header("Directional Fader")]
		/// the direction this fader should move in when fading in
		[Tooltip("the direction this fader should move in when fading in")]
		public Directions FadeInDirection = Directions.LeftToRight;
		/// the direction this fader should move in when fading out
		[Tooltip("the direction this fader should move in when fading out")]
		public Directions FadeOutDirection = Directions.LeftToRight;
        
		[Header("Timing")]
		/// the default duration of the fade in/out
		[Tooltip("the default duration of the fade in/out")]
		public float DefaultDuration = 0.2f;
		/// the default curve to use for this fader
		[Tooltip("the default curve to use for this fader")]
		public MMTweenType DefaultTween = new MMTweenType(MMTween.MMTweenCurve.LinearTween);
		/// whether or not the fade should happen in unscaled time 
		[Tooltip("whether or not the fade should happen in unscaled time")]
		public bool IgnoreTimescale = true;
		/// whether or not to automatically disable this fader on init
		[Tooltip("whether or not to automatically disable this fader on init")]
		public bool DisableOnInit = true;

		[Header("Delay")]
		/// a delay (in seconds) to apply before playing this fade
		[Tooltip("a delay (in seconds) to apply before playing this fade")]
		public float InitialDelay = 0f;

		[Header("Interaction")]
		/// whether or not the fader should block raycasts when visible
		[Tooltip("whether or not the fader should block raycasts when visible")]
		public bool ShouldBlockRaycasts = false; 

		/// the width of the fader
		public virtual float Width { get { return _rectTransform.rect.width; } }
		/// the height of the fader
		public virtual float Height { get { return _rectTransform.rect.height; } }

		[Header("Debug")]
		[MMInspectorButton("FadeIn1Second")]
		public bool FadeIn1SecondButton;
		[MMInspectorButton("FadeOut1Second")]
		public bool FadeOut1SecondButton;
		[MMInspectorButton("DefaultFade")]
		public bool DefaultFadeButton;
		[MMInspectorButton("ResetFader")]
		public bool ResetFaderButton;

		protected RectTransform _rectTransform;
		protected CanvasGroup _canvasGroup;
		protected float _currentDuration;
		protected MMTweenType _currentCurve;
		protected bool _fading = false;
		protected float _fadeStartedAt;
		protected Vector2 _initialPosition;

		protected Vector2 _fromPosition;
		protected Vector2 _toPosition;
		protected Vector2 _newPosition;
		protected bool _active;
		protected bool _initialized = false;

		/// <summary>
		/// Test method triggered by an inspector button
		/// </summary>
		protected virtual void ResetFader()
		{
			_rectTransform.anchoredPosition = _initialPosition;
		}

		/// <summary>
		/// Test method triggered by an inspector button
		/// </summary>
		protected virtual void DefaultFade()
		{
			MMFadeEvent.Trigger(DefaultDuration, 1f, DefaultTween, ID, IgnoreTimescale, this.transform.position);
		}

		/// <summary>
		/// Test method triggered by an inspector button
		/// </summary>
		protected virtual void FadeIn1Second()
		{
			MMFadeInEvent.Trigger(1f, DefaultTween, ID, IgnoreTimescale, this.transform.position);
		}

		/// <summary>
		/// Test method triggered by an inspector button
		/// </summary>
		protected virtual void FadeOut1Second()
		{
			MMFadeOutEvent.Trigger(1f, DefaultTween, ID, IgnoreTimescale, this.transform.position);
		}

		/// <summary>
		/// On Start, we initialize our fader
		/// </summary>
		protected virtual void Start()
		{
			Initialization();
		}

		/// <summary>
		/// On init, we grab our components, and disable/hide everything
		/// </summary>
		//protected virtual IEnumerator Initialization()
		protected virtual void Initialization()
		{
			_canvasGroup = this.gameObject.GetComponent<CanvasGroup>();
			_rectTransform = this.gameObject.GetComponent<RectTransform>();
			_initialPosition = _rectTransform.anchoredPosition;
			if (DisableOnInit)
			{
				DisableFader();
			}
			_initialized = true;
		}

		/// <summary>
		/// On Update, we update our alpha 
		/// </summary>
		protected virtual void Update()
		{
			if (_canvasGroup == null) { return; }

			if (_fading)
			{
				Fade();
			}
		}

		/// <summary>
		/// Fades the canvasgroup towards its target alpha
		/// </summary>
		protected virtual void Fade()
		{
			float currentTime = IgnoreTimescale ? Time.unscaledTime : Time.time;
			float endTime = _fadeStartedAt + _currentDuration;

			if (currentTime - _fadeStartedAt < _currentDuration)
			{
				_newPosition = MMTween.Tween(currentTime, _fadeStartedAt, endTime, _fromPosition, _toPosition, _currentCurve);
				_rectTransform.anchoredPosition = _newPosition;
			}
			else
			{
				StopFading();
			}
		}

		/// <summary>
		/// Stops the fading.
		/// </summary>
		protected virtual void StopFading()
		{
			_rectTransform.anchoredPosition = _toPosition;
			_fading = false;

			if (_initialPosition != _toPosition)
			{
				DisableFader();
			}
		}

		/// <summary>
		/// Starts a fade
		/// </summary>
		/// <param name="fadingIn"></param>
		/// <param name="duration"></param>
		/// <param name="curve"></param>
		/// <param name="id"></param>
		/// <param name="ignoreTimeScale"></param>
		/// <param name="worldPosition"></param>
		protected virtual IEnumerator StartFading(bool fadingIn, float duration, MMTweenType curve, int id,
			bool ignoreTimeScale, Vector3 worldPosition)
		{
			if (id != ID)
			{
				yield break;
			}

			if (InitialDelay > 0f)
			{
				yield return MMCoroutine.WaitFor(InitialDelay);
			}

			if (!_initialized)
			{
				Initialization();
			}

			if (curve == null)
			{
				curve = DefaultTween;
			}
            
			IgnoreTimescale = ignoreTimeScale;
			EnableFader();
			_fading = true;

			_fadeStartedAt = IgnoreTimescale ? Time.unscaledTime : Time.time;
			_currentCurve = curve;
			_currentDuration = duration;

			_fromPosition = _rectTransform.anchoredPosition;
			_toPosition = fadingIn ? _initialPosition : ExitPosition();

			_newPosition = MMTween.Tween(0f, 0f, duration, _fromPosition, _toPosition, _currentCurve);
			_rectTransform.anchoredPosition = _newPosition;
		}

		/// <summary>
		/// Determines the position of the fader before entry
		/// </summary>
		/// <returns></returns>
		protected virtual Vector2 BeforeEntryPosition()
		{
			switch (FadeInDirection)
			{
				case Directions.BottomToTop:
					return _initialPosition + Vector2.down * Height;
				case Directions.LeftToRight:
					return _initialPosition + Vector2.left * Width;
				case Directions.RightToLeft:
					return _initialPosition + Vector2.right * Width;
				case Directions.TopToBottom:
					return _initialPosition + Vector2.up * Height;
			}
			return Vector2.zero;
		}

		/// <summary>
		/// Determines the exit position of the fader
		/// </summary>
		/// <returns></returns>
		protected virtual Vector2 ExitPosition()
		{
			switch (FadeOutDirection)
			{
				case Directions.BottomToTop:
					return _initialPosition + Vector2.up * Height;
				case Directions.LeftToRight:
					return _initialPosition + Vector2.right * Width;
				case Directions.RightToLeft:
					return _initialPosition + Vector2.left * Width;
				case Directions.TopToBottom:
					return _initialPosition + Vector2.down * Height;
			}
			return Vector2.zero;
		}

		/// <summary>
		/// Disables the fader.
		/// </summary>
		protected virtual void DisableFader()
		{
			if (ShouldBlockRaycasts)
			{
				_canvasGroup.blocksRaycasts = false;
			}
			_active = false;
			_canvasGroup.alpha = 0;
			_rectTransform.anchoredPosition = BeforeEntryPosition();
			this.enabled = false;
		}

		/// <summary>
		/// Enables the fader.
		/// </summary>
		protected virtual void EnableFader()
		{
			this.enabled = true;
			if (ShouldBlockRaycasts)
			{
				_canvasGroup.blocksRaycasts = true;
			}
			_active = true;
			_canvasGroup.alpha = 1;
		}

		/// <summary>
		/// When catching a fade event, we fade our image in or out
		/// </summary>
		/// <param name="fadeEvent">Fade event.</param>
		public virtual void OnMMEvent(MMFadeEvent fadeEvent)
		{
			bool status = _active ? false : true;
			StartCoroutine(StartFading(status, fadeEvent.Duration, fadeEvent.Curve, fadeEvent.ID,
				fadeEvent.IgnoreTimeScale, fadeEvent.WorldPosition));
		}

		/// <summary>
		/// When catching an MMFadeInEvent, we fade our image in
		/// </summary>
		/// <param name="fadeEvent">Fade event.</param>
		public virtual void OnMMEvent(MMFadeInEvent fadeEvent)
		{
			StartCoroutine(StartFading(true, fadeEvent.Duration, fadeEvent.Curve, fadeEvent.ID,
				fadeEvent.IgnoreTimeScale, fadeEvent.WorldPosition));
		}

		/// <summary>
		/// When catching an MMFadeOutEvent, we fade our image out
		/// </summary>
		/// <param name="fadeEvent">Fade event.</param>
		public virtual void OnMMEvent(MMFadeOutEvent fadeEvent)
		{
			StartCoroutine(StartFading(false, fadeEvent.Duration, fadeEvent.Curve, fadeEvent.ID,
				fadeEvent.IgnoreTimeScale, fadeEvent.WorldPosition));
		}

		/// <summary>
		/// When catching an MMFadeStopEvent, we stop our fade
		/// </summary>
		/// <param name="fadeEvent">Fade event.</param>
		public virtual void OnMMEvent(MMFadeStopEvent fadeStopEvent)
		{
			if (fadeStopEvent.ID == ID)
			{
				_fading = false;
				if (fadeStopEvent.Restore)
				{
					_rectTransform.anchoredPosition = _initialPosition;
				}
			}
		}

		/// <summary>
		/// On enable, we start listening to events
		/// </summary>
		protected virtual void OnEnable()
		{
			this.MMEventStartListening<MMFadeEvent>();
			this.MMEventStartListening<MMFadeStopEvent>();
			this.MMEventStartListening<MMFadeInEvent>();
			this.MMEventStartListening<MMFadeOutEvent>();
		}

		/// <summary>
		/// On disable, we stop listening to events
		/// </summary>
		protected virtual void OnDestroy()
		{
			this.MMEventStopListening<MMFadeEvent>();
			this.MMEventStopListening<MMFadeStopEvent>();
			this.MMEventStopListening<MMFadeInEvent>();
			this.MMEventStopListening<MMFadeOutEvent>();
		}
	}
}