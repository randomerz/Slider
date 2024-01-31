using FMOD.Studio;
using System.Collections;
using System.Text;
using UnityEngine;

namespace SliderVocalization
{
    /// <summary>
    /// Intermediate class to allow uniform interface of the Phoneme and Punctuation vocalizers
    /// </summary>
    public abstract class BaseVocalizer : IVocalizer
    {
        public StringBuilder characters;
        public bool IsEmpty => characters.Length == 0;
        public int Progress => _progress;
        protected int _progress = 0;
        public void ClearProgress() => _progress = 0;

        public abstract float RandomizeVocalization(VocalizerParameters preset, VocalRandomizationContext context);
        public abstract void Stop();
        public abstract IEnumerator Vocalize(VocalizerParameters preset, VocalizationContext context, int idx = 0, int lengthOfComposite = 1);
    }

    public class PauseVocalizer : BaseVocalizer
    {

        public override float RandomizeVocalization(VocalizerParameters preset, VocalRandomizationContext context) => preset.clauseGap;
        public override IEnumerator Vocalize(VocalizerParameters preset, VocalizationContext context, int idx = 0, int lengthOfComposite = 1)
        {
            if (float.IsNormal(preset.clauseGap) && !float.IsNegative(preset.clauseGap))
                yield return new WaitForSeconds(preset.clauseGap);
        }

        public override void Stop() { }

        public override string ToString()
        {
#if UNITY_EDITOR
            return $"<color=magenta>|</color>";
#else
            return "";
#endif
        }
    }

    public class PunctuationVocalizer : BaseVocalizer
    {
        public override float RandomizeVocalization(VocalizerParameters preset, VocalRandomizationContext context) 
            => preset.duration * characters.Length;

        public override IEnumerator Vocalize(VocalizerParameters preset, VocalizationContext context, int idx = 0, int lengthOfComposite = 1)
        {
            for (int i = 0; i < characters.Length; i++)
            {
                _progress = i + 1;
                if (float.IsNormal(preset.duration) && !float.IsNegative(preset.duration))
                    yield return new WaitForSeconds(preset.duration);
            }
        }

        public override void Stop() { }

        public override string ToString()
        {
#if UNITY_EDITOR
            string str = characters.ToString();
            return $"<color=cyan>{str.Substring(0, Progress)}</color>{str.Substring(Progress)}";
#else
            return characters.ToString();
#endif
        }
    }

    public class PhonemeClusterVocalizer : BaseVocalizer
    {
        public bool isVowelCluster;
        public bool isStressed = false;
        #region RANDOMIZED PARAMS
        float duration;
        float totalDuration;
        float wordIntonationMultiplier;
        float initialPitch;
        float middlePitch;
        float finalPitch;
        float volumeAdjustmentDB;
        #endregion
        AudioManager.ManagedInstance playingInstance;

        public override float RandomizeVocalization(VocalizerParameters preset, VocalRandomizationContext context)
        {
            duration = preset.duration * (context.isCurrentWordLow ? (1 - preset.energeticWordSpeedup) : (1 + preset.energeticWordSpeedup));
            totalDuration = duration * characters.Length;
            wordIntonationMultiplier = context.isCurrentWordLow ? (1 - preset.wordIntonation) : (1 + preset.wordIntonation);
            initialPitch = context.lastWordFinalPitch;
            middlePitch = context.wordPitchIntonated * wordIntonationMultiplier * (1 + (preset.isPronouncedSyllables ? 0f : (Random.value - 0.5f) * 0.1f));
            finalPitch = context.wordPitchBase * wordIntonationMultiplier * (1 + (preset.isPronouncedSyllables ? 0f : (Random.value - 0.5f) * 0.1f));
            context.lastWordFinalPitch = finalPitch;
            volumeAdjustmentDB = preset.volumeAdjustmentDb;
            return totalDuration;
        }

        public override IEnumerator Vocalize(VocalizerParameters preset, VocalizationContext context, int idx, int lengthOfComposite)
        {
            ClearProgress();
            if (isStressed) preset.ModifyWith(preset.stressedVowelModifiers, createClone: false);

            float totalT = 0f;

            SoundWrapper wrapper = preset.synth
                .WithAttachmentToTransform(context.root)
                .WithFixedDuration(totalDuration)
                .WithVolume(preset.volume)
                .WithParameter("Pitch", initialPitch)
                .WithParameter("VolumeAdjustmentDB", volumeAdjustmentDB)
                .WithParameter("VowelOpeness", context.vowelOpenness)
                .WithPriorityOverDucking(true)
                .WithParameter("VowelForwardness", context.vowelForwardness);

            wrapper.dialogueParent = context.topLevelParent;

            playingInstance = AudioManager.Play(
                ref wrapper
            );

            if (playingInstance == null) yield break;

            for (int i = 0; i < characters.Length; i++)
            {
                char c = characters[i];
                float t = 0;
                var vowelDescriptor = WordVocalizer.vowelDescriptionTable[c];
                
                _progress = i + 1;
                while (t < duration)
                {
                    playingInstance.Tick(delegate (ref EventInstance inst)
                    {
                        float overallT = (totalT / totalDuration);
                        inst.setParameterByName("Pitch", 
                            overallT < 0.5f ? 
                                Mathf.Lerp(initialPitch, middlePitch, overallT * 2) 
                                : Mathf.Lerp(middlePitch, finalPitch, (overallT - 0.5f) * 2));
                        inst.setParameterByName("VowelOpeness", context.vowelOpenness);
                        inst.setParameterByName("VowelForwardness", context.vowelForwardness);
                    });
                    if (preset.isPronouncedSyllables)
                    {
                        context.vowelOpenness = Mathf.Lerp(context.vowelOpenness, vowelDescriptor.openness, t * preset.lerpSmoothnessInverted);
                        context.vowelForwardness = Mathf.Lerp(context.vowelForwardness, vowelDescriptor.forwardness, t * preset.lerpSmoothnessInverted);
                    } else
                    {
                        context.vowelOpenness = 0.5f;
                        context.vowelForwardness = 0.5f;
                    }
                    
                    t += Time.deltaTime;
                    totalT += Time.deltaTime;
                    yield return new WaitForFixedUpdate();
                }
            }
            Stop();
        }
        public override string ToString()
        {
#if UNITY_EDITOR
            string str = characters.ToString();
            string text = $"<color=green>{str.Substring(0, Progress)}</color>{str.Substring(Progress)}";
            string pre = $"{(isVowelCluster ? "<B>" : "")}{(isStressed ? "<size=16>" : "")}";
            string post = $"{(isStressed ? "</size>" : "")}{(isVowelCluster ? "</B>" : "")}";
            return $"{pre}{text}{post}";
#else
            return characters.ToString();
#endif
        }

        public override void Stop()
        {
            // AT: commenting this out prevents stopping mid-word even if a stop signal is issued
            // TODO: please uncomment if stopping mid-word is intended!
            
            // if (playingInstance != null)
            // {
            //     playingInstance.Stop();
            // }
        }
    }
}