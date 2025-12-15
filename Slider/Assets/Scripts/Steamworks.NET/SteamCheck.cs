#if !(UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || STEAMWORKS_WIN || STEAMWORKS_LIN_OSX)
#define DISABLESTEAMWORKS
#endif

using UnityEngine;

#if !DISABLESTEAMWORKS
using Steamworks;
#endif

public class SteamCheck : MonoBehaviour
{
    #if !DISABLESTEAMWORKS
    void Start()
    {
        if (!SteamManager.Initialized)
        {
            Debug.LogWarning("[Steam] Steam Manager is Not Initialized! Make sure Steam is running in the background if you want to test Steam-related features!");
        }
        else
        {
            Debug.Log("[Steam] Hello Steam user " + SteamFriends.GetPersonaName());
        }
    }
    #endif
}