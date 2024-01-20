using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback will request the spawn of a floating text, usually to signify damage, but not necessarily
	/// This requires that a MMFloatingTextSpawner be correctly setup in the scene, otherwise nothing will happen.
	/// To do so, create a new empty object, add a MMFloatingTextSpawner to it. Drag (at least) one MMFloatingText prefab into its PooledSimpleMMFloatingText slot.
	/// You'll find such prefabs already made in the MMTools/Tools/MMFloatingText/Prefabs folder, but feel free to create your own.
	/// Using that feedback will always spawn the same text. While this may be what you want, if you're using the Corgi Engine or TopDown Engine, you'll find dedicated versions
	/// directly hooked to the Health component, letting you display damage taken.
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback will request the spawn of a floating text, usually to signify damage, but not necessarily. " +
	              "This requires that a MMFloatingTextSpawner be correctly setup in the scene, otherwise nothing will happen. " +
	              "To do so, create a new empty object, add a MMFloatingTextSpawner to it. Drag (at least) one MMFloatingText prefab into its PooledSimpleMMFloatingText slot. " +
	              "You'll find such prefabs already made in the MMTools/Tools/MMFloatingText/Prefabs folder, but feel free to create your own. " +
	              "Using that feedback will always spawn the same text. While this may be what you want, if you're using the Corgi Engine or TopDown Engine, you'll find dedicated versions " +
	              "directly hooked to the Health component, letting you display damage taken.")]
	[FeedbackPath("UI/Floating Text")]
	public class MMF_FloatingText : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.UIColor; } }
		#endif

		/// the duration of this feedback is a fixed value or the lifetime
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(Lifetime); } set { Lifetime = value; } }
		public override bool HasChannel => true;
		public override bool HasRandomness => true;

		/// the possible places where the floating text should spawn at
		public enum PositionModes { TargetTransform, FeedbackPosition, PlayPosition }

		[MMFInspectorGroup("Floating Text", true, 64)]
		/// the Intensity to spawn this text with, will act as a lifetime/movement/scale multiplier based on the spawner's settings
		[Tooltip("the Intensity to spawn this text with, will act as a lifetime/movement/scale multiplier based on the spawner's settings")]
		public float Intensity = 1f;
		/// the value to display when spawning this text
		[Tooltip("the value to display when spawning this text")]
		public string Value = "100";
		/// if this is true, the intensity passed to this feedback will be the value displayed
		[Tooltip("if this is true, the intensity passed to this feedback will be the value displayed")]
		public bool UseIntensityAsValue = false;
		
		/// the possible methods that can be applied to the output value (when using intensity as the output value, string values won't get rounded) 
		public enum RoundingMethods { NoRounding, Round, Ceil, Floor }
		
		/// the rounding methods to apply to the output value (when using intensity as the output value, string values won't get rounded)
		[Tooltip("the rounding methods to apply to the output value (when using intensity as the output value, string values won't get rounded)")]
		[MMFInspectorGroup("Rounding", true, 68)]
		public RoundingMethods RoundingMethod = RoundingMethods.NoRounding;

		[MMFInspectorGroup("Color", true, 65)]
		/// whether or not to force a color on the new text, if not, the default colors of the spawner will be used
		[Tooltip("whether or not to force a color on the new text, if not, the default colors of the spawner will be used")]
		public bool ForceColor = false;
		/// the gradient to apply over the lifetime of the text
		[Tooltip("the gradient to apply over the lifetime of the text")]
		[GradientUsage(true)]
		public Gradient AnimateColorGradient = new Gradient();

		[MMFInspectorGroup("Lifetime", true, 66)]
		/// whether or not to force a lifetime on the new text, if not, the default colors of the spawner will be used
		[Tooltip("whether or not to force a lifetime on the new text, if not, the default colors of the spawner will be used")]
		public bool ForceLifetime = false;
		/// the forced lifetime for the spawned text
		[Tooltip("the forced lifetime for the spawned text")]
		[MMFCondition("ForceLifetime", true)]
		public float Lifetime = 0.5f;

		[MMFInspectorGroup("Position", true, 67)]
		/// where to spawn the new text (at the position of the feedback, or on a specified Transform)
		[Tooltip("where to spawn the new text (at the position of the feedback, or on a specified Transform)")]
		public PositionModes PositionMode = PositionModes.FeedbackPosition;
		/// in transform mode, the Transform on which to spawn the new floating text
		[Tooltip("in transform mode, the Transform on which to spawn the new floating text")]
		[MMFEnumCondition("PositionMode", (int)PositionModes.TargetTransform)]
		public Transform TargetTransform;
		/// the direction to apply to the new floating text (leave it to 0 to let the Spawner decide based on its settings)
		[Tooltip("the direction to apply to the new floating text (leave it to 0 to let the Spawner decide based on its settings)")]
		public Vector3 Direction = Vector3.zero;

		protected Vector3 _playPosition;
		protected string _value;

		/// <summary>
		/// On play we ask the spawner on the specified channel to spawn a new floating text
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
            
			float intensityMultiplier = ComputeIntensity(feedbacksIntensity, position);
			switch (PositionMode)
			{
				case PositionModes.FeedbackPosition:
					_playPosition = Owner.transform.position;
					break;
				case PositionModes.PlayPosition:
					_playPosition = position;
					break;
				case PositionModes.TargetTransform:
					_playPosition = TargetTransform.position;
					break;
			}

			if (RoundingMethod != RoundingMethods.NoRounding)
			{
				switch (RoundingMethod)
				{
					case RoundingMethods.Ceil:
						

						break;
				}
			}

			feedbacksIntensity = ApplyRounding(feedbacksIntensity);
			
			_value = UseIntensityAsValue ? feedbacksIntensity.ToString() : Value;
			
			MMFloatingTextSpawnEvent.Trigger(ChannelData, _playPosition, _value, Direction, Intensity * intensityMultiplier, ForceLifetime, Lifetime, ForceColor, AnimateColorGradient, ComputedTimescaleMode == TimescaleModes.Unscaled);
		}

		protected virtual float ApplyRounding(float value)
		{
			if (RoundingMethod == RoundingMethods.NoRounding)
			{
				return value;
			}

			switch (RoundingMethod)
			{
				case RoundingMethods.Round:
					return Mathf.Round(value);
				case RoundingMethods.Ceil:
					return Mathf.Ceil(value);
				case RoundingMethods.Floor:
					return Mathf.Floor(value);
			}

			return value;
		}
	}
}