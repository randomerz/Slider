using System.Collections;
using System.Collections.Generic;
#if MM_CINEMACHINE
using Cinemachine;
#endif
using UnityEngine;
using UnityEngine.Events;

namespace MoreMountains.Tools
{
	/// <summary>
	/// An abstract class that lets you define a zone that, when entered, enables a virtual camera, and takes care
	/// of all the boilerplate setup
	/// </summary>
	[AddComponentMenu("")]
	[ExecuteAlways]
	public abstract class MMCinemachineZone : MonoBehaviour
	{
		public enum Modes { Enable, Priority }
        
		[Header("Virtual Camera")]
		/// whether to enable/disable virtual cameras, or to play on their priority for transitions
		[Tooltip("whether to enable/disable virtual cameras, or to play on their priority for transitions")]
		public Modes Mode = Modes.Enable;
		/// whether or not the camera in this zone should start active
		[Tooltip("whether or not the camera in this zone should start active")]
		public bool CameraStartsActive = false;
		#if MM_CINEMACHINE
		/// the virtual camera associated to this zone (will try to grab one in children if none is set) 
		[Tooltip("the virtual camera associated to this zone (will try to grab one in children if none is set)")]
		public CinemachineVirtualCamera VirtualCamera;
		#endif

		/// when in priority mode, the priority this camera should have when the zone is active
		[Tooltip("when in priority mode, the priority this camera should have when the zone is active")]
		[MMEnumCondition("Mode", (int)Modes.Priority)]
		public int EnabledPriority = 10;
		/// when in priority mode, the priority this camera should have when the zone is inactive
		[Tooltip("when in priority mode, the priority this camera should have when the zone is inactive")]
		[MMEnumCondition("Mode", (int)Modes.Priority)]
		public int DisabledPriority = 0;

		[Header("Collisions")] 
		/// a layermask containing all the layers that should activate this zone
		[Tooltip("a layermask containing all the layers that should activate this zone")]
		public LayerMask TriggerMask;
        
		[Header("Confiner Setup")] 
		/// whether or not the zone should auto setup its camera's confiner on start - alternative is to manually click the ManualSetupConfiner, or do your own setup
		[Tooltip("whether or not the zone should auto setup its camera's confiner on start - alternative is to manually click the ManualSetupConfiner, or do your own setup")]
		public bool SetupConfinerOnStart = false;

		/// a debug button used to setup the confiner on click
		[MMInspectorButton("ManualSetupConfiner")]
		public bool GenerateConfinerSetup;
		
		[Header("State")]
		/// whether this room is the current room or not
		[Tooltip("whether this room is the current room or not")]
		[MMReadOnly]
		public bool CurrentRoom = false;
		/// whether this room has already been visited or not
		[Tooltip("whether this room has already been visited or not")]
		public bool RoomVisited = false;

		[Header("Events")] 
		/// a UnityEvent to trigger when entering the zone for the first time
		[Tooltip("a UnityEvent to trigger when entering the zone for the first time")]
		public UnityEvent OnEnterZoneForTheFirstTimeEvent;
		/// a UnityEvent to trigger when entering the zone
		[Tooltip("a UnityEvent to trigger when entering the zone")]
		public UnityEvent OnEnterZoneEvent;
		/// a UnityEvent to trigger when exiting the zone
		[Tooltip("a UnityEvent to trigger when exiting the zone")]
		public UnityEvent OnExitZoneEvent;

		[Header("Activation")]

		/// a list of gameobjects to enable when entering the zone, and disable when exiting it
		[Tooltip("a list of gameobjects to enable when entering the zone, and disable when exiting it")]
		public List<GameObject> ActivationList;

		[Header("Debug")] 
		/// whether or not to draw shape gizmos to help visualize the zone's bounds
		[Tooltip("whether or not to draw shape gizmos to help visualize the zone's bounds")]
		public bool DrawGizmos = true;
		/// the color of the gizmos to draw in edit mode
		[Tooltip("the color of the gizmos to draw in edit mode")] 
		public Color GizmosColor;
        
		protected GameObject _confinerGameObject;
		protected Vector3 _gizmoSize;

		#if MM_CINEMACHINE
        
		/// <summary>
		/// On Awake we proceed to init if app is playing
		/// </summary>
		protected virtual void Awake()
		{
			AlwaysInitialization();
			if (!Application.isPlaying)
			{
				return;
			}
			Initialization();
		}

		/// <summary>
		/// On Awake we initialize our collider
		/// </summary>
		protected virtual void AlwaysInitialization()
		{
			InitializeCollider();
		}

		/// <summary>
		/// On init we grab our virtual camera 
		/// </summary>
		protected virtual void Initialization()
		{
			if (VirtualCamera == null)
			{
				VirtualCamera = GetComponentInChildren<CinemachineVirtualCamera>();
			}

			if (VirtualCamera == null)
			{
				Debug.LogWarning("[MMCinemachineZone2D] " + this.name + " : no virtual camera is attached to this zone. Set one in its inspector.");
			}

			if (SetupConfinerOnStart)
			{
				SetupConfinerGameObject();	
			}
            
			foreach (GameObject go in ActivationList)
			{
				go.SetActive(false);
			}
		}

		/// <summary>
		/// On Start we setup the confiner
		/// </summary>
		protected virtual void Start()
		{
			if (!Application.isPlaying)
			{
				return;
			}

			if (SetupConfinerOnStart)
			{
				SetupConfiner();	
			}
			
			StartCoroutine(EnableCamera(CameraStartsActive, 1));
		}

		/// <summary>
		/// Describes what happens when initializing the collider
		/// </summary>
		protected abstract void InitializeCollider();

		/// <summary>
		/// Describes what happens when setting up the confiner
		/// </summary>
		protected abstract void SetupConfiner();

		/// <summary>
		/// A method used to manually create a confiner
		/// </summary>
		protected virtual void ManualSetupConfiner()
		{
			Initialization();
			SetupConfiner();
		}

		/// <summary>
		/// Creates an object to host the confiner
		/// </summary>
		protected virtual void SetupConfinerGameObject()
		{
			// we remove the object if needed
			Transform child = this.transform.Find("Confiner");
			if (child != null)
			{
				DestroyImmediate(child.gameObject);
			}
            
			// we create an empty child object
			_confinerGameObject = new GameObject();
			_confinerGameObject.transform.localPosition = Vector3.zero;
			_confinerGameObject.transform.SetParent(this.transform);
			_confinerGameObject.name = "Confiner";
		}

		/// <summary>
		/// An extra test you can override to add extra collider conditions
		/// </summary>
		/// <param name="collider"></param>
		/// <returns></returns>
		protected virtual bool TestCollidingGameObject(GameObject collider)
		{
			return true;
		}
        
		/// <summary>
		/// Enables the camera, either via enabled state or priority
		/// </summary>
		/// <param name="state"></param>
		/// <param name="frames"></param>
		/// <returns></returns>
		protected virtual IEnumerator EnableCamera(bool state, int frames)
		{
			if (VirtualCamera == null)
			{
				yield break;
			}

			if (frames > 0)
			{
				yield return MMCoroutine.WaitForFrames(frames);    
			}

			if (Mode == Modes.Enable)
			{
				VirtualCamera.enabled = state;
			}
			else if (Mode == Modes.Priority)
			{
				VirtualCamera.Priority = state ? EnabledPriority : DisabledPriority;
			}
		}

		protected virtual void EnterZone()
		{
			if (!RoomVisited)
			{
				OnEnterZoneForTheFirstTimeEvent.Invoke();	
			}
			
			CurrentRoom = true;
			RoomVisited = true;

			OnEnterZoneEvent.Invoke();
			StartCoroutine(EnableCamera(true, 0));
			foreach(GameObject go in ActivationList)
			{
				go.SetActive(true);
			}
		}

		protected virtual void ExitZone()
		{
			CurrentRoom = false;
			OnExitZoneEvent.Invoke();
			StartCoroutine(EnableCamera(false, 0));
			foreach (GameObject go in ActivationList)
			{
				go.SetActive(false);
			}
		}

		/// <summary>
		/// On Reset we initialize our gizmo color
		/// </summary>
		protected virtual void Reset()
		{
			GizmosColor = MMColors.RandomColor();
			GizmosColor.a = 0.2f;
		}

		#endif
	}    
}