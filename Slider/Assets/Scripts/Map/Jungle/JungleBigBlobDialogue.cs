using System;
using System.Collections;
using System.Collections.Generic;
using Localization;
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
    
    public Dictionary<string, LocalizationPair> TranslationTable { get; } =
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

        LocalizationPair? message = ShapeNameToSpecialMessage(item.itemName);
        if (message != null)
        {
            npc.Conds[^1].dialogueChain[0].animationOnStart = ShapeNameToSpecialAnimation(item.itemName);
        }
        else
        {
            message = ShapeNameToGenericMessage(item.itemName);
            npc.Conds[^1].dialogueChain[0].animationOnStart = "Idle";
        }

        SaveSystem.Current.SetString(DIALOGUE_SAVE_STRING, message ?? new LocalizationPair
        {
            original = "",
            translated = ""
        });
        c.SetSpec(true);
    }

    private LocalizationPair? ShapeNameToSpecialMessage(string shapeName) => shapeName switch
    {
        // "Bandage" => "",
        "Bread" => this.GetLocalized(ShapeStrings.BreadSpecialMsg),
        // "Camera" => "",
        // "Chest" => "",
        "Circle" =>this.GetLocalized(ShapeStrings.CircleSpecialMsg),
        // "Crate" => "",
        "Crutch" => this.GetLocalized(ShapeStrings.CrutchSpecialMsg),
        // "Female" => "",
        "Fish" => this.GetLocalized(ShapeStrings.FishSpecialMsg),
        "FishBowl" => this.GetLocalized(ShapeStrings.FishBowlSpecialMsg),
        "Flag" => this.GetLocalized(ShapeStrings.FlagSpecialMsg),
        "Glasses" => this.GetLocalized(ShapeStrings.GlassesSpecialMsg),
        "Heart" => this.GetLocalized(ShapeStrings.HeartSpecialMsg),
        // "House" => "",
        "Icecream" => this.GetLocalized(ShapeStrings.IcecreamSpecialMsg),
        "Line" => this.GetLocalized(ShapeStrings.LineSpecialMsg),
        // "Lolipop" => "",
        // "Male" => "",
        "Minecart" => this.GetLocalized(ShapeStrings.MinecartSpecialMsg),
        // "Mushroom" => "",
        "Pickaxe" => this.GetLocalized(ShapeStrings.PIckaxeSpecialMsg),
        "Plus" => this.GetLocalized(ShapeStrings.PlusSpecialMsg),
        // "Popsicle" => "",
        "Rail" => this.GetLocalized(ShapeStrings.RailSpecialMsg),
        // "SemiCircle" => "",
        // "Ship" => "",
        "Square" => this.GetLocalized(ShapeStrings.SquareSpecialMsg),
        "Triangle" => this.GetLocalized(ShapeStrings.TriangleSpecialMsg),
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

    private LocalizationPair ShapeNameToGenericMessage(string shapeName)
    {
        var shapeNameLocalized = new LocalizationPair
        {
            original = shapeName,
            translated = shapeName
        }; // fallback just use english...
        if (Enum.TryParse<ShapeStrings>(shapeName+"Name", out var shapeNameEnum))
        {
            shapeNameLocalized = this.GetLocalized(shapeNameEnum);
        }
        return this.GetLocalized(ShapeStrings.ShapeGenericMessageBeginning)
               + shapeNameLocalized
               + this.GetLocalized(ShapeStrings.ShapeGenericMessagePunctuation);
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
