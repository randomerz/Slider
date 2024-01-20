using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
	public static class MMCoroutine
	{
		/// <summary>
		/// Waits for the specified amount of frames
		/// use : yield return MMCoroutine.WaitFor(1);
		/// </summary>
		/// <param name="frameCount"></param>
		/// <returns></returns>
		public static IEnumerator WaitForFrames(int frameCount)
		{
			while (frameCount > 0)
			{
				frameCount--;
				yield return null;
			}
		}

		/// <summary>
		/// Waits for the specified amount of seconds (using regular time)
		/// use : yield return MMCoroutine.WaitFor(1f);
		/// </summary>
		/// <param name="seconds"></param>
		/// <returns></returns>
		public static IEnumerator WaitFor(float seconds)
		{
			for (float timer = 0f; timer < seconds; timer += Time.deltaTime)
			{
				yield return null;
			}
		}

		/// <summary>
		/// Waits for the specified amount of seconds (using unscaled time)
		/// use : yield return MMCoroutine.WaitForUnscaled(1f);
		/// </summary>
		/// <param name="seconds"></param>
		/// <returns></returns>
		public static IEnumerator WaitForUnscaled(float seconds)
		{
			for (float timer = 0f; timer < seconds; timer += Time.unscaledDeltaTime)
			{
				yield return null;
			}
		}
	}
}