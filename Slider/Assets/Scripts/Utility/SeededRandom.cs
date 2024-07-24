using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeededRandom
{
    public static int Range(int min, int max)
    {
        if(SaveSystem.IsCurrentProfileNull())
        {
            Debug.LogWarning("Tried to access seeded random without a save profile");
            return Random.Range(min, max);
        }

        if(!SaveSystem.Current.GetRandomStateInit())
        {
            Random.InitState(SaveSystem.Current.GetProfileName().GetHashCode());            
        }
        else
        {
            Random.state = SaveSystem.Current.GetRandomState();
        }

        int randInt = Random.Range(min, max);

        SaveSystem.Current.SetRandomState(Random.state);
        SaveSystem.Current.SetRandomStateInit(true);

        return randInt;
    }
}
