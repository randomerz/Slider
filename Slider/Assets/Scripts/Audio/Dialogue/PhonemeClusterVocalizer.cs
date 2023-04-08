using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

[System.Serializable]
public class PhonemeClusterVocalizer : IVocalizer
{
    public bool isVowelCluster;
    public bool isStressed = false;
    public string characters;

    public bool IsEmpty => characters.Length > 0;

#if UNITY_EDITOR
    public int Progress => _progress;
    private int _progress = 0;
    public void ClearProgress() => _progress = 0;
#endif

    public IEnumerator Vocalize(VocalizerPreset preset, VocalizationContext context, int idx, int lengthOfComposite)
    {
#if UNITY_EDITOR
        _progress = 0;
#endif
        var status = AudioManager.Play(preset.synth.WithAttachmentToTransform(context.root), startImmediately: false);
        if (!status.HasValue) yield break;
        var inst = status.Value;

        float initialPitch = Mathf.Lerp(context.wordPitchBase, context.wordPitchIntonated, (float)idx / lengthOfComposite);
        float finalPitch = Mathf.Lerp(context.wordPitchBase, context.wordPitchIntonated, (float)(idx + 1) / lengthOfComposite);
        float duration = preset.baseDuration;
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

        for (int i = 0; i < characters.Length; i++)
        {
            char c = characters[i];
            float t = 0;
            var vowelDescriptor = WordVocalizer.vowelDescriptionTable[c];

#if UNITY_EDITOR
            _progress = i + 1;
#endif

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
        inst.release();
    }

    public override string ToString()
    {
#if UNITY_EDITOR
        string text = $"<color=green>{ characters.Substring(0, Progress) }</color>{ characters.Substring(Progress) }";
#else
        string text = characters;
#endif
        string 
            pre = $"{( isVowelCluster ? "<B>" : "" )}{( isStressed ? "<size=16>" : "" )}",
            post = $"{(isStressed ? "</size>" : "")}{(isVowelCluster ? "</B>" : "")}";
        return $"{pre}{text}{post}";
    }
}
