using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace SliderVocalization
{
    public class SentenceVocalizer : IVocalizerComposite<WordVocalizer>
    {
        private static readonly string endingsStr = @"(?<=[,.?!])";
        private static readonly char[] endings = { ',', '.', '?', '!' };
        private static readonly HashSet<char> endingsSet = new(endings);
        /// <summary>
        /// Questions no longer tend to tone upwards when the 6W words are in the same clause
        /// </summary>
        private static readonly string[] questionNegation = { "who", "what", "when", "where", "why", "how" };

        internal List<WordVocalizer> words;
        public char punctuation;

        public List<WordVocalizer> Vocalizers => words;

        bool IVocalizer.IsEmpty => words.Count == 0;

        private WordVocalizer _Current;
        private VocalizerCompositeStatus _Status;

        public enum Intonation { flat, up, down };
        public Intonation intonation;
        public float GetBasePitchFromIntonation(VocalizerParameters preset)
            => (preset.overrideIntonation ? preset.intonationOverride : intonation) switch
            {
                Intonation.flat => preset.pitch,
                Intonation.up => preset.pitch * (1 + preset.sentenceIntonation),
                Intonation.down => preset.pitch * (1 - preset.sentenceIntonation),
                _ => preset.pitch
            };

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
                    if (!sv.GetIsEmpty()) vocalizers.Add(sv);
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

            foreach (string word in clause.Substring(0, clause.Length - endingTrim).Split(' ', System.StringSplitOptions.RemoveEmptyEntries))
            {
                WordVocalizer wv = new(word);
                if (!wv.IsEmpty) words.Add(wv);
            }

            switch (punctuation)
            {
                case '?':
                    intonation = Intonation.up;
                    foreach (var keyword in questionNegation)
                    {
                        if (words[0].characters.StartsWith(keyword))
                        {
                            intonation = Intonation.down;
                            break;
                        }
                    }
                    break;
                case '.':
                case '!':
                    intonation = Intonation.down;
                    break;
                default:
                    intonation = Intonation.flat;
                    break;
            }
        }

        private static string CleanUp(string paragraph) 
            => string.Join(' ', new string(
                paragraph.Select(c => char.IsLetter(c) ?
                    char.ToLower(c) :
                    char.IsDigit(c) || endingsSet.Contains(c) ? c : ' ').ToArray()
                ).Split(' ', System.StringSplitOptions.RemoveEmptyEntries));

        IEnumerator IVocalizerComposite<WordVocalizer>.Prevocalize(
            VocalizerParameters preset, VocalizationContext context, WordVocalizer prior, WordVocalizer upcoming, int upcomingIdx)
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
                context.isCurrentWordLow = context.isCurrentWordLow ? !preset.DoLowToHigh : context.isCurrentWordLow = preset.DoHighToLow;
            }
            else
            {
                // last word in an upwards intonated sentence is always high
                context.isCurrentWordLow = false;
            }

            // the last word can be heightened or lowered based on intonation
            context.wordPitchBase = preset.pitch;
            context.wordPitchIntonated = (upcomingIdx == words.Count - 1 ? GetBasePitchFromIntonation(preset) : preset.pitch);
            return null;
        }

        IEnumerator IVocalizerComposite<WordVocalizer>.Postvocalize(
            VocalizerParameters preset, VocalizationContext context, WordVocalizer completed, WordVocalizer upcoming, int upcomingIdx)
        {
            yield return new WaitForSecondsRealtime(preset.wordGap * (Random.value + 0.5f));
        }

        public WordVocalizer GetCurrent() => _Current;
        void IVocalizerComposite<WordVocalizer>.SetCurrent(WordVocalizer value) => _Current = value;
        public VocalizerCompositeStatus GetStatus() => _Status;
        void IVocalizerComposite<WordVocalizer>.SetStatus(VocalizerCompositeStatus value) => _Status = value;
    }
}