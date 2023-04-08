using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Vocalizer Preset")]
public class VocalizerPreset : ScriptableObject
{
    public Sound synth;
    [Header("Vowel-level configs")]
    [Range(0.5f, 2f), Tooltip("FMOD param: Pitch")]
    public float basePitch;
    [Range(0.75f, 1.25f)]
    public float stressedVowelPitchMultiplier;

    [Range(0.05f, 0.25f)]
    public float baseDuration;

    [Range(0, 1), Tooltip("FMOD \"param\": EventInstance.setVolume(0~1)")]
    public float baseVolume;
    [Range(-50, 10f), Tooltip("FMOD param: VolumeAdjustmentDb")]
    public float stressedVowelVolumeAdjustment;

    [Range(0.25f, 2f)]
    public float stressedVowelDurationMultiplier;

    [Range(1, 10), Tooltip("Higher value means faster interpolation and less rounded sounds")]
    public float lerpSmoothnessInverted;

    [Header("Word-level configs")]
    [Range(0.01f, 0.5f)]
    public float wordGap;

    [Header("Sentence-level configs")]
    [Range(0.01f, 0.5f)]
    public float clauseGap;
    [Range(1f, 2f)]
    public float intonationMultiplier;
}
