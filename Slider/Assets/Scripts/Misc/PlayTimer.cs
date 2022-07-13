using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayTimer : MonoBehaviour
{
    private void Update() {
        if(!SaveSystem.IsCurrentProfileNull() && Time.timeScale == 1)
            SaveSystem.Current.AddPlayTimeInSeconds(Time.deltaTime);
    }
}
