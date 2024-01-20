using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;
using System;

namespace MoreMountains.Tools
{
	[Serializable]
	[AddComponentMenu("More Mountains/Tools/Vision/MMConeOfVision2D")]
	public class MMConeOfVision2D : MonoBehaviour
	{
		public struct RaycastData
		{
			public bool Hit;
			public Vector3 Point;
			public float Distance;
			public float Angle;

			public RaycastData(bool hit, Vector3 point, float distance, float angle)
			{
				Hit = hit;
				Point = point;
				Distance = distance;
				Angle = angle;
			}
		}

		public struct MeshEdgePosition
		{
			public Vector3 PointA;
			public Vector3 PointB;

			public MeshEdgePosition(Vector3 pointA, Vector3 pointB)
			{
				PointA = pointA;
				PointB = pointB;
			}
		}

		[Header("Vision")]
		public LayerMask ObstacleMask;
		public float VisionRadius = 5f;
		[Range(0f, 360f)]
		public float VisionAngle = 20f;
		[Range(0f, 360f)]
		public float AngleOffset = 0f;
		[MMReadOnly]
		public Vector3 Direction;
		[MMReadOnly]
		public Vector3 EulerAngles;

		[Header("Target scanning")]
		public bool ShouldScanForTargets = true;
		public LayerMask TargetMask;
		public float ScanFrequencyInSeconds = 1f;
		[MMReadOnly]
		public List<Transform> VisibleTargets = new List<Transform>();

		[Header("Mesh")] 
		public bool ShouldDrawMesh = true;
		public float MeshDensity = 0.2f;
		public int EdgePrecision = 3;
		public float EdgeThreshold = 0.5f;

		public MeshFilter VisionMeshFilter;

		protected Mesh _visionMesh;
		protected Collider2D[] _targetsWithinDistance;
		protected Transform _target;
		protected Vector3 _directionToTarget;
		protected float _distanceToTarget;
		protected float _lastScanTimestamp;

		protected RaycastHit2D _scanForTargetsHit2D;
		protected List<Vector3> _viewPoints = new List<Vector3>();
		protected RaycastData _oldViewCast = new RaycastData();
		protected RaycastData _viewCast = new RaycastData();
        
		protected Vector3[] _vertices;
		protected int[] _triangles;
		protected Vector3 _minPoint, _maxPoint, _direction;
		protected RaycastData _returnRaycastData;
		protected RaycastHit2D _raycastAtAngleHit2D;
		protected int _numberOfVerticesLastTime = 0;

		protected virtual void Awake()
		{
			_visionMesh = new Mesh();
			Direction = Vector3.right;
			if (ShouldDrawMesh)
			{
				VisionMeshFilter.mesh = _visionMesh;  
			}
		}

		protected virtual void LateUpdate()
		{
			if ((Time.time - _lastScanTimestamp > ScanFrequencyInSeconds) && ShouldScanForTargets)
			{
				ScanForTargets();
			}
			DrawMesh();
		}

		public virtual void SetDirectionAndAngles(Vector3 direction, Vector3 eulerAngles)
		{
			Direction = direction;
			EulerAngles = eulerAngles;
			EulerAngles.y += AngleOffset;
		}
        
		protected virtual void ScanForTargets()
		{
			_lastScanTimestamp = Time.time;
			VisibleTargets.Clear();
			_targetsWithinDistance = Physics2D.OverlapCircleAll(this.transform.position, VisionRadius, TargetMask);
			foreach (Collider2D collider in _targetsWithinDistance)
			{
				_target = collider.transform;
				_directionToTarget = (_target.position - this.transform.position).normalized;
				if (Vector3.Angle(Direction, _directionToTarget) < VisionAngle / 2f)
				{
					_distanceToTarget = Vector3.Distance(this.transform.position, _target.position);

					_scanForTargetsHit2D = Physics2D.Raycast(this.transform.position, _directionToTarget, _distanceToTarget, ObstacleMask);
					if (!_scanForTargetsHit2D)
					{
						VisibleTargets.Add(_target);
					}
				}
			}
		}
        
		protected virtual void DrawMesh()
		{
			if (!ShouldDrawMesh)
			{
				return;
			}
            
			int steps = Mathf.RoundToInt(MeshDensity * VisionAngle);
			float stepsAngle = VisionAngle / steps;

			_viewPoints.Clear();

			for (int i = 0; i <= steps; i++)
			{
				float angle = stepsAngle * i + EulerAngles.y - VisionAngle / 2f;
				_viewCast = RaycastAtAngle(angle);

				if (i > 0)
				{
					bool thresholdExceeded = Mathf.Abs(_oldViewCast.Distance - _viewCast.Distance) > EdgeThreshold;

					if ((_oldViewCast.Hit != _viewCast.Hit) || (_oldViewCast.Hit && _viewCast.Hit && thresholdExceeded))
					{
						MeshEdgePosition edge = FindMeshEdgePosition(_oldViewCast, _viewCast);
						if (edge.PointA != Vector3.zero)
						{
							_viewPoints.Add(edge.PointA);
						}
						if (edge.PointB != Vector3.zero)
						{
							_viewPoints.Add(edge.PointB);
						}
					}
				}

				_viewPoints.Add(_viewCast.Point);
				_oldViewCast = _viewCast;
			}

			int numberOfVertices = _viewPoints.Count + 1;
			if (numberOfVertices != _numberOfVerticesLastTime)
			{
				Array.Resize(ref _vertices, numberOfVertices);
				Array.Resize(ref _triangles, (numberOfVertices - 2) * 3);
			}

			_vertices[0].x = 0;
			_vertices[0].y = 0;
			_vertices[0].z = 0;
            
			for (int i = 0; i < numberOfVertices - 1; i++)
			{
				_vertices[i + 1] = this.transform.InverseTransformPoint(_viewPoints[i]);

				if (i < numberOfVertices - 2)
				{
					_triangles[i * 3] = 0;
					_triangles[i * 3 + 1] = i + 1;
					_triangles[i * 3 + 2] = i + 2;
				}
			}

			_visionMesh.Clear();
			_visionMesh.vertices = _vertices;
			_visionMesh.triangles = _triangles;
			_visionMesh.RecalculateNormals();
            
			_numberOfVerticesLastTime = numberOfVertices;
		}

		MeshEdgePosition FindMeshEdgePosition(RaycastData minimumViewCast, RaycastData maximumViewCast)
		{
			float minAngle = minimumViewCast.Angle;
			float maxAngle = maximumViewCast.Angle;
			_minPoint = minimumViewCast.Point;
			_maxPoint = maximumViewCast.Point;

			for (int i = 0; i < EdgePrecision; i++)
			{
				float angle = (minAngle + maxAngle) / 2;
				RaycastData newViewCast = RaycastAtAngle(angle);

				bool thresholdExceeded = Mathf.Abs(minimumViewCast.Distance - newViewCast.Distance) > EdgeThreshold;
				if (newViewCast.Hit == minimumViewCast.Hit && !thresholdExceeded)
				{
					minAngle = angle;
					_minPoint = newViewCast.Point;
				}
				else
				{
					maxAngle = angle;
					_maxPoint = newViewCast.Point;
				}
			}

			return new MeshEdgePosition(_minPoint, _maxPoint);
		}

		RaycastData RaycastAtAngle(float angle)
		{
			_direction = MMMaths.DirectionFromAngle2D(angle, 0f);

			_raycastAtAngleHit2D = Physics2D.Raycast(this.transform.position, _direction, VisionRadius, ObstacleMask);

			if (_raycastAtAngleHit2D)
			{
				_returnRaycastData.Hit = true;
				_returnRaycastData.Point = _raycastAtAngleHit2D.point;
				_returnRaycastData.Distance = _raycastAtAngleHit2D.distance;
				_returnRaycastData.Angle = angle;
			}
			else
			{
				_returnRaycastData.Hit = false;
				_returnRaycastData.Point = this.transform.position + _direction * VisionRadius;
				_returnRaycastData.Distance = VisionRadius;
				_returnRaycastData.Angle = angle;
			}

			return _returnRaycastData;
		}
	}
}