using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterLandColliderManager : MonoBehaviour
{
    public Player player;
    public List<GameObject> waterColliders;
    public List<GameObject> landColliders;

    // for the player to go under the bridges in the jungle/desert
    public List<SpriteRenderer> bridgeSpriteRenderers = new List<SpriteRenderer>();

    void Start()
    {
        if (player == null) 
        {
            Debug.LogError("Field \"player\" of WaterLandColliderManager isn't set!");
            player = Player.GetInstance();
        }
        UpdateColliders();
    }

    public void SetOnWater(bool isOnWater)
    {
        player.SetIsOnWater(isOnWater);
        UpdateColliders();
    }

    public void UpdateColliders()
    {
        bool pow = player.GetIsOnWater();
        foreach (GameObject g in waterColliders)
        {
            g.SetActive(!pow);
        }
        foreach (GameObject g in landColliders)
        {
            g.SetActive(pow);
        }

        foreach (SpriteRenderer sr in bridgeSpriteRenderers)
        {
            sr.sortingLayerName = pow ? "Entity" : "Default";
        }
    }
}
