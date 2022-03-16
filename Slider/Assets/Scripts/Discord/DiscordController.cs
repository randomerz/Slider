using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiscordController : MonoBehaviour
{
    private const long CLIENT_ID = 953335446056882186;
    private Discord.Discord discord; // This looks hilarious but it's how the SDK works
    private long secondsSinceEpoch; // Used for tracking time elapsed

    // Start is called before the first frame update
    void Start()
    {
        // Going with not requiring Discord seems like the safer option to me.
        // Not entirely sure of the consequences here to be honest
        discord = new Discord.Discord(CLIENT_ID, (ulong)Discord.CreateFlags.NoRequireDiscord);
        InvokeRepeating("UpdateActivity", 0, 5);

        // We need our epoch time for tracking time elapsed
        TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
        secondsSinceEpoch = (int)t.TotalSeconds;
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
            State = $"{SGrid.current.MyArea.GetDisplayName()} ({SGrid.current.GetNumTilesCollected()} / {SGrid.current.GetTotalNumTiles()})",
            Timestamps =
              {
                  Start = secondsSinceEpoch
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
