using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SliderVocalization
{
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

        public IEnumerator Vocalize(VocalizerParameters parameters, VocalizationContext context, int idx, int lengthOfComposite)
        {
            ClearProgress();
            var status = AudioManager.Play(parameters.synth.WithAttachmentToTransform(context.root), startImmediately: false);
            if (!status.HasValue) yield break;
            inst = status.Value;

            if (isStressed) parameters.ModifyWith(parameters.stressedVowelModifiers, createClone: false);

            float wordIntonationMultiplier = context.isCurrentWordLow ? (1 - parameters.wordIntonation) : (1 + parameters.wordIntonation);
            float initialPitch = context.wordPitchBase * wordIntonationMultiplier;
            float finalPitch = context.wordPitchIntonated * wordIntonationMultiplier;
            float duration = parameters.duration * (context.isCurrentWordLow ? (1 - parameters.energeticWordSpeedup) : (1 + parameters.energeticWordSpeedup));
            float volumeAdjustmentDB = 0;

            inst.setVolume(parameters.volume);
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
                    context.vowelOpeness = Mathf.Lerp(context.vowelOpeness, vowelDescriptor.openness, t * parameters.lerpSmoothnessInverted);
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

        public void Stop()
        {
            inst.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            inst.release();
        }
    }
}