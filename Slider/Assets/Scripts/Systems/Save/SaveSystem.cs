using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// There's probably a better way of doing this: https://gamedev.stackexchange.com/questions/110958/what-is-the-proper-way-to-handle-data-between-scenes
// See Inversion of Control / Dependency Injection frameworks
// HOWEVER -- maybe that's overkill for this project

public class SaveSystem
{
    private Dictionary<Area, SGridData> areaToSGridData = new Dictionary<Area, SGridData>();
    private Dictionary<string, bool> missionStatus = new Dictionary<string, bool>();

    // private Dictionary<Area, Vector3> playerPos = new Dictionary<Area, Vector3>(); // temporary
    // TODO: save player inventory

    public SaveSystem() {

    }

    public void SaveSGridData(Area area, SGrid sgrid) {
        if (area == Area.None) {
            Debug.LogError("Tried saving an area for Region.None");
        }

        if (!areaToSGridData.ContainsKey(area)) {
            areaToSGridData[area] = new SGridData(sgrid);
            // playerPos[area] = Player.GetPosition();
            return;
        }

        areaToSGridData[area].UpdateGrid(sgrid);
        Debug.Log("length: " + areaToSGridData[area].grid.Count);
        // playerPos[area] = Player.GetPosition();
    }

    public SGridData GetSGridData(Area area) {
        if (area == Area.None) {
            Debug.LogError("Tried loading an area for Region.None");
        }

        if (!areaToSGridData.ContainsKey(area)) {
            return null;
        }

        return areaToSGridData[area];
    }

    // public Vector3 GetPlayerPos(Area area) {
    //     return playerPos[area];
    // }

    public void SaveMissions(Dictionary<string, bool> missions) {
        foreach (string s in missions.Keys) {
            missionStatus[s] = missions[s];
        }
    }

    public Dictionary<string, bool> GetMissions(List<string> missions) {
        Dictionary<string, bool> ret = new Dictionary<string, bool>();
        foreach (string s in missions) {
            ret[s] = missionStatus[s];
        }
        return ret;
    }
}
