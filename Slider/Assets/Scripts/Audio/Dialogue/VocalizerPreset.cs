using Steamworks;
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

    [Range(0.1f, 0.5f)]
    public float secondsBetweenSentences;

    [Range(1f, 2f)]
    public float intonationMultiplier;
    [Range(0.01f, 0.2f)]
    public float baseVowelDuration;

    [Range(1, 10)]
    public float lerpSmoothnessInverted;

    [Range(0.75f, 1.25f)]
    public float stressedVowelPitchMultiplier;

    [Range(0.25f, 2f)]
    public float stressedVowelDurationMultiplier;
}
