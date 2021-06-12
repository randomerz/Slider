using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Sound
{
    public string name;

    public AudioClip clip;

    [Range(0f, 1f)]
    public float volume = 1;
    [Range(0.1f, 3f)]
    public float pitch = 1;

    public bool loop;
    public bool doRandomPitch = true;

    [HideInInspector]
    public AudioSource source;
}