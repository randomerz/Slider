using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Vocalizer Preset")]
public class VocalizerPreset : ScriptableObject
{
    public FMODUnity.EventReference synth;
    [Range(0.5f, 2f)]
    public float basePitch;
    [Range(0.1f, 0.5f)]
    public float secondsBetweenWords;

    public AnimationCurve intonationUp, intonationDown, intonationFlat;

    public AnimationCurve GetIntonation(VocalIntonation key)
    {
        switch (key)
        {
            case VocalIntonation.Up: return intonationUp;
            case VocalIntonation.Down: return intonationDown;
            default: return intonationFlat;
        }
    }

    public enum VocalIntonation
    {
        Flat,
        Up,
        Down
    }
}
