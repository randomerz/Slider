using UnityEngine;
using System.Collections;

namespace MoreMountains.Tools
{
	/// <summary>
	/// Adds this class to particles to force their sorting layer
	/// </summary>
	[AddComponentMenu("More Mountains/Tools/Particles/MMVisibleParticle")]
	public class MMVisibleParticle : MonoBehaviour {

		/// <summary>
		/// Sets the particle system's renderer to the Visible Particles sorting layer
		/// </summary>
		protected virtual void Start () 
		{
			GetComponent<ParticleSystem>().GetComponent<Renderer>().sortingLayerName = "VisibleParticles";
		}		
	}
}