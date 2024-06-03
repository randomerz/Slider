using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JungleBigBlobDialogue : MonoBehaviour
{
    public const string DIALOGUE_SAVE_STRING = "JungleShopBlobDialogue";

    public NPC npc;
    public RecipeList recipeList;

    public void PlayerCarryingShape(Condition c)
    {
        Item item = PlayerInventory.GetCurrentItem();
        if (item == null)
        {
            c.SetSpec(false);
            return;
        }

        if (!IsInRecipeList(item.itemName))
        {
            c.SetSpec(false);
            return;
        }

        string message = ShapeNameToSpecialMessage(item.itemName);
        if (message != null)
        {
            npc.Conds[^1].dialogueChain[0].animationOnStart = ShapeNameToSpecialAnimation(item.itemName);
        }
        else
        {
            message = ShapeNameToGenericMessage(item.itemName);
            npc.Conds[^1].dialogueChain[0].animationOnStart = "Idle";
        }

        SaveSystem.Current.SetString(DIALOGUE_SAVE_STRING, message);
        c.SetSpec(true);
    }

    private string ShapeNameToSpecialMessage(string shapeName) => shapeName switch
    {
        // "Bandage" => "",
        "Bread" => "It's gluten free!",
        // "Camera" => "",
        // "Chest" => "",
        "Circle" => "OMG Circle!!! Just like me!!!!",
        // "Crate" => "",
        "Crutch" => "Crutch? Isn't that a police baton? Yay!!",
        // "Female" => "",
        "Fish" => "Blub blub blub",
        "FishBowl" => "NOOOO YOU TRAPPED MR. BLUB BLUB",
        "Flag" => "A race? I hope everyone's a winner!",
        "Glasses" => "I SEE you've made something cool.",
        "Heart" => "Aww <3",
        // "House" => "",
        "Icecream" => "Artificial vanilla, my favorite!",
        "Line" => "Does Barron want more lines..?",
        // "Lolipop" => "",
        // "Male" => "",
        "Minecart" => "OMG do you think I can fit in it?",
        // "Mushroom" => "",
        "Pickaxe" => "Diggy diggy hole",
        "Plus" => "Eww... is that... math?",
        // "Popsicle" => "",
        "Rail" => "I am going to 'Rail' you!",
        // "SemiCircle" => "",
        // "Ship" => "",
        "Square" => "Squares are okay... but I like circles more!",
        "Triangle" => "If you were a triangle you'd be acute one!",
        _ => null
    };

    // Idle, Happy, Disgusted, Smug, Interested, Shocked
    private string ShapeNameToSpecialAnimation(string shapeName) => shapeName switch
    {
        // "Bandage" => "Idle",
        "Bread" => "Idle",
        // "Camera" => "Idle",
        // "Chest" => "Idle",
        "Circle" => "Happy",
        // "Crate" => "Idle",
        "Crutch" => "Idle",
        // "Female" => "Idle",
        "Fish" => "Idle",
        "FishBowl" => "Shocked",
        "Flag" => "Happy",
        "Glasses" => "Smug",
        "Heart" => "Happy",
        // "House" => "Idle",
        "Icecream" => "Idle",
        "Line" => "Idle",
        // "Lolipop" => "Idle",
        // "Male" => "Idle",
        "Minecart" => "Idle",
        // "Mushroom" => "Idle",
        "Pickaxe" => "Happy",
        "Plus" => "Disgusted",
        // "Popsicle" => "Idle",
        "Rail" => "Smug",
        // "SemiCircle" => "Idle",
        // "Ship" => "Idle",
        "Square" => "Idle",
        "Triangle" => "Smug",
        _ => "Idle"
    };

    private string ShapeNameToGenericMessage(string shapeName)
    {
        return $"Woah! Is that a {shapeName}?!";
    }

    private bool IsInRecipeList(string shapeName)
    {
        foreach (Recipe recipe in recipeList.list)
        {
            if (recipe.result.shapeName == shapeName)
            {
                return true;
            }
        }
        return false;
    }
}
