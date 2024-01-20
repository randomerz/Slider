using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MoreMountains.Tools
{
	/// <summary>
	/// A class used to generate signals, normalized values between 0 and 1
	/// You can then use these values from a MMRadioBroadcaster, or simply evaluate its value to use wherever you want, 
	/// like a supercharged animation curve. In that case, simply disable the component, and read from it using its Evaluate method
	/// </summary>
	public class MMRadioSignalGenerator : MMRadioSignal
	{
		/// whether or not to display an animated preview
		public bool AnimatedPreview = false;
		/// whether this signal should play in back & forth (mirroring the curve around a tipping point)
		public bool BackAndForth = false;
		/// the tipping point at which to mirror the curve (between 0 and 1)
		[MMCondition("BackAndForth", true)]
		public float BackAndForthMirrorPoint = 0.5f;
		/// the list of signals to assemble to create the final signal
		public MMRadioSignalGeneratorItemList SignalList;
		/// how to clamp the result value
		[MMVector("Min", "Max")]
		public Vector2 Clamps = new Vector2(0f, 1f);
		/// the amplitude of the signal
		[Range(0f, 1f)]
		public float Bias = 0.5f;

		/// <summary>
		/// On reset, we initialize our list
		/// </summary>
		void Reset()
		{
			SignalList = new MMRadioSignalGeneratorItemList(){
				new MMRadioSignalGeneratorItem()
			};
		}
        
		/// <summary>
		/// Returns the y value of the generated signal curve, at the x time value specified in parameters
		/// </summary>
		/// <param name="time"></param>
		/// <returns></returns>
		public virtual float Evaluate(float time)
		{
			float level = 1f;
			if (SignalList.Count <= 0)
			{
				return level;
			}

			time = ApplyBias(time, Bias);
            
			for (int i = 0; i < SignalList.Count; i++)
			{
				if (SignalList[i].Active)
				{
					float newLevel = MMSignal.GetValueNormalized(time,
						SignalList[i].SignalType, SignalList[i].Phase,
						SignalList[i].Amplitude, SignalList[i].Frequency, SignalList[i].Offset,
						SignalList[i].Invert, SignalList[i].Curve, SignalList[i].TweenCurve,
						true, Clamps.x, Clamps.y, BackAndForth, BackAndForthMirrorPoint);
                    
					level = (SignalList[i].Mode == MMRadioSignalGeneratorItem.GeneratorItemModes.Multiply) ? level * newLevel : level + newLevel;
                    
                    
				}
			}
			CurrentLevel *= GlobalMultiplier;
            
			CurrentLevel = Mathf.Clamp(CurrentLevel, Clamps.x, Clamps.y);
			return level;
		}

		/// <summary>
		/// On Shake, we shake our level if needed
		/// </summary>
		protected override void Shake()
		{
			base.Shake();

			if (!Playing)
			{
				return;
			}

			if (SignalMode == SignalModes.OneTime)
			{
				float elapsedTime = TimescaleTime - _shakeStartedTimestamp;
				CurrentLevel = Evaluate(MMMaths.Remap(elapsedTime, 0f, Duration, 0f, 1f));   
			}
			else
			{
				CurrentLevel = Evaluate(DriverTime);
			}
		}

		/// <summary>
		/// Once the shake is complete, we apply our final level
		/// </summary>
		protected override void ShakeComplete()
		{
			base.ShakeComplete();
			CurrentLevel = Evaluate(1f);
		}

		/// <summary>
		/// returns a custom value to display in the graph in inspector
		/// </summary>
		/// <param name="time"></param>
		/// <returns></returns>
		public override float GraphValue(float time)
		{
			time = ApplyBias(time, Bias);
			return Evaluate(time);
		}
	}

	/// <summary>
	/// A class used to store generator items and their properties
	/// </summary>
	[System.Serializable]
	public class MMRadioSignalGeneratorItem
	{
		/// whether this individual signal should be multiplied or added to the rest
		public enum GeneratorItemModes { Multiply, Additive }

		/// whether to take this signal into account in the generator or not
		public bool Active = true;
		/// the type of this signal
		public MMSignal.SignalType SignalType = MMSignal.SignalType.Sine;
		/// if the type is animation curve, the curve to consider
		[MMEnumCondition("SignalType", (int)MMSignal.SignalType.AnimationCurve)]
		public AnimationCurve Curve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
		/// if the type is MMTween, the tween to consider
		[MMEnumCondition("SignalType", (int)MMSignal.SignalType.MMTween)]
		public MMTween.MMTweenCurve TweenCurve = MMTween.MMTweenCurve.EaseInOutQuartic;
		/// the selected mode (multiply or additive)
		public GeneratorItemModes Mode = GeneratorItemModes.Multiply;
		/// the phase of the signal
		[Range(-1f, 1f)]
		public float Phase = 0f;
		/// the frequency of the signal
		[Range(0f, 10f)]
		public float Frequency = 5f;
		/// the amplitude of the signal
		[Range(0f, 1f)]
		public float Amplitude = 1f;
		/// the offset of the signal 
		[Range(-1f, 1f)]
		public float Offset = 0f;
		/// whether or not to vertically invert the signal
		public bool Invert = false;
	}

	/// <summary>
	/// A reorderable list type used to store generator items
	/// </summary>
	[System.Serializable]
	public class MMRadioSignalGeneratorItemList : MMReorderableArray<MMRadioSignalGeneratorItem>
	{
	}
}