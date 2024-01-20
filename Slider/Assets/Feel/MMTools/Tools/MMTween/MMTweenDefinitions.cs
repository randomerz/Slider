using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
	public class MMTweenDefinitions
	{
		// Linear       ---------------------------------------------------------------------------------------------------------------------------

		public static float Linear_Tween(float t)
		{
			return t;
		}

		public static float LinearAnti_Tween(float t)
		{
			return 1 - t;
		}

		// Almost Identity 

		public static float AlmostIdentity(float t)
		{
			return t * t * (2.0f - t);
		}

		// Quadratic    ---------------------------------------------------------------------------------------------------------------------------

		public static float EaseIn_Quadratic(float t)
		{
			return t * t;
		}

		public static float EaseOut_Quadratic(float t)
		{
			return 1 - EaseIn_Quadratic(1 - t);
		}

		public static float EaseInOut_Quadratic(float t)
		{
			if (t < 0.5f)
			{
				return EaseIn_Quadratic(t * 2f) / 2f;
			}
			else
			{
				return 1 - EaseIn_Quadratic((1f - t) * 2f) / 2;
			}
		}

		// Cubic        ---------------------------------------------------------------------------------------------------------------------------

		public static float EaseIn_Cubic(float t)
		{
			return t * t * t;
		}

		public static float EaseOut_Cubic(float t)
		{
			return 1 - EaseIn_Cubic(1 - t);
		}

		public static float EaseInOut_Cubic(float t)
		{
			if (t < 0.5f)
			{
				return EaseIn_Cubic(t * 2f) / 2f;
			}
			else
			{
				return 1 - EaseIn_Cubic((1f - t) * 2f) / 2;
			}
		}

		// Quartic      ---------------------------------------------------------------------------------------------------------------------------

		public static float EaseIn_Quartic(float t)
		{
			return Mathf.Pow(t, 4f);
		}

		public static float EaseOut_Quartic(float t)
		{
			return 1 - EaseIn_Quartic(1 - t);
		}

		public static float EaseInOut_Quartic(float t)
		{
			if (t < 0.5f)
			{
				return EaseIn_Quartic(t * 2f) / 2f;
			}
			else
			{
				return 1 - EaseIn_Quartic((1f - t) * 2f) / 2;
			}
		}

		// Quintic      ---------------------------------------------------------------------------------------------------------------------------

		public static float EaseIn_Quintic(float t)
		{
			return Mathf.Pow(t, 5f);
		}

		public static float EaseOut_Quintic(float t)
		{
			return 1 - EaseIn_Quintic(1 - t);
		}

		public static float EaseInOut_Quintic(float t)
		{
			if (t < 0.5f)
			{
				return EaseIn_Quintic(t * 2f) / 2f;
			}
			else
			{
				return 1 - EaseIn_Quintic((1f - t) * 2f) / 2;
			}
		}

		// Bounce       ---------------------------------------------------------------------------------------------------------------------------

		public static float EaseIn_Bounce(float t)
		{
			float p = 0.3f;
			return Mathf.Pow(2, -10 * t) * Mathf.Sin((t - p / 4) * (2 * Mathf.PI) / p) + 1;
		}

		public static float EaseOut_Bounce(float t)
		{
			return 1 - EaseIn_Bounce(1 - t);
		}

		public static float EaseInOut_Bounce(float t)
		{
			if (t < 0.5f)
			{
				return EaseIn_Bounce(t * 2f) / 2f;
			}
			else
			{
				return 1 - EaseIn_Bounce((1f - t) * 2f) / 2;
			}
		}

		// Sinusoidal   ---------------------------------------------------------------------------------------------------------------------------

		public static float EaseIn_Sinusoidal(float t)
		{
			return 1 + Mathf.Sin(Mathf.PI / 2f * t - Mathf.PI / 2f);
		}

		public static float EaseOut_Sinusoidal(float t)
		{
			return 1 - EaseIn_Sinusoidal(1 - t);
		}

		public static float EaseInOut_Sinusoidal(float t)
		{
			if (t < 0.5f)
			{
				return EaseIn_Sinusoidal(t * 2f) / 2f;
			}
			else
			{
				return 1 - EaseIn_Sinusoidal((1f - t) * 2f) / 2;
			}
		}

		// Overhead     ---------------------------------------------------------------------------------------------------------------------------

		public static float EaseIn_Overhead(float t)
		{
			float back = 1.6f;
			return t * t * ((back + 1f) * t - back);
		}

		public static float EaseOut_Overhead(float t)
		{
			return 1 - EaseIn_Overhead(1 - t);
		}

		public static float EaseInOut_Overhead(float t)
		{
			if (t < 0.5f)
			{
				return EaseIn_Overhead(t * 2f) / 2f;
			}
			else
			{
				return 1 - EaseIn_Overhead((1f - t) * 2f) / 2;
			}
		}

		// Exponential  ---------------------------------------------------------------------------------------------------------------------------

		public static float EaseIn_Exponential(float t)
		{
			return t == 0f ? 0f : Mathf.Pow(1024f, t - 1f);
		}

		public static float EaseOut_Exponential(float t)
		{
			return 1 - EaseIn_Exponential(1 - t);
		}

		public static float EaseInOut_Exponential(float t)
		{
			if (t < 0.5f)
			{
				return EaseIn_Exponential(t * 2f) / 2f;
			}
			else
			{
				return 1 - EaseIn_Exponential((1f - t) * 2f) / 2;
			}
		}

		// Elastic      ---------------------------------------------------------------------------------------------------------------------------

		public static float EaseIn_Elastic(float t)
		{
			if (t == 0f) { return 0f; }
			if (t == 1f) { return 1f; }
			return -Mathf.Pow(2f, 10f * (t -= 1f)) * Mathf.Sin((t - 0.1f) * (2f * Mathf.PI) / 0.4f);
		}

		public static float EaseOut_Elastic(float t)
		{
			return 1 - EaseIn_Elastic(1 - t);
		}

		public static float EaseInOut_Elastic(float t)
		{
			if (t < 0.5f)
			{
				return EaseIn_Elastic(t * 2f) / 2f;
			}
			else
			{
				return 1 - EaseIn_Elastic((1f - t) * 2f) / 2;
			}
		}

		// Circular     ---------------------------------------------------------------------------------------------------------------------------

		public static float EaseIn_Circular(float t)
		{
			return 1f - Mathf.Sqrt(1f - t * t);
		}

		public static float EaseOut_Circular(float t)
		{
			return 1 - EaseIn_Circular(1 - t);
		}

		public static float EaseInOut_Circular(float t)
		{
			if (t < 0.5f)
			{
				return EaseIn_Circular(t * 2f) / 2f;
			}
			else
			{
				return 1 - EaseIn_Circular((1f - t) * 2f) / 2;
			}
		}

	}
}