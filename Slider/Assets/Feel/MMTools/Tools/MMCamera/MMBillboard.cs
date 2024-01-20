using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

namespace MoreMountains.Tools
{
	/// <summary>
	/// Add this class to an object (usually a sprite) and it'll face the camera at all times
	/// </summary>
	[AddComponentMenu("More Mountains/Tools/Camera/MMBillboard")]
	public class MMBillboard : MonoBehaviour
	{
		/// the camera we're facing
		public Camera MainCamera { get; set; }
		/// whether or not this object should automatically grab a camera on start
		[Tooltip("whether or not this object should automatically grab a camera on start")]
		public bool GrabMainCameraOnStart = true;
		/// whether or not to nest this object below a parent container
		[Tooltip("whether or not to nest this object below a parent container")]
		public bool NestObject = true;
		/// the Vector3 to offset the look at direction by
		[Tooltip("the Vector3 to offset the look at direction by")]
		public Vector3 OffsetDirection = Vector3.back;
		/// the Vector3 to consider as "world up"
		[Tooltip("the Vector3 to consider as 'world up'")] 
		public Vector3 Up = Vector3.up;

		protected GameObject _parentContainer;
		private Transform _transform;

		/// <summary>
		/// On awake we grab a camera if needed, and nest our object
		/// </summary>
		protected virtual void Awake()
		{
			_transform = transform;

			if (GrabMainCameraOnStart == true)
			{
				GrabMainCamera ();
			}
		}

		private void Start()
		{
			if (NestObject)
			{
				NestThisObject();
			}                
		}

		/// <summary>
		/// Nests this object below a parent container
		/// </summary>
		protected virtual void NestThisObject()
		{
			_parentContainer = new GameObject();
			SceneManager.MoveGameObjectToScene(_parentContainer, this.gameObject.scene);
			_parentContainer.name = "Parent"+transform.gameObject.name;
			_parentContainer.transform.position = transform.position;
			transform.SetParent(_parentContainer.transform);
		}

		/// <summary>
		/// Grabs the main camera.
		/// </summary>
		protected virtual void GrabMainCamera()
		{
			MainCamera = Camera.main;
		}

		/// <summary>
		/// On update, we change our parent container's rotation to face the camera
		/// </summary>
		protected virtual void Update()
		{
			if (NestObject)
			{
				_parentContainer.transform.LookAt(_parentContainer.transform.position + MainCamera.transform.rotation * OffsetDirection, MainCamera.transform.rotation * Up);
			}                
			else
			{
				_transform.LookAt(_transform.position + MainCamera.transform.rotation * OffsetDirection, MainCamera.transform.rotation * Up);
			}
		}
	}
}