using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OceanBoatSpawn : MonoBehaviour, ISavable
{
    //Handles spawning boats on loading into ocean
  
    [SerializeField] private List<GameObject> boats;
    [SerializeField] private List<GameObject> docks;

    private int boatSpawnLoc;

    public void SetBoatLocation(int loc)
    {
        boatSpawnLoc = loc;
    }

    public void ToggleBoatAndDocks()
    {
        if(boats.Count != docks.Count) {
            Debug.LogWarning("Must have same number of boats and docks!");
            return;
        }
        for(int i = 0; i < boats.Count; i++)
        {
            boats[i].SetActive(i == boatSpawnLoc);
            docks[i].SetActive(i != boatSpawnLoc);
        }
    }

    public void Load(SaveProfile profile)
    {
        boatSpawnLoc = profile.GetInt("OceanBoatSpawnLoc");
        if(profile == null || profile.GetSerializablePlayer() == null) return;
        if(!profile.GetSerializablePlayer().isOnWater)
        {
            ToggleBoatAndDocks();
        }
    }

    public void Save()
    {
        SaveSystem.Current.SetInt("OceanBoatSpawnLoc", boatSpawnLoc);
    }

}
