using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
	/// <summary>
	/// Prevents fast moving objects from going through colliders by casting a ray backwards after each movement
	/// </summary>
	[AddComponentMenu("More Mountains/Tools/Movement/MMPreventPassingThrough3D")]
	public class MMPreventPassingThrough3D : MonoBehaviour 
	{
		/// the layer mask to search obstacles on
		public LayerMask ObstaclesLayerMask; 
		/// the bounds adjustment variable
		public float SkinWidth = 0.1f;
		public bool RepositionRigidbody = true;
		/// the layer mask to filter when to reposition rigidbody
		public LayerMask RepositionRigidbodyLayerMask; 

		protected float _smallestBoundsWidth; 
		protected float _adjustedSmallestBoundsWidth; 
		protected float _squaredBoundsWidth; 
		protected Vector3 _positionLastFrame; 
		protected Rigidbody _rigidbody;
		protected Collider _collider;
		protected Vector3 _lastMovement;
		protected float _lastMovementSquared;

		protected virtual void OnValidate()
		{
			// force initialized RepositionRigidbodyLayerMask, same behavior as before
			if (RepositionRigidbody)
			{
				if (RepositionRigidbodyLayerMask.value == default)
				{
					RepositionRigidbodyLayerMask = ObstaclesLayerMask;
				}
			}
		}

		/// <summary>
		/// On Start we initialize our object
		/// </summary>
		protected virtual void Start() 
		{ 
			Initialization ();
		} 

		/// <summary>
		/// Grabs the rigidbody and computes the bounds width
		/// </summary>
		protected virtual void Initialization()
		{
			_rigidbody = GetComponent<Rigidbody>();
			_positionLastFrame = _rigidbody.position; 

			_collider = GetComponent<Collider>();

			_smallestBoundsWidth = Mathf.Min(Mathf.Min(_collider.bounds.extents.x, _collider.bounds.extents.y), _collider.bounds.extents.z); 
			_adjustedSmallestBoundsWidth = _smallestBoundsWidth * (1.0f - SkinWidth); 
			_squaredBoundsWidth = _smallestBoundsWidth * _smallestBoundsWidth; 
		}

		/// <summary>
		/// On Enable, we initialize our last frame position
		/// </summary>
		protected virtual void OnEnable()
		{
			_positionLastFrame = this.transform.position;
		}

		/// <summary>
		/// On fixedUpdate, checks the last movement and if needed casts a ray to detect obstacles
		/// </summary>
		protected virtual void FixedUpdate() 
		{ 
			_lastMovement = this.transform.position - _positionLastFrame; 
			_lastMovementSquared = _lastMovement.sqrMagnitude;

			// if we've moved further than our bounds, we may have missed something
			if (_lastMovementSquared > _squaredBoundsWidth) 
			{ 
				float movementMagnitude = Mathf.Sqrt(_lastMovementSquared);

				// we cast a ray backwards to see if we should have hit something
				RaycastHit hitInfo; 
				if (Physics.Raycast(_positionLastFrame, _lastMovement, out hitInfo, movementMagnitude, ObstaclesLayerMask.value))
				{
					if (!hitInfo.collider)
					{
						return;
					}						

					if (hitInfo.collider.isTrigger) 
					{
						hitInfo.collider.SendMessage("OnTriggerEnter", _collider);
					}						

					if (!hitInfo.collider.isTrigger)
					{
						this.gameObject.SendMessage("PreventedCollision3D", hitInfo, SendMessageOptions.DontRequireReceiver);
						if (RepositionRigidbody)
						{
							var hitLayer = hitInfo.collider.gameObject.layer;
							if (0 != (1 << hitLayer & RepositionRigidbodyLayerMask))
							{
								this.transform.position = hitInfo.point - (_lastMovement / movementMagnitude) * _adjustedSmallestBoundsWidth;
								_rigidbody.position = hitInfo.point - (_lastMovement / movementMagnitude) * _adjustedSmallestBoundsWidth;
							}
						}						
					}
				}
			} 
			_positionLastFrame = this.transform.position; 
		}
	}
}