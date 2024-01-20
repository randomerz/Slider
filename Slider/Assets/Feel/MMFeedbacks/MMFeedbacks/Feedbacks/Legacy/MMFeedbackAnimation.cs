using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// A feedback used to trigger an animation (bool, int, float or trigger) on the associated animator, with or without randomness
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback will allow you to send to an animator (bound in its inspector) a bool, int, float or trigger parameter, allowing you to trigger an animation, with or without randomness.")]
	[FeedbackPath("GameObject/Animation")]
	public class MMFeedbackAnimation : MMFeedback 
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
        
		/// the possible modes that pilot triggers        
		public enum TriggerModes { SetTrigger, ResetTrigger }
        
		/// the possible ways to set a value
		public enum ValueModes { None, Constant, Random, Incremental }

		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.GameObjectColor; } }
		#endif

		[Header("Animation")]
		/// the animator whose parameters you want to update
		[Tooltip("the animator whose parameters you want to update")]
		public Animator BoundAnimator;
        
		[Header("Trigger")]
		/// if this is true, will update the specified trigger parameter
		[Tooltip("if this is true, will update the specified trigger parameter")]
		public bool UpdateTrigger = false;
		/// the selected mode to interact with this trigger
		[Tooltip("the selected mode to interact with this trigger")]
		[MMFCondition("UpdateTrigger", true)]
		public TriggerModes TriggerMode = TriggerModes.SetTrigger;
		/// the trigger animator parameter to, well, trigger when the feedback is played
		[Tooltip("the trigger animator parameter to, well, trigger when the feedback is played")]
		[MMFCondition("UpdateTrigger", true)]
		public string TriggerParameterName;
        
		[Header("Random Trigger")]
		/// if this is true, will update a random trigger parameter, picked from the list below
		[Tooltip("if this is true, will update a random trigger parameter, picked from the list below")]
		public bool UpdateRandomTrigger = false;
		/// the selected mode to interact with this trigger
		[Tooltip("the selected mode to interact with this trigger")]
		[MMFCondition("UpdateRandomTrigger", true)]
		public TriggerModes RandomTriggerMode = TriggerModes.SetTrigger;
		/// the trigger animator parameters to trigger at random when the feedback is played
		[Tooltip("the trigger animator parameters to trigger at random when the feedback is played")]
		public List<string> RandomTriggerParameterNames;
        
		[Header("Bool")]
		/// if this is true, will update the specified bool parameter
		[Tooltip("if this is true, will update the specified bool parameter")]
		public bool UpdateBool = false;
		/// the bool parameter to turn true when the feedback gets played
		[Tooltip("the bool parameter to turn true when the feedback gets played")]
		[MMFCondition("UpdateBool", true)]
		public string BoolParameterName;
		/// when in bool mode, whether to set the bool parameter to true or false
		[Tooltip("when in bool mode, whether to set the bool parameter to true or false")]
		[MMFCondition("UpdateBool", true)]
		public bool BoolParameterValue = true;
        
		[Header("Random Bool")]
		/// if this is true, will update a random bool parameter picked from the list below
		[Tooltip("if this is true, will update a random bool parameter picked from the list below")]
		public bool UpdateRandomBool = false;
		/// when in bool mode, whether to set the bool parameter to true or false
		[Tooltip("when in bool mode, whether to set the bool parameter to true or false")]
		[MMFCondition("UpdateRandomBool", true)]
		public bool RandomBoolParameterValue = true;
		/// the bool parameter to turn true when the feedback gets played
		[Tooltip("the bool parameter to turn true when the feedback gets played")]
		public List<string> RandomBoolParameterNames;
        
		[Header("Int")]
		/// the int parameter to turn true when the feedback gets played
		[Tooltip("the int parameter to turn true when the feedback gets played")]
		public ValueModes IntValueMode = ValueModes.None;
		/// the int parameter to turn true when the feedback gets played
		[Tooltip("the int parameter to turn true when the feedback gets played")]
		[MMFEnumCondition("IntValueMode", (int)ValueModes.Constant, (int)ValueModes.Random, (int)ValueModes.Incremental)]
		public string IntParameterName;
		/// the value to set to that int parameter
		[Tooltip("the value to set to that int parameter")]
		[MMFEnumCondition("IntValueMode", (int)ValueModes.Constant)]
		public int IntValue;
		/// the min value (inclusive) to set at random to that int parameter
		[Tooltip("the min value (inclusive) to set at random to that int parameter")]
		[MMFEnumCondition("IntValueMode", (int)ValueModes.Random)]
		public int IntValueMin;
		/// the max value (exclusive) to set at random to that int parameter
		[Tooltip("the max value (exclusive) to set at random to that int parameter")]
		[MMFEnumCondition("IntValueMode", (int)ValueModes.Random)]
		public int IntValueMax = 5;
		/// the value to increment that int parameter by
		[Tooltip("the value to increment that int parameter by")]
		[MMFEnumCondition("IntValueMode", (int)ValueModes.Incremental)]
		public int IntIncrement = 1;

		[Header("Float")]
		/// the Float parameter to turn true when the feedback gets played
		[Tooltip("the Float parameter to turn true when the feedback gets played")]
		public ValueModes FloatValueMode = ValueModes.None;
		/// the float parameter to turn true when the feedback gets played
		[Tooltip("the float parameter to turn true when the feedback gets played")]
		[MMFEnumCondition("FloatValueMode", (int)ValueModes.Constant, (int)ValueModes.Random, (int)ValueModes.Incremental)]
		public string FloatParameterName;
		/// the value to set to that float parameter
		[Tooltip("the value to set to that float parameter")]
		[MMFEnumCondition("FloatValueMode", (int)ValueModes.Constant)]
		public float FloatValue;
		/// the min value (inclusive) to set at random to that float parameter
		[Tooltip("the min value (inclusive) to set at random to that float parameter")]
		[MMFEnumCondition("FloatValueMode", (int)ValueModes.Random)]
		public float FloatValueMin;
		/// the max value (exclusive) to set at random to that float parameter
		[Tooltip("the max value (exclusive) to set at random to that float parameter")]
		[MMFEnumCondition("FloatValueMode", (int)ValueModes.Random)]
		public float FloatValueMax = 5;
		/// the value to increment that float parameter by
		[Tooltip("the value to increment that float parameter by")]
		[MMFEnumCondition("FloatValueMode", (int)ValueModes.Incremental)]
		public float FloatIncrement = 1;

		protected int _triggerParameter;
		protected int _boolParameter;
		protected int _intParameter;
		protected int _floatParameter;
		protected List<int> _randomTriggerParameters;
		protected List<int> _randomBoolParameters;

		/// <summary>
		/// Custom Init
		/// </summary>
		/// <param name="owner"></param>
		protected override void CustomInitialization(GameObject owner)
		{
			base.CustomInitialization(owner);
			_triggerParameter = Animator.StringToHash(TriggerParameterName);
			_boolParameter = Animator.StringToHash(BoolParameterName);
			_intParameter = Animator.StringToHash(IntParameterName);
			_floatParameter = Animator.StringToHash(FloatParameterName);

			_randomTriggerParameters = new List<int>();
			foreach (string name in RandomTriggerParameterNames)
			{
				_randomTriggerParameters.Add(Animator.StringToHash(name));
			}

			_randomBoolParameters = new List<int>();
			foreach (string name in RandomBoolParameterNames)
			{
				_randomBoolParameters.Add(Animator.StringToHash(name));
			}
		}

		/// <summary>
		/// On Play, checks if an animator is bound and triggers parameters
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			if (BoundAnimator == null)
			{
				Debug.LogWarning("No animator was set for " + this.name);
				return;
			}

			float intensityMultiplier = Timing.ConstantIntensity ? 1f : feedbacksIntensity;

			if (UpdateTrigger)
			{
				if (TriggerMode == TriggerModes.SetTrigger)
				{
					BoundAnimator.SetTrigger(_triggerParameter);
				}
				if (TriggerMode == TriggerModes.ResetTrigger)
				{
					BoundAnimator.ResetTrigger(_triggerParameter);
				}
			}
            
			if (UpdateRandomTrigger)
			{
				int randomParameter = _randomTriggerParameters[Random.Range(0, _randomTriggerParameters.Count)];
                
				if (RandomTriggerMode == TriggerModes.SetTrigger)
				{
					BoundAnimator.SetTrigger(randomParameter);
				}
				if (RandomTriggerMode == TriggerModes.ResetTrigger)
				{
					BoundAnimator.ResetTrigger(randomParameter);
				}
			}

			if (UpdateBool)
			{
				BoundAnimator.SetBool(_boolParameter, BoolParameterValue);
			}

			if (UpdateRandomBool)
			{
				int randomParameter = _randomBoolParameters[Random.Range(0, _randomBoolParameters.Count)];
                
				BoundAnimator.SetBool(randomParameter, RandomBoolParameterValue);
			}

			switch (IntValueMode)
			{
				case ValueModes.Constant:
					BoundAnimator.SetInteger(_intParameter, IntValue);
					break;
				case ValueModes.Incremental:
					int newValue = BoundAnimator.GetInteger(_intParameter) + IntIncrement;
					BoundAnimator.SetInteger(_intParameter, newValue);
					break;
				case ValueModes.Random:
					int randomValue = Random.Range(IntValueMin, IntValueMax);
					BoundAnimator.SetInteger(_intParameter, randomValue);
					break;
			}

			switch (FloatValueMode)
			{
				case ValueModes.Constant:
					BoundAnimator.SetFloat(_floatParameter, FloatValue * intensityMultiplier);
					break;
				case ValueModes.Incremental:
					float newValue = BoundAnimator.GetFloat(_floatParameter) + FloatIncrement * intensityMultiplier;
					BoundAnimator.SetFloat(_floatParameter, newValue);
					break;
				case ValueModes.Random:
					float randomValue = Random.Range(FloatValueMin, FloatValueMax) * intensityMultiplier;
					BoundAnimator.SetFloat(_floatParameter, randomValue);
					break;
			}
            
		}
        
		/// <summary>
		/// On stop, turns the bool parameter to false
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !UpdateBool || !FeedbackTypeAuthorized)
			{
				return;
			}
            
			BoundAnimator.SetBool(_boolParameter, false);
		}
	}
}