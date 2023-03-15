using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BasementDoor : MonoBehaviour
{
    public UnityEvent OnSuccess;

    public void CheckBasement() 
    {
        bool hasKey = PlayerInventory.Contains("Basement Key", Area.Mountain);
        if(hasKey)
            OnSuccess?.Invoke();
        else
            AudioManager.Play("Artifact Error");
    }
}
