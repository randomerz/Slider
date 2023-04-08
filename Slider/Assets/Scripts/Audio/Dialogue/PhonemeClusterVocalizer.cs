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
        var status = AudioManager.Play(preset.synth.WithAttachmentToTransform(context.root));

        if (status.HasValue)
        {
            var inst = status.Value;
            float t = 0;
            while (t < 0.1f) {
                t += Time.deltaTime;
                yield return null;
            }
            inst.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        }
    }
}
