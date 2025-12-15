using System;

public class XBoxAchievementData
{
    public static string GetIdFromName(string name)
    {
        return name switch
        {
            "slidersCollected" => "1",
            "completedVillage" => "2",
            "collectFirstBreadge" => "3",
            "completedVillageSpeedrun" => "4",
            "completedCaves" => "5",
            "completedOcean" => "6",
            "completedJungle" => "7",
            "completedDesert" => "8",
            "completedFactory" => "9",
            "completedMilitary" => "10",
            "completedMountain" => "11",
            "completedMagitech" => "12",
            "collectedAnchor" => "13",
            "collectedBoots" => "14",
            "collectedScroll" => "15",
            "savedCat" => "16",
            "completedOceanMinAnchor" => "17",
            "completedAllJungleRecipes" => "18",
            "sparedChadSr" => "19",
            "threeBreadge" => "20",
            "allbreadge" => "21",
            "completedGame1Hour" => "22",
            "completedBigMoss" => "23",
            "mountainMinMinecart" => "24",
            "militaryExtraAlly" => "25",
            "jungleAllTiles" => "26",
            "desertKilledBird" => "27",
            "magitechEarlyGem" => "28",
            "completedGame2Hours" => "29",
            "completedRatRaceV2" => "30",
            _ => throw new ArgumentException($"Unknown achievement: {name}")
        };
    }
}