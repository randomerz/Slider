#if MM_CINEMACHINE
using Cinemachine;
#endif
using UnityEngine;

namespace MoreMountains.Tools
{
	/// <summary>
	/// 3D Implementation of the CinemachineZone abstract class
	/// </summary>
	[RequireComponent(typeof(Collider))]
	public class MMCinemachineZone3D : MMCinemachineZone
	{
		protected Collider _collider;
		protected Collider _confinerCollider;
		protected Rigidbody _confinerRigidbody;
		protected BoxCollider _boxCollider;
		protected SphereCollider _sphereCollider;
		#if MM_CINEMACHINE
		protected CinemachineConfiner _cinemachineConfiner;
        
		/// <summary>
		/// Gets and sets up the colliders
		/// </summary>
		protected override void InitializeCollider()
		{
			_collider = GetComponent<Collider>();
			_boxCollider = GetComponent<BoxCollider>();
			_sphereCollider = GetComponent<SphereCollider>();
			_collider.isTrigger = true;
		}

		/// <summary>
		/// Creates and sets up the camera's confiner
		/// </summary>
		protected override void SetupConfiner()
		{
			// we add a rigidbody to it and set it up
			_confinerRigidbody = _confinerGameObject.AddComponent<Rigidbody>();
			_confinerRigidbody.useGravity = false;
			_confinerRigidbody.gameObject.isStatic = true;
			_confinerRigidbody.isKinematic = true;
            
			// we copy the collider and set it up

			CopyCollider();
			_confinerGameObject.transform.localPosition = Vector3.zero;
            
            
			// we set the composite collider as the virtual camera's confiner
			_cinemachineConfiner = VirtualCamera.gameObject.MMGetComponentAroundOrAdd<CinemachineConfiner>();
			_cinemachineConfiner.m_ConfineMode = CinemachineConfiner.Mode.Confine3D;
			_cinemachineConfiner.m_ConfineScreenEdges = true;

			if (_boxCollider != null)
			{
				_cinemachineConfiner.m_BoundingVolume = _boxCollider;
			}

			if (_sphereCollider != null)
			{
				_cinemachineConfiner.m_BoundingVolume = _sphereCollider;
			}
		}

		/// <summary>
		/// Copies the initial collider to the composite
		/// </summary>
		protected virtual void CopyCollider()
		{
			if (_boxCollider != null)
			{
				BoxCollider boxCollider = _confinerGameObject.AddComponent<BoxCollider>();
				boxCollider.size = _boxCollider.size;
				boxCollider.center = _boxCollider.center;
				boxCollider.isTrigger = true;
			}

			if (_sphereCollider != null)
			{
				SphereCollider sphereCollider = _confinerGameObject.AddComponent<SphereCollider>();
				sphereCollider.isTrigger = true;
				sphereCollider.center = _sphereCollider.center;
				sphereCollider.radius = _sphereCollider.radius;
			}
		}

		/// <summary>
		/// On enter, enables the camera and triggers the enter event
		/// </summary>
		/// <param name="collider"></param>
		protected virtual void OnTriggerEnter(Collider collider)
		{
			if (!TestCollidingGameObject(collider.gameObject))
			{
				return;
			}
			if (TriggerMask.MMContains (collider.gameObject))
			{
				EnterZone();
			}
		}

		/// <summary>
		/// On exit, disables the camera and invokes the exit event
		/// </summary>
		/// <param name="collider"></param>
		protected virtual void OnTriggerExit(Collider collider)
		{
			if (!TestCollidingGameObject(collider.gameObject))
			{
				return;
			}
			if (TriggerMask.MMContains (collider.gameObject))
			{
				ExitZone();
			}
		}
		#endif
        
		#if UNITY_EDITOR
		/// <summary>
		/// Draws gizmos to show the shape of the zone
		/// </summary>
		protected virtual void OnDrawGizmos()
		{
			if (!DrawGizmos)
			{
				return;
			}
            
			Gizmos.color = GizmosColor;

			if ((_boxCollider != null) && _boxCollider.enabled)
			{
				_gizmoSize =  _boxCollider.bounds.size ;
				Gizmos.DrawCube(_boxCollider.bounds.center, _gizmoSize);
			}
			if (_sphereCollider != null && _sphereCollider.enabled)
			{
				Gizmos.DrawSphere(this.transform.position + _sphereCollider.center, _sphereCollider.radius);                
			}
		}
		#endif
	}    
}