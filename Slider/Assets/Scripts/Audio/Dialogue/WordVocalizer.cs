using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WordVocalizer
{
    // clusters of sounds such as "au" in australia and "thr" in thresh
    public class PhonemeCluster
    {
        public int idx;
        public int length;
        public bool isVowelCluster;
        public bool isStressed = false;
    }
    
    public string characters;
    public List<PhonemeCluster> clusters;
    public List<PhonemeCluster> vowelClusters;

    private static readonly char[] vowels = { 'a', 'e', 'i', 'o', 'u', 'y' };
    private static readonly char[] consonants = { 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'j', 'k', 'l', 'm', 'n', 'p', 'q', 'r', 's', 't', 'v', 'w', 'x', 'z' };
    private static readonly HashSet<char> vowelsSet = new(vowels);
    private static readonly HashSet<char> consonantsSet = new(consonants);
    private static char RandomVowel => vowels[(int) (Random.value * vowels.Length)];
    private static char RandomConsonant => consonants[(int) (Random.value * consonants.Length)];
    
    /// <param name="raw">Alphanumeric string with no whitespace</param>
    public WordVocalizer (string raw)
    {
        clusters = new List<PhonemeCluster>();
        vowelClusters = new List<PhonemeCluster>();
        characters = PreprocessRawString(raw);
        PlaceStress();
    }

    private string PreprocessRawString(string raw)
    {
        bool lastCharIsVowel = false;
        var charList = new List<char>(raw.Length);
        for (int i = 0; i < raw.Length; i++)
        {
            // replace numberic digits with random vowels
            char c = char.IsDigit(raw[i]) ? RandomVowel : raw[i];
            bool thisCharIsVowel = vowelsSet.Contains(c);
            // create new cluster entry
            if (i == 0 || thisCharIsVowel != lastCharIsVowel)
            {
                clusters.Add(new PhonemeCluster()
                {
                    idx = i,
                    length = 1,
                    isVowelCluster = thisCharIsVowel
                });
                if (thisCharIsVowel)
                {
                    vowelClusters.Add(clusters[^1]);
                }
            }
            // push to last cluster entry
            else
            {
                clusters[^1].length += 1;
            }

            lastCharIsVowel = thisCharIsVowel;
            charList.Add(c);
        }
        return new string(charList.ToArray());
    }

    private WordVocalizer PlaceStress()
    {
        if (vowelClusters.Count == 1)
        {
            vowelClusters[0].isStressed = true;
        } else if (vowelClusters.Count == 2)
        {
            vowelClusters[Random.value < 0.6f ? 0 : 1].isStressed = true;
        }
        else
        {
            // pick below V/2 stressed syllables
            int stressCount = Mathf.Max(1, Mathf.FloorToInt(Random.value * vowelClusters.Count / 2f));
            List<PhonemeCluster> unstressedPool = new (vowelClusters);
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

        return this;
    }

    public override string ToString()
    {
        string s = "";
        foreach (var cluster in clusters)
        {
            string format = cluster.isVowelCluster ? "<B><I>$</I></B>" : "$";
            s += format.Replace("$", characters.Substring(cluster.idx, cluster.length));
        }
        return s;
    }
}
