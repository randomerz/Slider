using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MoreMountains.Tools
{
	/// <summary>
	/// This class will let you trigger a OnRandomInterval event periodically, at random intervals
	/// </summary>
	public class MMPeriodicExecution : MonoBehaviour
	{
		/// the min and max duration of the interval between two events, in seconds
		[MMVector("Min", "Max")]
		public Vector2 RandomIntervalDuration = new Vector2(1f, 3f);
		/// the event to play at the end of each interval 
		public UnityEvent OnRandomInterval;
        
		protected float _lastUpdateAt = 0f;
		protected float _currentInterval = 0f;
        
		/// <summary>
		/// On Start we initialize our interval duration
		/// </summary>
		protected virtual void Start()
		{
			DetermineNewInterval();
		}
        
		/// <summary>
		/// On Update we check if we've reached the end of an interval
		/// </summary>
		protected virtual void Update()
		{
			if (Time.time - _lastUpdateAt > _currentInterval)
			{
				OnRandomInterval?.Invoke();
				_lastUpdateAt = Time.time;
				DetermineNewInterval();
			}
		}

		/// <summary>
		/// Randomizes a new duration
		/// </summary>
		protected virtual void DetermineNewInterval()
		{
			_currentInterval = Random.Range(RandomIntervalDuration.x, RandomIntervalDuration.y);
		}
	}
}