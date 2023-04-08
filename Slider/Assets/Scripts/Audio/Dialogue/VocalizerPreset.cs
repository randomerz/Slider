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

    /// <summary>
    /// The PhonemeClusterVocalizer will try to smooth things out
    /// This granularity makes sure the smoothing only applies once X seconds
    /// Setting this to 0 or negative means completely continuous
    /// </summary>
    [Range(0f, 0.1f)]
    public float phonemeGranularitySeconds;

    [Range(1f, 2f)]
    public float intonationMultiplier;
    [Range(0.01f, 0.2f)]
    public float baseVowelDuration;
}
