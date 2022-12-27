using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GemManager : MonoBehaviour, ISavable
{
    private Dictionary<Area, bool> gems = new Dictionary<Area, bool>();

    public void Save()
    {
        SaveSystem.Current.SetBool("magiTechOcean", gems.GetValueOrDefault(Area.Ocean));
        SaveSystem.Current.SetBool("magiTechMilitary", gems.GetValueOrDefault(Area.Military));
        SaveSystem.Current.SetBool("magiTechFactory", gems.GetValueOrDefault(Area.Factory));

        SaveSystem.Current.SetBool("magiTechMountain", gems.GetValueOrDefault(Area.Mountain));
        SaveSystem.Current.SetBool("magiTechVillage", gems.GetValueOrDefault(Area.Village));
        SaveSystem.Current.SetBool("magiTechCaves", gems.GetValueOrDefault(Area.Caves));

        SaveSystem.Current.SetBool("magiTechDesert", gems.GetValueOrDefault(Area.Desert));
        SaveSystem.Current.SetBool("magiTechJungle", gems.GetValueOrDefault(Area.Jungle));
        SaveSystem.Current.SetBool("magiTechMagitech", gems.GetValueOrDefault(Area.MagiTech));
    }

    public void Load(SaveProfile profile)
    {
        gems.Add(Area.Ocean, SaveSystem.Current.GetBool("magiTechOcean"));
        gems.Add(Area.Military, SaveSystem.Current.GetBool("magiTechMilitary"));
        gems.Add(Area.Factory, SaveSystem.Current.GetBool("magiTechFactory"));

        gems.Add(Area.Mountain, SaveSystem.Current.GetBool("magiTechMountain"));
        gems.Add(Area.Village, SaveSystem.Current.GetBool("magiTechVillage"));
        gems.Add(Area.Caves, SaveSystem.Current.GetBool("magiTechCaves"));

        gems.Add(Area.Desert, SaveSystem.Current.GetBool("magiTechDesert"));
        gems.Add(Area.Jungle, SaveSystem.Current.GetBool("magiTechJungle"));
        gems.Add(Area.MagiTech, SaveSystem.Current.GetBool("magiTechMagiTech"));
    }

    public void HasOceanGem(Condition c) => c.SetSpec(gems.GetValueOrDefault(Area.Ocean, false));
    public void HasMilitaryGem(Condition c) => c.SetSpec(gems.GetValueOrDefault(Area.Military, false));
    public void HasFactoryGem(Condition c) => c.SetSpec(gems.GetValueOrDefault(Area.Factory, false));
    public void HasMountainGem(Condition c) => c.SetSpec(gems.GetValueOrDefault(Area.Mountain, false));
    public void HasVillageGem(Condition c) => c.SetSpec(gems.GetValueOrDefault(Area.Village, false));
    public void HasCavesGem(Condition c) => c.SetSpec(gems.GetValueOrDefault(Area.Caves, false));
    public void HasDesertGem(Condition c) => c.SetSpec(gems.GetValueOrDefault(Area.Desert, false));
    public void HasJungleGem(Condition c) => c.SetSpec(gems.GetValueOrDefault(Area.Jungle, false));
    public void HasMagiTechGem(Condition c) => c.SetSpec(gems.GetValueOrDefault(Area.MagiTech, false));

    public void HasAllGems(Condition c)
    {
        foreach (bool b in gems.Values)
        {
            if (!b)
            {
                c.SetSpec(false);
                return;
            }
        }
        c.SetSpec(true);
    }

    public void TurnInGem()
    {
        Item item = PlayerInventory.GetCurrentItem();
        if (item == null)
        {
            AudioManager.Play("ArtifactError");
            return;
        }
        if (Enum.TryParse(item.itemName, out Area itemNameAsEnum))
        {
            gems[itemNameAsEnum] = true;
            //Funni turn-in coroutine
            PlayerInventory.RemoveAndDestroyItem();
            Debug.Log(itemNameAsEnum);
        }
        else if (item.itemName == "Mountory")
        {
            gems[Area.Factory] = true;
            gems[Area.Mountain] = true;
            PlayerInventory.RemoveAndDestroyItem();
        }
        else
        {
            Debug.LogWarning("Tried to turn in invalid item: " + item);
        }
    }
}
