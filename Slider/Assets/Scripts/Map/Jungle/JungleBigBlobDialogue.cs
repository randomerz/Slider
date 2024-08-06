using System;
using System.Collections;
using System.Collections.Generic;
using Localization;
using UnityEngine;

public class JungleBigBlobDialogue : MonoBehaviour, IDialogueTableProvider
{
    public const string DIALOGUE_SAVE_STRING = "JungleShopBlobDialogue";
    private readonly List<string> BASIC_SHAPES = new() {
        "Line",
        "Semicircle",
        "Triangle",
    };

    public NPC npc;
    public RecipeList recipeList;
    
    #region Localization

    enum ShapeStrings
    {
        BreadName,
        CircleName,
        ChestName,
        CrutchName,
        FishName,
        FishBowlName,
        FlagName,
        GlassesName,
        HeartName,
        IcecreamName,
        LineName,
        LollipopName,
        MinecartName,
        MushroomName,
        PickaxeName,
        PlusName,
        RailName,
        ShipName,
        SquareName,
        TriangleName,
        BreadSpecialMsg,
        CircleSpecialMsg,
        ChestSpecialMsg,
        CrutchSpecialMsg,
        FishSpecialMsg,
        FishBowlSpecialMsg,
        FlagSpecialMsg,
        GlassesSpecialMsg,
        HeartSpecialMsg,
        IcecreamSpecialMsg,
        LineSpecialMsg,
        LollipopSpecialMsg, 
        MinecartSpecialMsg,
        MushroomSpecialMsg,
        PickaxeSpecialMsg,
        PlusSpecialMsg,
        RailSpecialMsg,
        ShipSpecialMsg,
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
               { ShapeStrings.ChestName, "Chest" },
               { ShapeStrings.CrutchName, "Crutch" },
               { ShapeStrings.FishName, "Fish" },
               { ShapeStrings.FishBowlName, "FishBowl" },
               { ShapeStrings.FlagName, "Flag" },
               { ShapeStrings.GlassesName, "Glasses" },
               { ShapeStrings.HeartName,"Heart" },
               { ShapeStrings.IcecreamName, "Icecream" },
               { ShapeStrings.LineName, "Line" },
               { ShapeStrings.LollipopName, "Lollipop" },
               { ShapeStrings.MinecartName, "Minecart" },
               { ShapeStrings.MushroomName, "Mushroom" },
               { ShapeStrings.PickaxeName, "Pickaxe" },
               { ShapeStrings.PlusName, "Plus" },
               { ShapeStrings.RailName, "Rail" },
               { ShapeStrings.ShipName, "Ship" },
               { ShapeStrings.SquareName, "Square" },
               { ShapeStrings.TriangleName, "Triangle" },
               { ShapeStrings.BreadSpecialMsg, "It's gluten free!" },
               { ShapeStrings.CircleSpecialMsg, "OMG Circle!!! Just like me!!!!" },
               { ShapeStrings.ChestSpecialMsg, "That would make for some great buried treasure!" }
               { ShapeStrings.CrutchSpecialMsg, "Crutch? Isn't that a police baton? Yay!!" },
               { ShapeStrings.FishSpecialMsg, "Blub blub blub" },
               { ShapeStrings.FishBowlSpecialMsg, "NOOOO YOU TRAPPED MR. BLUB BLUB" },
               { ShapeStrings.FlagSpecialMsg, "A race? I hope everyone's a winner!" },
               { ShapeStrings.GlassesSpecialMsg, "I SEE you've made something cool." },
               { ShapeStrings.HeartSpecialMsg, "Aww <3" },
               { ShapeStrings.IcecreamSpecialMsg, "Artificial vanilla, my favorite!" },
               { ShapeStrings.LineSpecialMsg, "Does Barron want more lines..?" },
               { ShapeStrings.LollipopSpecialMsg, "Yummy!!!" },
               { ShapeStrings.MinecartSpecialMsg, "OMG do you think I can fit in it?" },
               { ShapeStrings.MushroomSpecialMsg, "Watch out!! It might explode!" },
               { ShapeStrings.PickaxeSpecialMsg, "Diggy diggy hole" },
               { ShapeStrings.PlusSpecialMsg, "Eww... is that... math?" },
               { ShapeStrings.RailSpecialMsg, "I am going to 'Rail' you!" },
               { ShapeStrings.ShipSpecialMsg, "Ahoy!!!" }
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

        SaveSystem.Current.SetLocalizedString(DIALOGUE_SAVE_STRING, (LocalizationPair) message);
        c.SetSpec(true);
    }

    private LocalizationPair? ShapeNameToSpecialMessage(string shapeName) => shapeName switch
    {
        // "Bandage" => "",
        "Bread" => this.GetLocalized(ShapeStrings.BreadSpecialMsg),
        // "Camera" => "",
        "Chest" => this.GetLocalized(ShapeStrings.ChestSpecialMsg),
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
        "Lollipop" => this.GetLocalized(ShapeStrings.LollipopSpecialMsg),
        // "Male" => "",
        "Minecart" => this.GetLocalized(ShapeStrings.MinecartSpecialMsg),
        "Mushroom" => this.GetLocalized(ShapeStrings.MushroomSpecialMsg),
        "Pickaxe" => this.GetLocalized(ShapeStrings.PickaxeSpecialMsg),
        "Plus" => this.GetLocalized(ShapeStrings.PlusSpecialMsg),
        // "Popsicle" => "",
        "Rail" => this.GetLocalized(ShapeStrings.RailSpecialMsg),
        // "SemiCircle" => "",
        "Ship" => this.GetLocalized(ShapeStrings.ShipSpecialMsg),
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
        "Chest" => "Idle",
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
        "Lollipop" => "Idle",
        // "Male" => "Idle",
        "Minecart" => "Idle",
        "Mushroom" => " Shocked",
        "Pickaxe" => "Happy",
        "Plus" => "Disgusted",
        // "Popsicle" => "Idle",
        "Rail" => "Smug",
        // "Semicircle" => "Idle",
        "Ship" => "Idle",
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
        if (BASIC_SHAPES.Contains(shapeName))
        {
            return true;
        }

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
