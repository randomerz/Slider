using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
	/// the possible modes for the timescale
	public enum TimescaleModes { Scaled, Unscaled }

	/// <summary>
	/// A class collecting delay, cooldown and repeat values, to be used to define the behaviour of each MMFeedback
	/// </summary>
	[System.Serializable]
	public class MMFeedbackTiming
	{
		/// the possible ways this feedback can play based on the host MMFeedbacks' directions
		public enum MMFeedbacksDirectionConditions { Always, OnlyWhenForwards, OnlyWhenBackwards };
		/// the possible ways this feedback can play
		public enum PlayDirections { FollowMMFeedbacksDirection, OppositeMMFeedbacksDirection, AlwaysNormal, AlwaysRewind }

		[Header("Timescale")]
		/// whether we're working on scaled or unscaled time
		[Tooltip("whether we're working on scaled or unscaled time")]
		public TimescaleModes TimescaleMode = TimescaleModes.Scaled;
        
		[Header("Exceptions")]
		/// if this is true, holding pauses won't wait for this feedback to finish 
		[Tooltip("if this is true, holding pauses won't wait for this feedback to finish")]
		public bool ExcludeFromHoldingPauses = false;
		/// whether to count this feedback in the parent MMFeedbacks(Player) total duration or not
		[Tooltip("whether to count this feedback in the parent MMFeedbacks(Player) total duration or not")]
		public bool ContributeToTotalDuration = true;

		[Header("Delays")]
		/// the initial delay to apply before playing the delay (in seconds)
		[Tooltip("the initial delay to apply before playing the delay (in seconds)")]
		public float InitialDelay = 0f;
		/// the cooldown duration mandatory between two plays
		[Tooltip("the cooldown duration mandatory between two plays")]
		public float CooldownDuration = 0f;

		[Header("Stop")]
		/// if this is true, this feedback will interrupt itself when Stop is called on its parent MMFeedbacks, otherwise it'll keep running
		[Tooltip("if this is true, this feedback will interrupt itself when Stop is called on its parent MMFeedbacks, otherwise it'll keep running")]
		public bool InterruptsOnStop = true;

		[Header("Repeat")]
		/// the repeat mode, whether the feedback should be played once, multiple times, or forever
		[Tooltip("the repeat mode, whether the feedback should be played once, multiple times, or forever")]
		public int NumberOfRepeats = 0;
		/// if this is true, the feedback will be repeated forever
		[Tooltip("if this is true, the feedback will be repeated forever")]
		public bool RepeatForever = false;
		/// the delay (in seconds) between two firings of this feedback. This doesn't include the duration of the feedback. 
		[Tooltip("the delay (in seconds) between two firings of this feedback. This doesn't include the duration of the feedback.")]
		public float DelayBetweenRepeats = 1f;

		[Header("Play Direction")]
		/// this defines how this feedback should play when the host MMFeedbacks is played :
		/// - always (default) : this feedback will always play
		/// - OnlyWhenForwards : this feedback will only play if the host MMFeedbacks is played in the top to bottom direction (forwards)
		/// - OnlyWhenBackwards : this feedback will only play if the host MMFeedbacks is played in the bottom to top direction (backwards)
		[Tooltip("this defines how this feedback should play when the host MMFeedbacks is played :" +
		         "- always (default) : this feedback will always play" +
		         "- OnlyWhenForwards : this feedback will only play if the host MMFeedbacks is played in the top to bottom direction (forwards)" +
		         "- OnlyWhenBackwards : this feedback will only play if the host MMFeedbacks is played in the bottom to top direction (backwards)")]
		public MMFeedbacksDirectionConditions MMFeedbacksDirectionCondition = MMFeedbacksDirectionConditions.Always;
		/// this defines the way this feedback will play. It can play in its normal direction, or in rewind (a sound will play backwards, 
		/// an object normally scaling up will scale down, a curve will be evaluated from right to left, etc)
		/// - BasedOnMMFeedbacksDirection : will play normally when the host MMFeedbacks is played forwards, in rewind when it's played backwards
		/// - OppositeMMFeedbacksDirection : will play in rewind when the host MMFeedbacks is played forwards, and normally when played backwards
		/// - Always Normal : will always play normally, regardless of the direction of the host MMFeedbacks
		/// - Always Rewind : will always play in rewind, regardless of the direction of the host MMFeedbacks
		[Tooltip("this defines the way this feedback will play. It can play in its normal direction, or in rewind (a sound will play backwards," +
		         " an object normally scaling up will scale down, a curve will be evaluated from right to left, etc)" +
		         "- BasedOnMMFeedbacksDirection : will play normally when the host MMFeedbacks is played forwards, in rewind when it's played backwards" +
		         "- OppositeMMFeedbacksDirection : will play in rewind when the host MMFeedbacks is played forwards, and normally when played backwards" +
		         "- Always Normal : will always play normally, regardless of the direction of the host MMFeedbacks" +
		         "- Always Rewind : will always play in rewind, regardless of the direction of the host MMFeedbacks")]
		public PlayDirections PlayDirection = PlayDirections.FollowMMFeedbacksDirection;

		[Header("Intensity")]
		/// if this is true, intensity will be constant, even if the parent MMFeedbacks is played at a lower intensity
		[Tooltip("if this is true, intensity will be constant, even if the parent MMFeedbacks is played at a lower intensity")]
		public bool ConstantIntensity = false;
		/// if this is true, this feedback will only play if its intensity is higher or equal to IntensityIntervalMin and lower than IntensityIntervalMax
		[Tooltip("if this is true, this feedback will only play if its intensity is higher or equal to IntensityIntervalMin and lower than IntensityIntervalMax")]
		public bool UseIntensityInterval = false;
		/// the minimum intensity required for this feedback to play
		[Tooltip("the minimum intensity required for this feedback to play")]
		[MMFCondition("UseIntensityInterval", true)]
		public float IntensityIntervalMin = 0f;
		/// the maximum intensity required for this feedback to play
		[Tooltip("the maximum intensity required for this feedback to play")]
		[MMFCondition("UseIntensityInterval", true)]
		public float IntensityIntervalMax = 0f;

		[Header("Sequence")]
		/// A MMSequence to use to play these feedbacks on
		[Tooltip("A MMSequence to use to play these feedbacks on")]
		public MMSequence Sequence;
		/// The MMSequence's TrackID to consider
		[Tooltip("The MMSequence's TrackID to consider")]
		public int TrackID = 0;
		/// whether or not to use the quantized version of the target sequence
		[Tooltip("whether or not to use the quantized version of the target sequence")]
		public bool Quantized = false;
		/// if using the quantized version of the target sequence, the BPM to apply to the sequence when playing it
		[Tooltip("if using the quantized version of the target sequence, the BPM to apply to the sequence when playing it")]
		[MMFCondition("Quantized", true)]
		public int TargetBPM = 120;
		
		/// from any class, you can set UseScriptDrivenTimescale:true, from there, instead of looking at Time.time, Time.deltaTime (or their unscaled equivalents), this feedback will compute time based on the values you feed them via ScriptDrivenDeltaTime and ScriptDrivenTime
		public bool UseScriptDrivenTimescale { get; set; }
		/// the value this feedback should use for delta time
		public float ScriptDrivenDeltaTime { get; set; }
		/// the value this feedback should use for time
		public float ScriptDrivenTime { get; set; }
	}
}