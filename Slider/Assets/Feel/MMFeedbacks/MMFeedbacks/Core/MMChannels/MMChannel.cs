using System;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// The possible modes used to identify a channel, either via an int or a MMChannel scriptable object
	/// </summary>
	public enum MMChannelModes
	{
		Int,
		MMChannel
	}
	
	/// <summary>
	/// A data structure used to pass channel information
	/// </summary>
	[Serializable]
	public class MMChannelData
	{
		public MMChannelModes MMChannelMode;
		public int Channel;
		public MMChannel MMChannelDefinition;

		public MMChannelData(MMChannelModes mode, int channel, MMChannel channelDefinition)
		{
			MMChannelMode = mode;
			Channel = channel;
			MMChannelDefinition = channelDefinition;
		}
	}

	/// <summary>
	/// Extensions class for MMChannelData
	/// </summary>
	public static class MMChannelDataExtensions
	{
		public static MMChannelData Set(this MMChannelData data, MMChannelModes mode, int channel, MMChannel channelDefinition)
		{
			data.MMChannelMode = mode;
			data.Channel = channel;
			data.MMChannelDefinition = channelDefinition;
			return data;
		}
	}
	
	/// <summary>
	/// A scriptable object you can create assets from, to identify Channels, used mostly (but not only) in feedbacks and shakers,
	/// to determine a channel of communication, usually between emitters and receivers
	/// </summary>
	[CreateAssetMenu(menuName = "MoreMountains/MMChannel", fileName = "MMChannel")]
	public class MMChannel : ScriptableObject
	{
		public static bool Match(MMChannelData dataA, MMChannelData dataB)
		{
			if (dataA.MMChannelMode != dataB.MMChannelMode)
			{
				return false;
			}

			if (dataA.MMChannelMode == MMChannelModes.Int)
			{
				return dataA.Channel == dataB.Channel;
			}
			else
			{
				return dataA.MMChannelDefinition == dataB.MMChannelDefinition;
			}
		}
		public static bool Match(MMChannelData dataA, MMChannelModes modeB, int channelB, MMChannel channelDefinitionB)
		{
			if (dataA == null)
			{
				return true;
			}
			
			if (dataA.MMChannelMode != modeB)
			{
				return false;
			}

			if (dataA.MMChannelMode == MMChannelModes.Int)
			{
				return dataA.Channel == channelB;
			}
			else
			{
				return dataA.MMChannelDefinition == channelDefinitionB;
			}
		}
	}
}