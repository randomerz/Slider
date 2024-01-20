using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if MM_CINEMACHINE
using Cinemachine;
#endif
using MoreMountains.Feedbacks;

namespace MoreMountains.FeedbacksForThirdParty
{
	/// <summary>
	/// Add this to a Cinemachine virtual camera and it'll be able to listen to MMCinemachinePriorityEvent, usually triggered by a MMFeedbackCinemachineTransition
	/// </summary>
	[AddComponentMenu("More Mountains/Feedbacks/Shakers/Cinemachine/MMCinemachinePriorityListener")]
	#if MM_CINEMACHINE
	[RequireComponent(typeof(CinemachineVirtualCameraBase))]
	#endif
	public class MMCinemachinePriorityListener : MonoBehaviour
	{
        
		[HideInInspector] 
		public TimescaleModes TimescaleMode = TimescaleModes.Scaled;
        
        
		public virtual float GetTime() { return (TimescaleMode == TimescaleModes.Scaled) ? Time.time : Time.unscaledTime; }
		public virtual float GetDeltaTime() { return (TimescaleMode == TimescaleModes.Scaled) ? Time.deltaTime : Time.unscaledDeltaTime; }
        
		[Header("Priority Listener")]
		[Tooltip("whether to listen on a channel defined by an int or by a MMChannel scriptable object. Ints are simple to setup but can get messy and make it harder to remember what int corresponds to what. " +
		         "MMChannel scriptable objects require you to create them in advance, but come with a readable name and are more scalable")]
		public MMChannelModes ChannelMode = MMChannelModes.Int;
		/// the channel to listen to - has to match the one on the feedback
		[Tooltip("the channel to listen to - has to match the one on the feedback")]
		[MMFEnumCondition("ChannelMode", (int)MMChannelModes.Int)]
		public int Channel = 0;
		/// the MMChannel definition asset to use to listen for events. The feedbacks targeting this shaker will have to reference that same MMChannel definition to receive events - to create a MMChannel,
		/// right click anywhere in your project (usually in a Data folder) and go MoreMountains > MMChannel, then name it with some unique name
		[Tooltip("the MMChannel definition asset to use to listen for events. The feedbacks targeting this shaker will have to reference that same MMChannel definition to receive events - to create a MMChannel, " +
		         "right click anywhere in your project (usually in a Data folder) and go MoreMountains > MMChannel, then name it with some unique name")]
		[MMFEnumCondition("ChannelMode", (int)MMChannelModes.MMChannel)]
		public MMChannel MMChannelDefinition = null;

		#if MM_CINEMACHINE
		protected CinemachineVirtualCameraBase _camera;
		protected int _initialPriority;
        
		/// <summary>
		/// On Awake we store our virtual camera
		/// </summary>
		protected virtual void Awake()
		{
			_camera = this.gameObject.GetComponent<CinemachineVirtualCameraBase>();
			_initialPriority = _camera.Priority;
		}

		/// <summary>
		/// When we get an event we change our priorities if needed
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="forceMaxPriority"></param>
		/// <param name="newPriority"></param>
		/// <param name="forceTransition"></param>
		/// <param name="blendDefinition"></param>
		/// <param name="resetValuesAfterTransition"></param>
		public virtual void OnMMCinemachinePriorityEvent(MMChannelData channelData, bool forceMaxPriority, int newPriority, bool forceTransition, CinemachineBlendDefinition blendDefinition, bool resetValuesAfterTransition, TimescaleModes timescaleMode, bool restore = false)
		{
			TimescaleMode = timescaleMode;
			if (MMChannel.Match(channelData, ChannelMode, Channel, MMChannelDefinition))
			{
				if (restore)
				{
					_camera.Priority = _initialPriority;	
					return;
				}
				_camera.Priority = newPriority;
			}
			else
			{
				if (forceMaxPriority)
				{
					if (restore)
					{
						_camera.Priority = _initialPriority;	
						return;
					}
					_camera.Priority = 0;
				}
			}
		}

		/// <summary>
		/// On enable we start listening for events
		/// </summary>
		protected virtual void OnEnable()
		{
			MMCinemachinePriorityEvent.Register(OnMMCinemachinePriorityEvent);
		}

		/// <summary>
		/// Stops listening for events
		/// </summary>
		protected virtual void OnDisable()
		{
			MMCinemachinePriorityEvent.Unregister(OnMMCinemachinePriorityEvent);
		}
		#endif
	}

	/// <summary>
	/// An event used to pilot priorities on cinemachine virtual cameras and brain transitions
	/// </summary>
	public struct MMCinemachinePriorityEvent
	{
		#if MM_CINEMACHINE
		static private event Delegate OnEvent;
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void RuntimeInitialization() { OnEvent = null; }
		static public void Register(Delegate callback) { OnEvent += callback; }
		static public void Unregister(Delegate callback) { OnEvent -= callback; }

		public delegate void Delegate(MMChannelData channelData, bool forceMaxPriority, int newPriority, bool forceTransition, CinemachineBlendDefinition blendDefinition, bool resetValuesAfterTransition, TimescaleModes timescaleMode, bool restore = false);
		static public void Trigger(MMChannelData channelData, bool forceMaxPriority, int newPriority, bool forceTransition, CinemachineBlendDefinition blendDefinition, bool resetValuesAfterTransition, TimescaleModes timescaleMode, bool restore = false)
		{
			OnEvent?.Invoke(channelData, forceMaxPriority, newPriority, forceTransition, blendDefinition, resetValuesAfterTransition, timescaleMode, restore);
		}
		#endif
	}
}