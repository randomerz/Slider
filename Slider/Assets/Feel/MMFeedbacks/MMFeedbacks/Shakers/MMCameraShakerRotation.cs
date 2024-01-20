using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// A variant of the Camera Shaker that acts on the rotation instead of the position. Careful, can cause vertigo to some users.
	/// </summary>
	public class MMCameraShakerRotation : MMCameraShaker
	{
		/// <summary>
		/// Shakes the camera for Duration seconds, by the desired amplitude and frequency
		/// </summary>
		/// <param name="duration">Duration.</param>
		/// <param name="amplitude">Amplitude.</param>
		/// <param name="frequency">Frequency.</param>
		public override void ShakeCamera(float duration, float amplitude, float frequency, float amplitudeX, float amplitudeY, float amplitudeZ, bool useUnscaledTime)
		{
			if ((amplitudeX != 0f) || (amplitudeY != 0f) || (amplitudeZ != 0f))
			{
				_wiggle.RotationWiggleProperties.AmplitudeMin.x = -amplitudeX;
				_wiggle.RotationWiggleProperties.AmplitudeMin.y = -amplitudeY;
				_wiggle.RotationWiggleProperties.AmplitudeMin.z = -amplitudeZ;
                
				_wiggle.RotationWiggleProperties.AmplitudeMax.x = amplitudeX;
				_wiggle.RotationWiggleProperties.AmplitudeMax.y = amplitudeY;
				_wiggle.RotationWiggleProperties.AmplitudeMax.z = amplitudeZ;
			}
			else
			{
				_wiggle.RotationWiggleProperties.AmplitudeMin = Vector3.one * -amplitude;
				_wiggle.RotationWiggleProperties.AmplitudeMax = Vector3.one * amplitude;
			}
            
			_wiggle.RotationWiggleProperties.UseUnscaledTime = useUnscaledTime;
			_wiggle.RotationWiggleProperties.FrequencyMin = frequency;
			_wiggle.RotationWiggleProperties.FrequencyMax = frequency;
			_wiggle.RotationWiggleProperties.NoiseFrequencyMin = frequency * Vector3.one;
			_wiggle.RotationWiggleProperties.NoiseFrequencyMax = frequency * Vector3.one; 
			_wiggle.WiggleRotation(duration);
		}
	}    
}