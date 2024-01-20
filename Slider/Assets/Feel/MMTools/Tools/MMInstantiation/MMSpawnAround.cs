using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
	/// <summary>
	/// This class is used to describe spawn properties, to be used by the MMSpawnAround class.
	/// It's meant to be exposed and used by classes that are designed to spawn objects, typically loot systems 
	/// </summary>
	[System.Serializable]
	public class MMSpawnAroundProperties
	{
		/// the possible shapes objects can be spawned within
		public enum MMSpawnAroundShapes { Sphere, Cube }
		/// the shape within which objects should spawn
		[Header("Shape")] 
		[Tooltip("the shape within which objects should spawn")]
		public MMSpawnAroundShapes Shape = MMSpawnAroundShapes.Sphere;

		[Header("Position")]
		/// a Vector3 that specifies the normal to the plane you want to spawn objects on (if you want to spawn objects on the x/z plane, the normal to that plane would be the y axis (0,1,0)
		[Tooltip("a Vector3 that specifies the normal to the plane you want to spawn objects on (if you want to spawn objects on the x/z plane, the normal to that plane would be the y axis (0,1,0)")]
		public Vector3 NormalToSpawnPlane = Vector3.up;
		/// the minimum distance to the origin of the spawn at which objects can be spawned
		[Tooltip("the minimum distance to the origin of the spawn at which objects can be spawned")]
		[MMEnumCondition("Shape", (int)MMSpawnAroundShapes.Sphere)]
		public float MinimumSphereRadius = 1f;
		/// the maximum distance to the origin of the spawn at which objects can be spawned
		[Tooltip("the maximum distance to the origin of the spawn at which objects can be spawned")]
		[MMEnumCondition("Shape", (int)MMSpawnAroundShapes.Sphere)]
		public float MaximumSphereRadius = 2f;
		/// the minimum size of the cube's base
		[Tooltip("the minimum size of the cube's base")]
		[MMEnumCondition("Shape", (int)MMSpawnAroundShapes.Cube)]
		public Vector3 MinimumCubeBaseSize = Vector3.one;
		/// the maximum size of the cube's base
		[Tooltip("the maximum size of the cube's base")]
		[MMEnumCondition("Shape", (int)MMSpawnAroundShapes.Cube)]
		public Vector3 MaximumCubeBaseSize = new Vector3(2f, 2f, 2f);

		[Header("NormalAxisOffset")]
		/// the minimum offset to apply on the normal axis
		[Tooltip("the minimum offset to apply on the normal axis")]
		public float MinimumNormalAxisOffset = 0f;
		/// the maximum offset to apply on the normal axis
		[Tooltip("the maximum offset to apply on the normal axis")]
		public float MaximumNormalAxisOffset = 0f;

		[Header("NormalAxisOffsetCurve")]
		/// whether or not to use a curve to offset the object's spawn position along the spawn plane
		[Tooltip("whether or not to use a curve to offset the object's spawn position along the spawn plane")]
		public bool UseNormalAxisOffsetCurve = false;
		/// a curve used to define how distance to the origin should be altered (potentially above min/max distance)
		[Tooltip("a curve used to define how distance to the origin should be altered (potentially above min/max distance)")]
		[MMCondition("UseNormalAxisOffsetCurve",true)]
		public AnimationCurve NormalOffsetCurve = new AnimationCurve(new Keyframe(0, 1f), new Keyframe(1, 1f));
		/// the value to which the curve's zero should be remapped to
		[Tooltip("the value to which the curve's zero should be remapped to")]
		[MMCondition("UseNormalAxisOffsetCurve",true)]
		public float NormalOffsetCurveRemapZero = 0f;
		/// the value to which the curve's one should be remapped to
		[Tooltip("the value to which the curve's one should be remapped to")]
		[MMCondition("UseNormalAxisOffsetCurve",true)]
		public float NormalOffsetCurveRemapOne = 1f;
		/// whether or not to invert the curve (horizontally)
		[Tooltip("whether or not to invert the curve (horizontally)")]
		[MMCondition("UseNormalAxisOffsetCurve",true)]
		public bool InvertNormalOffsetCurve = false;

		[Header("Rotation")]
		/// the minimum random rotation to apply (in degrees)
		[Tooltip("the minimum random rotation to apply (in degrees)")]
		public Vector3 MinimumRotation = Vector3.zero;
		/// the maximum random rotation to apply (in degrees)
		[Tooltip("the maximum random rotation to apply (in degrees)")]
		public Vector3 MaximumRotation = Vector3.zero;

		[Header("Scale")]
		/// the minimum random scale to apply
		[Tooltip("the minimum random scale to apply")]
		public Vector3 MinimumScale = Vector3.one;
		/// the maximum random scale to apply
		[Tooltip("the maximum random scale to apply")]
		public Vector3 MaximumScale = Vector3.one;
	}
    
	/// <summary>
	/// This static class is a spawn helper, useful to randomize position, rotation and scale when you need to
	/// instantiate objects  
	/// </summary>
	public static class MMSpawnAround 
	{
		public static void ApplySpawnAroundProperties(GameObject instantiatedObj, MMSpawnAroundProperties props, Vector3 origin)
		{            
			// we randomize the position
			instantiatedObj.transform.position = SpawnAroundPosition(props, origin);
			// we randomize the rotation
			instantiatedObj.transform.rotation = SpawnAroundRotation(props);
			// we randomize the scale
			instantiatedObj.transform.localScale = SpawnAroundScale(props);
		}

		/// <summary>
		/// Returns the position at which the object should spawn
		/// </summary>
		/// <param name="props"></param>
		/// <param name="origin"></param>
		/// <returns></returns>
		public static Vector3 SpawnAroundPosition(MMSpawnAroundProperties props, Vector3 origin)
		{
			// we get the position of the object based on the defined plane and distance
			Vector3 newPosition;
			if (props.Shape == MMSpawnAroundProperties.MMSpawnAroundShapes.Sphere)
			{
				float distance = Random.Range(props.MinimumSphereRadius, props.MaximumSphereRadius);
				newPosition = Vector3.Cross(Random.insideUnitSphere, props.NormalToSpawnPlane);
				newPosition.Normalize();
				newPosition *= distance;
			}
			else
			{
				float randomX = Random.Range(props.MinimumCubeBaseSize.x, props.MaximumCubeBaseSize.x);
				newPosition.x = Random.Range(-randomX, randomX) / 2f;
				float randomY = Random.Range(props.MinimumCubeBaseSize.y, props.MaximumCubeBaseSize.y);
				newPosition.y = Random.Range(-randomY, randomY) / 2f;
				float randomZ = Random.Range(props.MinimumCubeBaseSize.z, props.MaximumCubeBaseSize.z);
				newPosition.z = Random.Range(-randomZ, randomZ) / 2f;
				newPosition = Vector3.Cross(newPosition, props.NormalToSpawnPlane); 
			}

			float randomOffset = Random.Range(props.MinimumNormalAxisOffset, props.MaximumNormalAxisOffset);
			// we correct the position based on the NormalOffsetCurve
			if (props.UseNormalAxisOffsetCurve)
			{
				float normalizedOffset = 0f;
				if (randomOffset != 0)
				{
					if (props.InvertNormalOffsetCurve)
					{
						normalizedOffset = MMMaths.Remap(randomOffset, props.MinimumNormalAxisOffset, props.MaximumNormalAxisOffset, 1f, 0f);
					}
					else
					{
						normalizedOffset = MMMaths.Remap(randomOffset, props.MinimumNormalAxisOffset, props.MaximumNormalAxisOffset, 0f, 1f);
					}
				}

				float offset = props.NormalOffsetCurve.Evaluate(normalizedOffset);
				offset = MMMaths.Remap(offset, 0f, 1f, props.NormalOffsetCurveRemapZero, props.NormalOffsetCurveRemapOne);

				newPosition *= offset;
			}
			// we apply the normal offset
			newPosition += props.NormalToSpawnPlane.normalized * randomOffset;

			// relative position
			newPosition += origin;

			return newPosition;
		}

		/// <summary>
		/// Returns the scale at which the object should spawn
		/// </summary>
		/// <param name="props"></param>
		/// <returns></returns>
		public static Vector3 SpawnAroundScale(MMSpawnAroundProperties props)
		{
			return MMMaths.RandomVector3(props.MinimumScale, props.MaximumScale);
		}

		/// <summary>
		/// Returns the rotation at which the object should spawn
		/// </summary>
		/// <param name="props"></param>
		/// <returns></returns>
		public static Quaternion SpawnAroundRotation(MMSpawnAroundProperties props)
		{
			return Quaternion.Euler(MMMaths.RandomVector3(props.MinimumRotation, props.MaximumRotation));
		}

		/// <summary>
		/// Draws gizmos to show the shape of the spawn area
		/// </summary>
		/// <param name="props"></param>
		/// <param name="origin"></param>
		/// <param name="quantity"></param>
		/// <param name="size"></param>
		public static void DrawGizmos(MMSpawnAroundProperties props, Vector3 origin, int quantity, float size, Color gizmosColor)
		{
			Gizmos.color = gizmosColor;
			for (int i = 0; i < quantity; i++)
			{
				Gizmos.DrawCube(SpawnAroundPosition(props, origin), SpawnAroundScale(props) * size);
			}
		}
	}
}