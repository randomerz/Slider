using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScavengerHuntBoard : MonoBehaviour
{
    private List<string> available_tasks = new List<string> ();
    string tasks_available = "Want to be a legend? Talk to Bob.";
    public void OnEnable()
    {
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
            this.tasks_available = $"Scavengings to Acquire: \n{string.Join(", ", this.available_tasks)}";
        else if(tasks_available != "Want to be a legend? Talk to Bob." && SGrid.Current.GetNumTilesCollected() == 9)
            this.tasks_available = "You've scavenged the whole sea!";
        SaveSystem.Current.SetString("OceanTavernBoardAvailable", tasks_available);
    }


   //whenever player turns in an item, remove it from the list of available tasks
    public void RemoveTask(object sender, ShopManager.OnTurnedItemInArgs args )
    {
        available_tasks.Remove(args.item);
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
                available_tasks.Add("A Trusty Anchor");
                available_tasks.Add("A Delicate Rose");
                break;
            case 4:
                available_tasks.Add("Cat Beard's Treasure");
                break;
            case 5:
                available_tasks.Add("A Magical Gem");
                break;
            case 6:
                //check if 7 is also active
                if(PlayerInventory.Contains("Slider 7", Area.Ocean))
                    available_tasks.Add("A Funky Mushroom");
                break;
            case 7:
                //check if 6 is active
                if(PlayerInventory.Contains("Slider 6", Area.Ocean))
                    available_tasks.Add("A Funky Mushroom");
                break;
            case 8:
                available_tasks.Add("A Golden Fish");
                break;
            case 9:
                available_tasks.Add("A Peculiar Rock");
                break;
        }
        SetScavengerBoardText();
    }


}