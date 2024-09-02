using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WaterLandColliderManager : MonoBehaviour
{
    public Player player;
    public List<GameObject> waterColliders;
    public List<GameObject> landColliders;

    // for the player to go under the bridges in the jungle/desert
    public List<SpriteRenderer> bridgeSpriteRenderers = new List<SpriteRenderer>();
    public UnityEvent OnSetWater;

    private bool hasStartHappened;

    void Start()
    {
        if (player == null) 
        {
            Debug.LogError("Field \"player\" of WaterLandColliderManager isn't set!");
            player = Player.GetInstance();
        }
        UpdateColliders();

        CoroutineUtils.ExecuteAfterEndOfFrame(() => hasStartHappened = true, this);
    }

    public void SetOnWater(bool isOnWater)
    {
        if (hasStartHappened)
        {
            // same sound as the anchor equip :/
            // AudioManager.Play(isOnWater ? "Create Save" : "Delete Save");
        }
        player.SetIsOnWater(isOnWater);
        UpdateColliders();
        if (isOnWater)
        {
            OnSetWater?.Invoke();
        }
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
