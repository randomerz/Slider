using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback lets you broadcast a float value to the MMRadio system
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback lets you broadcast a float value to the MMRadio system.")]
	[FeedbackPath("GameObject/Broadcast")]
	public class MMFeedbackBroadcast : MMFeedbackBase
	{
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.UIColor; } }
		#endif

		[Header("Target Channel")]
		/// the channel to write the level to
		[Tooltip("the channel to write the level to")]
		public int Channel;

		[Header("Level")]
		/// the curve to tween the intensity on
		[Tooltip("the curve to tween the intensity on")]
		[MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.OverTime)]
		public MMTweenType Curve = new MMTweenType(new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0)));
		/// the value to remap the intensity curve's 0 to
		[Tooltip("the value to remap the intensity curve's 0 to")]
		[MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.OverTime)]
		public float RemapZero = 0f;
		/// the value to remap the intensity curve's 1 to
		[Tooltip("the value to remap the intensity curve's 1 to")]
		[MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.OverTime)]
		public float RemapOne = 1f;
		/// the value to move the intensity to in instant mode
		[Tooltip("the value to move the intensity to in instant mode")]
		[MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.Instant)]
		public float InstantChange;
		/// a debug view of the current level being broadcasted
		[Tooltip("a debug view of the current level being broadcasted")]
		[MMReadOnly]
		public float DebugLevel;
		/// whether or not a broadcast is in progress (will be false while the value is not changing, and thus not broadcasting)
		[Tooltip("whether or not a broadcast is in progress (will be false while the value is not changing, and thus not broadcasting)")]
		[MMReadOnly]
		public bool BroadcastInProgress = false;

		public float ThisLevel { get; set; }
		protected float _levelLastFrame;

		/// <summary>
		/// We setup our target with this object
		/// </summary>
		protected override void FillTargets()
		{
			MMFeedbackBaseTarget target = new MMFeedbackBaseTarget();
			MMPropertyReceiver receiver = new MMPropertyReceiver();
			receiver.TargetObject = this.gameObject;
			receiver.TargetComponent = this;
			receiver.TargetPropertyName = "ThisLevel";
			receiver.RelativeValue = RelativeValues;
			target.Target = receiver;
			target.LevelCurve = Curve;
			target.RemapLevelZero = RemapZero;
			target.RemapLevelOne = RemapOne;
			target.InstantLevel = InstantChange;

			_targets.Add(target);
		}

		/// <summary>
		/// On Update we process our broadcast
		/// </summary>
		protected virtual void Update()
		{
			ProcessBroadcast();
		}

		/// <summary>
		/// Broadcasts the value if needed
		/// </summary>
		protected virtual void ProcessBroadcast()
		{
			BroadcastInProgress = false;
			if (ThisLevel != _levelLastFrame)
			{
				MMRadioLevelEvent.Trigger(Channel, ThisLevel);
				BroadcastInProgress = true;
			}
			DebugLevel = ThisLevel;
			_levelLastFrame = ThisLevel;
		}

	}
}