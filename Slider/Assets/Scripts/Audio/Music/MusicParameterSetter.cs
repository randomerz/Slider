using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Helper script for Unity editor
public class MusicParameterSetter : MonoBehaviour
{
    public string trackName;
    public string parameterName;
    public float defaultValue;

    public void UpdateParameter()
    {
        AudioManager.SetMusicParameter(trackName, parameterName, defaultValue);
    }

    public void UpdateParameterGlobal()
    {
        AudioManager.SetGlobalParameter(parameterName, defaultValue);
    }

    public void UpdateParameter(float value)
    {
        AudioManager.SetMusicParameter(trackName, parameterName, value);
    }

    public void UpdateParameterGlobal(float value)
    {
        AudioManager.SetGlobalParameter(parameterName, value);
    }
}
