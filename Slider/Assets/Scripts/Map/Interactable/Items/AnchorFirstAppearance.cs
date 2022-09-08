using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnchorFirstAppearance : MonoBehaviour
{
    public Anchor anchor;
    public SpriteRenderer spriteRenderer;
    public PlayerActionHints hints;
    
    void Start()
    {
        if (PlayerInventory.Instance.GetHasCollectedAnchor())
        {
            anchor.UnanchorTile();
            Destroy(gameObject);
            return;
        }
        // add to tracker
        UITrackerManager.AddNewTracker(gameObject, anchor.trackerSprite);
        
        anchor.OnPickUp.AddListener(DoCutscene);
    }

    public void DoCutscene()
    {
        ItemPickupEffect.StartCutscene(spriteRenderer.sprite, "Anchor");
        anchor.OnPickUp.RemoveListener(DoCutscene);
        hints.TriggerHint("anchor");
        Destroy(this);
    }
}
