using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GemManager : MonoBehaviour, ISavable
{
    private const string NUM_REMAINING_GEMS_STRING = "magiTechNumRemainingGems";
    private const string GEM_FUEL_HINT_STRING = "MagitechGemFuelHint";

    private Dictionary<Area, bool> gems = new Dictionary<Area, bool>();
    private Dictionary<Area, GameObject> gemSprites = new();

    public List<GameObject> sprites = new();
    public List<Transform> poofTransforms = new();
    [Tooltip("Put in world progression order (Village->Caves)")]
    public List<Item> gemItems = new();
    public Item oceanDuplicateItem;

    private bool hasGemTransporter;
    public bool HasGemTransporter => hasGemTransporter;
    
    public PipeLiquid pipeLiquid;
    public Animator animator;

    private void Start() 
    {
        oceanDuplicateItem.gameObject.SetActive(false);
        if (SaveSystem.Current.GetBool("MagiTechRemovedOceanGemAsExample"))
        {
            if (!gems[Area.Ocean])
            {
                oceanDuplicateItem.gameObject.SetActive(true);
                foreach (Item i in gemItems)
                {
                    if (i.itemName == "Ocean")
                    {
                        i.gameObject.SetActive(false);
                    }
                }
            }
        }
    }

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

    public void HasEightGems(Condition c)
    {
        int num = 0;

        foreach (bool b in gems.Values)
        {
            if (b)
            {
                num += 1;
            }
        }

        c.SetSpec(num >= 8);
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
            AudioManager.Play("Artifact Error");
        }
        UpdateGemSprites();
    }

    public void GiveAllGems()
    {
        foreach (Area key in gems.Keys.ToList())
        {
            gems[key] = true;
        }
        UpdateGemSprites();
        UpdateNumRemainingGems();
        UpdateGemHint();
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
        Save();
        UpdateGemSprites();
    }

    public void DisableGem(Area area)
    {
        if (area == Area.Factory || area == Area.Mountain)
        {
            gems[Area.Factory] = false;
            gems[Area.Mountain] = false;
            ParticleManager.SpawnParticle(ParticleType.SmokePoof, poofTransforms[(int)Area.Factory - 1].position);
            ParticleManager.SpawnParticle(ParticleType.SmokePoof, poofTransforms[(int)Area.Mountain - 1].position);
        }
        else
        {
            gems[area] = false;
            ParticleManager.SpawnParticle(ParticleType.SmokePoof, poofTransforms[(int)area - 1].position);
        }
        UpdateGemSprites();
    }

    public void UpdateGemSprites()
    {
        if(gemSprites == null || gemSprites.Keys.Count != 9)
            BuildSpriteDictionary();

        UpdateNumRemainingGems();
        UpdateGemHint();

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

    public void UpdateGemHint()
    {
        string specific = "Uh oh, something went wrong! Make sure to get all the gems and let a dev know.";
        List<string> all = new();
        int num = 0;

        foreach (Area a in gems.Keys)
        {
            if (a == Area.MagiTech)
            {
                continue;
            }

            if (!gems[a])
            {
                num += 1;

                all.Add(a.ToString());

                switch (a)
                {
                    case Area.Village:
                        specific = "I heard the Village Gem was spotted near the laser.";
                        break;
                    case Area.Caves:
                        specific = "You don't have the Cave Gem yet? It was on one of the large rocks in the past.";
                        break;
                    case Area.Ocean:
                        specific = "The Ocean gem..? Uh... something is wrong.";
                        break;
                    case Area.Jungle:
                        specific = "The Jungle gem was owned by the same wizard who made this recipe!";
                        break;
                    case Area.Desert:
                        specific = "I heard the Desert gem was lost in the ancient temple in the Impact Zone.";
                        break;
                    case Area.Factory:
                        specific = "The Factory gem should be in the museum.";
                        break;
                    case Area.Mountain:
                        specific = "The Mountain gem should be in the museum.";
                        break;
                    case Area.Military:
                        specific = "I heard the Military gem was spotted behind one of the rocks in the past.";
                        break;
                    case Area.MagiTech:
                        specific = "The magitech gem... is on the tile I haven't given you! Something went wrong!";
                        break;
                };
            }
        }

        string combined = $"Hmmm... we're missing multiple gems. Can you get me: {String.Join(", ", all)}?";

        SaveSystem.Current.SetString(GEM_FUEL_HINT_STRING, num >= 2 ? combined : specific);
    }

    public void TakeOutOceanGem()
    {
        // If ocean gem is alr out
        if (!gems[Area.Ocean])
        {
            return; 
        }
        // As long as ocean gem isn't destroyed
        if (oceanDuplicateItem != null)
        {
            oceanDuplicateItem.gameObject.SetActive(true);
            ParticleManager.SpawnParticle(ParticleType.SmokePoof, oceanDuplicateItem.transform.position);
            DisableGem(Area.Ocean);
            SaveSystem.Current.SetBool("MagiTechRemovedOceanGemAsExample", true);
        }
    }

    public void EnableGFuel(bool fillImmediate = false)
    {
        if (pipeLiquid.isFilling || pipeLiquid.isFull) return;
        if (fillImmediate)
            pipeLiquid.SetPipeFull();
        else
            pipeLiquid.FillPipe();
    }

    // For debug/trailer purposes
    public void TrailerActivateMachine()
    {
        animator.Play("Active");
    }
}
