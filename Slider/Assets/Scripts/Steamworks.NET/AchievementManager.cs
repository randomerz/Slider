#if !(UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || STEAMWORKS_WIN || STEAMWORKS_LIN_OSX)
#define DISABLESTEAMWORKS
#endif

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if !DISABLESTEAMWORKS
using Steamworks;
#endif

/// <summary>
/// This class handles storing achievement data and sending it to steam. Use <see cref="SetAchievementStat(string, int)"/> to set/update a particular achievement stat.
/// You can also use <see cref="GetAchievementData"/> to get all stored achievement data. This is used for saving achievement data to save profiles.
/// </summary>
public class AchievementManager : Singleton<AchievementManager>
{
    private Dictionary<string, int> achievementStats = new Dictionary<string, int>();

    private void Awake()
    {
        InitializeSingleton();
    }

    /// <summary>
    /// Set a statistic with the given key. This key needs to match one setup in Steamworks (message Daniel or Travis to have them create a statistic!)
    /// </summary>
    public static void SetAchievementStat(string statName, bool dontGiveIfCheated, int value, bool dontGiveIfTeleported = false)
    {
        Debug.Log($"[AchievementManager] Called update {statName} to {value}.");

        if (dontGiveIfCheated)
        {
            if (SaveSystem.Current != null && SaveSystem.Current.GetBool("UsedCheats"))
            {
                Debug.Log($"[AchievementManager] Skipped updating {statName} to {value} because this profile used cheats.");
                return;
            }
        }

        if (dontGiveIfTeleported)
        {
            if (SaveSystem.Current != null && SaveSystem.Current.GetBool("UsedTeleport"))
            {
                Debug.Log($"[AchievementManager] Skipped updating {statName} to {value} because this profile used teleport cheats.");
                return;
            }
        }

        if (_instance != null)
        {
            _instance.achievementStats[statName] = value;
            Debug.Log($"[AchievementManager] Updating {statName} to {value}.");

            try
            {
                _instance.SendAchievementStatsToSteam();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[AchievementManager] Failed to send achievement stats to Steam: {e.Message}");
            }

            try
            {
                if (statName == "collectedBreadge")
                {
                    _instance.SendAchievementToXbox("collectFirstBreadge", value);
                    _instance.SendAchievementToXbox("threeBreadge", value);
                    _instance.SendAchievementToXbox("allbreadge", value);
                }
                else
                {
                    _instance.SendAchievementToXbox(statName, value);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[AchievementManager] Failed to send achievement stats to Xbox: {e.Message}");
            }
        }
        
    }

    /// <summary>
    /// Increment a statistic with the given key by the given amount. This key needs to match one setup in Steamworks 
    /// (message Daniel or Travis to have them create a statistic!)
    /// </summary>
    public static void IncrementAchievementStat(string statName, bool dontGiveIfCheated, int increment = 1)
    {
        if (_instance != null)
        {
            SetAchievementStat(statName, dontGiveIfCheated, _instance.achievementStats.GetValueOrDefault(statName, 0) + increment);
        }
    }

    /// <returns>An array of all AchievementStatistics, or an empty array if they cannot be found</returns>
    public static AchievementStatistic[] GetAchievementData()
    {
        try
        {
            AchievementStatistic[] pairs = new AchievementStatistic[_instance.achievementStats.Count];
            int i = 0;
            foreach (string key in _instance.achievementStats.Keys)
            {
                pairs[i] = new AchievementStatistic(key, _instance.achievementStats[key]);
                i++;
            }
            return pairs;
        } catch (NullReferenceException)
        {
            Debug.LogWarning("[AchievementManager] Failed to load achievement stats. This could indicate that you are not " +
                "connected to Steam or that AchievementManager is not present in your scene.");
            return new AchievementStatistic[0];
        }
        
    }

    /// <summary>
    /// This replaces all achievement stats with the passed in key-value pair array.
    /// This is dangerous and should only really be used when loading achievement stats from a save profile.
    /// </summary>
    /// <param name="achievementStatistics"></param>
    public static void OverwriteAchievementData(AchievementStatistic[] achievementStatistics)
    {
        try {
            if (achievementStatistics != null)
            {
                foreach (AchievementStatistic statistic in achievementStatistics)
                {
                    _instance.achievementStats[statistic.Key] = statistic.Value;
                }
                _instance.SendAchievementStatsToSteam();
            }
        } catch (NullReferenceException)
        {
            Debug.LogWarning("[AchievementManager] Failed to load achievement stats. This could indicate that you are not " +
                "connected to Steam or that AchievementManager is not present in your scene.");
            return;
        }
    }

    private void SendAchievementStatsToSteam()
    {
#if !DISABLESTEAMWORKS
        if (SteamManager.Initialized && SteamUser.BLoggedOn())
        {
            // SteamUserStats.RequestCurrentStats(); // this is an async call it probably doesnt do anything here
            foreach (string key in achievementStats.Keys)
            {
                if (SteamUserStats.GetStat(key, out int statData))
                {
                    if (achievementStats[key] <= statData)
                    {
                        // Debug.Log($"[AchievementManager] Skipped updating {key} to {achievementStats[key]} because steam stat is greater: {statData}.");
                        continue;
                    }
                }

                SteamUserStats.SetStat(key, achievementStats[key]);
            }
            SteamUserStats.StoreStats();
        }
#endif
    }

    private void SendAchievementToXbox(string achievementName, int statisiticValue)
    {
#if MICROSOFT_GDK_SUPPORT
        uint percentComplete = GetPercentFromStat(achievementName, statisiticValue);
        string achievementId = XBoxAchievementData.GetIdFromName(achievementName);
        Debug.Log($"[AchievementManager] Unlocking Xbox achievement {achievementName} ({achievementId}) with {percentComplete}% complete.");
        GDKProxy.UnlockAchievement(achievementId, percentComplete);
#endif
    }

    private uint GetPercentFromStat(string achievementName, int statisticValue)
    {
        // All stats are "all or nothing" except for the breadge ones

        if (achievementName == "collectFirstBreadge")
        {
            return (uint)(statisticValue * 100);
        }
        if (achievementName == "threeBreadge")
        {
            return (uint)(statisticValue * 100 / 3);
        }
        if (achievementName == "allbreadge")
        {
            return (uint)(statisticValue * 100 / 9);
        }

        return (uint)statisticValue * 100;
    }
}

/// <summary>
/// Serializable struct that represents a key-value pair for a Steam achievement statistic.
/// </summary>
[System.Serializable]
public struct AchievementStatistic
{
    public string Key { get; private set; }
    public int Value { get; private set; }

    public AchievementStatistic(string key, int value)
    {
        Key = key;
        Value = value;
    }
}
