using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

[System.Serializable]
public class SentenceVocalizer : IVocalizerComposite<WordVocalizer>
{
    private static readonly string endingsStr = @"(?<=[,.?!])";
    private static readonly char[] endings = { ',','.','?','!' };
    private static readonly HashSet<char> endingsSet = new(endings);

    public List<WordVocalizer> words;
    public char punctuation;
    public List<WordVocalizer> Vocalizers => words;
    public bool Vocalizable => words.Count > 0;

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
                if (sv.Vocalizable) vocalizers.Add(sv);
            }
        }
        return vocalizers;
    }

    /// <param name="clause">Alphanumeric words separated by space with the last character being one of the characters in "endings"</param>
    private SentenceVocalizer(string clause)
    {
        Debug.Log($"clause: {clause}");
        words = new List<WordVocalizer>();
        int endingTrim = endingsSet.Contains(clause[^1]) ? 1 : 0; // remove ending character if it is a punctuation
        punctuation = endingTrim > 0 ? clause[^1] : '.'; // default to period when no punctuation
        foreach(string word in clause.Substring(0, clause.Length - endingTrim).Split(' ', System.StringSplitOptions.RemoveEmptyEntries))
        {
            WordVocalizer wv = new(word);
            if (wv.Vocalizable) words.Add(wv);
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

    public IEnumerator Prevocalize(WordVocalizer prior, WordVocalizer upcoming)
    {
        throw new System.NotImplementedException();
    }

    public IEnumerator Postvocalize(WordVocalizer completed, WordVocalizer upcoming)
    {
        throw new System.NotImplementedException();
    }
} 