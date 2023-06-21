using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySound : MonoBehaviour
{
    public string soundName;

    public void Play()
    {
        AudioManager.PickSound(soundName).AndPlay();
    }

    public void Play(string s)
    {
        AudioManager.Play(s);
    }
}
