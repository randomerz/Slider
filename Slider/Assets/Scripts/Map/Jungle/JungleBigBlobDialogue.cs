using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JungleBigBlobDialogue : MonoBehaviour
{
    public string DIALOGUE_SAVE_STRING = "JungleShopBlobDialogue";

    public RecipeList recipeList;

    public void PlayerCarryingShape(Condition c)
    {
        Item item = PlayerInventory.GetCurrentItem();
        if (item == null)
        {
            Debug.Log("player not carrying item");
            c.SetSpec(false);
            return;
        }

        if (!IsInRecipeList(item.itemName))
        {
            Debug.Log("recipe not in list");
            c.SetSpec(false);
            return;
        }

        string message = ShapeNameToSpecialMessage(item.itemName);
        if (message == null)
        {
            message = ShapeNameToGenericMessage(item.itemName);
        }

        SaveSystem.Current.SetString(DIALOGUE_SAVE_STRING, message);
        c.SetSpec(true);
    }

    private string ShapeNameToSpecialMessage(string shapeName) => shapeName switch
    {
        // "Bandage" => "",
        // "Breadge" => "",
        // "Camera" => "",
        // "Chest" => "",
        "Circle" => "OMG Circle!!! Just like me!!!!",
        // "Crate" => "",
        // "Crutch" => "",
        // "Female" => "",
        // "Fish" => "",
        // "FishBowl" => "",
        // "Flag" => "",
        // "Glasses" => "",
        "Heart" => "Aww <3",
        // "House" => "",
        // "Icecream" => "",
        // "Line" => "",
        // "Lolipop" => "",
        // "Male" => "",
        // "Minecart" => "",
        // "Mushroom" => "",
        // "Pickaxe" => "",
        // "Plus" => "",
        // "Popsicle" => "",
        // "Rail" => "",
        // "SemiCircle" => "",
        // "Ship" => "",
        // "Square" => "",
        // "Triangle" => "",
        _ => null
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
