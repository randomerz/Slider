using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Analytics;

[System.Serializable]
public class SentenceVocalizer : IVocalizerComposite<WordVocalizer>
{
    private static readonly string endingsStr = @"(?<=[,.?!])";
    private static readonly char[] endings = { ',','.','?','!' };
    private static readonly HashSet<char> endingsSet = new(endings);
    /// <summary>
    /// Questions no longer tend to tone upwards when the 6W words are in the same clause
    /// </summary>
    private static readonly string[] questionNegation = { "who", "what", "when", "where", "why", "how" };

    public List<WordVocalizer> words;
    public char punctuation;
    public List<WordVocalizer> Vocalizers => words;
    public bool IsEmpty => words.Count == 0;

    public enum Intonation { flat, up, down };
    public Intonation intonation;

    public static List<SentenceVocalizer> Parse(string paragraph)
    {
        string[] clauses = Regex.Split(CleanUp(paragraph), endingsStr); // split while keeping the delimiter
        var vocalizers = new List<SentenceVocalizer>(clauses.Length);
        foreach (var clause in clauses)
        {
            if (clause.Length <= 1) continue;
            else
            {
                SentenceVocalizer sv = new(clause);
                if (!sv.IsEmpty) vocalizers.Add(sv);
            }
        }
        return vocalizers;
    }

    /// <param name="clause">Alphanumeric words separated by space with the last character being one of the characters in "endings"</param>
    private SentenceVocalizer(string clause)
    {
        words = new List<WordVocalizer>();
        int endingTrim = endingsSet.Contains(clause[^1]) ? 1 : 0; // remove ending character if it is a punctuation
        punctuation = endingTrim > 0 ? clause[^1] : '.'; // default to period when no punctuation

        foreach(string word in clause.Substring(0, clause.Length - endingTrim).Split(' ', System.StringSplitOptions.RemoveEmptyEntries))
        {
            WordVocalizer wv = new(word);
            if (!wv.IsEmpty) words.Add(wv);
        }

        if (punctuation == '?')
        {
            intonation = Intonation.up;
            foreach (var keyword in questionNegation)
            {
                if (words[0].characters.StartsWith(keyword))
                {
                    intonation = Intonation.down;
                    break;
                }
            }
        } else if (punctuation == '.' || punctuation == '!')
        {
            intonation = Intonation.down;
        } else
        {
            intonation = Intonation.flat;
        }
    }

    private static string CleanUp(string paragraph)
    {
        char[] clean = new char[paragraph.Length];
        for (int i = 0; i < paragraph.Length; i++)
        {
            char c = paragraph[i];
            if (char.IsLetter(c))
            {
                clean[i] = char.ToLower(c);
            } else if (char.IsDigit(c) || endingsSet.Contains(c))
            {
                clean[i] = c;
            } else
            {
                clean[i] = ' ';
            }
        }

        // collapse whitespaces
        return string.Join(' ', new string(clean).Split(' ', System.StringSplitOptions.RemoveEmptyEntries));
    }

    public IEnumerator Prevocalize(VocalizerPreset preset, VocalizationContext context, WordVocalizer prior, WordVocalizer upcoming, int upcomingIdx)
    {
        if (prior == null)
        {
            // for the first word, initialize intonation of first word
            // guess that short sentences more likely to start high
            float pFirstWordHigh = (Vocalizers.Count <= 3) ? 0.75f : 0.25f;
            context.isCurrentWordLow = Random.value > pFirstWordHigh;
        }
        else if (upcomingIdx != Vocalizers.Count - 1 || intonation != Intonation.up)
        {
            if (context.isCurrentWordLow)
            {
                context.isCurrentWordLow = !preset.DoLowToHigh;
            } else
            {
                context.isCurrentWordLow = preset.DoHighToLow;
            }
        } else
        {
            // last word in an upwards intonated sentence is always high
            context.isCurrentWordLow = false;
        }

        // the last word can be heightened or lowered based on intonation
        context.wordPitchBase = preset.basePitch;
        context.wordPitchIntonated = (upcomingIdx == words.Count - 1 ? GetIntonation(preset) : preset.basePitch);
        return null;
    }

    public IEnumerator Postvocalize(VocalizerPreset preset, VocalizationContext context, WordVocalizer completed, WordVocalizer upcoming, int upcomingIdx)
    {
        yield return new WaitForSecondsRealtime(preset.wordGap * (Random.value + 0.5f));
    }

    public float GetIntonation(VocalizerPreset preset)
    {
        switch (intonation)
        {
            case Intonation.up: return preset.basePitch * (1 + preset.sentenceIntonation);
            case Intonation.down: return preset.basePitch * (1 - preset.sentenceIntonation);
            default: return preset.basePitch;
        }
    }

#if UNITY_EDITOR
    public void ClearProgress()
    {
        foreach (var w in words) w.ClearProgress();
    }
#endif
}