using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Collectible : MonoBehaviour
{
    public string cName;
    private Area area;
    
    [SerializeField] private bool shouldDisableAtStart = false;

    [SerializeField] private SpriteRenderer spriteRenderer;
    public UnityEvent onCollect;
    
    private void Start() 
    {
        if (area == Area.None)
            area = SGrid.current.GetArea();
        if (shouldDisableAtStart)
            gameObject.SetActive(false);
    }

    public void SetArea(Area a) 
    {
        a = area;
    }

    public Area GetArea() 
    {
        return area;
    }


    public void DoPickUp()
    {
        //Debug.Log("Cutscene Triggered");
        ItemPickupEffect.StartCutscene(spriteRenderer.sprite, cName, DoOnCollect);
        DespwanCollectable(gameObject);
    }

    public void DoOnCollect() 
    {
        PlayerInventory.Add(this);
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
        if (FindObjectOfType<NPCManager>() != null) {
            FindObjectOfType<NPCManager>().ChangeWorldState();
        }
        SGrid.current.EnableStile(stileId);
    }

}
