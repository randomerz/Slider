using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;

public class AchievementManager : MonoBehaviour
{
    private Dictionary<string, int> achievementStats;

    public void SetAchievementStat(string statName, int value)
    {
        achievementStats[statName] = value;
        SendAchievementStatsToSteam();
    }

    private void SendAchievementStatsToSteam()
    {
        foreach (string key in achievementStats.Keys)
        {
            SteamUserStats.SetStat(key, achievementStats[key]);
        }
    }

    public KeyValuePair<string, int>[] GetAchievementData()
    {
        KeyValuePair<string, int>[] pairs = new KeyValuePair<string, int>[achievementStats.Count];
        for (int i = 0; i < 
    }
}
