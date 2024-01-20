using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace MoreMountains.Tools
{
	/// <summary>
	/// a custom editor for the MMGizmo component 
	/// </summary>
	[CustomEditor(typeof(MMGizmo), true)]
	[CanEditMultipleObjects]
	public class MMGizmoEditor : Editor
	{
		/// <summary>
		/// Lets you press G when in scene view to toggle gizmos on or off
		/// </summary>
		[Shortcut("Toggle Gizmos", typeof(SceneView), KeyCode.G, displayName = "ToggleGizmos")]
		public static void ToggleGizmos()
		{
			SceneView.lastActiveSceneView.drawGizmos = !SceneView.lastActiveSceneView.drawGizmos;
		}
		
		/// <summary>
		/// When the target object is selected, we draw our gizmos
		/// </summary>
		/// <param name="mmGizmo"></param>
		/// <param name="gizmoType"></param>
		[DrawGizmo(GizmoType.Selected)]
		private static void DrawGizmoSelected(MMGizmo mmGizmo, GizmoType gizmoType)
		{
			if (!mmGizmo.DisplayGizmo)
			{
				return;
			}
			DrawGizmos(mmGizmo);
		}
		
		/// <summary>
		/// When the target object is not selected, we draw our gizmos if needed
		/// </summary>
		/// <param name="mmGizmo"></param>
		/// <param name="gizmoType"></param>
		[DrawGizmo(GizmoType.NonSelected)]
		private static void DrawGizmoNonSelected(MMGizmo mmGizmo, GizmoType gizmoType)
		{
			if (!mmGizmo.DisplayGizmo)
			{
				return;
			}
			if (mmGizmo.DisplayMode != MMGizmo.DisplayModes.Always)
			{
				return;
			}
			DrawGizmos(mmGizmo);
		}

		/// <summary>
		/// Draws gizmos and text
		/// </summary>
		/// <param name="mmGizmo"></param>
		private static void DrawGizmos(MMGizmo mmGizmo)
		{
			if (!mmGizmo.Initialized)
			{
				Initialization(mmGizmo);
			}
			
			if (TestDistance(mmGizmo, mmGizmo.ViewDistance))
			{
				Gizmos.color = mmGizmo.GizmoColor;
				Gizmos.matrix = mmGizmo.transform.localToWorldMatrix;
			
				switch (mmGizmo.GizmoType)
				{
					case MMGizmo.GizmoTypes.Collider:
						DrawColliderGizmo(mmGizmo);
						break;
					case MMGizmo.GizmoTypes.Position:
						DrawPositionGizmo(mmGizmo);
						break;
				}
			}
			DrawText(mmGizmo);
		}

		/// <summary>
		/// Tests whether or not gizmos should be drawn based on distance to the scene camera
		/// </summary>
		/// <param name="mmGizmo"></param>
		/// <param name="viewDistance"></param>
		/// <returns></returns>
		private static bool TestDistance(MMGizmo mmGizmo, float viewDistance)
		{
			float distanceToCamera = 0f;
			
			if (SceneView.currentDrawingSceneView == null)
			{
				distanceToCamera = Vector3.Distance(mmGizmo.transform.position, Camera.main.transform.position);
				return (distanceToCamera < viewDistance);
			}
			else
			{
				distanceToCamera = Vector3.Distance(mmGizmo.transform.position, SceneView.currentDrawingSceneView.camera.transform.position);
				return (distanceToCamera < viewDistance);	
			}
		}
		
		/// <summary>
		/// On Enable we initialize our gizmo
		/// </summary>
		protected virtual void OnEnable()
		{
			Initialization(target as MMGizmo);
		}

		/// <summary>
		/// On validate we initialize our gizmo
		/// </summary>
		protected void OnValidate()
		{
			Initialization(target as MMGizmo);
		}

		/// <summary>
		/// Initializes the gizmo, caching components, values, and inits the GUIStyle
		/// </summary>
		/// <param name="mmGizmo"></param>
		private static void Initialization(MMGizmo mmGizmo)
		{
			mmGizmo._sphereCollider = mmGizmo.gameObject.GetComponent<SphereCollider>();
			mmGizmo._boxCollider = mmGizmo.gameObject.GetComponent<BoxCollider>();
			mmGizmo._meshCollider = mmGizmo.gameObject.GetComponent<MeshCollider>();
			mmGizmo._circleCollider2D = mmGizmo.gameObject.GetComponent<CircleCollider2D>();
			mmGizmo._boxCollider2D = mmGizmo.gameObject.GetComponent<BoxCollider2D>();

			mmGizmo._sphereColliderNotNull = (mmGizmo._sphereCollider != null);
			mmGizmo._boxColliderNotNull = (mmGizmo._boxCollider != null);
			mmGizmo._meshColliderNotNull = (mmGizmo._meshCollider != null);
			mmGizmo._circleCollider2DNotNull = (mmGizmo._circleCollider2D != null);
			mmGizmo._boxCollider2DNotNull = (mmGizmo._boxCollider2D != null);

			mmGizmo._vector3Zero = Vector3.zero;
			mmGizmo._textureRect = new Rect(0f, 0f, mmGizmo.TextureSize.x, mmGizmo.TextureSize.y);
			mmGizmo._positionTextureNotNull = (mmGizmo.PositionTexture != null);

			mmGizmo._textGUIStyle = new GUIStyle();
			mmGizmo._textGUIStyle.normal.textColor = mmGizmo.TextColor;
			mmGizmo._textGUIStyle.fontSize = mmGizmo.TextSize;
			mmGizmo._textGUIStyle.fontStyle = mmGizmo.TextFontStyle;
			mmGizmo._textGUIStyle.padding = new RectOffset((int)mmGizmo.TextPadding.x, (int)mmGizmo.TextPadding.y, (int)mmGizmo.TextPadding.z, (int)mmGizmo.TextPadding.w);
			mmGizmo._textGUIStyle.normal.background = MMGUI.MakeTex(600, 100, mmGizmo.TextBackgroundColor);

			mmGizmo.Initialized = true;
		}

		/// <summary>
		/// Draws a gizmo for the associated collider
		/// </summary>
		/// <param name="mmGizmo"></param>
		private static void DrawColliderGizmo(MMGizmo mmGizmo)
		{
			if (mmGizmo._sphereColliderNotNull)
			{
				if (mmGizmo.ColliderRenderType == MMGizmo.ColliderRenderTypes.Full)
				{
					Gizmos.DrawSphere(ComputeGizmoPosition(mmGizmo, mmGizmo._sphereCollider.center), mmGizmo._sphereCollider.radius);	
				}
				else
				{
					Gizmos.DrawWireSphere(ComputeGizmoPosition(mmGizmo, mmGizmo._sphereCollider.center), mmGizmo._sphereCollider.radius);	
				}
			}

			if (mmGizmo._boxColliderNotNull)
			{
				if (mmGizmo.ColliderRenderType == MMGizmo.ColliderRenderTypes.Full)
				{
					Gizmos.DrawCube(ComputeGizmoPosition(mmGizmo, mmGizmo._boxCollider.center), mmGizmo._boxCollider.size);
				}
				else
				{
					Gizmos.DrawWireCube(ComputeGizmoPosition(mmGizmo, mmGizmo._boxCollider.center), mmGizmo._boxCollider.size);
				}
			}

			if (mmGizmo._circleCollider2DNotNull)
			{
				if (mmGizmo.ColliderRenderType == MMGizmo.ColliderRenderTypes.Full)
				{
					Gizmos.DrawSphere((Vector3)ComputeGizmoPosition(mmGizmo, mmGizmo._circleCollider2D.offset), mmGizmo._circleCollider2D.radius);
				}
				else
				{
					Gizmos.DrawWireSphere((Vector3)ComputeGizmoPosition(mmGizmo, mmGizmo._circleCollider2D.offset), mmGizmo._circleCollider2D.radius);
				}
			}

			if (mmGizmo._boxCollider2DNotNull)
			{
				Vector3 gizmoSize = new Vector3();
				gizmoSize.x =  mmGizmo._boxCollider2D.size.x ;
				gizmoSize.y =  mmGizmo._boxCollider2D.size.y ;
				gizmoSize.z = 0.1f;
				if (mmGizmo.ColliderRenderType == MMGizmo.ColliderRenderTypes.Full)
				{
					Gizmos.DrawCube(ComputeGizmoPosition(mmGizmo, mmGizmo._boxCollider2D.offset), gizmoSize);
				}
				else
				{
					Gizmos.DrawWireCube(ComputeGizmoPosition(mmGizmo, mmGizmo._boxCollider2D.offset), gizmoSize);
				}
			}

			if (mmGizmo._meshColliderNotNull)
			{
				if (mmGizmo.ColliderRenderType == MMGizmo.ColliderRenderTypes.Full)
				{
					Gizmos.DrawMesh(mmGizmo._meshCollider.sharedMesh);
				}
				else
				{
					Gizmos.DrawWireMesh(mmGizmo._meshCollider.sharedMesh);
				}
			}
		}

		/// <summary>
		/// Draws a position gizmo
		/// </summary>
		/// <param name="mmGizmo"></param>
		private static void DrawPositionGizmo(MMGizmo mmGizmo)
		{
			switch (mmGizmo.PositionMode)
			{
				case MMGizmo.PositionModes.Point:
					MMDebug.DrawGizmoPoint(ComputeGizmoPosition(mmGizmo, mmGizmo._vector3Zero), mmGizmo.GizmoColor, mmGizmo.PositionSize);
					break;
				case MMGizmo.PositionModes.Cube:
					Gizmos.DrawCube(ComputeGizmoPosition(mmGizmo, mmGizmo._vector3Zero), Vector3.one * mmGizmo.PositionSize);
					break;
				case MMGizmo.PositionModes.Sphere:
					Gizmos.DrawSphere(ComputeGizmoPosition(mmGizmo, mmGizmo._vector3Zero), mmGizmo.PositionSize);
					break;
				case MMGizmo.PositionModes.WireCube:
					Gizmos.DrawWireCube(ComputeGizmoPosition(mmGizmo, mmGizmo._vector3Zero), Vector3.one * mmGizmo.PositionSize);
					break;
				case MMGizmo.PositionModes.WireSphere:
					Gizmos.DrawWireSphere(ComputeGizmoPosition(mmGizmo, mmGizmo._vector3Zero), mmGizmo.PositionSize);
					break;
				case MMGizmo.PositionModes.Texture:
					if (mmGizmo._positionTextureNotNull)
					{
						Handles.BeginGUI();
						mmGizmo._worldToGUIPosition = HandleUtility.WorldToGUIPoint(ComputeGizmoPosition(mmGizmo, mmGizmo.transform.position, false));
						mmGizmo._textureRect = new Rect(mmGizmo._worldToGUIPosition.x - mmGizmo.TextureSize.x/2f, mmGizmo._worldToGUIPosition.y - mmGizmo.TextureSize.y/2f, mmGizmo.TextureSize.x, mmGizmo.TextureSize.y);
						GUI.Label(mmGizmo._textureRect, mmGizmo.PositionTexture);
						Handles.EndGUI();
					}
					break;
				case MMGizmo.PositionModes.Arrows:
					Handles.color = Handles.xAxisColor;
					Handles.ArrowHandleCap(0, ComputeGizmoPosition(mmGizmo, mmGizmo.transform.position, false),
						Quaternion.LookRotation(mmGizmo.transform.right, mmGizmo.transform.up), mmGizmo.PositionSize, EventType.Repaint);
					Handles.color = Handles.yAxisColor;
					Handles.ArrowHandleCap(0, ComputeGizmoPosition(mmGizmo, mmGizmo.transform.position, false),
						Quaternion.LookRotation(mmGizmo.transform.up, mmGizmo.transform.up), mmGizmo.PositionSize, EventType.Repaint);
					Handles.color = Handles.zAxisColor;
					Handles.ArrowHandleCap(0, ComputeGizmoPosition(mmGizmo, mmGizmo.transform.position, false),
						Quaternion.LookRotation(mmGizmo.transform.forward, mmGizmo.transform.up), mmGizmo.PositionSize, EventType.Repaint);
					break;
				case MMGizmo.PositionModes.RightArrow:
					Handles.color = mmGizmo.GizmoColor;
					Handles.ArrowHandleCap(0, ComputeGizmoPosition(mmGizmo, mmGizmo.transform.position, false),
						Quaternion.LookRotation(mmGizmo.transform.right, mmGizmo.transform.up), mmGizmo.PositionSize, EventType.Repaint);
					break;
				case MMGizmo.PositionModes.UpArrow:
					Handles.color = mmGizmo.GizmoColor;
					Handles.ArrowHandleCap(0, ComputeGizmoPosition(mmGizmo, mmGizmo.transform.position, false),
						Quaternion.LookRotation(mmGizmo.transform.up, mmGizmo.transform.up), mmGizmo.PositionSize, EventType.Repaint);
					break;
				case MMGizmo.PositionModes.ForwardArrow:
					Handles.color = mmGizmo.GizmoColor;
					Handles.ArrowHandleCap(0, ComputeGizmoPosition(mmGizmo, mmGizmo.transform.position, false),
						Quaternion.LookRotation(mmGizmo.transform.forward, mmGizmo.transform.up), mmGizmo.PositionSize, EventType.Repaint);
					break;
				case MMGizmo.PositionModes.Lines:
					Vector3 origin = ComputeGizmoPosition(mmGizmo, mmGizmo._vector3Zero);
					Vector3 destination = origin + Vector3.right * mmGizmo.PositionSize;
					Gizmos.DrawLine(origin, destination);
					destination = origin + Vector3.up * mmGizmo.PositionSize;
					Gizmos.DrawLine(origin, destination);
					destination = origin + Vector3.forward * mmGizmo.PositionSize;
					Gizmos.DrawLine(origin, destination);
					break;
				case MMGizmo.PositionModes.RightLine:
					Vector3 rightOrigin = ComputeGizmoPosition(mmGizmo, mmGizmo._vector3Zero);
					Vector3 rightDestination = rightOrigin + Vector3.right * mmGizmo.PositionSize;
					Gizmos.DrawLine(rightOrigin, rightDestination);
					break;
				case MMGizmo.PositionModes.UpLine:
					Vector3 upOrigin = ComputeGizmoPosition(mmGizmo, mmGizmo._vector3Zero);
					Vector3 upDestination = upOrigin + Vector3.up * mmGizmo.PositionSize;
					Gizmos.DrawLine(upOrigin, upDestination);
					break;
				case MMGizmo.PositionModes.ForwardLine:
					Vector3 fwdOrigin = ComputeGizmoPosition(mmGizmo, mmGizmo._vector3Zero);
					Vector3 fwdDestination = fwdOrigin + Vector3.forward * mmGizmo.PositionSize;
					Gizmos.DrawLine(fwdOrigin, fwdDestination);
					break;
			}
		}

		/// <summary>
		/// Draws our gizmo text
		/// </summary>
		/// <param name="mmGizmo"></param>
		private static void DrawText(MMGizmo mmGizmo)
		{
			if (!mmGizmo.DisplayText)
			{
				return;
			}
			
			if (!TestDistance(mmGizmo, mmGizmo.TextMaxDistance))
			{
				return;
			}

			switch (mmGizmo.TextMode)
			{
				case MMGizmo.TextModes.GameObjectName:
					mmGizmo._textToDisplay = mmGizmo.gameObject.name;
					break;
				case MMGizmo.TextModes.CustomText:
					mmGizmo._textToDisplay = mmGizmo.TextToDisplay;
					break;
				case MMGizmo.TextModes.Position:
					mmGizmo._textToDisplay = mmGizmo.transform.position.ToString();
					break;
				case MMGizmo.TextModes.Rotation:
					mmGizmo._textToDisplay = mmGizmo.transform.rotation.ToString();
					break;
				case MMGizmo.TextModes.Scale:
					mmGizmo._textToDisplay = mmGizmo.transform.localScale.ToString();
					break;
				case MMGizmo.TextModes.Property:
					if (mmGizmo.TargetProperty.PropertyFound)
					{
						mmGizmo._textToDisplay = mmGizmo.TargetProperty.GetRawValue().ToString();
					}
					break;
			}

			if (mmGizmo._textToDisplay != "")
			{
				Handles.Label(mmGizmo.transform.position + mmGizmo.TextOffset, mmGizmo._textToDisplay, mmGizmo._textGUIStyle);	
			}
		}

		/// <summary>
		/// Computes the position at which to draw the gizmo
		/// </summary>
		/// <param name="mmGizmo"></param>
		/// <param name="position"></param>
		/// <param name="relativeLock"></param>
		/// <returns></returns>
		private static Vector3 ComputeGizmoPosition(MMGizmo mmGizmo, Vector3 position, bool relativeLock = true)
		{
			mmGizmo._newPosition = position + mmGizmo.GizmoOffset;

			if (mmGizmo.LockX || mmGizmo.LockY || mmGizmo.LockZ)
			{
				Vector3 mmGizmoNewPosition = mmGizmo._newPosition;
				if (mmGizmo.LockX) { mmGizmoNewPosition.x = relativeLock ? - mmGizmo.transform.position.x + mmGizmo.LockedX : mmGizmo.LockedX; }
				if (mmGizmo.LockY) { mmGizmoNewPosition.y = relativeLock ? - mmGizmo.transform.position.y + mmGizmo.LockedY : mmGizmo.LockedY; }
				if (mmGizmo.LockZ) { mmGizmoNewPosition.z = relativeLock ? - mmGizmo.transform.position.z + mmGizmo.LockedZ : mmGizmo.LockedZ; } 
				mmGizmo._newPosition = mmGizmoNewPosition;
			}

			return mmGizmo._newPosition;
		}
		
	}	
}