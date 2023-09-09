using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public class InventoryEvent : System.EventArgs {
        public Collectible collectible;
    }

    private static PlayerInventory instance;
    
    public static System.EventHandler<InventoryEvent> OnPlayerGetCollectible;

    private static List<Collectible.CollectibleData> collectibles = new List<Collectible.CollectibleData>();

    private static List<Item> equipables = new List<Item>();
    private static IEnumerator<Item> itemIterator = equipables.GetEnumerator();
    private static Item currentItem = null;
    //private static int sortingOrder;

    private bool hasCollectedAnchor = false;
    [SerializeField] private Transform itemPickupTransform;
    [SerializeField] private GameObject anchorPrefab;

    private bool didInit;

    public int itemSpriteCalls = 0;
    public SpriteRenderer spriteRenderer;

    public static PlayerInventory Instance
    {
        get => instance;
    }

    private void Start()
    {
        // Happens if you load a scene for the first time (i.e. Load() isn't called)
        Init();
    }

    // Called by Player.Init() too
    public void Init()
    {
        if (didInit)
            return;
        didInit = true;

        if (instance == null)
        {
            instance = this;
        }

        equipables.Clear();
        itemIterator = equipables.GetEnumerator();
        currentItem = null;
        // populate inventory on scene start
        if (hasCollectedAnchor)
        {
            GameObject anchor = Instantiate(anchorPrefab, itemPickupTransform.position, Quaternion.identity, itemPickupTransform);
            anchor.SetActive(false);
            equipables.Add(anchor.GetComponent<Item>());
            anchor.GetComponent<Item>().SetCollider(false);
        }
    }

    public void Reset()
    {
        collectibles.Clear();
        equipables.Clear();
        itemIterator = equipables.GetEnumerator();
        currentItem = null;
        hasCollectedAnchor = false;
    }

    public void SetCollectiblesList(List<Collectible.CollectibleData> value)
    {
        collectibles = value;
    }

    public List<Collectible.CollectibleData> GetCollectiblesList()
    {
        return collectibles;
    }

    public void SetHasCollectedAnchor(bool value)
    {
        SaveSystem.Current.SetBool("playerHasCollectedAnchor", value);
        hasCollectedAnchor = value;
    }

    public bool GetHasCollectedAnchor()
    {
        return hasCollectedAnchor;
    }


    /// Finds the anchor if it is placed in the map and tries to return it to the Player's inventory.
    public static void ReturnAnchorFromMap()
    {
        // If is equipped
        if (currentItem != null && currentItem.itemName == "Anchor")
        {
            return;
        }

        // If already in inventory
        foreach (Item i in equipables)
        {
            if (i.itemName == "Anchor")
            {
                return;
            }
        }

        // Find all anchors in map
        foreach (Anchor anchor in FindObjectsOfType<Anchor>())
        {
            // do we need to check for the case that it is currently being picked up? probably not bc of timing

            AddItem(anchor);
            anchor.RemoveFromTile();
            anchor.transform.SetParent(PlayerAction.Instance.GetPickedItemLocationTransform());
            anchor.AnimatePickUpEnd(PlayerAction.Instance.GetPickedItemLocationTransform().position);
        }

        NextItem();
    }

    public static void AddCollectibleFromData(Collectible.CollectibleData data) => collectibles.Add(data);

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
//        Instance.SetItemSortingOrder(sortingOrder);
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
        // Debug.Log(currentItem);
        // Debug.Log(equipables);
        if (res)
        {
            currentItem = itemIterator.Current;
            currentItem.gameObject.SetActive(true);
            currentItem.OnEquip();
            //Instance.SetItemSortingOrder(sortingOrder);
        }
        else
        {
            itemIterator.Reset();
            currentItem = null;
        }
    }

    public static void RemoveItemNoReturn()
    {
        RemoveItem(); 
    }

    public static Item RemoveItem()
    {
        if (currentItem != null)
            equipables.Remove(currentItem);
        Item temp = currentItem;
        currentItem = null;
        itemIterator = equipables.GetEnumerator();
        //Instance.SetItemSortingOrder(0);
        return temp;
    }

    public static void RemoveAndDestroyItem()
    {
        if (currentItem != null)
            equipables.Remove(currentItem);
        Destroy(currentItem.gameObject);
        currentItem = null;
        itemIterator = equipables.GetEnumerator();
        //Instance.SetItemSortingOrder(0);
    }

    public static Item GetCurrentItem()
    {
        return currentItem;
    }

    public void SetItemSortingOrderInc(int num)
    {
        itemSpriteCalls++;
        SetItemSortingOrder(num);
    }

    public void SetItemSortingOrderDec(int num)
    {
        itemSpriteCalls--;
        if(itemSpriteCalls == 0)
            SetItemSortingOrder(num);
        
    }

    public void SetItemSortingOrder(int num)
    {
        if(currentItem == null) return;
        spriteRenderer.sortingOrder = num;
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
