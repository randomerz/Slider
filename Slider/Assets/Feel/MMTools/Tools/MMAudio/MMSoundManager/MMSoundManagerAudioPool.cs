using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

namespace MoreMountains.Tools
{
	/// <summary>
	/// This class manages an object pool of audiosources
	/// </summary>
	[Serializable]
	public class MMSoundManagerAudioPool 
	{
		protected List<AudioSource> _pool;
        
		/// <summary>
		/// Fills the pool with ready-to-use audiosources
		/// </summary>
		/// <param name="poolSize"></param>
		/// <param name="parent"></param>
		public virtual void FillAudioSourcePool(int poolSize, Transform parent)
		{
			if (_pool == null)
			{
				_pool = new List<AudioSource>();
			}
            
			if ((poolSize <= 0) || (_pool.Count >= poolSize))
			{
				return;
			}

			foreach (AudioSource source in _pool)
			{
				UnityEngine.Object.Destroy(source.gameObject);
			}

			for (int i = 0; i < poolSize; i++)
			{
				GameObject temporaryAudioHost = new GameObject("MMAudioSourcePool_"+i);
				SceneManager.MoveGameObjectToScene(temporaryAudioHost.gameObject, parent.gameObject.scene);
				AudioSource tempSource = temporaryAudioHost.AddComponent<AudioSource>();
				MMFollowTarget followTarget = temporaryAudioHost.AddComponent<MMFollowTarget>();
				followTarget.enabled = false;
				followTarget.DisableSelfOnSetActiveFalse = true;
				temporaryAudioHost.transform.SetParent(parent);
				temporaryAudioHost.SetActive(false);
				_pool.Add(tempSource);
			}
		}

		/// <summary>
		/// Disables an audio source after it's done playing
		/// </summary>
		/// <param name="duration"></param>
		/// <param name="targetObject"></param>
		/// <returns></returns>
		public virtual IEnumerator AutoDisableAudioSource(float duration, AudioSource source, AudioClip clip, bool doNotAutoRecycleIfNotDonePlaying, float playbackTime, float playbackDuration)
		{
			float initialWait = (playbackDuration > 0) ? playbackDuration : duration;
			yield return MMCoroutine.WaitForUnscaled(initialWait);
			if (source.clip != clip)
			{
				yield break;
			}
			if (doNotAutoRecycleIfNotDonePlaying)
			{
				float maxTime = (playbackDuration > 0) ? playbackTime + playbackDuration : source.clip.length;
				while (source.time < maxTime)
				{
					yield return null;
				}
			}
			source.gameObject.SetActive(false);
		}

		/// <summary>
		/// Pulls an available audio source from the pool
		/// </summary>
		/// <param name="poolCanExpand"></param>
		/// <param name="parent"></param>
		/// <returns></returns>
		public virtual AudioSource GetAvailableAudioSource(bool poolCanExpand, Transform parent)
		{
			foreach (AudioSource source in _pool)
			{
				if (!source.gameObject.activeInHierarchy)
				{
					source.gameObject.SetActive(true);
					return source;
				}
			}

			if (poolCanExpand)
			{
				GameObject temporaryAudioHost = new GameObject("MMAudioSourcePool_"+_pool.Count);
				SceneManager.MoveGameObjectToScene(temporaryAudioHost.gameObject, parent.gameObject.scene);
				AudioSource tempSource = temporaryAudioHost.AddComponent<AudioSource>();
				temporaryAudioHost.transform.SetParent(parent);
				temporaryAudioHost.SetActive(true);
				_pool.Add(tempSource);
				return tempSource;
			}

			return null;
		}

		/// <summary>
		/// Stops an audiosource and returns it to the pool
		/// </summary>
		/// <param name="sourceToStop"></param>
		/// <returns></returns>
		public virtual bool FreeSound(AudioSource sourceToStop)
		{
			foreach (AudioSource source in _pool)
			{
				if (source == sourceToStop)
				{
					source.Stop();
					source.gameObject.SetActive(false);
					return true;
				}
			}
			return false;
		}
	}
}