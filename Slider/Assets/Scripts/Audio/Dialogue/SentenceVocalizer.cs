using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

[System.Serializable]
public class SentenceVocalizer
{
    private static readonly string endingsStr = @"(?<=[,.?!])";
    private static readonly char[] endings = { ',','.','?','!' };
    private static readonly HashSet<char> endingsSet = new(endings);

    public static List<SentenceVocalizer> Parse(string paragraph)
    {
        string[] clauses = Regex.Split(CleanUp(paragraph), endingsStr); // split while keeping the delimiter
        var vocalizers = new List<SentenceVocalizer>(clauses.Length);
        foreach (var clause in clauses)
        {
            if (clause.Length <= 1) continue;
            else vocalizers.Add(new SentenceVocalizer(clause));
        }
        return vocalizers;
    }

    public List<WordVocalizer> words;
    public char punctuation;

    /// <param name="clause">Alphanumeric words separated by space with the last character being one of the characters in "endings"</param>
    private SentenceVocalizer(string clause)
    {
        words = new List<WordVocalizer>();
        int endingTrim = endingsSet.Contains(clause[^1]) ? 1 : 0; // remove ending character if it is a punctuation
        punctuation = endingTrim > 0 ? clause[^1] : '.'; // default to period when no punctuation
        foreach(string word in clause.Substring(0, clause.Length - endingTrim).Split(' ', System.StringSplitOptions.RemoveEmptyEntries))
        {
            words.Add(new WordVocalizer(word));
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
} 