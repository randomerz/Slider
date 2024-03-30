using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SliderVocalization
{
    public class WordVocalizer : IVocalizerComposite<BaseVocalizer>
    {
        private static readonly char[] vowels = { 'a', 'e', 'i', 'o', 'u', 'y' };
        public static readonly Dictionary<char, VowelDescription> vowelDescriptionTable = new()
        {
            // Values estimated from IPA chart https://en.wikipedia.org/wiki/Table_of_vowels
            ['a'] = new VowelDescription() { openness = 1.0f, forwardness = 0.6f },
            ['e'] = new VowelDescription() { openness = 0.3f, forwardness = 0.8f },
            ['i'] = new VowelDescription() { openness = 0.0f, forwardness = 1.0f },
            ['o'] = new VowelDescription() { openness = 0.3f, forwardness = 0.5f },
            ['u'] = new VowelDescription() { openness = 0.0f, forwardness = 0.0f },
            ['y'] = new VowelDescription() { openness = 0.0f, forwardness = 1.0f }
        };
        private static char RandomVowel => vowels[(int)(Random.value * vowels.Length)];

        public bool IsEmpty => m_Vocalizers == null || m_Vocalizers.Count == 0;

        /// <summary>
        /// Return vowelClusters for reading every single vowel in word
        /// Return significantCluster for reading only the stressed vowel groups (or the only vowel if no stressed vowels)
        ///   - For most of the time there is only one stress in a word, but for very long words there might be multiple
        /// </summary>
        public List<BaseVocalizer> Vocalizers => m_Vocalizers;
        private List<BaseVocalizer> m_Vocalizers;

        private BaseVocalizer _Current;
        private VocalizerCompositeState _state;

        internal string characters;
        private List<PhonemeClusterVocalizer> clusters;
        internal List<PhonemeClusterVocalizer> vowelClusters;

        /// <summary>
        /// Creates a series of phoneme vocalizers that will be read out loud
        /// </summary>
        /// <param name="raw">Alphanumeric string with no whitespace</param>
        /// <returns></returns>
        public static WordVocalizer MakeSpokenVocalizer(string raw)
        {
            WordVocalizer result = new();
            if (raw.Length == 0)
            {
                Debug.LogError("Word cannot be empty");
                return result;
            }

            result.clusters = new List<PhonemeClusterVocalizer>();
            // replace numeric digits with random vowels
            result.characters = new string(raw.Select(c => char.ToLower(char.IsDigit(c) ? RandomVowel : c)).ToArray());

            // convert word to clusters
            bool lastCharIsVowel = vowelDescriptionTable.ContainsKey(result.characters[0]);
            result.clusters.Add(new PhonemeClusterVocalizer()
            {
                isVowelCluster = lastCharIsVowel,
                characters = new StringBuilder().Append(result.characters[0])
            });
            for (int i = 1; i < result.characters.Length; i++)
            {
                char c = result.characters[i];
                bool currCharIsVowel = vowelDescriptionTable.ContainsKey(c);
                if (lastCharIsVowel != currCharIsVowel)
                {
                    result.clusters.Add(new PhonemeClusterVocalizer()
                    {
                        isVowelCluster = currCharIsVowel,
                        characters = new StringBuilder().Append(c)
                    });
                }
                else
                {
                    result.clusters[^1].characters.Append(c);
                }
                lastCharIsVowel = currCharIsVowel;
            }
            result.vowelClusters = result.clusters.Where(c => c.isVowelCluster && !c.IsEmpty).ToList();

            // Account for words without vowels
            if (result.vowelClusters.Count == 0)
            {
                result.vowelClusters.Add(new PhonemeClusterVocalizer()
                {
                    isVowelCluster = true,
                    characters = new StringBuilder().Append(RandomVowel)
                });
            }

            result.PlaceStress();

            result.m_Vocalizers = result.vowelClusters.Where(c => c.isStressed).Select(c => c as BaseVocalizer).ToList();
            if (result.m_Vocalizers.Count == 0) result.m_Vocalizers.Add(result.vowelClusters[0]);

            return result;
        }
        
        public static WordVocalizer MakeFixedLengthSoundVocalizer()
        {
            WordVocalizer result = new();
            result.m_Vocalizers = new();
            result.m_Vocalizers.Add(new FixedLengthSoundVocalizer());
            result.characters = null;
            return result;
        }

        /// <summary>
        /// Makes a silent vocalizer that waits for a duration proportional to the length of punctuation
        /// </summary>
        /// <param name="punctuation"></param>
        /// <returns></returns>
        public static WordVocalizer MakePauseVocalizer(string punctuation)
            => new()
            {
                m_Vocalizers = new List<BaseVocalizer> { new PunctuationVocalizer() { characters = new StringBuilder(punctuation) } }
            };

        public static WordVocalizer MakeClauseEndingVocalizer()
            => new()
            {
                m_Vocalizers = new List<BaseVocalizer> { new PauseVocalizer() }
            };

        private void PlaceStress()
        {
            switch (vowelClusters.Count)
            {
                case 1:
                    // for very short words with single vowel, make a guess that it is some preposition like 'a' and 'is'
                    vowelClusters[0].isStressed = vowelClusters[0].characters.Length == 1 && characters.Length < 3;
                    break;
                case 2:
                    // for 2 syllable words the first syllable is slightly more likely to be stressed
                    vowelClusters[Random.value < 0.6f ? 0 : 1].isStressed = true;
                    break;
                default:
                    // pick around V/2 stressed syllables
                    int stressCount = Mathf.Max(1, Mathf.RoundToInt(Random.value * vowelClusters.Count / 2f));
                    List<PhonemeClusterVocalizer> unstressedPool = new(vowelClusters);
                    for (int i = 0; i < stressCount && unstressedPool.Count > 0; i++)
                    {
                        int idx = Mathf.FloorToInt(Random.value * unstressedPool.Count);
                        unstressedPool[idx].isStressed = true;
                        // if a vowel cluster is stressed, the next vowel cluster is unstressed
                        if (idx != unstressedPool.Count - 1)
                        {
                            unstressedPool.RemoveAt(idx + 1);
                        }
                        unstressedPool.RemoveAt(idx);
                    }
                    break;
            }
        }

        public override string ToString()
        {
            string s = "";
            if (clusters != null)
            {
                if (clusters != null)
                {
                    foreach (var cluster in clusters)
                    {
                        s += cluster.ToString();
                    }
                }
            } else
            {
                foreach (var vocalizer in m_Vocalizers)
                {
                    s += vocalizer.ToString();
                }
            }
            
            return s;
        }

        public VocalizerCompositeState GetVocalizationState() => _state;
        void IVocalizerComposite<BaseVocalizer>.SetVocalizationState(VocalizerCompositeState newState)
        {
            _state = newState;
        }

        void IVocalizerComposite<BaseVocalizer>.PreRandomize(
            VocalizerParameters preset, VocalRandomizationContext context, BaseVocalizer upcoming)
        { }
    }

    public class VowelDescription
    {
        public float forwardness;
        public float openness;
    }
}