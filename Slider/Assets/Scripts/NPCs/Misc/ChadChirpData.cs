using System;
using System.Collections.Generic;
using System.Linq;
using Localization;

public partial class ChadChirp
{
    #region Localization

    enum ChadChirpStrings
    {
        Onwards,
        CameFromJungle,
        CameFromMilitary,
        ArrivedInDesert,
        ArrivedInFactoryPresent,
        ArrivedInMagiTechPresent,
        UsedAnchor,
        WentThroughPortalPast,
        WentThroughPortalPresent,
        LeftGreedyWizard,
        WalkNearLaser,
        WalkNearRocket,
        WalkNearCastle,
        EnteredMuseum,
        WalkNearGemFuelMachine,
        WalkNearCasinoTable,
        WalkNearCasinoExplosives,
        EnteredDesertTemple,
        EnteredDesertTempleSnail,
        WalkedInDinoMouth,
        WalkNearChadSr,
        WalkNearPresentMegaCrystal,
        WalkNearPastMegaCrystal,
        RandomSmallTalk1,
        RandomSmallTalk2,
        RandomSmallTalk3,
    }
    
    public Dictionary<string, LocalizationPair> TranslationTable { get; } = IDialogueTableProvider.InitializeTable(
        ChadChirpData.chirpDataList.ToDictionary(
            cData => Enum.Parse<ChadChirpStrings>(cData.id),
            cData => cData.text
            )
        );

    #endregion

    // TODO: refactor to array of RandomSmallTalk strings in the table, then just use the table entry length
    private const int NUMBER_OF_SMALL_TALKS = 3;
    
    public class ChadChirpData
    {
        public string id;
        public string text;
        public int priority; // 2 for "CanChirps", 1 for "WantChirps"
        public bool canBeRepeated; // If can be repeated, then priority also goes from 2 -> 1
        public bool hasBeenUsed;

        public LocalizationPair GetLocalized(ChadChirp context)
        {
            if (!Enum.TryParse<ChadChirpStrings>(id, out var key))
            {
                return (LocalizationPair) text;
            }
            return context.GetLocalized(key);
        }

        public static string GetChirpUsedSaveString(ChadChirpData data) => $"MiscChadChirpUsed_{data.id}";

        // TODO: Make chad less annoying, more optimistic/interested, and more funny
        public static readonly List<ChadChirpData> chirpDataList = new() {
            // Areas
            new() { 
                id = "CameFromJungle", 
                text = "It's been so long since I've seen Barron... it'd probably be too awkward anyways.",
                priority = 2,
                canBeRepeated = true,
            },
            new() { 
                id = "CameFromMilitary", 
                text = "War? Yeah, I'm a pro at RPS Tactics what about it?",
                priority = 2,
                canBeRepeated = true,
            },
            // new() { 
            //     id = "ArrivedInDesert", 
            //     text = "Oh... what are we doing here?",
            //     priority = 2,
            //     canBeRepeated = true,
            // },
            new() { 
                id = "ArrivedInFactoryPresent", 
                text = "You know trespassing is a crime right? Right? Please say something.",
                priority = 2,
                canBeRepeated = true,
            },
            // new() { 
            //     id = "ArrivedInFactoryPast", 
            //     text = "",
            //     priority = 2,
            //     canBeRepeated = true,
            // },
            new() { 
                id = "ArrivedInMagiTechPresent", 
                text = "They doing some wild stuff here dude. You know, science.",
                priority = 2,
                canBeRepeated = true,
            },
            // new() { 
            //     id = "ArrivedInMagiTechPast", 
            //     text = "",
            //     priority = 2,
            //     canBeRepeated = true,
            // },

            // General
            new() { 
                id = "UsedAnchor", 
                text = "What in the- watch where you're dropping that thing!",
                priority = 1,
                canBeRepeated = false,
            },
            new() { 
                id = "WentThroughPortalPast", 
                text = "I'll never get used to that...",
                priority = 1,
                canBeRepeated = false,
            },
            new() { 
                id = "WentThroughPortalPresent", 
                text = "I think I'm gonna barf.",
                priority = 1,
                canBeRepeated = false,
            },

            // Magitech
            new() { 
                id = "LeftGreedyWizard", 
                text = "He wants that much money?! MRI doesn't even have that kind of cash.",
                priority = 2,
                canBeRepeated = true,
            },
            new() { 
                id = "WalkNearLaser", 
                text = "Heh, I'm sure I can turn on the laser if we really need to.",
                priority = 1,
                canBeRepeated = true,
            },
            new() { 
                id = "WalkNearRocket", 
                text = "I heard you plan on going to space... I guess that's pretty cool.",
                priority = 1,
                canBeRepeated = false,
            },
            new() { 
                id = "WalkNearCastle", 
                text = "One day, I'll be the king of something like this.",
                priority = 2,
                canBeRepeated = false,
            },
            new() { 
                id = "EnteredMuseum", 
                text = "For people with time travel you'd think they'd have more stuff.",
                priority = 2,
                canBeRepeated = false,
            },
            new() { 
                id = "WalkNearGemFuelMachine", 
                text = "Woah woah woah, why are there so many colors here?",
                priority = 2,
                canBeRepeated = false,
            },
            
            // Desert
            new() { 
                id = "WalkNearCasinoTable", 
                text = "Yeahhh I'm cool off dice for the time being.",
                priority = 2,
                canBeRepeated = false,
            },
            new() { 
                id = "WalkNearCasinoExplosives", 
                text = "Explosives..?",
                priority = 2,
                canBeRepeated = false,
            },
            new() { 
                id = "EnteredDesertTemple", 
                text = "This place is ripe with artifacts!",
                priority = 2,
                canBeRepeated = false,
            },
            new() { 
                id = "EnteredDesertTempleSnail", 
                text = "Groovy!",
                priority = 2,
                canBeRepeated = false,
            },
            new() { 
                id = "WalkedInDinoMouth", 
                text = "Watch out!! Haha just kidding it's already dead.",
                priority = 2,
                canBeRepeated = false,
            },

            // Factory
            new() { 
                id = "WalkNearChadSr", 
                text = "Woah, who's this handsome devil.",
                priority = 2,
                canBeRepeated = false,
            },
            new() { 
                id = "WalkNearPresentMegaCrystal", 
                text = "Didn't they dig this up recently? The Archeologist was going off about it.",
                priority = 2,
                canBeRepeated = false,
            },
            new() { 
                id = "WalkNearPastMegaCrystal", 
                text = "That crystal... it's been here for a thousand years?!",
                priority = 2,
                canBeRepeated = false,
            },

            // Make sure to update "NUMBER_OF_SMALL_TALKS" if you add more
            new() { 
                id = "RandomSmallTalk1", 
                text = "So... how's <var>Cat</var> doing?",
                priority = 1,
                canBeRepeated = false,
            },
            new() { 
                id = "RandomSmallTalk2", 
                text = "Are we there yet?",
                priority = 1,
                canBeRepeated = false,
            },
            new() { 
                id = "RandomSmallTalk3", 
                text = "I wonder what the Archeologist is up to.",
                priority = 1,
                canBeRepeated = false,
            },
        };
    }
}