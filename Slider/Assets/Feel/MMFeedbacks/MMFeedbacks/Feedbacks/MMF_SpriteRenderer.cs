using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback will let you change the color of a target sprite renderer over time, and flip it on X or Y. You can also use it to command one or many MMSpriteRendererShakers.
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback will let you change the color of a target sprite renderer over time, and flip it on X or Y. You can also use it to command one or many MMSpriteRendererShakers.")]
	[FeedbackPath("Renderer/SpriteRenderer")]
	public class MMF_SpriteRenderer : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.RendererColor; } }
		public override bool EvaluateRequiresSetup() => (BoundSpriteRenderer == null);
		public override string RequiredTargetText => BoundSpriteRenderer != null ? BoundSpriteRenderer.name : "";
		public override string RequiresSetupText => "This feedback requires that a BoundSpriteRenderer be set to be able to work properly. You can set one below.";
		#endif

		/// the duration of this feedback is the duration of the sprite renderer, or 0 if instant
		public override float FeedbackDuration { get { return (Mode == Modes.Instant) ? 0f : ApplyTimeMultiplier(Duration); } set { Duration = value; } }
		public override bool HasChannel => true;
		public override bool HasRandomness => true;
		public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => BoundSpriteRenderer = FindAutomatedTarget<SpriteRenderer>();

		/// the possible modes for this feedback
		public enum Modes { OverTime, Instant, ShakerEvent, ToDestinationColor, ToDestinationColorAndBack }
		/// the different ways to grab initial color
		public enum InitialColorModes { InitialColorOnInit, InitialColorOnPlay }

		[MMFInspectorGroup("Sprite Renderer", true, 51, true)]
		/// the SpriteRenderer to affect when playing the feedback
		[Tooltip("the SpriteRenderer to affect when playing the feedback")]
		public SpriteRenderer BoundSpriteRenderer;
		/// whether the feedback should affect the sprite renderer instantly or over a period of time
		[Tooltip("whether the feedback should affect the sprite renderer instantly or over a period of time")]
		public Modes Mode = Modes.OverTime;
		/// how long the sprite renderer should change over time
		[Tooltip("how long the sprite renderer should change over time")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime, (int)Modes.ShakerEvent, (int)Modes.ToDestinationColor, (int)Modes.ToDestinationColorAndBack)]
		public float Duration = 0.2f;
		/// whether or not that sprite renderer should be turned off on start
		[Tooltip("whether or not that sprite renderer should be turned off on start")]
		public bool StartsOff = false;
		/// whether or not to reset shaker values after shake
		[Tooltip("whether or not to reset shaker values after shake")]
		[MMFEnumCondition("Mode", (int)Modes.ShakerEvent)]
		public bool ResetShakerValuesAfterShake = true;
		/// whether or not to reset the target's values after shake
		[Tooltip("whether or not to reset the target's values after shake")]
		[MMFEnumCondition("Mode", (int)Modes.ShakerEvent)]
		public bool ResetTargetValuesAfterShake = true;
		/// whether or not to broadcast a range to only affect certain shakers
		[Tooltip("whether or not to broadcast a range to only affect certain shakers")]
		[MMFEnumCondition("Mode", (int)Modes.ShakerEvent)]
		public bool OnlyBroadcastInRange = false;
		/// the range of the event, in units
		[Tooltip("the range of the event, in units")]
		[MMFEnumCondition("Mode", (int)Modes.ShakerEvent)]
		public float EventRange = 100f;
		/// the transform to use to broadcast the event as origin point
		[Tooltip("the transform to use to broadcast the event as origin point")]
		[MMFEnumCondition("Mode", (int)Modes.ShakerEvent)]
		public Transform EventOriginTransform;
		/// if this is true, calling that feedback will trigger it, even if it's in progress. If it's false, it'll prevent any new Play until the current one is over
		[Tooltip("if this is true, calling that feedback will trigger it, even if it's in progress. If it's false, it'll prevent any new Play until the current one is over")] 
		public bool AllowAdditivePlays = false;
		/// whether to grab the initial color to (potentially) go back to at init or when the feedback plays
		[Tooltip("whether to grab the initial color to (potentially) go back to at init or when the feedback plays")] 
		public InitialColorModes InitialColorMode = InitialColorModes.InitialColorOnPlay;
        
		[MMFInspectorGroup("Color", true, 52)]
		/// whether or not to modify the color of the sprite renderer
		[Tooltip("whether or not to modify the color of the sprite renderer")]
		public bool ModifyColor = true;
		/// the colors to apply to the sprite renderer over time
		[Tooltip("the colors to apply to the sprite renderer over time")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime, (int)Modes.ShakerEvent)]
		public Gradient ColorOverTime;
		/// the color to move to in instant mode
		[Tooltip("the color to move to in instant mode")]
		[MMFEnumCondition("Mode", (int)Modes.Instant, (int)Modes.ShakerEvent)]
		public Color InstantColor;
		/// the color to move to in ToDestination mode
		[Tooltip("the color to move to in instant mode")]
		[MMFEnumCondition("Mode", (int)Modes.Instant, (int)Modes.ToDestinationColor, (int)Modes.ToDestinationColorAndBack)]
		public Color ToDestinationColor = Color.red;
		/// the color to move to in ToDestination mode
		[Tooltip("the color to move to in instant mode")]
		[MMFEnumCondition("Mode", (int)Modes.Instant, (int)Modes.ToDestinationColor, (int)Modes.ToDestinationColorAndBack)]
		public AnimationCurve ToDestinationColorCurve = new AnimationCurve(new Keyframe(0, 0f), new Keyframe(1, 1f));
        
		[MMFInspectorGroup("Flip", true, 53)]
		/// whether or not to flip the sprite on X
		[Tooltip("whether or not to flip the sprite on X")]
		public bool FlipX = false;
		/// whether or not to flip the sprite on Y
		[Tooltip("whether or not to flip the sprite on Y")]
		public bool FlipY = false;

		protected Coroutine _coroutine;
		protected Color _initialColor;
		protected bool _initialFlipX;
		protected bool _initialFlipY;
        
		/// <summary>
		/// On init we turn the sprite renderer off if needed
		/// </summary>
		/// <param name="owner"></param>
		protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);

			if (EventOriginTransform == null)
			{
				EventOriginTransform = Owner.transform;
			}

			if (Active)
			{
				if (StartsOff)
				{
					Turn(false);
				}
			}

			if ((BoundSpriteRenderer != null) && (InitialColorMode == InitialColorModes.InitialColorOnInit))
			{
				_initialColor = BoundSpriteRenderer.color;
				_initialFlipX = BoundSpriteRenderer.flipX;
				_initialFlipY = BoundSpriteRenderer.flipY;
			}
		}

		/// <summary>
		/// On Play we turn our sprite renderer on and start an over time coroutine if needed
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
            
			if ((BoundSpriteRenderer != null) && (InitialColorMode == InitialColorModes.InitialColorOnPlay))
			{
				_initialColor = BoundSpriteRenderer.color;
				_initialFlipX = BoundSpriteRenderer.flipX;
				_initialFlipY = BoundSpriteRenderer.flipY;
			}
            
			float intensityMultiplier = ComputeIntensity(feedbacksIntensity, position);
			Turn(true);
			switch (Mode)
			{
				case Modes.Instant:
					if (ModifyColor)
					{
						BoundSpriteRenderer.color = InstantColor;
					}
					Flip();
					break;
				case Modes.OverTime:
					if (!AllowAdditivePlays && (_coroutine != null))
					{
						return;
					}
					_coroutine = Owner.StartCoroutine(SpriteRendererSequence());
					break;
				case Modes.ShakerEvent:
					MMSpriteRendererShakeEvent.Trigger(FeedbackDuration, ModifyColor, ColorOverTime, 
						FlipX, FlipY,   
						intensityMultiplier,
						ChannelData, ResetShakerValuesAfterShake, ResetTargetValuesAfterShake,
						OnlyBroadcastInRange, EventRange, EventOriginTransform.position);
					break;
				case Modes.ToDestinationColor:
					if (!AllowAdditivePlays && (_coroutine != null))
					{
						return;
					}
					_coroutine = Owner.StartCoroutine(SpriteRendererToDestinationSequence(false));
					break;
				case Modes.ToDestinationColorAndBack:
					if (!AllowAdditivePlays && (_coroutine != null))
					{
						return;
					}
					_coroutine = Owner.StartCoroutine(SpriteRendererToDestinationSequence(true));
					break;
			}
		}

		/// <summary>
		/// This coroutine will modify the values on the SpriteRenderer
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator SpriteRendererSequence()
		{
			float journey = NormalPlayDirection ? 0f : FeedbackDuration;
			IsPlaying = true;
			Flip();
			while ((journey >= 0) && (journey <= FeedbackDuration) && (FeedbackDuration > 0))
			{
				float remappedTime = MMFeedbacksHelpers.Remap(journey, 0f, FeedbackDuration, 0f, 1f);

				SetSpriteRendererValues(remappedTime);

				journey += NormalPlayDirection ? FeedbackDeltaTime : -FeedbackDeltaTime;
				yield return null;
			}
			SetSpriteRendererValues(FinalNormalizedTime);
			if (StartsOff)
			{
				Turn(false);
			}            
			_coroutine = null;
			IsPlaying = false;
			yield return null;
		}

		/// <summary>
		/// This coroutine will modify the values on the SpriteRenderer
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator SpriteRendererToDestinationSequence(bool andBack)
		{
			float journey = NormalPlayDirection ? 0f : FeedbackDuration;
			IsPlaying = true;
			Flip();
			while ((journey >= 0) && (journey <= FeedbackDuration) && (FeedbackDuration > 0))
			{
				float remappedTime = MMFeedbacksHelpers.Remap(journey, 0f, FeedbackDuration, 0f, 1f);

				if (andBack)
				{
					remappedTime = (remappedTime < 0.5f)
						? MMFeedbacksHelpers.Remap(remappedTime, 0f, 0.5f, 0f, 1f)
						: MMFeedbacksHelpers.Remap(remappedTime, 0.5f, 1f, 1f, 0f);
				}
                
				float evalTime = ToDestinationColorCurve.Evaluate(remappedTime);
                
				if (ModifyColor)
				{
					BoundSpriteRenderer.color = Color.LerpUnclamped(_initialColor, ToDestinationColor, evalTime);
				}

				journey += NormalPlayDirection ? FeedbackDeltaTime : -FeedbackDeltaTime;
				yield return null;
			}
			if (ModifyColor)
			{
				BoundSpriteRenderer.color = andBack ? _initialColor : ToDestinationColor;
			}
			if (StartsOff)
			{
				Turn(false);
			}            
			_coroutine = null;
			IsPlaying = false;
			yield return null;
		}

		/// <summary>
		/// Flips the sprite on X or Y based on the FlipX/FlipY settings
		/// </summary>
		protected virtual void Flip()
		{
			if (FlipX)
			{
				BoundSpriteRenderer.flipX = !BoundSpriteRenderer.flipX;
			}
			if (FlipY)
			{
				BoundSpriteRenderer.flipY = !BoundSpriteRenderer.flipY;
			}
		}

		/// <summary>
		/// Sets the various values on the sprite renderer on a specified time (between 0 and 1)
		/// </summary>
		/// <param name="time"></param>
		protected virtual void SetSpriteRendererValues(float time)
		{
			if (ModifyColor)
			{
				BoundSpriteRenderer.color = ColorOverTime.Evaluate(time);
			}
		}

		/// <summary>
		/// Stops the transition on stop if needed
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
		{
			if (!Active || !FeedbackTypeAuthorized || (_coroutine == null))
			{
				return;
			}
			base.CustomStopFeedback(position, feedbacksIntensity);
            
			Owner.StopCoroutine(_coroutine);
			IsPlaying = false;
			_coroutine = null;
		}

		/// <summary>
		/// Turns the sprite renderer on or off
		/// </summary>
		/// <param name="status"></param>
		protected virtual void Turn(bool status)
		{
			BoundSpriteRenderer.gameObject.SetActive(status);
			BoundSpriteRenderer.enabled = status;
		}
		
		/// <summary>
		/// On restore, we restore our initial state
		/// </summary>
		protected override void CustomRestoreInitialValues()
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			
			if (BoundSpriteRenderer != null)
			{
				BoundSpriteRenderer.color = _initialColor;
				BoundSpriteRenderer.flipX = _initialFlipX;
				BoundSpriteRenderer.flipY = _initialFlipY;
			}
		}
        
		/// <summary>
		/// On disable, 
		/// </summary>
		public override void OnDisable()
		{
			_coroutine = null;
		}
	}
}