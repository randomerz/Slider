using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.Feedbacks
{
	[RequireComponent(typeof(MMFeedbacks))]
	[AddComponentMenu("More Mountains/Feedbacks/Shakers/Feedbacks/MMFeedbacksShaker")]
	public class MMFeedbacksShaker : MMShaker
	{
		protected MMFeedbacks _mmFeedbacks;

		/// <summary>
		/// On init we initialize our values
		/// </summary>
		protected override void Initialization()
		{
			base.Initialization();
			_mmFeedbacks = this.gameObject.GetComponent<MMFeedbacks>();
		}

		public virtual void OnMMFeedbacksShakeEvent(MMChannelData channelData = null, bool useRange = false, float eventRange = 0f, Vector3 eventOriginPosition = default(Vector3))
		{
			if (!CheckEventAllowed(channelData, useRange, eventRange, eventOriginPosition) || (!Interruptible && Shaking))
			{
				return;
			}
			Play();
		}

		protected override void ShakeStarts()
		{
			_mmFeedbacks.PlayFeedbacks();
		}

		/// <summary>
		/// When that shaker gets added, we initialize its shake duration
		/// </summary>
		protected virtual void Reset()
		{
			ShakeDuration = 0.01f;
		}

		/// <summary>
		/// Starts listening for events
		/// </summary>
		public override void StartListening()
		{
			base.StartListening();
			MMFeedbacksShakeEvent.Register(OnMMFeedbacksShakeEvent);
		}

		/// <summary>
		/// Stops listening for events
		/// </summary>
		public override void StopListening()
		{
			base.StopListening();
			MMFeedbacksShakeEvent.Unregister(OnMMFeedbacksShakeEvent);
		}
	}

	public struct MMFeedbacksShakeEvent
	{
		static private event Delegate OnEvent;
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void RuntimeInitialization() { OnEvent = null; }
		static public void Register(Delegate callback) { OnEvent += callback; }
		static public void Unregister(Delegate callback) { OnEvent -= callback; }

		public delegate void Delegate(MMChannelData channelData = null, bool useRange = false, float eventRange = 0f, Vector3 eventOriginPosition = default(Vector3));

		static public void Trigger(MMChannelData channelData = null, bool useRange = false, float eventRange = 0f, Vector3 eventOriginPosition = default(Vector3))
		{
			OnEvent?.Invoke(channelData, useRange, eventRange, eventOriginPosition);
		}
	}
}