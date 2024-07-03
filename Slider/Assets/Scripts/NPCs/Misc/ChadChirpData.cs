using System.Collections.Generic;

public partial class ChadChirp
{
    public class ChadChirpData
    {
        public string id;
        public string text;
        public int priority; // 2 for "CanChirps", 1 for "WantChirps"
        public bool canBeRepeated; // If can be repeated, then priority also goes from 2 -> 1
        public bool hasBeenUsed;

        public static string GetChirpUsedSaveString(ChadChirpData data) => $"MiscChadChirpUsed_{data.id}";

        // TODO: Make chad less annoying, more optimistic/interested, and more funny
        public static readonly List<ChadChirpData> chirpDataList = new() {
            // Areas
            new() { 
                id = "CameFromJungle", 
                text = "Sorry... I'm not a big fan of the people in Canopy Town.",
                priority = 2,
                canBeRepeated = true,
            },
            new() { 
                id = "CameFromMilitary", 
                text = "War? Yeah, I'm a pro at RPS Tactics what about it?",
                priority = 2,
                canBeRepeated = true,
            },
            new() { 
                id = "ArrivedInDesert", 
                text = "Oh... what are we doing here?",
                priority = 2,
                canBeRepeated = true,
            },
            new() { 
                id = "ArrivedInFactoryPresent", 
                text = "Hey isn't this area closed to the public?",
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
                text = "They've really got some crazy ideas here.",
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
                text = "aHh... Sorry that caught me off guard...",
                priority = 1,
                canBeRepeated = false,
            },
            new() { 
                id = "WentThroughPortal", 
                text = "I'll never get used to that...",
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
                text = "That laser is so... big...",
                priority = 1,
                canBeRepeated = false,
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
                text = "The museum? It could use some more artifacts I suppose...",
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
                text = "The location of our final duel...",
                priority = 2,
                canBeRepeated = false,
            },
            new() { 
                id = "WalkNearCasinoExplosives", 
                text = "Wait... what are you going to do with those explosives..?",
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
                text = "Hey, that crystal was in that paper the Archeologist was reading!",
                priority = 2,
                canBeRepeated = false,
            },
            new() { 
                id = "WalkNearPastMegaCrystal", 
                text = "That crystal... it's stayed intact for a thousand years?!",
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