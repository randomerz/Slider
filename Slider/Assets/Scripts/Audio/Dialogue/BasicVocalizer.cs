using FMOD.Studio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SliderVocalization
{
    /// <summary>
    /// Intermediate class to allow uniform interface of the Phoneme and Punctuation vocalizers
    /// </summary>
    public abstract class BaseVocalizer : IVocalizer
    {
        public string characters;
        public bool IsEmpty => characters.Length == 0;

        public int Progress => _progress;
        protected int _progress = 0;
        public void ClearProgress() => _progress = 0;

        public abstract float RandomizeVocalization(VocalizerParameters parameters, VocalRandomizationContext context);
        public abstract void Stop();
        public abstract IEnumerator Vocalize(VocalizerParameters parameters, VocalizationContext context, int idx = 0, int lengthOfComposite = 1);

    }

    public class PauseVocalizer : BaseVocalizer
    {
        public override string ToString()
        {
#if UNITY_EDITOR
            return $"<color=magenta>|</color>";
#else
            return "";
#endif
        }

        public override float RandomizeVocalization(VocalizerParameters parameters, VocalRandomizationContext context) => parameters.clauseGap;
        public override void Stop() { }

        public override IEnumerator Vocalize(VocalizerParameters parameters, VocalizationContext context, int idx = 0, int lengthOfComposite = 1)
        {
            yield return new WaitForSeconds(parameters.clauseGap);
        }
    }

    public class PunctuationVocalizer : BaseVocalizer
    {
        public override float RandomizeVocalization(VocalizerParameters parameters, VocalRandomizationContext context) 
            => parameters.duration * characters.Length;

        public override void Stop() { }

        public override IEnumerator Vocalize(VocalizerParameters parameters, VocalizationContext context, int idx = 0, int lengthOfComposite = 1)
        {
            for (int i = 0; i < characters.Length; i++)
            {
                _progress = i + 1;
                yield return new WaitForSeconds(parameters.duration);
            }
        }

        public override string ToString()
        {
#if UNITY_EDITOR
            return $"<color=cyan>{characters.Substring(0, Progress)}</color>{characters.Substring(Progress)}";
#else
            return characters;
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
        float finalPitch;
        float volumeAdjustmentDB;
        #endregion

        AudioManager.ManagedInstance playingInstance;

        public override float RandomizeVocalization(VocalizerParameters parameters, VocalRandomizationContext context)
        {
            duration = parameters.duration * (context.isCurrentWordLow ? (1 - parameters.energeticWordSpeedup) : (1 + parameters.energeticWordSpeedup));
            totalDuration = duration * characters.Length;
            wordIntonationMultiplier = context.isCurrentWordLow ? (1 - parameters.wordIntonation) : (1 + parameters.wordIntonation);
            initialPitch = context.wordPitchBase * wordIntonationMultiplier * (1 + (Random.value - 0.5f) * 0.1f);
            finalPitch = context.wordPitchIntonated * wordIntonationMultiplier;
            volumeAdjustmentDB = parameters.volumeAdjustmentDb;
            return totalDuration;
        }

        public override IEnumerator Vocalize(VocalizerParameters parameters, VocalizationContext context, int idx, int lengthOfComposite)
        {
            ClearProgress();
            if (isStressed) parameters.ModifyWith(parameters.stressedVowelModifiers, createClone: false);

            float totalT = 0f;
            playingInstance = AudioManager.Play(parameters.synth
                .WithAttachmentToTransform(context.root)
                .WithFixedDuration(totalDuration)
                .WithVolume(parameters.volume)
                .WithParameter("Pitch", initialPitch)
                .WithParameter("VolumeAdjustmentDB", volumeAdjustmentDB)
                .WithParameter("VowelOpeness", context.vowelOpenness)
                .WithParameter("VowelForwardness", context.vowelForwardness)
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
                        float t = (totalT / totalDuration);
                        inst.setParameterByName("Pitch", Mathf.Lerp(initialPitch, finalPitch, t));
                        inst.setParameterByName("VowelOpeness", context.vowelOpenness);
                        inst.setParameterByName("VowelForwardness", context.vowelForwardness);
                    });
                    context.vowelOpenness = Mathf.Lerp(context.vowelOpenness, vowelDescriptor.openness, t * parameters.lerpSmoothnessInverted);
                    context.vowelForwardness = Mathf.Lerp(context.vowelForwardness, vowelDescriptor.forwardness, t * parameters.lerpSmoothnessInverted);
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
            string text = $"<color=green>{characters.Substring(0, Progress)}</color>{characters.Substring(Progress)}";
            string pre = $"{(isVowelCluster ? "<B>" : "")}{(isStressed ? "<size=16>" : "")}";
            string post = $"{(isStressed ? "</size>" : "")}{(isVowelCluster ? "</B>" : "")}";
            return $"{pre}{text}{post}";
#else
            return characters;
#endif
        }


        public override void Stop()
        {
            playingInstance?.Stop();
        }
    }
}