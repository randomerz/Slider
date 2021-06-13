using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public string iName;
    public static bool coffeeHasBeenDrunk = false;
    public static bool hasBeenDug = false;

    public void TriggerCutscene(GameObject item)
    {
        Debug.Log("Cutscene Triggered");      
        if (iName == "Coffee")
        {
            Debug.Log("Coffee Works");
            coffeeHasBeenDrunk = true;
        }
        else if (iName == "Dig")
        {
            Debug.Log("Dig Works");
            hasBeenDug = true;
        }
        else
        {
            FindObjectOfType<NPCManager>().ChangeWorldState();
        }
        DespwanItem(item);
        Debug.Log("Current World State is " + NPCManager.currSliders);
    }

    public void DespwanItem(GameObject item)
    {
        item.SetActive(false);
    }
}
