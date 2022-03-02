using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterLandColliderManager : MonoBehaviour
{
    public List<GameObject> waterColliders;
    public List<GameObject> landColliders;

    void Start()
    {
        UpdateColliders();
    }

    public void SetOnWater(bool isOnWater)
    {
        Player.SetIsOnWater(isOnWater);
        UpdateColliders();
    }

    public void UpdateColliders()
    {
        bool pow = Player.GetIsOnWater();
        foreach (GameObject g in waterColliders)
        {
            g.SetActive(!pow);
        }
        foreach (GameObject g in landColliders)
        {
            g.SetActive(pow);
        }
    }
}
