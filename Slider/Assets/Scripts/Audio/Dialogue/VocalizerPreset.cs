using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Vocalizer Preset")]
public class VocalizerPreset : ScriptableObject
{
    public Sound synth;
    [Range(0.5f, 2f)]
    public float basePitch;
    [Range(0.1f, 0.5f)]
    public float secondsBetweenWords;

    public AnimationCurve intonationUp, intonationDown;

    public AnimationCurve GetIntonation(VocalIntonation key)
    {
        switch (key)
        {
            case VocalIntonation.Up: return intonationUp;
            case VocalIntonation.Down: return intonationDown;
            default: return null;
        }
    }

    public enum VocalIntonation
    {
        None,
        Up,
        Down
    }
}
