using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class is for saving data about the world, like the fish particles in the village river. This way
// when we leave the scene and come back, we can save whether the particles should render or not.
public class WorldData
{

    // I don't like saving them as strings, but I dont want a mega enum and i couldnt think of anything else
    private static string[] validStates = new string[] {
        "fishOn",
    };

    // maybe instead of a dictionary, we can just have a second array of length len(validStates) so serialization is easier
    private static Dictionary<string, bool> savedStates = new Dictionary<string, bool>();

    private static bool IsStateValid(string state)
    {
        foreach (string s in validStates)
        {
            if (state == s)
                return true;
        }
        return false;
    }

    public static bool GetState(string state)
    {
        if (!IsStateValid(state))
        {
            Debug.LogWarning("World state " + state + " is not a valid state!");
            return false;
        }

        if (!savedStates.ContainsKey(state))
        {
            // check the save file

            return false;
        }

        return savedStates[state];
    }

    public static void SetState(string state, bool value)
    {
        if (!IsStateValid(state))
        {
            Debug.LogWarning("World state " + state + " is not a valid state!");
            return;
        }

        savedStates[state] = value;
    }
}
