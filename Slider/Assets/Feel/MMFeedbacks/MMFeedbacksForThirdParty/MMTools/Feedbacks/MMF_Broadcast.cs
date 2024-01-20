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
	public class MMF_Broadcast : MMF_FeedbackBase
	{
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.UIColor; } }
		#endif
		public override bool HasChannel => true;

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

		protected MMF_BroadcastProxy _proxy;
        
		/// <summary>
		/// On init we store our initial alpha
		/// </summary>
		/// <param name="owner"></param>
		protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);

			_proxy = Owner.gameObject.AddComponent<MMF_BroadcastProxy>();
			_proxy.Channel = Channel;
			PrepareTargets();
		}

		/// <summary>
		/// We setup our target with this object
		/// </summary>
		protected override void FillTargets()
		{
			MMF_FeedbackBaseTarget target = new MMF_FeedbackBaseTarget();
			MMPropertyReceiver receiver = new MMPropertyReceiver();
			receiver.TargetObject = Owner.gameObject;
			receiver.TargetComponent = _proxy;
			receiver.TargetPropertyName = "ThisLevel";
			receiver.RelativeValue = RelativeValues;
			target.Target = receiver;
			target.LevelCurve = Curve;
			target.RemapLevelZero = RemapZero;
			target.RemapLevelOne = RemapOne;
			target.InstantLevel = InstantChange;

			_targets.Add(target);
		}
	}
}