using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TEMP_DynamicMusic : MonoBehaviour
{
    public bool doMountain;
    public bool doDesert;

    public FMODUnity.StudioEventEmitter emitter;

    void Start()
    {
        
    }

    void Update()
    {
        if (doMountain) UpdateMountain();
        if (doDesert) UpdateDesert();
    }

    private void UpdateMountain()
    {
        float value = Player.GetPosition().y > -75 ? 0 : 1;
        emitter.SetParameter("Temperature", value);
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
}
