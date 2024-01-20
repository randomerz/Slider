#if MM_CINEMACHINE
using Cinemachine;
#endif
using MoreMountains.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
	/// <summary>
	/// A class used to store gyro properties per camera
	/// </summary>
	[Serializable]
	[AddComponentMenu("More Mountains/Tools/Cinemachine/MMGyroCam")]
	public class MMGyroCam
	{
		#if MM_CINEMACHINE
		/// the bound cinemachine camera
		public CinemachineVirtualCamera Cam;
		#endif
		/// the transform this camera should look at
		public Transform LookAt;
		/// the transform this camera should rotate around
		public Transform RotationCenter;
		/// the minimum rotation to apply to this camera (in degrees)
		public Vector2 MinRotation = new Vector2(-2f, -2f);
		/// the maximum rotation to apply to this camera (in degrees)
		public Vector2 MaxRotation = new Vector2(2f, 2f);
		/// a transform to follow if the camera is animated
		public Transform AnimatedPosition;
        
		/// the camera's initial angles
		[MMReadOnly]
		public Vector3 InitialAngles;
		/// the camera's initial position
		[MMReadOnly]
		public Vector3 InitialPosition;
	}

	/// <summary>
	/// Add this class to a camera rig (an empty object), bind some Cinemachine virtual cameras to it, and they'll move around the specified object as your gyro powered device moves
	/// </summary>
	public class MMGyroParallax : MMGyroscope
	{
		[Header("Cameras")]
		/// the list of cameras to move as the gyro moves
		public List<MMGyroCam> Cams;
        
		protected Vector3 _newAngles;
        
		#if MM_CINEMACHINE

		/// <summary>
		/// On start we initialize our rig
		/// </summary>
		protected override void Start()
		{
			base.Start();
			Initialization();
		}

		/// <summary>
		/// Grabs the cameras and stores their position
		/// </summary>
		public virtual void Initialization()
		{
			foreach (MMGyroCam cam in Cams)
			{
				cam.InitialAngles = cam.Cam.transform.localEulerAngles;
				cam.InitialPosition = cam.Cam.transform.position;
			}
		}

		/// <summary>
		/// On Update we move our cameras
		/// </summary>
		protected override void Update()
		{
			base.Update();
			MoveCameras();
		}

		/// <summary>
		/// Moves cameras around based on gyro input
		/// </summary>
		protected virtual void MoveCameras()
		{
			foreach (MMGyroCam cam in Cams)
			{
				float newX = 0f;
				float newY = 0f;

				var gyroGravity = LerpedCalibratedGyroscopeGravity;
				if (gyroGravity.x > 0)
				{
					newX = MMMaths.Remap(LerpedCalibratedGyroscopeGravity.x, 0.5f, 0, cam.MinRotation.x, 0);
				}
				if (gyroGravity.x < 0)
				{
					newX = MMMaths.Remap(LerpedCalibratedGyroscopeGravity.x, 0, -.5f, 0, cam.MaxRotation.x);
				}
				if (gyroGravity.y > 0)
				{
					newY = MMMaths.Remap(LerpedCalibratedGyroscopeGravity.y, 0.5f, 0, cam.MinRotation.y, 0f);
				}
				if (gyroGravity.y < 0)
				{
					newY = MMMaths.Remap(LerpedCalibratedGyroscopeGravity.y, 0f, -0.5f, 0f, cam.MaxRotation.y);
				}

				var camTransform = cam.Cam.transform;

				if (cam.AnimatedPosition != null)
				{
					_newAngles = cam.AnimatedPosition.localEulerAngles;
					_newAngles.x += newX;
					_newAngles.z += newY;

					camTransform.position = cam.AnimatedPosition.position;
					camTransform.localEulerAngles = cam.AnimatedPosition.localEulerAngles;
				}
				else
				{
					_newAngles = cam.InitialAngles;
					_newAngles.x += newX;
					_newAngles.z += newY;
                    
					camTransform.position = cam.InitialPosition;
					camTransform.localEulerAngles = cam.InitialAngles;
				}

				var rotationTransform = cam.RotationCenter.transform;
				camTransform.RotateAround(rotationTransform.position, rotationTransform.up, newX);
				camTransform.RotateAround(rotationTransform.position, rotationTransform.right, newY);

				if (cam.Cam.LookAt == null) // cinemachine is not tracking a target
				{
					if (cam.LookAt != null) // local lookout target
					{
						camTransform.LookAt(cam.LookAt);
					}
					else
					{
						camTransform.LookAt(cam.RotationCenter);
					}
				}
			}
		}
		#endif
	}
}