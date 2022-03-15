using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiscordController : MonoBehaviour
{
    private const long CLIENT_ID = 953335446056882186;
    private Discord.Discord discord;

    // Start is called before the first frame update
    void Start()
    {
        /*
            Grab that Client ID from earlier
            Discord.CreateFlags.Default will require Discord to be running for the game to work
            If Discord is not running, it will:
            1. Close your game
            2. Open Discord
            3. Attempt to re-open your game
            Step 3 will fail when running directly from the Unity editor
            Therefore, always keep Discord running during tests, or use Discord.CreateFlags.NoRequireDiscord
        */
        discord = new Discord.Discord(CLIENT_ID, (ulong)Discord.CreateFlags.Default);
    }

    // Update is called once per frame
    void Update()
    {
        var activityManager = discord.GetActivityManager();

        var activity = new Discord.Activity
        {
            State = $"Exploring {SGrid.current.MyArea}",
            Details = "Playing the Trumpet!",
            Timestamps =
              {
                  Start = 5,
              },
            Assets =
              {
                  LargeImage = "foo largeImageKey", // Larger Image Asset Value
                  LargeText = "foo largeImageText", // Large Image Tooltip
                  SmallImage = "foo smallImageKey", // Small Image Asset Value
                  SmallText = "foo smallImageText", // Small Image Tooltip
              },
            Party =
              {
                  Id = "foo partyID",
                  Size = {
                      CurrentSize = 1,
                      MaxSize = 4,
                  },
              },
            Secrets =
              {
                  Match = "foo matchSecret",
                  Join = "foo joinSecret",
                  Spectate = "foo spectateSecret",
              },
            Instance = true,
        };

        activityManager.UpdateActivity(activity, (result) =>
        {
            if (result == Discord.Result.Ok)
            {
                Debug.Log("Success!");
            }
            else
            {
                Debug.Log("Failed");
            }
        });

        discord.RunCallbacks();
    }
}
