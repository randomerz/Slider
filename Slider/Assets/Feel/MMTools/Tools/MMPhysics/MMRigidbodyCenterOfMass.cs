using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
	/// <summary>
	/// Add this class to a Rigidbody or Rigidbody2D to be able to edit its center of mass from the inspector directly
	/// </summary>
	public class MMRigidbodyCenterOfMass : MonoBehaviour
	{
		/// the possible modes this class can start on
		public enum AutomaticSetModes { Awake, Start, ScriptOnly }

		[Header("CenterOfMass")]
		/// the offset to apply to the center of mass
		public Vector3 CenterOfMassOffset;

		[Header("Automation")]
		/// whether to set the center of mass on awake, start, or via script only
		public AutomaticSetModes AutomaticSetMode = AutomaticSetModes.Awake;
		/// whether or not this component should auto destroy after a set
		public bool AutoDestroyComponentAfterSet = true;

		[Header("Test")]
		/// the size of the gizmo point to display at the center of mass
		public float GizmoPointSize = 0.05f;
		/// a button to test the set method
		[MMInspectorButton("SetCenterOfMass")]
		public bool SetCenterOfMassButton;

		protected Vector3 _gizmoCenter;
		protected Rigidbody _rigidbody;
		protected Rigidbody2D _rigidbody2D;

		/// <summary>
		/// On Awake we grab our components and set our center of mass if needed
		/// </summary>
		protected virtual void Awake()
		{
			Initialization();

			if (AutomaticSetMode == AutomaticSetModes.Awake)
			{
				SetCenterOfMass();
			}
		}

		/// <summary>
		/// On Start we set our center of mass if needed
		/// </summary>
		protected virtual void Start()
		{
			if (AutomaticSetMode == AutomaticSetModes.Start)
			{
				SetCenterOfMass();
			}
		}

		/// <summary>
		/// Grabs the rigidbody or rigidbody2D components
		/// </summary>
		protected virtual void Initialization()
		{
			_rigidbody = this.gameObject.MMGetComponentNoAlloc<Rigidbody>();
			_rigidbody2D = this.gameObject.MMGetComponentNoAlloc<Rigidbody2D>();
		}

		/// <summary>
		/// Sets the center of mass on the rigidbody or rigidbody2D
		/// </summary>
		public virtual void SetCenterOfMass()
		{
			if (_rigidbody != null)
			{
				_rigidbody.centerOfMass = CenterOfMassOffset;
			}

			if (_rigidbody2D != null)
			{
				_rigidbody2D.centerOfMass = CenterOfMassOffset;
			}

			if (AutoDestroyComponentAfterSet)
			{
				Destroy(this);
			}
		}

		/// <summary>
		/// On DrawGizmosSelected, we draw a yellow point at the position of our center of mass
		/// </summary>
		protected virtual void OnDrawGizmosSelected()
		{
			_gizmoCenter = this.transform.TransformPoint(CenterOfMassOffset);
			MMDebug.DrawGizmoPoint(_gizmoCenter, GizmoPointSize, Color.yellow);
		}
	}
}