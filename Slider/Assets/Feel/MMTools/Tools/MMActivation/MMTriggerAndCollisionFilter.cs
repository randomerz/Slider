using UnityEngine;

namespace MoreMountains.Tools
{
	[System.Flags]
	public enum TriggerAndCollisionMask
	{
		IgnoreAll = 0,
		OnTriggerEnter     = 1 << 0,
		OnTriggerStay      = 1 << 1,
		OnTriggerExit      = 1 << 2,
		OnCollisionEnter   = 1 << 3,
		OnCollisionStay    = 1 << 4,
		OnCollisionExit    = 1 << 5,
		OnTriggerEnter2D   = 1 << 6,
		OnTriggerStay2D    = 1 << 7,
		OnTriggerExit2D    = 1 << 8,
		OnCollisionEnter2D = 1 << 9,
		OnCollisionStay2D  = 1 << 10,
		OnCollisionExit2D  = 1 << 11,
        
		OnAnyTrigger3D = OnTriggerEnter | OnTriggerStay | OnTriggerExit,
		OnAnyCollision3D = OnCollisionEnter | OnCollisionStay | OnCollisionExit,
		OnAnyTrigger2D = OnTriggerEnter2D | OnTriggerStay2D | OnTriggerExit2D,
		OnAnyCollision2D = OnCollisionEnter2D | OnCollisionStay2D | OnCollisionExit2D,
        
		OnAnyTrigger = OnAnyTrigger3D | OnAnyTrigger2D,
		OnAnyCollision = OnAnyCollision3D | OnAnyCollision2D,
        
		All_3D = OnAnyTrigger3D | OnAnyCollision3D,
		All_2D = OnAnyTrigger2D | OnAnyCollision2D,
		All    = All_3D | All_2D,
	}
	public abstract class MMTriggerAndCollisionFilter : MonoBehaviour
	{
		public TriggerAndCollisionMask TriggerAndCollisionFilter = TriggerAndCollisionMask.All;

		// Tested to check if callback should be used or ignored
		protected virtual bool UseEvent(TriggerAndCollisionMask value) => 0 != (TriggerAndCollisionFilter & value);

		// Collision 2D ------------------------------------------------------------------------------------

		protected abstract void OnCollisionEnter2D_(Collision2D collision);
		void OnCollisionEnter2D (Collision2D collision)
		{
			if (UseEvent(TriggerAndCollisionMask.OnCollisionEnter2D))
			{
				OnCollisionEnter2D_(collision);
			}
		}

		protected abstract void OnCollisionExit2D_(Collision2D collision);
		void OnCollisionExit2D (Collision2D collision)
		{
			if (UseEvent(TriggerAndCollisionMask.OnCollisionExit2D))
			{
				OnCollisionExit2D_(collision);
			}
		}

		protected abstract void OnCollisionStay2D_(Collision2D collision);
		void OnCollisionStay2D (Collision2D collision)
		{
			if (UseEvent(TriggerAndCollisionMask.OnCollisionStay2D))
			{
				OnCollisionStay2D_(collision);
			}
		}

		// Trigger 2D ------------------------------------------------------------------------------------

		protected abstract void OnTriggerEnter2D_(Collider2D collider);
		void OnTriggerEnter2D (Collider2D collider)
		{
			if (UseEvent(TriggerAndCollisionMask.OnTriggerEnter2D))
			{
				OnTriggerEnter2D_(collider);
			}
		}

		protected abstract void OnTriggerExit2D_(Collider2D collider);
		void OnTriggerExit2D (Collider2D collider)
		{
			if (UseEvent(TriggerAndCollisionMask.OnTriggerExit2D))
			{
				OnTriggerExit2D_(collider);
			}
		}

		protected abstract void OnTriggerStay2D_ (Collider2D collider);
		void OnTriggerStay2D (Collider2D collider)
		{
			if (UseEvent(TriggerAndCollisionMask.OnTriggerStay2D))
			{
				OnTriggerStay2D_(collider);
			}
		}

		// Collision ------------------------------------------------------------------------------------

		protected abstract void OnCollisionEnter_ (Collision c);
		void OnCollisionEnter(Collision c)
		{
			if (UseEvent(TriggerAndCollisionMask.OnCollisionEnter))
			{
				OnCollisionEnter_(c);
			}
		}

		protected abstract void OnCollisionExit_ (Collision c);
		void OnCollisionExit(Collision c)
		{
			if (UseEvent(TriggerAndCollisionMask.OnCollisionExit))
			{
				OnCollisionExit_(c);
			}
		}

		protected abstract void OnCollisionStay_ (Collision c);
		void OnCollisionStay(Collision c)
		{
			if (UseEvent(TriggerAndCollisionMask.OnCollisionStay))
			{
				OnCollisionStay_(c);
			}
		}

		// Trigger  ------------------------------------------------------------------------------------

		protected abstract void OnTriggerEnter_(Collider collider);
		void OnTriggerEnter (Collider collider)
		{
			if (UseEvent(TriggerAndCollisionMask.OnTriggerEnter))
			{
				OnTriggerEnter_(collider);
			}
		}

		protected abstract void OnTriggerExit_(Collider collider);
		void OnTriggerExit (Collider collider)
		{
			if (UseEvent(TriggerAndCollisionMask.OnTriggerExit))
			{
				OnTriggerExit_(collider);
			}
		}

		protected abstract void OnTriggerStay_(Collider collider);
		void OnTriggerStay (Collider collider)
		{
			if (UseEvent(TriggerAndCollisionMask.OnTriggerStay))
			{
				OnTriggerStay_(collider);
			}
		}
	}
}