using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
	/// <summary>
	/// This component lets you parent the transform you put it on to any target parent (or to the root if none is set), on Awake, Start or anytime you call its Parent() method
	/// </summary>
	public class MMParentingOnStart : MonoBehaviour
	{
		/// the possible modes this can run on
		public enum Modes { Awake, Start, Script }
		/// the selected mode
		public Modes Mode = Modes.Awake;
		/// the parent to parent to, leave empty if you want to unparent completely
		public Transform TargetParent;

		/// <summary>
		/// On Awake we parent if needed
		/// </summary>
		protected virtual void Awake()
		{
			if (Mode == Modes.Awake)
			{
				Parent();
			}
		}

		/// <summary>
		/// On Start we parent if needed
		/// </summary>
		protected virtual void Start()
		{
			if (Mode == Modes.Start)
			{
				Parent();
			}
		}

		/// <summary>
		/// Sets this transform's parent to the target
		/// </summary>
		public virtual void Parent()
		{
			this.transform.SetParent(TargetParent);
		}
	}
}