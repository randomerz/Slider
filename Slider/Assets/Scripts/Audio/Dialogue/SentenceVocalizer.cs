using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace SliderVocalization
{
    public class SentenceVocalizer : IVocalizerComposite<WordVocalizer>
    {
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
        private WordVocalizer FirstSpokenVocalizer;
        private WordVocalizer LastSpokenVocalizer;
        private VocalizerCompositeState _state;

        public enum Intonation { flat, up, down };
        public Intonation intonation;
        public float GetBasePitchFromIntonation(VocalizerParameters preset)
            => (preset.overrideIntonation ? preset.intonationOverride : intonation) switch
            {
                Intonation.flat => preset.pitch,
                Intonation.up => preset.pitch * (1 + preset.sentenceIntonationUp),
                Intonation.down => preset.pitch * (1 - preset.sentenceIntonationDown),
                _ => preset.pitch
            };

        public static List<SentenceVocalizer> Parse(string paragraph)
        {
            MatchCollection clauses = Regex.Matches(paragraph, @"[^.,!?]+[.,!?\s]+(?=[^.,!?\s]|$)");
            List<SentenceVocalizer> vocalizers = new (clauses.Count);
            foreach (Match clause in clauses)
            {
                if (clause.Length <= 1) continue;
                else
                {
                    SentenceVocalizer sv = new(paragraph.Substring(clause.Index, clause.Length));
                    if (!sv.GetIsEmpty()) vocalizers.Add(sv);
                }
            }
            return vocalizers;
        }

        /// <param name="clause">Alphanumeric words separated by space with the last character being one of the characters in "endings"</param>
        private SentenceVocalizer(string clause)
        {
            words = new List<WordVocalizer>();
            MatchCollection alphaNumeric = Regex.Matches(clause, @"[A-Za-z00-9]+");

            if (alphaNumeric.Count == 0) return;

            punctuation = default;
            FirstSpokenVocalizer = null;
            for (int i = 0; i < alphaNumeric.Count; i++)
            {
                Match match = alphaNumeric[i];

                int afterWordEnd = match.Index + match.Length;
                int nextStart = match.NextMatch().Index;
                if (nextStart == 0) nextStart = clause.Length; // returns 0 when next match does not exist

                string gap = clause.Substring(afterWordEnd, nextStart - afterWordEnd);

                foreach(char punctuation in gap)
                {
                    if (endingsSet.Contains(punctuation))
                    {
                        this.punctuation = punctuation;
                        break;
                    }
                }

                LastSpokenVocalizer = WordVocalizer.MakeSpokenVocalizer(clause.Substring(match.Index, match.Length));
                if (!LastSpokenVocalizer.IsEmpty)
                {
                    FirstSpokenVocalizer ??= LastSpokenVocalizer;
                    words.Add(LastSpokenVocalizer);
                    // words.Add(WordVocalizer.MakePauseVocalizer(gap));
                }
            }
            if (punctuation == default) punctuation = '.';
            words.Add(WordVocalizer.MakeClauseEndingVocalizer());

            switch (punctuation)
            {
                case '?':
                    intonation = Intonation.up;
                    foreach (var keyword in questionNegation)
                    {
                        // most questions starting from the 6 W's tend downward
                        if (words[0].characters.StartsWith(keyword) && Random.value < 0.75f)
                        {
                            intonation = Intonation.down;
                            break;
                        }
                    }
                    break;
                case '.':
                case '!':
                    // not realistic, but simulates shouting
                    if (words.Count <= 3 && Random.value < 0.5f)
                    {
                        intonation = Intonation.up;
                    } else
                    {
                        intonation = Intonation.down;
                    }
                    break;
                default:
                    intonation = Intonation.flat;
                    break;
            }
        }

        void IVocalizerComposite<WordVocalizer>.PreRandomize(VocalizerParameters preset, VocalRandomizationContext context, WordVocalizer upcoming)
        {
            if (upcoming == FirstSpokenVocalizer)
            {
                // for the first word, initialize intonation of first word
                // guess that short sentences more likely to start high
                float pFirstWordHigh = (Vocalizers.Count <= 3) ? 0.75f : 0.25f;
                context.isCurrentWordLow = Random.value > pFirstWordHigh;
            }
            else if (upcoming == LastSpokenVocalizer)
            {
                // match last word intonation with sentence
                if (intonation == Intonation.up)
                {
                    context.isCurrentWordLow = false;
                }
                else if (intonation == Intonation.down)
                {
                    context.isCurrentWordLow = true;
                }
            }
            else
            {
                context.isCurrentWordLow = context.isCurrentWordLow ? !preset.DoLowToHigh : context.isCurrentWordLow = preset.DoHighToLow;
            }

            // the last word can be heightened or lowered based on intonation
            context.wordPitchBase = preset.pitch;
            context.lastWordFinalPitch = context.wordPitchBase;
            context.wordPitchIntonated = (upcoming == LastSpokenVocalizer ? GetBasePitchFromIntonation(preset) : preset.pitch);
        }
        
        public VocalizerCompositeState GetVocalizationState() => _state;
        void IVocalizerComposite<WordVocalizer>.SetVocalizationState(VocalizerCompositeState newState)
        {
            _state = newState;
        }
    }
}