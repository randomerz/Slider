using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace  MoreMountains.Tools
{
	/// <summary>
	/// Add this class to an object and it will automatically teleport to the other end of the screen when reaching the screen's edges
	/// </summary>
	public class MMViewportEdgeTeleporter : MonoBehaviour
	{
		[Header("Camera")] 
		/// whether or not to grab the Camera.main and store it on init
		public bool AutoGrabMainCamera;
		/// the camera used to compute viewport positions
		public Camera MainCamera;

		[Header("Viewport Bounds")] 
		/// the origin values of the viewport
		[MMVector("X","Y")]
		public Vector2 ViewportOrigin = new Vector2(0f, 0f);
		/// the dimensions of the viewport
		[MMVector("W","H")]
		public Vector2 ViewportDimensions = new Vector2(1f, 1f);
        
		[Header("Teleport Bounds")] 
		/// the origin of the teleport destination zone
		[MMVector("X","Y")]
		public Vector2 TeleportOrigin = new Vector2(0f, 0f);
		/// the dimensions of the teleport destination zone
		[MMVector("W","H")]
		public Vector2 TeleportDimensions = new Vector2(1f, 1f);

		[Header("Events")] 
		/// an event to trigger on teleport
		public UnityEvent OnTeleport;
        
		protected Vector3 _viewportPosition;
		protected Vector3 _newViewportPosition;
        
		/// <summary>
		/// On Awake we initialize our teleporter
		/// </summary>
		protected virtual void Awake()
		{
			Initialization();
		}

		/// <summary>
		/// Stores the main camera if needed
		/// </summary>
		protected virtual void Initialization()
		{
			if (AutoGrabMainCamera)
			{
				MainCamera = Camera.main;
			}
		}

		/// <summary>
		/// Sets a new camera
		/// </summary>
		/// <param name="newCamera"></param>
		public virtual void SetCamera(Camera newCamera)
		{
			MainCamera = newCamera;
		}

		/// <summary>
		/// On Update we check our position relative to the edges
		/// </summary>
		protected virtual void Update()
		{
			DetectEdges();
		}

		/// <summary>
		/// Detects edges, compares with our object's position, and moves it if needed
		/// </summary>
		protected virtual void DetectEdges()
		{
			_viewportPosition = MainCamera.WorldToViewportPoint(this.transform.position);
            
			bool teleport = false;
            
			if (_viewportPosition.x < ViewportOrigin.x) 
			{
				_newViewportPosition.x = TeleportDimensions.x;
				_newViewportPosition.y = _viewportPosition.y;
				_newViewportPosition.z = _viewportPosition.z;
				teleport = true;
			}
			else if (_viewportPosition.x >= ViewportDimensions.x) 
			{
				_newViewportPosition.x = TeleportOrigin.x;
				_newViewportPosition.y = _viewportPosition.y;
				_newViewportPosition.z = _viewportPosition.z;
				teleport = true;
			}
			if (_viewportPosition.y < ViewportOrigin.y) 
			{
				_newViewportPosition.x = _viewportPosition.x;
				_newViewportPosition.y = TeleportDimensions.y;
				_newViewportPosition.z = _viewportPosition.z;
				teleport = true;
			}
			else if (_viewportPosition.y >= ViewportDimensions.y) 
			{
				_newViewportPosition.x = _viewportPosition.x;
				_newViewportPosition.y = TeleportOrigin.y;
				_newViewportPosition.z = _viewportPosition.z;
				teleport = true;
			}

			if (teleport)
			{
				OnTeleport?.Invoke();
				this.transform.position = MainCamera.ViewportToWorldPoint(_newViewportPosition);
			}
		}
	}    
}