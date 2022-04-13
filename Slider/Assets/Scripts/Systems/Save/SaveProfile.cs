using System.Collections.Generic;
using UnityEngine;

public class SaveProfile
{
    // metadata
    private string profileName;
    private float playTimeInSeconds;
    private bool completionStatus;

    private Dictionary<Area, SGridData> areaToSGridData = new Dictionary<Area, SGridData>();

    private Dictionary<string, bool> bools = new Dictionary<string, bool>();
    private Dictionary<string, string> strings = new Dictionary<string, string>();

    // private Dictionary<Area, Vector3> playerPos = new Dictionary<Area, Vector3>(); // temporary
    // TODO: save player inventory

    public SaveProfile(string profileName)
    {
        this.profileName = profileName;
        strings["Cat"] = profileName;
    }

    #region Getters / Setters

    public string GetProfileName()
    {
        return profileName;
    }

    public float GetPlayTimeInSeconds()
    {
        return playTimeInSeconds;
    }

    public void SetPlayTimeInSeconds(float value)
    {
        playTimeInSeconds = value;
    }

    public bool GetCompletionStatus()
    {
        return completionStatus;
    }

    public void SetCompletionStatus(bool value)
    {
        completionStatus = value;
    }

    public Dictionary<Area, SGridData> GetAreaToSGridData()
    {
        return areaToSGridData;
    }

    #endregion

    public void SaveSGridData(Area area, SGrid sgrid)
    {
        if (area == Area.None)
        {
            Debug.LogError("Tried saving an area for Region.None");
        }

        if (!areaToSGridData.ContainsKey(area))
        {
            areaToSGridData[area] = new SGridData(sgrid);
            // playerPos[area] = Player.GetPosition();
            return;
        }

        areaToSGridData[area].UpdateGrid(sgrid);
        // Debug.Log("length: " + areaToSGridData[area].grid.Count);
        // playerPos[area] = Player.GetPosition();
    }

    public SGridData GetSGridData(Area area)
    {
        if (area == Area.None)
        {
            Debug.LogError("Tried loading an area for Region.None");
        }

        if (!areaToSGridData.ContainsKey(area))
        {
            return null;
        }

        return areaToSGridData[area];
    }

    // public Vector3 GetPlayerPos(Area area) {
    //     return playerPos[area];
    // }

    public void SaveBools(Dictionary<string, bool> missions)
    {
        foreach (string s in missions.Keys)
        {
            bools[s] = missions[s];
        }
    }

    public Dictionary<string, bool> GetBools(List<string> missions)
    {
        Dictionary<string, bool> ret = new Dictionary<string, bool>();
        foreach (string s in missions)
        {
            ret[s] = bools[s];
        }
        return ret;
    }
}