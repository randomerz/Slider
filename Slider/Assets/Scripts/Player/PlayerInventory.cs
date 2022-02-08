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

    private static List<Collectible.CollectibleData> collectibles = new List<Collectible.CollectibleData>(); // separate Sliders + items?
    private static List<Item> equipables = new List<Item>();
    private static IEnumerator<Item> itemIterator = equipables.GetEnumerator();
    private static Item currentItem = null;
    
    private void Awake() {
        if (instance == null) {
            instance = this;
        }
    }


    public static void AddCollectible(Collectible collectible) {
        // Debug.Log("Adding " + collectible.GetArea() + " " + collectible.GetName());

        collectibles.Add(collectible.GetCollectibleData());
        OnPlayerGetCollectible?.Invoke(instance, new InventoryEvent {collectible = collectible});
    }

    public static void AddItem(Item item)
    {
        if (currentItem != null)
        {
            currentItem.gameObject.SetActive(false);
        }
        equipables.Insert(0, item);
        itemIterator = equipables.GetEnumerator();
        itemIterator.MoveNext();
        currentItem = itemIterator.Current;

        currentItem.OnEquip();
    }

    public static void NextItem()
    {

        if (currentItem != null)
        {
            currentItem.gameObject.SetActive(false);
        }
        else
        {
            itemIterator = equipables.GetEnumerator();
        }
        bool res = itemIterator.MoveNext();
        Debug.Log(currentItem);
        Debug.Log(equipables);
        if (res)
        {
            currentItem = itemIterator.Current;
            currentItem.gameObject.SetActive(true);
            currentItem.OnEquip();
        }
        else
        {
            itemIterator.Reset();
            currentItem = null;
        }
    }

    public static void RemoveItem()
    {
        if (currentItem != null)
            equipables.Remove(currentItem);
        currentItem = null;
        itemIterator = equipables.GetEnumerator();
    }

    public static Item GetCurrentItem()
    {
        return currentItem;
    }

    /// <summary>
    /// Checks if the collectible is in the List, collectibles, by string name
    /// </summary>
    /// <param name="collectible">The collectible to check</param>
    /// <returns></returns>
    public static bool Contains(Collectible collectible) {
        return Contains(collectible.GetName(), collectible.GetArea());
    }

    public static bool Contains(string collectibleName, Area area=Area.None) {
        foreach (Collectible.CollectibleData cd in collectibles) {
            if ((cd.name == collectibleName) && 
                (area == Area.None || area == cd.area))
            {
                // Debug.Log("Found a match for " + area + " " + collectibleName);
                return true;
            }
        }
        return false;
    }
}
