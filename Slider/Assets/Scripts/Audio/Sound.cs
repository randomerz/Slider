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
    public int dopplerScale = 0;
}