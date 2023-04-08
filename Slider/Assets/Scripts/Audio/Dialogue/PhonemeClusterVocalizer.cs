using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PhonemeClusterVocalizer : IVocalizer
{
    public bool isVowelCluster;
    public bool isStressed = false;
    public string characters;

    public bool IsEmpty => characters.Length > 0;

    public IEnumerator Vocalize(VocalizerPreset preset, VocalizationContext context, int idx, int lengthOfComposite)
    {
        var status = AudioManager.Play(preset.synth.WithAttachmentToTransform(context.root), startImmediately: false);
        if (!status.HasValue) yield break;
        var inst = status.Value;

        float initialPitch = context.wordPitchBase + (isStressed ? 0.25f : 0);
        inst.setParameterByName("Pitch", initialPitch);

        inst.setParameterByName("VowelOpeness", context.vowelOpeness);
        inst.setParameterByName("VowelForwardness", context.vowelForwardness);
        inst.start();

        for (int i = 0; i < characters.Length; i++)
        {
            char c = characters[i];
            float t = 0, tGranulatiryCounter = 0;
            var vowelDescriptor = WordVocalizer.vowelDescriptionTable[c];
            float duration = preset.baseVowelDuration;

            while (t < duration)
            {
                if (tGranulatiryCounter > preset.phonemeGranularitySeconds)
                {
                    inst.setParameterByName("VowelOpeness", context.vowelOpeness);
                    inst.setParameterByName("VowelForwardness", context.vowelForwardness);
                    context.vowelOpeness = Mathf.Lerp(context.vowelOpeness, vowelDescriptor.openness, tGranulatiryCounter * 5);
                    context.vowelForwardness = Mathf.Lerp(context.vowelForwardness, vowelDescriptor.forwardness, tGranulatiryCounter * 5);

                    tGranulatiryCounter %= preset.phonemeGranularitySeconds;
                }

                t += Time.deltaTime;
                tGranulatiryCounter += Time.deltaTime;
                yield return null;
            }
        }

        inst.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }
}
