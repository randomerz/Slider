using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class WordVocalizer: IVocalizerComposite<PhonemeClusterVocalizer>
{
    private static readonly char[] vowels = { 'a', 'e', 'i', 'o', 'u', 'y' };
    private static readonly char[] consonants = { 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'j', 'k', 'l', 'm', 'n', 'p', 'q', 'r', 's', 't', 'v', 'w', 'x', 'z' };
    public static readonly Dictionary<char, VowelDescription> vowelDescriptionTable = new()
    {
        ['a'] = new VowelDescription() { openness = 1.0f, forwardness = 0.6f },
        ['e'] = new VowelDescription() { openness = 0.3f, forwardness = 0.8f },
        ['i'] = new VowelDescription() { openness = 0.0f, forwardness = 1.0f },
        ['o'] = new VowelDescription() { openness = 0.3f, forwardness = 0.5f },
        ['u'] = new VowelDescription() { openness = 0.0f, forwardness = 0.0f },
        ['y'] = new VowelDescription() { openness = 0.0f, forwardness = 1.0f }
    };
    private static readonly HashSet<char> consonantsSet = new(consonants);
    private static char RandomVowel => vowels[(int)(Random.value * vowels.Length)];
    private static char RandomConsonant => consonants[(int)(Random.value * consonants.Length)];

    public bool IsEmpty => pronouncedCluster == null;

    /// <summary>
    /// Reserved for multi-cluster reading. Currently only does one cluster per word (the first stressed one, or the first one if nothing is stressed)
    /// </summary>
    public List<PhonemeClusterVocalizer> Vocalizers => pronouncedCluster;

    public string characters;
    private List<PhonemeClusterVocalizer> clusters;
    public List<PhonemeClusterVocalizer> vowelClusters;
    public List<PhonemeClusterVocalizer> pronouncedCluster;

    /// <param name="raw">Alphanumeric string with no whitespace</param>
    public WordVocalizer (string raw)
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
            pronouncedCluster = new() { vowelClusters.FirstOrDefault(c => c.isStressed) ?? vowelClusters[0] };
        }
    }

    private void PlaceStress()
    {
        if (vowelClusters.Count == 1)
        {
            // for very short words with single vowel, make a guess that it is some preposition like 'a' and 'is'
            if (vowelClusters[0].characters.Length == 1 && characters.Length < 3)
            {
                vowelClusters[0].isStressed = false;
            } else
            {
                vowelClusters[0].isStressed = true;
            }
        } else if (vowelClusters.Count == 2)
        {
            vowelClusters[Random.value < 0.6f ? 0 : 1].isStressed = true;
        }
        else
        {
            // pick below V/2 stressed syllables
            int stressCount = Mathf.Max(1, Mathf.FloorToInt(Random.value * vowelClusters.Count / 2f));
            List<PhonemeClusterVocalizer> unstressedPool = new (vowelClusters);
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
        }
    }

    public List<(List<VowelDescription> vowelDescriptors, bool isStressed)> VowelClusterDescriptions ()
    {
        List<(List<VowelDescription>, bool isStressed)> descriptors = new();
        foreach (var vowelCluster in vowelClusters)
        {
            List<VowelDescription> descriptorsInCluster = new(vowelCluster.characters.Length);
            foreach (char c in vowelCluster.characters)
            {
                descriptorsInCluster.Add(vowelDescriptionTable[c]);
            }
            descriptors.Add((descriptorsInCluster, vowelCluster.isStressed));
        }
        return descriptors;
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

    public IEnumerator Prevocalize(
        VocalizerPreset preset, VocalizationContext context, PhonemeClusterVocalizer prior, PhonemeClusterVocalizer upcoming, int upcomingIdx)
    {
        return null;
    }

    public IEnumerator Postvocalize(
        VocalizerPreset preset, VocalizationContext context, PhonemeClusterVocalizer completed, PhonemeClusterVocalizer upcoming, int upcomingIdx)
    {
        return null;
    }

#if UNITY_EDITOR
    public void ClearProgress()
    {
        foreach (var cluster in vowelClusters) cluster.ClearProgress();
    }
#endif
}

public class VowelDescription
{
    public float forwardness;
    public float openness;
}