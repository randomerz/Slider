using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RichPresence : Singleton<RichPresence>
{
    List<IPresenceProxy> proxies = new();
    
    void Awake()
    {
        InitializeSingleton(ifInstanceAlreadySetThenDestroy:this);

        proxies.Add(new DiscordController());
#if MICROSOFT_GDK_SUPPORT
        proxies.Add(new GDKPresenceController());
#endif
#if !DISABLESTEAMWORKS
        proxies.Add(new SteamPresenceController());
#endif
    }

    private void Start() 
    {
        foreach (var proxy in proxies)
        {
            proxy.Start();
        }

        // Update activity status whenever a slider is collected or the scene is changed
        SGrid.OnSTileCollected += (object sender, SGrid.OnSTileEnabledArgs args) => UpdateActivity();
        SceneManager.sceneLoaded += (Scene scene, LoadSceneMode mode) => UpdateActivity();

        // UpdateActivity();            
        
        //Debug.Log("Starting Rich Presence");
    }

    private void Update() 
    {
        foreach (var proxy in proxies)
        {
            proxy.Update();
        }
    }

    private void OnApplicationQuit() 
    {
        foreach (var proxy in proxies)
        {
            proxy.OnApplicationQuit();
        }
    }

    private void UpdateActivity()
    {
        foreach (var proxy in proxies)
        {
            proxy.UpdateActivity();
        }
    }




    // Ex. "Exploring the Village (4/9)"
    public static string GetDetailedPrecenseString()
    {
        // "In the menus"
        string state = LocalizationLoader.LoadInTheMenusDiscordTranslation();
        if (SGrid.Current != null)
        {
            state = $"{SGrid.Current.MyArea.GetDiscordName()} ({SGrid.Current.GetNumTilesCollected()} / {SGrid.Current.GetTotalNumTiles()})";
        }
        return state;
    }

    // Ex. "Exploring the Village"
    public static string GetBasicPrecenseString()
    {
        // "In the menus"
        string state = LocalizationLoader.LoadInTheMenusDiscordTranslation();
        if (SGrid.Current != null)
        {
            state = $"{SGrid.Current.MyArea.GetDiscordName()}";
        }
        return state;
    }
}