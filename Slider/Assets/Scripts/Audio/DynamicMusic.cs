using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// THIS SCRIPT IS NO LONGER BEING USED, and should only be used as reference
public class DynamicMusic : MonoBehaviour
{
    public Area area;
    // public bool doMountain;
    // public bool doDesert;

    public FMODUnity.StudioEventEmitter emitter;

    void Start()
    {
        DontDestroyOnLoad(this);
    }

    void Update()
    {
        switch (area)
        {
            case Area.Caves:
                UpdateCaves();
                break;
            case Area.Ocean:
                break;
            case Area.Jungle:
                UpdateJungle();
                break;
            case Area.Desert:
                UpdateDesert();
                break;
            case Area.Mountain:
                UpdateMountain();
                break;
        }
    }

    private void UpdateCaves()
    {

    }

    private void UpdateOcean()
    {

    }

    private void UpdateJungle()
    {

    }

    private void UpdateDesert()
    {
        STile s5 = SGrid.current.GetStile(5);
        float dist1 = s5.isTileActive ? (Player.GetPosition() - s5.transform.position).magnitude : 17; // center
        float dist2 = s5.isTileActive ? (Player.GetPosition() - (s5.transform.position + Vector3.right * 8.5f)).magnitude : 17; // right
        STile s6 = SGrid.current.GetStile(6);
        float dist3 = s6.isTileActive ? (Player.GetPosition() - s6.transform.position).magnitude : 17; // center
        float dist4 = s6.isTileActive ? (Player.GetPosition() - (s6.transform.position + Vector3.left * 8.5f)).magnitude : 17; // left
        emitter.SetParameter("DistToCasino", Mathf.Min(dist1, dist2, dist3, dist4));
        Debug.Log("Setting DistToCasino to " + Mathf.Min(dist1, dist2, dist3, dist4));
    }

    private void UpdateMountain()
    {
        float value = Player.GetPosition().y > 62.5 ? 0 : 1;
        emitter.SetParameter("Temperature", value);
    }
}
