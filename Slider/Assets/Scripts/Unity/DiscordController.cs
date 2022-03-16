using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiscordController : MonoBehaviour
{
    private const long CLIENT_ID = 953335446056882186;
    private Discord.Discord discord;
    private TimeSpan startTime;

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
        InvokeRepeating("UpdateActivity", 0, 5);
        startTime = DateTime.Now.TimeOfDay;
    }

    // Update is called once per frame
    void Update()
    {
        discord.RunCallbacks();
    }

    void UpdateActivity()
    {
        var activityManager = discord.GetActivityManager();

        var activity = new Discord.Activity
        {
            // Remember to adjust this for mountain and any other weirdChamp areas
            State = $"Exploring {SGrid.current.MyArea} ({SGrid.current.GetNumTilesCollected()} / {SGrid.current.width * SGrid.current.height})",
            Timestamps =
              {
                  Start = (long) startTime.TotalSeconds
              },
            Assets =
              {
                  LargeImage = "foo largeImageKey", // Larger Image Asset Value
                  LargeText = "foo largeImageText", // Large Image Tooltip
                  SmallImage = "foo smallImageKey", // Small Image Asset Value
                  SmallText = "foo smallImageText", // Small Image Tooltip
              },
            Secrets =
              {
                  Match = "foo matchSecret",
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
    }

    private void OnApplicationQuit()
    {
        discord.Dispose(); // Stops rich presence when the game closes
    }
}
