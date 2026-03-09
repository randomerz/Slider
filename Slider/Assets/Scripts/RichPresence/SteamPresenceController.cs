using UnityEngine;
#if !DISABLESTEAMWORKS
using Steamworks;
#endif

public class SteamPresenceController : IPresenceProxy 
{
    public void Start() {}
    public void Update() {}
    public void OnApplicationQuit() {}

    public void UpdateActivity()
    {
#if !DISABLESTEAMWORKS

        if (!SteamManager.Initialized)
        {
            return;
        }
        
        if (SGrid.Current != null)
        {
            if (!Steamworks.SteamFriends.SetRichPresence("steam_display", $"#{SGrid.Current.MyArea}"))
            {
                Debug.LogError("[Steam] Failed to set Steam Rich Presence");
            }
            if (!Steamworks.SteamFriends.SetRichPresence("SLIDERS", SGrid.Current.GetNumTilesCollected().ToString()))
            {
                Debug.LogError("[Steam] Failed to set Steam Rich Presence");
            }
        }
        else
        {
            if (!Steamworks.SteamFriends.SetRichPresence("steam_display", "#Menus"))
            {
                Debug.LogError("[Steam] Failed to set Steam Rich Presence");
            }
        }
#endif
    }
}