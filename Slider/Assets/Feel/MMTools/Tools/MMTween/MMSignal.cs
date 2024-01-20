using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
	/// <summary>
	/// This class lets you output the value corresponding to one of the basic signal types it contains. Useful to draw basic signal curves.
	/// </summary>
	public class MMSignal
	{
		public enum SignalType
		{
			Sine,
			Pulse,
			Sawtooth,
			Square,
			Triangle,
			DigitalNoise,
			WhiteNoise,
			PerlinNoise,
			ValueNoise,
			AnimationCurve,
			MMTween
		}
                
		/// <summary>
		/// Returns the corresponding value based on the selected SignalType for a given time value
		/// </summary>
		/// <param name="time"></param>
		/// <param name="signalType"></param>
		/// <param name="phase"></param>
		/// <param name="amplitude"></param>
		/// <param name="frequency"></param>
		/// <param name="offset"></param>
		/// <param name="Invert"></param>
		/// <returns></returns>
		public static float GetValue(float time, SignalType signalType, float phase, float amplitude, float frequency, float offset, bool Invert = false, AnimationCurve curve = null, MMTween.MMTweenCurve tweenCurve = MMTween.MMTweenCurve.LinearTween)
		{
			float value = 0f;
			float invert = Invert ? -1 : 1;
			float t = frequency * time + phase;

			switch (signalType)
			{ 
				case SignalType.Sine: 
					value = (float)Mathf.Sin(2f * Mathf.PI * t);
					break;
				case SignalType.Square:
					value = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * t));
					break;
				case SignalType.Triangle:
					value = 1f - 4f * (float)Mathf.Abs(Mathf.Round(t - 0.25f) - (t - 0.25f));
					break;
				case SignalType.Sawtooth:
					value = 2f * (t - (float)Mathf.Floor(t + 0.5f));
					break;
				case SignalType.Pulse: 
					value = (Mathf.Abs(Mathf.Sin(2 * Mathf.PI * t)) < 1.0 - 10E-3) ? (0) : (1);
					break;
				case SignalType.WhiteNoise:
					value = 2f * Random.Range(0,int.MaxValue) / int.MaxValue - 1f;
					break;
				case SignalType.DigitalNoise:
					value = Random.Range(0,2);
					break;
				case SignalType.PerlinNoise:
					value = Mathf.PerlinNoise(time * frequency, time * amplitude);
					break;
				case SignalType.ValueNoise:
					value = ValueNoise(time, frequency) * amplitude;
					break;
				case SignalType.AnimationCurve:
					if (curve == null) { return 0f; }
					t = (t != 1f) ? t - Mathf.Floor(t) : 1f;
					value = curve.Evaluate(t);
					break;
				case SignalType.MMTween:
					t = (t != 1f) ? t - Mathf.Floor(t) : 1f;
					value = MMTween.Tween(t, 0f, 1f, 0f, 1f, tweenCurve);
					break;
			}

			return (invert * amplitude * value + offset);
		}

		public static float GetValueNormalized(float time, SignalType signalType,
			float phase, float amplitude, float frequency, float offset, bool Invert = false,
			AnimationCurve curve = null, MMTween.MMTweenCurve tweenCurve = MMTween.MMTweenCurve.LinearTween,
			bool clamp = true, float clampMin = 0f, float clampMax = 1f, bool backAndForth = false, float backAndForthTippingPoint = 0.5f)
		{
			float value = 0f;
			float invert = Invert ? -1 : 1;

			if (backAndForth)
			{
				if (time < backAndForthTippingPoint)
				{
					time = MMMaths.Remap(time, 0f, backAndForthTippingPoint, 0f, 1f);
				}
				else if (time == backAndForthTippingPoint)
				{
					time = 1f;
				}
				else if (time > backAndForthTippingPoint)
				{
					time = MMMaths.Remap(time, backAndForthTippingPoint, 1f, 1f, 0f);
				}
			}

			float t = frequency * time + phase;
                        
			switch (signalType)
			{
				case SignalType.Sine:
					value = (float)Mathf.Sin(2f * Mathf.PI * t);
					value = MMMaths.Remap(value, -1f, 1f, 0f, 1f);
					break;
				case SignalType.Square:
					value = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * t));
					value = MMMaths.Remap(value, -1f, 1f, 0f, 1f);
					break;
				case SignalType.Triangle:
					value = 1f - 4f * (float)Mathf.Abs(Mathf.Round(t - 0.25f) - (t - 0.25f));
					value = MMMaths.Remap(value, -1f, 1f, 0f, 1f);
					break;
				case SignalType.Sawtooth:
					value = 2f * (t - (float)Mathf.Floor(t + 0.5f));
					value = MMMaths.Remap(value, -1f, 1f, 0f, 1f);
					break;
				case SignalType.Pulse:
					value = (Mathf.Abs(Mathf.Sin(2 * Mathf.PI * t)) < 1.0 - 10E-3) ? (0) : (1);
					break;
				case SignalType.WhiteNoise:
					value = 2f * Random.Range(0, int.MaxValue) / int.MaxValue - 1f;
					value = MMMaths.Remap(value, -1f, 1f, 0f, 1f);
					break;
				case SignalType.DigitalNoise:
					value = Random.Range(0, 2);
					break;
				case SignalType.PerlinNoise:
					value = Mathf.PerlinNoise(t, t * amplitude);
					break;
				case SignalType.ValueNoise:
					value = ValueNoise(time, frequency) * amplitude;
					break;
				case SignalType.AnimationCurve:
					if (curve == null) { return 0f; }
					t = (t != 1f) ? t - Mathf.Floor(t) : 1f;
					value = curve.Evaluate(t);
					break;
				case SignalType.MMTween:
					t = (t != 1f) ? t - Mathf.Floor(t) : 1f;
					value = MMTween.Tween(t, 0f, 1f, 0f, 1f, tweenCurve);
					break;
			}

			if (Invert)
			{
				value = MMMaths.Remap(value, 0f, 1f, 1f, 0f);
			}
            
			float returnValue =  amplitude * value + offset;
            
			// we clamp the value
			if (clamp)
			{
				returnValue = Mathf.Clamp(returnValue, clampMin, clampMax);
			}

			return returnValue;
		}

		private static int[] hash = {
			151,160,137, 91, 90, 15,131, 13,201, 95, 96, 53,194,233,  7,225,
			140, 36,103, 30, 69,142,  8, 99, 37,240, 21, 10, 23,190,  6,148,
			247,120,234, 75,  0, 26,197, 62, 94,252,219,203,117, 35, 11, 32,
			57,177, 33, 88,237,149, 56, 87,174, 20,125,136,171,168, 68,175,
			74,165, 71,134,139, 48, 27,166, 77,146,158,231, 83,111,229,122,
			60,211,133,230,220,105, 92, 41, 55, 46,245, 40,244,102,143, 54,
			65, 25, 63,161,  1,216, 80, 73,209, 76,132,187,208, 89, 18,169,
			200,196,135,130,116,188,159, 86,164,100,109,198,173,186,  3, 64,
			52,217,226,250,124,123,  5,202, 38,147,118,126,255, 82, 85,212,
			207,206, 59,227, 47, 16, 58, 17,182,189, 28, 42,223,183,170,213,
			119,248,152,  2, 44,154,163, 70,221,153,101,155,167, 43,172,  9,
			129, 22, 39,253, 19, 98,108,110, 79,113,224,232,178,185,112,104,
			218,246, 97,228,251, 34,242,193,238,210,144, 12,191,179,162,241,
			81, 51,145,235,249, 14,239,107, 49,192,214, 31,181,199,106,157,
			184, 84,204,176,115,121, 50, 45,127,  4,150,254,138,236,205, 93,
			222,114, 67, 29, 24, 72,243,141,128,195, 78, 66,215, 61,156,180
		};
		private const int hashMask = 255;

		protected static float ValueNoise(float time, float frequency)
		{

			time *= frequency;
			int i = Mathf.FloorToInt(time);
			i &= hashMask;
			return hash[i] * (1f / hashMask);
		}
	}      
}