using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public class InventoryEvent {
        public Collectible collectible;
    }
    
    public static System.EventHandler<InventoryEvent> OnPlayerGetCollectible;

    private static PlayerInventory instance;

    private static List<Collectible> collectibles = new List<Collectible>(); // separate Sliders + items?
    
    private void Awake() {
        if (instance == null) {
            instance = this;
        }
    }


    public static void Add(Collectible collectible) {
        collectibles.Add(collectible);
        OnPlayerGetCollectible?.Invoke(instance, new InventoryEvent {collectible = collectible});
    }

    /// <summary>
    /// Checks if the item is in the List, collectibles, by string name
    /// </summary>
    /// <param name="collectible">The collectible to check</param>
    /// <returns></returns>
    public static bool Contains(Collectible collectible) {
        return Contains(collectible.cName);
    }

    public static bool Contains(string collectibleName) {
        foreach (Collectible c in collectibles) {
            if (c.cName == collectibleName)
                return true;
        }
        return false;
    }
}
