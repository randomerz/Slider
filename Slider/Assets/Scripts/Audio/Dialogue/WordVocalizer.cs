using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SliderVocalization
{
    public class WordVocalizer : IVocalizerComposite<PhonemeClusterVocalizer>
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

        public bool IsEmpty => significantCluster == null;

        /// <summary>
        /// Return vowelClusters for reading every single vowel in word
        /// Return significantCluster for reading only the stressed vowel groups (or the only vowel if no stressed vowels)
        ///   - For most of the time there is only one stress in a word, but for very long words there might be multiple
        /// </summary>
        public List<PhonemeClusterVocalizer> Vocalizers => significantCluster;

        private PhonemeClusterVocalizer _Current;
        private VocalizerCompositeStatus _Status;

        internal string characters;
        private readonly List<PhonemeClusterVocalizer> clusters;
        internal List<PhonemeClusterVocalizer> vowelClusters;
        internal List<PhonemeClusterVocalizer> significantCluster;

        /// <param name="raw">Alphanumeric string with no whitespace</param>
        public WordVocalizer(string raw)
        {
            if (raw.Length == 0)
            {
                Debug.LogError("Word cannot be empty");
                return;
            }

            clusters = new List<PhonemeClusterVocalizer>();
            // replace numeric digits with random vowels
            characters = new string(raw.Select(c => char.IsDigit(c) ? RandomVowel : c).ToArray());

            // convert word to clusters
            bool lastCharIsVowel = vowelDescriptionTable.ContainsKey(characters[0]);
            clusters.Add(new PhonemeClusterVocalizer()
            {
                isVowelCluster = lastCharIsVowel,
                characters = characters[0].ToString()
            });
            for (int i = 1; i < characters.Length; i++)
            {
                char c = characters[i];
                bool currCharIsVowel = vowelDescriptionTable.ContainsKey(c);
                if (lastCharIsVowel != currCharIsVowel)
                {
                    clusters.Add(new PhonemeClusterVocalizer()
                    {
                        isVowelCluster = currCharIsVowel,
                        characters = c.ToString()
                    });
                }
                else
                {
                    clusters[^1].characters += c;
                }
                lastCharIsVowel = currCharIsVowel;
            }
            vowelClusters = clusters.Where(c => c.isVowelCluster && !c.IsEmpty).ToList();
            PlaceStress();

            if (vowelClusters.Count != 0)
            {
                significantCluster = vowelClusters.Where(c => c.isStressed).ToList();
                if (significantCluster.Count == 0) significantCluster.Add(vowelClusters[0]);
            }
        }

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
            foreach (var cluster in clusters)
            {
                s += cluster.ToString();
            }
            return s;
        }

        public PhonemeClusterVocalizer GetCurrent() => _Current;
        void IVocalizerComposite<PhonemeClusterVocalizer>.SetCurrent(PhonemeClusterVocalizer value) => _Current = value;
        public VocalizerCompositeStatus GetStatus() => _Status;
        void IVocalizerComposite<PhonemeClusterVocalizer>.SetStatus(VocalizerCompositeStatus value) => _Status = value;

        float IVocalizerComposite<PhonemeClusterVocalizer>.PreRandomize(
            VocalizerParameters preset, VocalRandomizationContext context, PhonemeClusterVocalizer prior, PhonemeClusterVocalizer upcoming, int upcomingIdx)
            => 0.0f;
    }

    public class VowelDescription
    {
        public float forwardness;
        public float openness;
    }
}