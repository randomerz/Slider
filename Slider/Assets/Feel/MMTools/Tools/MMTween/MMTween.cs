using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

namespace MoreMountains.Tools
{
	/// <summary>
	/// The formulas described here are (loosely) based on Robert Penner's easing equations http://robertpenner.com/easing/
	/// </summary>

	public class MMTween : MonoBehaviour
	{
		/// <summary>
		/// A list of all the possible curves you can tween a value along
		/// </summary>
		public enum MMTweenCurve
		{
			LinearTween,        
			EaseInQuadratic,    EaseOutQuadratic,   EaseInOutQuadratic,
			EaseInCubic,        EaseOutCubic,       EaseInOutCubic,
			EaseInQuartic,      EaseOutQuartic,     EaseInOutQuartic,
			EaseInQuintic,      EaseOutQuintic,     EaseInOutQuintic,
			EaseInSinusoidal,   EaseOutSinusoidal,  EaseInOutSinusoidal,
			EaseInBounce,       EaseOutBounce,      EaseInOutBounce,
			EaseInOverhead,     EaseOutOverhead,    EaseInOutOverhead,
			EaseInExponential,  EaseOutExponential, EaseInOutExponential,
			EaseInElastic,      EaseOutElastic,     EaseInOutElastic,
			EaseInCircular,     EaseOutCircular,    EaseInOutCircular,
			AntiLinearTween,    AlmostIdentity
		}
		
		public static TweenDelegate[] TweenDelegateArray = new TweenDelegate[]
		{
			LinearTween,        
			EaseInQuadratic,    EaseOutQuadratic,   EaseInOutQuadratic,
			EaseInCubic,        EaseOutCubic,       EaseInOutCubic,
			EaseInQuartic,      EaseOutQuartic,     EaseInOutQuartic,
			EaseInQuintic,      EaseOutQuintic,     EaseInOutQuintic,
			EaseInSinusoidal,   EaseOutSinusoidal,  EaseInOutSinusoidal,
			EaseInBounce,       EaseOutBounce,      EaseInOutBounce,
			EaseInOverhead,     EaseOutOverhead,    EaseInOutOverhead,
			EaseInExponential,  EaseOutExponential, EaseInOutExponential,
			EaseInElastic,      EaseOutElastic,     EaseInOutElastic,
			EaseInCircular,     EaseOutCircular,    EaseInOutCircular,
			AntiLinearTween,    AlmostIdentity
		};

		// Core methods ---------------------------------------------------------------------------------------------------------------

		/// <summary>
		/// Moves a value between a startValue and an endValue based on a currentTime, along the specified tween curve
		/// </summary>
		/// <param name="currentTime"></param>
		/// <param name="initialTime"></param>
		/// <param name="endTime"></param>
		/// <param name="startValue"></param>
		/// <param name="endValue"></param>
		/// <param name="curve"></param>
		/// <returns></returns>
		public static float Tween(float currentTime, float initialTime, float endTime, float startValue, float endValue, MMTweenCurve curve)
		{
			currentTime = MMMaths.Remap(currentTime, initialTime, endTime, 0f, 1f);
			currentTime = TweenDelegateArray[(int)curve](currentTime);
			return startValue + currentTime * (endValue - startValue);
		}

		public static float Evaluate(float t, MMTweenCurve curve)
		{
			return TweenDelegateArray[(int)curve](t);
		}

		public static float Evaluate(float t, MMTweenType tweenType)
		{
			if (tweenType.MMTweenDefinitionType == MMTweenDefinitionTypes.MMTween)
			{
				return Evaluate(t, tweenType.MMTweenCurve);
			}
			if (tweenType.MMTweenDefinitionType == MMTweenDefinitionTypes.AnimationCurve)
			{
				return tweenType.Curve.Evaluate(t);
			}
			return 0f;
		}

		public delegate float TweenDelegate(float currentTime);
		
		public static float LinearTween(float currentTime) { return MMTweenDefinitions.Linear_Tween(currentTime); }
		public static float AntiLinearTween(float currentTime) { return MMTweenDefinitions.LinearAnti_Tween(currentTime); }
		public static float EaseInQuadratic(float currentTime) { return MMTweenDefinitions.EaseIn_Quadratic(currentTime); }
		public static float EaseOutQuadratic(float currentTime) { return MMTweenDefinitions.EaseOut_Quadratic(currentTime); }
		public static float EaseInOutQuadratic(float currentTime) { return MMTweenDefinitions.EaseInOut_Quadratic(currentTime); }
		public static float EaseInCubic(float currentTime) { return MMTweenDefinitions.EaseIn_Cubic(currentTime); }
		public static float EaseOutCubic(float currentTime) { return MMTweenDefinitions.EaseOut_Cubic(currentTime); }
		public static float EaseInOutCubic(float currentTime) { return MMTweenDefinitions.EaseInOut_Cubic(currentTime); }
		public static float EaseInQuartic(float currentTime) { return MMTweenDefinitions.EaseIn_Quartic(currentTime); }
		public static float EaseOutQuartic(float currentTime) { return MMTweenDefinitions.EaseOut_Quartic(currentTime); }
		public static float EaseInOutQuartic(float currentTime) { return MMTweenDefinitions.EaseInOut_Quartic(currentTime); }
		public static float EaseInQuintic(float currentTime) { return MMTweenDefinitions.EaseIn_Quintic(currentTime); }
		public static float EaseOutQuintic(float currentTime) { return MMTweenDefinitions.EaseOut_Quintic(currentTime); }
		public static float EaseInOutQuintic(float currentTime) { return MMTweenDefinitions.EaseInOut_Quintic(currentTime); }
		public static float EaseInSinusoidal(float currentTime) { return MMTweenDefinitions.EaseIn_Sinusoidal(currentTime); }
		public static float EaseOutSinusoidal(float currentTime) { return MMTweenDefinitions.EaseOut_Sinusoidal(currentTime); }
		public static float EaseInOutSinusoidal(float currentTime) { return MMTweenDefinitions.EaseInOut_Sinusoidal(currentTime); }
		public static float EaseInBounce(float currentTime) { return MMTweenDefinitions.EaseIn_Bounce(currentTime); }
		public static float EaseOutBounce(float currentTime) { return MMTweenDefinitions.EaseOut_Bounce(currentTime); }
		public static float EaseInOutBounce(float currentTime) { return MMTweenDefinitions.EaseInOut_Bounce(currentTime); }
		public static float EaseInOverhead(float currentTime) { return MMTweenDefinitions.EaseIn_Overhead(currentTime); }
		public static float EaseOutOverhead(float currentTime) { return MMTweenDefinitions.EaseOut_Overhead(currentTime); }
		public static float EaseInOutOverhead(float currentTime) { return MMTweenDefinitions.EaseInOut_Overhead(currentTime); }
		public static float EaseInExponential(float currentTime) { return MMTweenDefinitions.EaseIn_Exponential(currentTime); }
		public static float EaseOutExponential(float currentTime) { return MMTweenDefinitions.EaseOut_Exponential(currentTime); }
		public static float EaseInOutExponential(float currentTime) { return MMTweenDefinitions.EaseInOut_Exponential(currentTime); }
		public static float EaseInElastic(float currentTime) { return MMTweenDefinitions.EaseIn_Elastic(currentTime); }
		public static float EaseOutElastic(float currentTime) { return MMTweenDefinitions.EaseOut_Elastic(currentTime); }
		public static float EaseInOutElastic(float currentTime) { return MMTweenDefinitions.EaseInOut_Elastic(currentTime); }
		public static float EaseInCircular(float currentTime) { return MMTweenDefinitions.EaseIn_Circular(currentTime); }
		public static float EaseOutCircular(float currentTime) { return MMTweenDefinitions.EaseOut_Circular(currentTime); }
		public static float EaseInOutCircular(float currentTime) { return MMTweenDefinitions.EaseInOut_Circular(currentTime); }
		public static float AlmostIdentity(float currentTime) { return MMTweenDefinitions.AlmostIdentity(currentTime); }

		/// <summary>
		/// To use :
		/// public MMTween.MMTweenCurve Tween = MMTween.MMTweenCurve.EaseInOutCubic;
		/// private MMTween.TweenDelegate _tween;
		///
		/// _tween = MMTween.GetTweenMethod(Tween);
		/// float t = _tween(someFloat);
		/// </summary>
		/// <param name="tween"></param>
		/// <returns></returns>
		public static TweenDelegate GetTweenMethod(MMTweenCurve tween)
		{
			switch (tween)
			{
				case MMTweenCurve.LinearTween: return LinearTween;
				case MMTweenCurve.AntiLinearTween: return AntiLinearTween;
				case MMTweenCurve.EaseInQuadratic: return EaseInQuadratic;
				case MMTweenCurve.EaseOutQuadratic: return EaseOutQuadratic;
				case MMTweenCurve.EaseInOutQuadratic: return EaseInOutQuadratic;
				case MMTweenCurve.EaseInCubic: return EaseInCubic;
				case MMTweenCurve.EaseOutCubic: return EaseOutCubic;
				case MMTweenCurve.EaseInOutCubic: return EaseInOutCubic;
				case MMTweenCurve.EaseInQuartic: return EaseInQuartic;
				case MMTweenCurve.EaseOutQuartic: return EaseOutQuartic;
				case MMTweenCurve.EaseInOutQuartic: return EaseInOutQuartic;
				case MMTweenCurve.EaseInQuintic: return EaseInQuintic;
				case MMTweenCurve.EaseOutQuintic: return EaseOutQuintic;
				case MMTweenCurve.EaseInOutQuintic: return EaseInOutQuintic;
				case MMTweenCurve.EaseInSinusoidal: return EaseInSinusoidal;
				case MMTweenCurve.EaseOutSinusoidal: return EaseOutSinusoidal;
				case MMTweenCurve.EaseInOutSinusoidal: return EaseInOutSinusoidal;
				case MMTweenCurve.EaseInBounce: return EaseInBounce;
				case MMTweenCurve.EaseOutBounce: return EaseOutBounce;
				case MMTweenCurve.EaseInOutBounce: return EaseInOutBounce;
				case MMTweenCurve.EaseInOverhead: return EaseInOverhead;
				case MMTweenCurve.EaseOutOverhead: return EaseOutOverhead;
				case MMTweenCurve.EaseInOutOverhead: return EaseInOutOverhead;
				case MMTweenCurve.EaseInExponential: return EaseInExponential;
				case MMTweenCurve.EaseOutExponential: return EaseOutExponential;
				case MMTweenCurve.EaseInOutExponential: return EaseInOutExponential;
				case MMTweenCurve.EaseInElastic: return EaseInElastic;
				case MMTweenCurve.EaseOutElastic: return EaseOutElastic;
				case MMTweenCurve.EaseInOutElastic: return EaseInOutElastic;
				case MMTweenCurve.EaseInCircular: return EaseInCircular;
				case MMTweenCurve.EaseOutCircular: return EaseOutCircular;
				case MMTweenCurve.EaseInOutCircular: return EaseInOutCircular;
				case MMTweenCurve.AlmostIdentity: return AlmostIdentity;
			}
			return LinearTween;
		}

		public static Vector2 Tween(float currentTime, float initialTime, float endTime, Vector2 startValue, Vector2 endValue, MMTweenCurve curve)
		{
			startValue.x = Tween(currentTime, initialTime, endTime, startValue.x, endValue.x, curve);
			startValue.y = Tween(currentTime, initialTime, endTime, startValue.y, endValue.y, curve);
			return startValue;
		}

		public static Vector3 Tween(float currentTime, float initialTime, float endTime, Vector3 startValue, Vector3 endValue, MMTweenCurve curve)
		{
			startValue.x = Tween(currentTime, initialTime, endTime, startValue.x, endValue.x, curve);
			startValue.y = Tween(currentTime, initialTime, endTime, startValue.y, endValue.y, curve);
			startValue.z = Tween(currentTime, initialTime, endTime, startValue.z, endValue.z, curve);
			return startValue;
		}

		public static Quaternion Tween(float currentTime, float initialTime, float endTime, Quaternion startValue, Quaternion endValue, MMTweenCurve curve)
		{
			float turningRate = Tween(currentTime, initialTime, endTime, 0f, 1f, curve);
			startValue = Quaternion.Slerp(startValue, endValue, turningRate);
			return startValue;
		}

		// Animation curve methods --------------------------------------------------------------------------------------------------------------

		public static float Tween(float currentTime, float initialTime, float endTime, float startValue, float endValue, AnimationCurve curve)
		{
			currentTime = MMMaths.Remap(currentTime, initialTime, endTime, 0f, 1f);
			currentTime = curve.Evaluate(currentTime);
			return startValue + currentTime * (endValue - startValue);
		}

		public static Vector2 Tween(float currentTime, float initialTime, float endTime, Vector2 startValue, Vector2 endValue, AnimationCurve curve)
		{
			startValue.x = Tween(currentTime, initialTime, endTime, startValue.x, endValue.x, curve);
			startValue.y = Tween(currentTime, initialTime, endTime, startValue.y, endValue.y, curve);
			return startValue;
		}

		public static Vector3 Tween(float currentTime, float initialTime, float endTime, Vector3 startValue, Vector3 endValue, AnimationCurve curve)
		{
			startValue.x = Tween(currentTime, initialTime, endTime, startValue.x, endValue.x, curve);
			startValue.y = Tween(currentTime, initialTime, endTime, startValue.y, endValue.y, curve);
			startValue.z = Tween(currentTime, initialTime, endTime, startValue.z, endValue.z, curve);
			return startValue;
		}

		public static Quaternion Tween(float currentTime, float initialTime, float endTime, Quaternion startValue, Quaternion endValue, AnimationCurve curve)
		{
			float turningRate = Tween(currentTime, initialTime, endTime, 0f, 1f, curve);
			startValue = Quaternion.Slerp(startValue, endValue, turningRate);
			return startValue;
		}

		// Tween type methods ------------------------------------------------------------------------------------------------------------------------

		public static float Tween(float currentTime, float initialTime, float endTime, float startValue, float endValue, MMTweenType tweenType)
		{
			if (tweenType.MMTweenDefinitionType == MMTweenDefinitionTypes.MMTween)
			{
				return Tween(currentTime, initialTime, endTime, startValue, endValue, tweenType.MMTweenCurve);
			}
			if (tweenType.MMTweenDefinitionType == MMTweenDefinitionTypes.AnimationCurve)
			{
				return Tween(currentTime, initialTime, endTime, startValue, endValue, tweenType.Curve);
			}
			return 0f;
		}
		public static Vector2 Tween(float currentTime, float initialTime, float endTime, Vector2 startValue, Vector2 endValue, MMTweenType tweenType)
		{
			if (tweenType.MMTweenDefinitionType == MMTweenDefinitionTypes.MMTween)
			{
				return Tween(currentTime, initialTime, endTime, startValue, endValue, tweenType.MMTweenCurve);
			}
			if (tweenType.MMTweenDefinitionType == MMTweenDefinitionTypes.AnimationCurve)
			{
				return Tween(currentTime, initialTime, endTime, startValue, endValue, tweenType.Curve);
			}
			return Vector2.zero;
		}
		public static Vector3 Tween(float currentTime, float initialTime, float endTime, Vector3 startValue, Vector3 endValue, MMTweenType tweenType)
		{
			if (tweenType.MMTweenDefinitionType == MMTweenDefinitionTypes.MMTween)
			{
				return Tween(currentTime, initialTime, endTime, startValue, endValue, tweenType.MMTweenCurve);
			}
			if (tweenType.MMTweenDefinitionType == MMTweenDefinitionTypes.AnimationCurve)
			{
				return Tween(currentTime, initialTime, endTime, startValue, endValue, tweenType.Curve);
			}
			return Vector3.zero;
		}
		public static Quaternion Tween(float currentTime, float initialTime, float endTime, Quaternion startValue, Quaternion endValue, MMTweenType tweenType)
		{
			if (tweenType.MMTweenDefinitionType == MMTweenDefinitionTypes.MMTween)
			{
				return Tween(currentTime, initialTime, endTime, startValue, endValue, tweenType.MMTweenCurve);
			}
			if (tweenType.MMTweenDefinitionType == MMTweenDefinitionTypes.AnimationCurve)
			{
				return Tween(currentTime, initialTime, endTime, startValue, endValue, tweenType.Curve);
			}
			return Quaternion.identity;
		}

		// MOVE METHODS ---------------------------------------------------------------------------------------------------------
		public static Coroutine MoveTransform(MonoBehaviour mono, Transform targetTransform, Vector3 origin, Vector3 destination, 
			WaitForSeconds delay, float delayDuration, float duration, MMTween.MMTweenCurve curve, bool ignoreTimescale = false)
		{
			return mono.StartCoroutine(MoveTransformCo(targetTransform, origin, destination, delay, delayDuration, duration, curve, ignoreTimescale));
		}

		public static Coroutine MoveRectTransform(MonoBehaviour mono, RectTransform targetTransform, Vector3 origin, Vector3 destination,
			WaitForSeconds delay, float delayDuration, float duration, MMTween.MMTweenCurve curve, bool ignoreTimescale = false)
		{
			return mono.StartCoroutine(MoveRectTransformCo(targetTransform, origin, destination, delay, delayDuration, duration, curve, ignoreTimescale));
		}

		public static Coroutine MoveTransform(MonoBehaviour mono, Transform targetTransform, Transform origin, Transform destination, WaitForSeconds delay, float delayDuration, float duration, 
			MMTween.MMTweenCurve curve, bool updatePosition = true, bool updateRotation = true, bool ignoreTimescale = false)
		{
			return mono.StartCoroutine(MoveTransformCo(targetTransform, origin, destination, delay, delayDuration, duration, curve, updatePosition, updateRotation, ignoreTimescale));
		}

		public static Coroutine RotateTransformAround(MonoBehaviour mono, Transform targetTransform, Transform center, Transform destination, float angle, WaitForSeconds delay, float delayDuration, 
			float duration, MMTween.MMTweenCurve curve, bool ignoreTimescale = false)
		{
			return mono.StartCoroutine(RotateTransformAroundCo(targetTransform, center, destination, angle, delay, delayDuration, duration, curve, ignoreTimescale));
		}

		protected static IEnumerator MoveRectTransformCo(RectTransform targetTransform, Vector3 origin, Vector3 destination, WaitForSeconds delay,
			float delayDuration, float duration, MMTween.MMTweenCurve curve, bool ignoreTimescale = false)
		{
			if (delayDuration > 0f)
			{
				yield return delay;
			}
			float timeLeft = duration;
			while (timeLeft > 0f)
			{
				targetTransform.localPosition = MMTween.Tween(duration - timeLeft, 0f, duration, origin, destination, curve);
				timeLeft -= ignoreTimescale ? Time.unscaledDeltaTime : Time.deltaTime;
				yield return null;
			}
			targetTransform.localPosition = destination;
		}

		protected static IEnumerator MoveTransformCo(Transform targetTransform, Vector3 origin, Vector3 destination, WaitForSeconds delay, 
			float delayDuration, float duration, MMTween.MMTweenCurve curve, bool ignoreTimescale = false)
		{
			if (delayDuration > 0f)
			{
				yield return delay;
			}
			float timeLeft = duration;
			while (timeLeft > 0f)
			{
				targetTransform.transform.position = MMTween.Tween(duration - timeLeft, 0f, duration, origin, destination, curve);
				timeLeft -= ignoreTimescale ? Time.unscaledDeltaTime : Time.deltaTime;
				yield return null;
			}
			targetTransform.transform.position = destination;
		}

		protected static IEnumerator MoveTransformCo(Transform targetTransform, Transform origin, Transform destination, WaitForSeconds delay, float delayDuration, float duration, 
			MMTween.MMTweenCurve curve, bool updatePosition = true, bool updateRotation = true, bool ignoreTimescale = false)
		{
			if (delayDuration > 0f)
			{
				yield return delay;
			}
			float timeLeft = duration;
			while (timeLeft > 0f)
			{
				if (updatePosition)
				{
					targetTransform.transform.position = MMTween.Tween(duration - timeLeft, 0f, duration, origin.position, destination.position, curve);
				}
				if (updateRotation)
				{
					targetTransform.transform.rotation = MMTween.Tween(duration - timeLeft, 0f, duration, origin.rotation, destination.rotation, curve);
				}
				timeLeft -= ignoreTimescale ? Time.unscaledDeltaTime : Time.deltaTime;
				yield return null;
			}
			if (updatePosition) { targetTransform.transform.position = destination.position; }
			if (updateRotation) { targetTransform.transform.localEulerAngles = destination.localEulerAngles; }
		}

		protected static IEnumerator RotateTransformAroundCo(Transform targetTransform, Transform center, Transform destination, float angle, WaitForSeconds delay, float delayDuration, float duration, 
			MMTween.MMTweenCurve curve, bool ignoreTimescale = false)
		{
			if (delayDuration > 0f)
			{
				yield return delay;
			}

			Vector3 initialRotationPosition = targetTransform.transform.position;
			Quaternion initialRotationRotation = targetTransform.transform.rotation;

			float rate = 1f / duration;

			float timeSpent = 0f;
			while (timeSpent < duration)
			{

				float newAngle = MMTween.Tween(timeSpent, 0f, duration, 0f, angle, curve);

				targetTransform.transform.position = initialRotationPosition;
				initialRotationRotation = targetTransform.transform.rotation;
				targetTransform.RotateAround(center.transform.position, center.transform.up, newAngle);
				targetTransform.transform.rotation = initialRotationRotation;

				timeSpent += ignoreTimescale ? Time.unscaledDeltaTime : Time.deltaTime;
				yield return null;
			}
			targetTransform.transform.position = destination.position;
		}
	}
}