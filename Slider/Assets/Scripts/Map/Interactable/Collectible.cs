using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Collectible : MonoBehaviour
{
    [System.Serializable]
    public class CollectibleData 
    {
        public string name;
        public Area area;
    }

    [SerializeField] private CollectibleData cData;

    
    [SerializeField] private bool shouldDisableAtStart = false;

    [SerializeField] private SpriteRenderer spriteRenderer;
    public UnityEvent onCollect;
    
    private void Start() 
    {
        if (cData.area == Area.None)
            cData.area = SGrid.current.GetArea();
        if (shouldDisableAtStart)
            gameObject.SetActive(false);
    }

    public CollectibleData GetCollectibleData() 
    {
        return cData;
    }

    public string GetName() 
    {
        return cData.name;
    }

    public void SetArea(Area a) 
    {
        a = cData.area;
    }

    public Area GetArea() 
    {
        return cData.area;
    }


    public void DoPickUp()
    {
        //Debug.Log("Cutscene Triggered");
        ItemPickupEffect.StartCutscene(spriteRenderer.sprite, cData.name, DoOnCollect);
        DespwanCollectable(gameObject);
    }

    public void DoOnCollect() 
    {
        PlayerInventory.AddCollectible(this);
        onCollect.Invoke();
    }

    public void DespwanCollectable(GameObject item)
    {
        item.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            DoPickUp();
        }
    }


    // common methods for onCollect
    public void ActivateSTile(int stileId) 
    {
        SGrid.current.CollectSTile(stileId);
    }


    public SpriteRenderer getSpriteRenderer()
    {
        return spriteRenderer;
    }

}
