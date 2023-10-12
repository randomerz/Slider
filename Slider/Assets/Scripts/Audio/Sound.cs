using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using System.Runtime.CompilerServices;

[System.Serializable]
public class Sound
{
    public string name;
    public EventReference fmodEvent;
    public bool canPause = true;

    [Range(1f, 10f)]
    [Tooltip("Only set if sound requires more than 5x of scale, otherwise configure inside FMOD.\nNote this is multiplied with the FMOD doppler scale.")]
    public float dopplerScale = 0;
}