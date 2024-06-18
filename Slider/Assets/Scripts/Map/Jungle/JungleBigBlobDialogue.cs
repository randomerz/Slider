using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JungleBigBlobDialogue : MonoBehaviour, IDialogueTableProvider
{
    public const string DIALOGUE_SAVE_STRING = "JungleShopBlobDialogue";

    public NPC npc;
    public RecipeList recipeList;
    
    #region Localization

    enum ShapeStrings
    {
        BreadName,
        CircleName,
        CrutchName,
        FishName,
        FishBowlName,
        FlagName,
        GlassesName,
        HeartName,
        IcecreamName,
        LineName,
        MinecartName,
        PIckaxeName,
        PlusName,
        RailName,
        SquareName,
        TriangleName,
        BreadSpecialMsg,
        CircleSpecialMsg,
        CrutchSpecialMsg,
        FishSpecialMsg,
        FishBowlSpecialMsg,
        FlagSpecialMsg,
        GlassesSpecialMsg,
        HeartSpecialMsg,
        IcecreamSpecialMsg,
        LineSpecialMsg,
        MinecartSpecialMsg,
        PIckaxeSpecialMsg,
        PlusSpecialMsg,
        RailSpecialMsg,
        SquareSpecialMsg,
        TriangleSpecialMsg,
        ShapeGenericMessageBeginning,
        ShapeGenericMessagePunctuation
    }
    
    public Dictionary<string, (string original, string translated)> TranslationTable { get; }
    private Dictionary<string, (string original, string translated)> _translationTable =
        IDialogueTableProvider.InitializeTable(
            new Dictionary<ShapeStrings, string>
            {
               { ShapeStrings.BreadName, "Bread" },
               { ShapeStrings.CircleName, "Circle" },
               { ShapeStrings.CrutchName, "Crutch" },
               { ShapeStrings.FishName, "Fish" },
               { ShapeStrings.FishBowlName, "FishBowl" },
               { ShapeStrings.FlagName, "Flag" },
               { ShapeStrings.GlassesName, "Glasses" },
               { ShapeStrings.HeartName,"Heart" },
               { ShapeStrings.IcecreamName, "Icecream" },
               { ShapeStrings.LineName, "Line"},
               { ShapeStrings.MinecartName, "Minecart" },
               { ShapeStrings.PIckaxeName, "Pickaxe" },
               { ShapeStrings.PlusName, "Plus" },
               { ShapeStrings.RailName, "Rail" },
               { ShapeStrings.SquareName, "Square" },
               { ShapeStrings.TriangleName, "Triangle" },
               { ShapeStrings.BreadSpecialMsg, "It's gluten free!" },
               { ShapeStrings.CircleSpecialMsg, "OMG Circle!!! Just like me!!!!" },
               { ShapeStrings.CrutchSpecialMsg, "Crutch? Isn't that a police baton? Yay!!" },
               { ShapeStrings.FishSpecialMsg, "Blub blub blub" },
               { ShapeStrings.FishBowlSpecialMsg, "NOOOO YOU TRAPPED MR. BLUB BLUB" },
               { ShapeStrings.FlagSpecialMsg, "A race? I hope everyone's a winner!" },
               { ShapeStrings.GlassesSpecialMsg, "I SEE you've made something cool." },
               { ShapeStrings.HeartSpecialMsg, "Aww <3" },
               { ShapeStrings.IcecreamSpecialMsg, "Artificial vanilla, my favorite!" },
               { ShapeStrings.LineSpecialMsg, "Does Barron want more lines..?" },
               { ShapeStrings.MinecartSpecialMsg, "OMG do you think I can fit in it?" },
               { ShapeStrings.PIckaxeSpecialMsg, "Diggy diggy hole" },
               { ShapeStrings.PlusSpecialMsg, "Eww... is that... math?" },
               { ShapeStrings.RailSpecialMsg, "I am going to 'Rail' you!" },
               { ShapeStrings.SquareSpecialMsg, "Squares are okay... but I like circles more!" },
               { ShapeStrings.TriangleSpecialMsg, "If you were a triangle you'd be acute one!"},
               { ShapeStrings.ShapeGenericMessageBeginning, "Woah! Is that a " },
               { ShapeStrings.ShapeGenericMessagePunctuation, "?!" },
            });
    #endregion

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
        "Bread" => this.GetLocalizedSingle(ShapeStrings.BreadSpecialMsg),
        // "Camera" => "",
        // "Chest" => "",
        "Circle" =>this.GetLocalizedSingle(ShapeStrings.CircleSpecialMsg),
        // "Crate" => "",
        "Crutch" => this.GetLocalizedSingle(ShapeStrings.CrutchSpecialMsg),
        // "Female" => "",
        "Fish" => this.GetLocalizedSingle(ShapeStrings.FishSpecialMsg),
        "FishBowl" => this.GetLocalizedSingle(ShapeStrings.FishBowlSpecialMsg),
        "Flag" => this.GetLocalizedSingle(ShapeStrings.FlagSpecialMsg),
        "Glasses" => this.GetLocalizedSingle(ShapeStrings.GlassesSpecialMsg),
        "Heart" => this.GetLocalizedSingle(ShapeStrings.HeartSpecialMsg),
        // "House" => "",
        "Icecream" => this.GetLocalizedSingle(ShapeStrings.IcecreamSpecialMsg),
        "Line" => this.GetLocalizedSingle(ShapeStrings.LineSpecialMsg),
        // "Lolipop" => "",
        // "Male" => "",
        "Minecart" => this.GetLocalizedSingle(ShapeStrings.MinecartSpecialMsg),
        // "Mushroom" => "",
        "Pickaxe" => this.GetLocalizedSingle(ShapeStrings.PIckaxeSpecialMsg),
        "Plus" => this.GetLocalizedSingle(ShapeStrings.PlusSpecialMsg),
        // "Popsicle" => "",
        "Rail" => this.GetLocalizedSingle(ShapeStrings.RailSpecialMsg),
        // "SemiCircle" => "",
        // "Ship" => "",
        "Square" => this.GetLocalizedSingle(ShapeStrings.SquareSpecialMsg),
        "Triangle" => this.GetLocalizedSingle(ShapeStrings.TriangleSpecialMsg),
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
        string shapeNameLocalized = shapeName; // fallback just use english...
        if (Enum.TryParse<ShapeStrings>(shapeName+"Name", out var shapeNameEnum))
        {
            shapeNameLocalized = this.GetLocalizedSingle(shapeNameEnum);
        }
        return this.GetLocalizedSingle(ShapeStrings.ShapeGenericMessageBeginning)
               + shapeNameLocalized
               + this.GetLocalizedSingle(ShapeStrings.ShapeGenericMessagePunctuation);
        // return $"Woah! Is that a {shapeName}?!";
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
