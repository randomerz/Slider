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
	[FeedbackPath("Animation/Animation Parameter")]
	public class MMF_Animation : MMF_Feedback 
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
        
		/// the possible modes that pilot triggers        
		public enum TriggerModes { SetTrigger, ResetTrigger }
        
		/// the possible ways to set a value
		public enum ValueModes { None, Constant, Random, Incremental }

		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.AnimationColor; } }
		public override bool EvaluateRequiresSetup() { return (BoundAnimator == null); }
		public override string RequiredTargetText { get { return BoundAnimator != null ? BoundAnimator.name : "";  } }
		public override string RequiresSetupText { get { return "This feedback requires that a BoundAnimator be set to be able to work properly. You can set one below."; } }
		#endif
		
		/// the duration of this feedback is the declared duration 
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(DeclaredDuration); } set { DeclaredDuration = value;  } }
		public override bool HasRandomness => true;
		public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => BoundAnimator = FindAutomatedTarget<Animator>();

		[MMFInspectorGroup("Animation", true, 12, true)]
		/// the animator whose parameters you want to update
		[Tooltip("the animator whose parameters you want to update")]
		public Animator BoundAnimator;
		/// the list of extra animators whose parameters you want to update
		[Tooltip("the list of extra animators whose parameters you want to update")]
		public List<Animator> ExtraBoundAnimators;
		/// the duration for the player to consider. This won't impact your animation, but is a way to communicate to the MMF Player the duration of this feedback. Usually you'll want it to match your actual animation, and setting it can be useful to have this feedback work with holding pauses.
		[Tooltip("the duration for the player to consider. This won't impact your animation, but is a way to communicate to the MMF Player the duration of this feedback. Usually you'll want it to match your actual animation, and setting it can be useful to have this feedback work with holding pauses.")]
		public float DeclaredDuration = 0f;
        
		[MMFInspectorGroup("Trigger", true, 16)]
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
        
		[MMFInspectorGroup("Random Trigger", true, 20)]
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
        
		[MMFInspectorGroup("Bool", true, 17)]
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
        
		[MMFInspectorGroup("Random Bool", true, 19)]
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
        
		[MMFInspectorGroup("Int", true, 24)]
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

		[MMFInspectorGroup("Float", true, 22)]
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
		protected override void CustomInitialization(MMF_Player owner)
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
				Debug.LogWarning("No animator was set for " + Owner.name);
				return;
			}

			float intensityMultiplier = ComputeIntensity(feedbacksIntensity, position);

			ApplyValue(BoundAnimator, intensityMultiplier);
			foreach (Animator animator in ExtraBoundAnimators)
			{
				ApplyValue(animator, intensityMultiplier);
			}
		}

		/// <summary>
		/// Applies values on the target Animator
		/// </summary>
		/// <param name="targetAnimator"></param>
		/// <param name="intensityMultiplier"></param>
		protected virtual void ApplyValue(Animator targetAnimator, float intensityMultiplier)
		{
			if (UpdateTrigger)
			{
				if (TriggerMode == TriggerModes.SetTrigger)
				{
					targetAnimator.SetTrigger(_triggerParameter);
				}
				if (TriggerMode == TriggerModes.ResetTrigger)
				{
					targetAnimator.ResetTrigger(_triggerParameter);
				}
			}
            
			if (UpdateRandomTrigger)
			{
				int randomParameter = _randomTriggerParameters[Random.Range(0, _randomTriggerParameters.Count)];
                
				if (RandomTriggerMode == TriggerModes.SetTrigger)
				{
					targetAnimator.SetTrigger(randomParameter);
				}
				if (RandomTriggerMode == TriggerModes.ResetTrigger)
				{
					targetAnimator.ResetTrigger(randomParameter);
				}
			}

			if (UpdateBool)
			{
				targetAnimator.SetBool(_boolParameter, BoolParameterValue);
			}

			if (UpdateRandomBool)
			{
				int randomParameter = _randomBoolParameters[Random.Range(0, _randomBoolParameters.Count)];
                
				targetAnimator.SetBool(randomParameter, RandomBoolParameterValue);
			}

			switch (IntValueMode)
			{
				case ValueModes.Constant:
					targetAnimator.SetInteger(_intParameter, IntValue);
					break;
				case ValueModes.Incremental:
					int newValue = targetAnimator.GetInteger(_intParameter) + IntIncrement;
					targetAnimator.SetInteger(_intParameter, newValue);
					break;
				case ValueModes.Random:
					int randomValue = Random.Range(IntValueMin, IntValueMax);
					targetAnimator.SetInteger(_intParameter, randomValue);
					break;
			}

			switch (FloatValueMode)
			{
				case ValueModes.Constant:
					targetAnimator.SetFloat(_floatParameter, FloatValue * intensityMultiplier);
					break;
				case ValueModes.Incremental:
					float newValue = targetAnimator.GetFloat(_floatParameter) + FloatIncrement * intensityMultiplier;
					targetAnimator.SetFloat(_floatParameter, newValue);
					break;
				case ValueModes.Random:
					float randomValue = Random.Range(FloatValueMin, FloatValueMax) * intensityMultiplier;
					targetAnimator.SetFloat(_floatParameter, randomValue);
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
			foreach (Animator animator in ExtraBoundAnimators)
			{
				animator.SetBool(_boolParameter, false);
			}
		}
	}
}