using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SliderVocalization {
    [CreateAssetMenu(menuName = "Scriptable Objects/Vocalizer Preset")]
    public class VocalizerPreset : ScriptableObject
    {
        public VocalizerParameters Data;
        public static implicit operator VocalizerParameters(VocalizerPreset preset) => preset.Data;
    }

    // Might be better as a class but making it a structs allows built-in copying
    [System.Serializable]
    public struct VocalizerParameters
    {
        public Sound synth;

        [Tooltip("One sound per clause instead of per word / vowel group")]
        public bool isPronouncePerClause;

        [Tooltip("Not syllable if not meant to be read by NPC, for example words on the sign")]
        public bool isPronouncedSyllables;

        [Tooltip("Maximum fadeout time for a sound to clean up itself before being force stopped")]
        public float maximumFadeout;

        [Header("Vowel-level configs")]
        [Range(0.5f, 2f), Tooltip("FMOD param: Pitch")]
        public float pitch;
        [Range(0.05f, 0.25f)]
        public float duration;
        [Range(0, 1), Tooltip("FMOD \"param\": EventInstance.setVolume(0~1)")]
        public float volume;
        [HideInInspector]
        public float volumeAdjustmentDb;
        public VocalizerParametersModifier stressedVowelModifiers;

        [Range(0.1f, 10), Tooltip("Higher value means faster interpolation and less rounded sounds")]
        public float lerpSmoothnessInverted;

        [Header("Word-level configs")]
        [Range(-1f, 1f)]
        public float wordIntonation;
        [Range(-1f, 1f)]
        public float energeticWordSpeedup;
        [Range(0f, 1f), Tooltip("Transition probabilities between word intonations")]
        public float pLowToHigh, pHighToLow;

        [Header("Sentence-level configs")]
        [Range(0.01f, 0.5f)]
        public float clauseGap;
        [Range(-1f, 1f)]
        public float sentenceIntonationUp;
        [Range(-1f, 1f)]
        public float sentenceIntonationDown;

        public bool overrideIntonation;
        public SentenceVocalizer.Intonation intonationOverride;

        public bool DoHighToLow => Random.value < pHighToLow;
        public bool DoLowToHigh => Random.value < pLowToHigh;

        public VocalizerParameters ModifyWith(VocalizerParametersModifier parameters, bool createClone)
        {
            if (createClone)
            {
                var clone = this;
                parameters.ApplyOn(ref clone);
                return clone;
            } else
            {
                parameters.ApplyOn(ref this);
                return this;
            }
        }
    }

    [System.Serializable]
    public struct VocalizerParametersModifier
    {
        [Range(-.25f, .25f)]
        public float pitchAddition;
        [Range(-50f, 20f)]
        public float volumeAdjustmentDb;
        [Range(-.15f, .15f)]
        public float durationAddition;
        public bool overrideIntonation;
        public SentenceVocalizer.Intonation intonationOverride;

        public void ApplyOn(ref VocalizerParameters preset)
        {
            preset.pitch += pitchAddition;
            preset.volumeAdjustmentDb += volumeAdjustmentDb;
            preset.duration += durationAddition;
            if (overrideIntonation)
            {
                preset.overrideIntonation = overrideIntonation;
                preset.intonationOverride = intonationOverride;
            }
        }
    }
}