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
    private static List<Item> equipables = new List<Item>();
    private static IEnumerator<Item> itemIterator = equipables.GetEnumerator();
    private static Item currentItem = null;
    
    private void Awake() {
        if (instance == null) {
            instance = this;
        }
    }


    public static void Add(Collectible collectible) {
        collectibles.Add(collectible);
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
        return Contains(collectible.cName, collectible.GetArea());
    }

    public static bool Contains(string collectibleName, Area area=Area.None) {
        foreach (Collectible c in collectibles) {
            if ((c.cName == collectibleName) && 
                (area == Area.None || area == c.GetArea()))
            {
                return true;
            }
        }
        return false;
    }
}
