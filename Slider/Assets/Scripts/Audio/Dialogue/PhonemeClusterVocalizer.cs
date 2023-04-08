using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PhonemeClusterVocalizer : IVocalizer
{
    public bool isVowelCluster;
    public bool isStressed = false;
    public string characters;

    public bool Vocalizable => characters.Length > 0;

    public IEnumerator Vocalize(VocalizerPreset preset, VocalizationContext context)
    {
        throw new System.NotImplementedException();
    }
}
