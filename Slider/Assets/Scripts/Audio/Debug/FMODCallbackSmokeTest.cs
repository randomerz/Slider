using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FMODCallbackSmokeTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private static string[] keys =
    {
        "a", "aewifaew", "q3sz", "zzzzz"
    };

    // Update is called once per frame
    void Update()
    {
        AudioManager.PickSound("Village Pick Up").WithSingleInstanceKey(keys[Random.Range(0, keys.Length)]).AndPlay();
    }
}
