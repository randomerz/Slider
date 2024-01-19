using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GemManager : MonoBehaviour, ISavable
{
    private const string NUM_REMAINING_GEMS_STRING = "magiTechNumRemainingGems";

    private Dictionary<Area, bool> gems = new Dictionary<Area, bool>();
    private Dictionary<Area, GameObject> gemSprites = new();

    public List<GameObject> sprites = new();
    public List<Transform> poofTransforms = new();
    [Tooltip("Put in world progression order (Village->Caves)")]
    public List<Item> gemItems = new();

    private bool hasGemTransporter;
    public bool HasGemTransporter => hasGemTransporter;
    
    public PipeLiquid pipeLiquid;
    public Animator animator;

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

        SaveSystem.Current.SetBool("MagitechHasGemTransporter", hasGemTransporter);
        SaveSystem.Current.SetBool("MagitechGFuelFilling", pipeLiquid.isFilling);
        SaveSystem.Current.SetBool("MagitechGFuelFull", pipeLiquid.isFull);
    }

    public void Load(SaveProfile profile)
    {
        gems.Add(Area.Ocean, profile.GetBool("magiTechOcean"));
        gems.Add(Area.Military, profile.GetBool("magiTechMilitary"));
        gems.Add(Area.Factory, profile.GetBool("magiTechFactory"));

        gems.Add(Area.Mountain, profile.GetBool("magiTechMountain"));
        gems.Add(Area.Village, profile.GetBool("magiTechVillage"));
        gems.Add(Area.Caves, profile.GetBool("magiTechCaves"));

        gems.Add(Area.Desert, profile.GetBool("magiTechDesert"));
        gems.Add(Area.Jungle, profile.GetBool("magiTechJungle"));
        gems.Add(Area.MagiTech, profile.GetBool("magiTechMagiTech"));

        BuildSpriteDictionary();
        UpdateGemSprites();

        if(profile.GetBool("MagitechHasGemTransporter"))
            EnableGemTransporter();
        
        if(profile.GetBool("MagitechGFuelFull"))
            EnableGFuel(true);
        else if (profile.GetBool("MagitechGFuelFilling"))
            EnableGFuel(false);
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

    public bool HasAreaGem(Area area)
    {
        return gems.GetValueOrDefault(area, false);
    }

    public void BuildSpriteDictionary()
    {
        for(int i = 1; i <= sprites.Count; i++)
        {
            GameObject sprite = sprites[i-1];
            gemSprites.Add((Area)(i), sprite);
        }
    }

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
            AudioManager.Play("Artifact Error");
            return;
        }
        if (Enum.TryParse(item.itemName, out Area itemNameAsEnum))
        {
            gems[itemNameAsEnum] = true;
            ParticleManager.SpawnParticle(ParticleType.SmokePoof, poofTransforms[(int)itemNameAsEnum - 1].position);
            PlayerInventory.RemoveItem().gameObject.SetActive(false);
        }
        else if (item.itemName == "Mountory")
        {
            gems[Area.Factory] = true;
            gems[Area.Mountain] = true;
            ParticleManager.SpawnParticle(ParticleType.SmokePoof, poofTransforms[(int)Area.Factory - 1].position);
            ParticleManager.SpawnParticle(ParticleType.SmokePoof, poofTransforms[(int)Area.Mountain - 1].position);
            PlayerInventory.RemoveItem().gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("Tried to turn in invalid item: " + item);
        }
        UpdateGemSprites();
    }

    public void UpdateGemSprites()
    {
        if(gemSprites == null || gemSprites.Keys.Count != 9)
            BuildSpriteDictionary();
        UpdateNumRemainingGems();
        foreach(Area a in gems.Keys)
        {
            gemSprites[a].SetActive(gems[a]);
        }
        if (HasAllGems())
        {
            animator.Play("Active");
        }
    }
    
    private void UpdateNumRemainingGems()
    {
        int num = 0;

        foreach (bool b in gems.Values)
        {
            if (!b)
            {
                num += 1;
            }
        }

        SaveSystem.Current.SetString(NUM_REMAINING_GEMS_STRING, num.ToString());
    }

    private bool HasAllGems()
    {
        foreach (bool b in gems.Values)
        {
            if (!b)
            {
                return false;
            }
        }
        return true;
    }

    public void EnableGFuel(bool fillImmediate = false)
    {
        if (pipeLiquid.isFilling || pipeLiquid.isFull) return;
        if (fillImmediate)
            pipeLiquid.SetPipeFull();
        else
            pipeLiquid.FillPipe();
    }

    public void EnableGemTransporter()
    {
        hasGemTransporter = true;
    }

    public void TransportGem(Item gem)
    {
        if (Enum.TryParse(gem.itemName, out Area itemNameAsEnum))
        {
            gems[itemNameAsEnum] = true;
            ParticleManager.SpawnParticle(ParticleType.SmokePoof, poofTransforms[(int)itemNameAsEnum - 1].position);
        }
        else if (gem.itemName == "Mountory")
        {
            gems[Area.Factory] = true;
            gems[Area.Mountain] = true;
            ParticleManager.SpawnParticle(ParticleType.SmokePoof, poofTransforms[(int)Area.Factory - 1].position);
            ParticleManager.SpawnParticle(ParticleType.SmokePoof, poofTransforms[(int)Area.Mountain - 1].position);
        }
        UpdateGemSprites();
    }

    // For debug/trailer purposes
    public void TrailerActivateMachine()
    {
        animator.Play("Active");
    }
}
