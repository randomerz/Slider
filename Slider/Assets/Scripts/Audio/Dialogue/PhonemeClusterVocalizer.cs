using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PhonemeClusterVocalizer : IVocalizer
{
    public bool isVowelCluster;
    public bool isStressed = false;
    public string characters;

    public bool IsEmpty => characters.Length == 0;

    private FMOD.Studio.EventInstance inst;

    public int Progress => _progress;
    private int _progress = 0;
    public void ClearProgress() => _progress = 0;

    public IEnumerator Vocalize(VocalizerPreset preset, VocalizationContext context, int idx, int lengthOfComposite)
    {
        ClearProgress();
        var status = AudioManager.Play(preset.synth.WithAttachmentToTransform(context.root), startImmediately: false);
        if (!status.HasValue) yield break;
        inst = status.Value;

        float wordIntonationMultiplier = context.isCurrentWordLow ? (1 - preset.wordIntonation) : (1 + preset.wordIntonation);
        float initialPitch = context.wordPitchBase * wordIntonationMultiplier;
        float finalPitch = context.wordPitchIntonated * wordIntonationMultiplier;
        float duration = preset.baseDuration * (context.isCurrentWordLow ? (1 - preset.energeticWordSpeedup) : (1 + preset.energeticWordSpeedup));
        float volumeAdjustmentDB = 0;

        if (isStressed) {
            finalPitch *= preset.stressedVowelPitchMultiplier;
            duration *= preset.stressedVowelDurationMultiplier;
            volumeAdjustmentDB = preset.stressedVowelVolumeAdjustment;
        }

        inst.setVolume(preset.baseVolume);
        inst.setParameterByName("Pitch", initialPitch);
        inst.setParameterByName("VolumeAdjustmentDB", volumeAdjustmentDB);
        inst.setParameterByName("VowelOpeness", context.vowelOpeness);
        inst.setParameterByName("VowelForwardness", context.vowelForwardness);
        inst.start();

        float totalDuration = duration * characters.Length;
        float totalT = 0f;

        for (int i = 0; i < characters.Length; i++)
        {
            char c = characters[i];
            float t = 0;
            var vowelDescriptor = WordVocalizer.vowelDescriptionTable[c];

            _progress = i + 1;

            while (t < duration)
            {
                inst.setParameterByName("Pitch", Mathf.Lerp(initialPitch, finalPitch, totalT / totalDuration));
                inst.setParameterByName("VowelOpeness", context.vowelOpeness);
                inst.setParameterByName("VowelForwardness", context.vowelForwardness);
                context.vowelOpeness = Mathf.Lerp(context.vowelOpeness, vowelDescriptor.openness, t * preset.lerpSmoothnessInverted);
                context.vowelForwardness = Mathf.Lerp(context.vowelForwardness, vowelDescriptor.forwardness, t * preset.lerpSmoothnessInverted);

                t += Time.deltaTime;
                totalT += Time.deltaTime;
                yield return null;
            }
        }

        Stop();
    }

    public override string ToString()
    {
#if UNITY_EDITOR
        string text = $"<color=green>{ characters.Substring(0, Progress) }</color>{ characters.Substring(Progress) }";
        string
            pre = $"{(isVowelCluster ? "<B>" : "")}{(isStressed ? "<size=16>" : "")}",
            post = $"{(isStressed ? "</size>" : "")}{(isVowelCluster ? "</B>" : "")}";
        return $"{pre}{text}{post}";
#else
        return characters;
#endif
    }

    public void Stop()
    {
        inst.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        inst.release();
    }
}
