using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles Discord Rich Presence. Should probably be attached to GameManager or
/// some other object which exists at game start and persists through scenes.
/// </summary>
public class DiscordController : MonoBehaviour
{
    private const long CLIENT_ID = 953335446056882186;
    private Discord.Discord discord; // This looks hilarious but it's how the SDK works
    private long secondsSinceEpoch; // Used for tracking time elapsed

    void Start()
    {
        if (discord == null)
        {
            // Going with not requiring Discord seems like the safer option to me.
            // Not entirely sure of the consequences here to be honest
            discord = new Discord.Discord(CLIENT_ID, (ulong)Discord.CreateFlags.NoRequireDiscord);
            //InvokeRepeating("UpdateActivity", 0, 5);

            // We need our epoch time for tracking time elapsed
            TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
            secondsSinceEpoch = (int)t.TotalSeconds;

            // Update activity status whenever a slider is collected or the scene is changed
            SGrid.OnSTileCollected += (object sender, SGrid.OnSTileCollectedArgs args) => UpdateActivity();
            SceneManager.sceneLoaded += (Scene scene, LoadSceneMode mode) => UpdateActivity();

            UpdateActivity();

            Debug.Log("Starting Rich Presence");
        }
    }

    void Update()
    {
        // Not entirely sure what this does, but apparently it's important
        discord?.RunCallbacks();
    }

    /// <summary>
    /// Call this whenever we want to update the rich presence status.
    /// Currently that's only when the player picks up a slider or changes scenes.
    /// </summary>
    void UpdateActivity()
    {
        var activityManager = discord.GetActivityManager();

        var state = "At the Start Screen";
        if (SGrid.current != null)
        {
            state = $"{SGrid.current.MyArea.GetDisplayName()} ({SGrid.current.GetNumTilesCollected()} / {SGrid.current.GetTotalNumTiles()})";
        }
        var activity = new Discord.Activity
        {
            State = state,
            Timestamps =
            {
                // You give Discord an Epoch time in seconds and it displays the time elapsed since then
                Start = secondsSinceEpoch
            },
            Instance = true,
        };

        activityManager.UpdateActivity(activity, (result) => { });
    }

    private void OnApplicationQuit()
    {
        discord?.Dispose(); // Stops rich presence when the game closes
    }
}
