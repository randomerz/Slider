using UnityEngine;

namespace MoreMountains.Tools
{
	/// <summary>
	/// Add this component to an object and it'll let you display a gizmo for its position or collider, and an optional text
	/// </summary>
	public class MMGizmo : MonoBehaviour 
	{
		/// <summary>
		/// the possible types of gizmos to display
		/// </summary>
		public enum GizmoTypes { None, Collider, Position }
		/// <summary>
		/// whether to display gizmos always or only when the object is selected
		/// </summary>
		public enum DisplayModes { Always, OnlyWhenSelected }

		/// <summary>
		/// the shape of the gizmo to display the position of the object
		/// </summary>
		public enum PositionModes
		{
			Point, Cube, WireCube, Sphere, WireSphere, Texture, Arrows, RightArrow, UpArrow, ForwardArrow,
			Lines, RightLine, UpLine, ForwardLine
		}
		/// <summary>
		/// what to display as text for that gizmo
		/// </summary>
		public enum TextModes { GameObjectName, CustomText, Position, Rotation, Scale, Property }
		/// <summary>
		/// when displaying a collider, whether to display a full or wire gizmo
		/// </summary>
		public enum ColliderRenderTypes { Full, Wire }

		[Header("Modes")] 
		/// if this is true, gizmos will be displayed, if this is false, gizmos won't be displayed
		[Tooltip("if this is true, gizmos will be displayed, if this is false, gizmos won't be displayed")]
		public bool DisplayGizmo = true; 
		/// what the gizmos should represent. Collider will show the bounds of the associated collider, Position will show the position of the object 
		[Tooltip("what the gizmos should represent. Collider will show the bounds of the associated collider, Position will show the position of the object")]
		public GizmoTypes GizmoType = GizmoTypes.Position; 
		/// whether gizmos should always be displayed, or only when selected
		[Tooltip("whether gizmos should always be displayed, or only when selected")]
		public DisplayModes DisplayMode = DisplayModes.Always;
		
		[Header("Settings")] 
		/// the color of the collider or position gizmo 
		[Tooltip("the color of the collider or position gizmo")]
		public Color GizmoColor = MMColors.ReunoYellow; 
		/// the shape of the gizmo when in position mode
		[Tooltip("the shape of the gizmo when in position mode")]
		[MMEnumCondition("GizmoType", (int)GizmoTypes.Position)]
		public PositionModes PositionMode = PositionModes.Point; 
		/// the texture to display as a gizmo when in position & texture mode
		[Tooltip("the texture to display as a gizmo when in position & texture mode")]
		[MMEnumCondition("PositionMode", (int)PositionModes.Texture)]
		public Texture PositionTexture; 
		/// the size of the texture to display as a gizmo
		[Tooltip("the size of the texture to display as a gizmo")]
		[MMEnumCondition("PositionMode", (int)PositionModes.Texture)]
		public Vector2 TextureSize = new Vector2(50f,50f); 
		/// the size of the gizmo when in position mode
		[Tooltip("the size of the gizmo when in position mode")]
		[MMEnumCondition("GizmoType", (int)GizmoTypes.Position)]
		public float PositionSize = 0.2f; 
		/// whether to display the collider gizmo as a wire or a full mesh
		[Tooltip("whether to display the collider gizmo as a wire or a full mesh")]
		[MMEnumCondition("GizmoType", (int)GizmoTypes.Collider)]
		public ColliderRenderTypes ColliderRenderType = ColliderRenderTypes.Full;
		/// the distance from the scene view camera beyond which the gizmo won't be displayed
		[Tooltip("the distance from the scene view camera beyond which the gizmo won't be displayed")]
		public float ViewDistance = 20f; 
		
		[Header("Offsets")]
		/// an offset to apply when drawing a collider or position gizmo
		[Tooltip("an offset to apply when drawing a collider or position gizmo")]
		public Vector3 GizmoOffset = Vector3.zero;

		/// whether or not to lock the position of the gizmo on the x axis, regardless of the position of the object
		[Tooltip("whether or not to lock the position of the gizmo on the x axis, regardless of the position of the object")]
		public bool LockX = false;
		/// the position at which to put the gizmo when locked on the x axis
		[Tooltip("the position at which to put the gizmo when locked on the x axis")]
		[MMCondition("LockX", true)]
		public float LockedX = 0f;
		
		/// whether or not to lock the position of the gizmo on the y axis, regardless of the position of the object
		[Tooltip("whether or not to lock the position of the gizmo on the y axis, regardless of the position of the object")]
		public bool LockY = false;
		/// the position at which to put the gizmo when locked on the y axis
		[Tooltip("the position at which to put the gizmo when locked on the y axis")]
		[MMCondition("LockY", true)]
		public float LockedY = 0f;
		
		/// whether or not to lock the position of the gizmo on the z axis, regardless of the position of the object
		[Tooltip("whether or not to lock the position of the gizmo on the z axis, regardless of the position of the object")]
		public bool LockZ = false;
		/// the position at which to put the gizmo when locked on the z axis
		[Tooltip("the position at which to put the gizmo when locked on the z axis")]
		[MMCondition("LockZ", true)]
		public float LockedZ = 0f;

		[Header("Text")]  
		/// whether or not to display text on that gizmo
		[Tooltip("whether or not to display text on that gizmo")]
		public bool DisplayText = false; 
		/// what to display as text for that gizmo (some custom text, the object's name, position, rotation, scale, or a target property)
		[Tooltip("what to display as text for that gizmo (some custom text, the object's name, position, rotation, scale, or a target property)")]
		[MMCondition("DisplayText", true)]
		public TextModes TextMode; 
		/// when in CustomText mode, the text to display on that gizmo
		[Tooltip("when in CustomText mode, the text to display on that gizmo")]
		[MMEnumCondition("TextMode", (int)TextModes.CustomText)]
		public string TextToDisplay = "Some Text"; 
		/// the offset to apply to the text
		[Tooltip("the offset to apply to the text")]
		[MMCondition("DisplayText", true)]
		public Vector3 TextOffset = new Vector3(0f, 0.5f, 0f);
		/// what style to use for the text's font
		[Tooltip("what style to use for the text's font")]
		[MMCondition("DisplayText", true)]
		public FontStyle TextFontStyle = FontStyle.Normal; 
		/// the size of the text's font
		[Tooltip("the size of the text's font")]
		[MMCondition("DisplayText", true)]
		public int TextSize = 12; 
		/// the color in which to display the gizmo's text
		[Tooltip("the color in which to display the gizmo's text")]
		[MMCondition("DisplayText", true)]
		public Color TextColor = MMColors.ReunoYellow; 
		/// the color of the background behind the text
		[Tooltip("the color of the background behind the text")]
		[MMCondition("DisplayText", true)]
		public Color TextBackgroundColor = new Color(0,0,0,0.3f); 
		/// the padding to apply to the text's background
		[Tooltip("the padding to apply to the text's background")]
		[MMCondition("DisplayText", true)]
		public Vector4 TextPadding = new Vector4(5,0,5,0); 
		/// the distance from the scene view camera beyond which the gizmo text won't be displayed
		[Tooltip("the distance from the scene view camera beyond which the gizmo text won't be displayed")]
		[MMCondition("DisplayText", true)]
		public float TextMaxDistance = 14f;
		/// when in Property mode, the property whose value to display on the gizmo
		[Tooltip("when in Property mode, the property whose value to display on the gizmo")]
		public MMPropertyPicker TargetProperty;
		
		public bool Initialized { get; set; }
		public SphereCollider _sphereCollider { get; set; }
		public BoxCollider _boxCollider { get; set; }
		public MeshCollider _meshCollider { get; set; }
		public CircleCollider2D _circleCollider2D { get; set; }
		public BoxCollider2D _boxCollider2D { get; set; }
		public Vector3 _vector3Zero { get; set; }
		public Vector3 _newPosition { get; set; }
		public Vector2 _worldToGUIPosition { get; set; }
		public Rect _textureRect { get; set; }
		public GUIStyle _textGUIStyle { get; set; }
		public string _textToDisplay { get; set; }
		public bool _sphereColliderNotNull { get; set; }
		public bool _boxColliderNotNull { get; set; }
		public bool _meshColliderNotNull { get; set; }
		public bool _circleCollider2DNotNull { get; set; }
		public bool _boxCollider2DNotNull { get; set; }
		public bool _positionTextureNotNull { get; set; }
		
		#if UNITY_EDITOR
		
		/// <summary>
		/// On awake we initialize our property
		/// </summary>
		protected virtual void Awake()
		{
			TargetProperty.Initialization(this.gameObject);
		}
		
		#else 
		
		/// <summary>
		/// If we're not in editor, we disable ourselves
		/// </summary>
		protected virtual void Awake()
		{
			this.enabled = false;
		}
		
		#endif 
		
		
	}	
}