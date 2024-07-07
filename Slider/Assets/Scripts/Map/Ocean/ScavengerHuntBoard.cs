using System;
using System.Collections;
using System.Collections.Generic;
using Localization;
using UnityEngine;

[Obsolete]
public class ScavengerHuntBoard : MonoBehaviour, IDialogueTableProvider
{
    private List<LocalizationPair> available_tasks = new();
    LocalizationPair tasks_available; // = "Want to be a legend? Talk to Bob.";
    
    #region localization

    enum TaskStrings
    {
        ListScavengingsBegin,
        TalkToBob,
        Done,
        Anchor,
        Rose,
        Cat,
        Gem,
        Mushroom,
        GoldenFish,
        Rock,
    }

    public Dictionary<string, LocalizationPair> TranslationTable { get; } = IDialogueTableProvider.InitializeTable(
        new Dictionary<TaskStrings, string>
        {
            { TaskStrings.ListScavengingsBegin, "Scavengings to Acquire: " },
            { TaskStrings.TalkToBob, "Want to be a legend? Talk to Bob." },
            { TaskStrings.Done, "You've scavenged the whole sea!" },
            { TaskStrings.Anchor, "A Trusty Anchor" },
            { TaskStrings.Rose, "A Delicate Rose" },
            { TaskStrings.Cat, "Cat Beard's Treasure" },
            { TaskStrings.Gem, "A Magical Gem" },
            { TaskStrings.Mushroom, "A Funky Mushroom" },
            { TaskStrings.GoldenFish, "A Golden Fish" },
            { TaskStrings.Rock, "A Peculiar Rock" },
        });

    #endregion
    
    public void OnEnable()
    {
        tasks_available = this.GetLocalized(TaskStrings.TalkToBob);
        ShopManager.OnTurnedItemIn += RemoveTask;
        SGrid.OnSTileEnabled += AddTask;
        SetScavengerBoardText();
    }


    public void OnDisable()
    {
        ShopManager.OnTurnedItemIn -= RemoveTask;
        SGrid.OnSTileEnabled -= AddTask;
    }
    
    public void SetScavengerBoardText()
    {
        if(this.available_tasks.Count >0 && SaveSystem.Current.GetBool("oceanHasTalkedToBob"))
            this.tasks_available = this.GetLocalized(TaskStrings.ListScavengingsBegin) + (LocalizationPair)(" \n") + LocalizationPair.Join(", ", this.available_tasks);
        else if(tasks_available != this.GetLocalized(TaskStrings.TalkToBob) && SGrid.Current.GetNumTilesCollected() == 9)
            this.tasks_available = this.GetLocalized(TaskStrings.Done);
        SaveSystem.Current.SetLocalizedString("OceanTavernBoardAvailable", tasks_available);
    }


   //whenever player turns in an item, remove it from the list of available tasks
    public void RemoveTask(object sender, ShopManager.OnTurnedItemInArgs args )
    {
        available_tasks.RemoveAll(pair => pair.original == args.item);
        SetScavengerBoardText();
    }
    //whenever player acquires new tile update the list of available tasks
    public void AddTask(object sender, SGrid.OnSTileEnabledArgs args )
    {
        int tileId = args.stile.islandId;
        switch(tileId){
            case 1:
                break;
            case 2:
                break;
            case 3:
                available_tasks.Add(this.GetLocalized(TaskStrings.Anchor));
                available_tasks.Add(this.GetLocalized(TaskStrings.Rose));
                break;
            case 4:
                available_tasks.Add(this.GetLocalized(TaskStrings.Cat));
                break;
            case 5:
                available_tasks.Add(this.GetLocalized(TaskStrings.Gem));
                break;
            case 6:
                //check if 7 is also active
                if(PlayerInventory.Contains("Slider 7", Area.Ocean))
                    available_tasks.Add(this.GetLocalized(TaskStrings.Mushroom));
                break;
            case 7:
                //check if 6 is active
                if(PlayerInventory.Contains("Slider 6", Area.Ocean))
                    available_tasks.Add(this.GetLocalized(TaskStrings.Mushroom));
                break;
            case 8:
                available_tasks.Add(this.GetLocalized(TaskStrings.GoldenFish));
                break;
            case 9:
                available_tasks.Add(this.GetLocalized(TaskStrings.Rock));
                break;
        }
        SetScavengerBoardText();
    }


}