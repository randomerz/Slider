using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class JungleShapeManager : MonoBehaviour, ISavable
{
    //refactor later to just be a singleton
    public static JungleShapeManager instance { get; private set; }
    private string prefix = "jungleTurnedIn_";

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }

    public bool TurnInShape(Shape wanted)
    {
        //get teh item the player is holding
        Item held = PlayerInventory.GetCurrentItem();

        if (held == null)
        {
            return false;
        }

        //check if correct shape
        if (held.itemName.Equals(wanted.name))
        {
            //print("Turn in " + wanted.name);
            PlayerInventory.RemoveAndDestroyItem();
            SaveSystem.Current.SetBool(prefix + wanted.name, true);
        }

        return false;
    }


    public void Save()
    {

    }

    public void Load(SaveProfile profile)
    {

    }
}
