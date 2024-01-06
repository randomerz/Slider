using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SerializablePlayer
{
    // Player
    public float[] position; // is this a good idea?
    public bool isOnWater;
    public bool isInHouse;

    // Player Inventory
    public List<Collectible.CollectibleData> collectibles = new List<Collectible.CollectibleData>();
    public bool hasCollectedAnchor;

    // no items for now -- if we ever serialize item positions in game we should do sm here too
    // otherwise we might have weird cases where player picks item and leaves with it
}
