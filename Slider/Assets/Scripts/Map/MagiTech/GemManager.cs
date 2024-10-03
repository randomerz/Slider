using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Localization;
using UnityEngine;

public class GemManager : MonoBehaviour, ISavable, IDialogueTableProvider
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

    public DesyncItem presentConductiveBob;
    public DesyncItem pastConductiveBob;

    private enum GemHintCode
    {
        SingleGemHint,
        MultiGemHint,
    }

    private static readonly string[] AREA_HINTS = new string[] {
        "Uh oh, something went wrong! Make sure to get all the gems and let a dev know.",
        "I heard the Village Gem was spotted near the laser.",
        "The Cave Gem was a part of some Desync experiments in the past.",
        "The Ocean gem..? Uh... something is wrong.",
        "The Jungle gem was owned by the same wizard who made this recipe!",
        "I heard the Desert gem was lost in the ancient temple in the Impact Zone.",
        "The Factory gem should be in the museum.",
        "The Mountain gem should be in the museum.",
        "The Military gem was spotted behind one of the rocks in the past, towards the west.",
        "The MagiTech gem... is on the tile I haven't given you! Something went wrong!",
    };
    
    public Dictionary<string, LocalizationPair> TranslationTable { get; } = IDialogueTableProvider.InitializeTable(
        new Dictionary<GemHintCode, string[]>
        {
            {
                GemHintCode.SingleGemHint,
                AREA_HINTS
            },
            {
                GemHintCode.MultiGemHint,
                new string[] { "Hmmm... the gems should be somewhere in this area. Can you get me: <remainingGems/>?" }
            },
        }
    );

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
        SaveSystem.Current.SetBool("magiTechMagiTech", gems.GetValueOrDefault(Area.MagiTech));

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

        AudioManager.Play("Hat Click");
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
        SGrid.Current.GivePlayerTheCollectible("Gem Fuel Recipe");
        return true;
    }

    public void UpdateGemHint()
    {
        string specific = this.GetLocalized(GemHintCode.SingleGemHint, 0).TranslatedOrFallback;
        List<Area> all = new();
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

                all.Add(a);
            }
        }

        if (num == 0)
        {
            presentConductiveBob.SetIsTracked(false);
            pastConductiveBob.SetIsTracked(false);
        }
        else if (num == 1)
        {
            specific = this.GetLocalizedSingle(GemHintCode.MultiGemHint, (int)all[0]);
            if (all[0] == Area.Caves)
            {
                presentConductiveBob.SetIsTracked(true);
                pastConductiveBob.SetIsTracked(true);
            }
        }

        string combined = this.Interpolate(
            this.GetLocalizedSingle(GemHintCode.MultiGemHint),
            new() {{
                "remainingGems",
                string.Join(", ", all.Select(a => a.GetDisplayName()))
            }}
        );

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
            AudioManager.PlayWithPitch("Hat Click", 0.7f);
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
