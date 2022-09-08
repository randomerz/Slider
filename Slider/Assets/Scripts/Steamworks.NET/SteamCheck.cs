using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteamCheck : MonoBehaviour
{
    // Start is called before the first frame update
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

}