using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaveDoor : MonoBehaviour, ISavable
{
    [SerializeField] private GameObject doorCollider;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite openSprite;
    [SerializeField] private Sprite closedSprite;
    [SerializeField] private bool isOpen = false;

    public void OpenDoor(){
        isOpen = true;
        doorCollider.SetActive(false);
        spriteRenderer.sprite = openSprite;
    }

    public void CloseDoor(){
        isOpen = false;
        doorCollider.SetActive(true);
        spriteRenderer.sprite = closedSprite;
    }

    public void Save()
    {
        SaveSystem.Current.SetBool("CaveDoorOpen", isOpen);
    }

    public void Load(SaveProfile s)
    {
        isOpen = s.GetBool("CaveDoorOpen");
        if(isOpen)
            OpenDoor();
    }

    public void IsDoorOpen(Condition c){
        c.SetSpec(isOpen);
    }
}
