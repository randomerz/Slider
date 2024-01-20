using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
	public enum MMTweenDefinitionTypes { MMTween, AnimationCurve }

	[Serializable]
	public class MMTweenType
	{
		public static MMTweenType DefaultEaseInCubic { get; } = new MMTweenType(MMTween.MMTweenCurve.EaseInCubic);
		public MMTweenDefinitionTypes MMTweenDefinitionType = MMTweenDefinitionTypes.MMTween;
		public MMTween.MMTweenCurve MMTweenCurve = MMTween.MMTweenCurve.EaseInCubic;
		public AnimationCurve Curve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1f));
		public bool Initialized = false;

		public MMTweenType(MMTween.MMTweenCurve newCurve)
		{
			MMTweenCurve = newCurve;
			MMTweenDefinitionType = MMTweenDefinitionTypes.MMTween;
		}
		public MMTweenType(AnimationCurve newCurve)
		{
			Curve = newCurve;
			MMTweenDefinitionType = MMTweenDefinitionTypes.AnimationCurve;
		}

		public float Evaluate(float t)
		{
			return MMTween.Evaluate(t, this);
		}
	}
}