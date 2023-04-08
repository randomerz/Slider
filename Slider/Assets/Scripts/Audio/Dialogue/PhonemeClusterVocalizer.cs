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

        float initialPitch = Mathf.Lerp(context.wordPitchBase, context.wordPitchIntonated, (float)idx / lengthOfComposite);
        float finalPitch = Mathf.Lerp(context.wordPitchBase, context.wordPitchIntonated, (float)(idx + 1) / lengthOfComposite);
        if (isStressed) finalPitch *= preset.stressedVowelPitchMultiplier;

        inst.setParameterByName("Pitch", initialPitch);
        inst.setParameterByName("VowelOpeness", context.vowelOpeness);
        inst.setParameterByName("VowelForwardness", context.vowelForwardness);
        inst.start();

        for (int i = 0; i < characters.Length; i++)
        {
            char c = characters[i];
            float t = 0;
            var vowelDescriptor = WordVocalizer.vowelDescriptionTable[c];
            float duration = preset.baseVowelDuration;
            if (isStressed) duration *= preset.stressedVowelDurationMultiplier;

            while (t < duration)
            {
                inst.setParameterByName("Pitch", Mathf.Lerp(initialPitch, finalPitch, t / duration));
                inst.setParameterByName("VowelOpeness", context.vowelOpeness);
                inst.setParameterByName("VowelForwardness", context.vowelForwardness);
                context.vowelOpeness = Mathf.Lerp(context.vowelOpeness, vowelDescriptor.openness, t * preset.lerpSmoothnessInverted);
                context.vowelForwardness = Mathf.Lerp(context.vowelForwardness, vowelDescriptor.forwardness, t * preset.lerpSmoothnessInverted);

                t += Time.deltaTime;
                yield return null;
            }
        }

        inst.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }
}
