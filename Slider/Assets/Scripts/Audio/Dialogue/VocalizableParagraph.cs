using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SliderVocalization
{
    public class VocalizableParagraph : MonoBehaviour, IVocalizerComposite<SentenceVocalizer>
    {
        public VocalizerPreset preset;
        [SerializeField, HideInInspector] private string text;
        [SerializeField, HideInInspector] private List<SentenceVocalizer> sentences;
        public VocalizationContext currentVocalizationContext;
        [SerializeField] private VocalizerParameterModifierLibrary modifierLibrary;

        public string Text => text;
        public List<SentenceVocalizer> Vocalizers => sentences;

        private SentenceVocalizer _Current;
        private VocalizerCompositeStatus _Status;

        public WaitUntil WaitUntilCanPlay() => new (() => _Status == VocalizerCompositeStatus.CanPlay);

        internal void StartReadSentence(SentenceVocalizer voc, NPCEmotes.Emotes emote)
        {
            this.Stop();
            voc.Stop();
            currentVocalizationContext = new(transform);
            StartCoroutine(voc.Vocalize(((VocalizerParameters) preset).ModifyWith(modifierLibrary[emote], createClone: true), currentVocalizationContext));
        }

        public void StartReadAll(NPCEmotes.Emotes emote)
        {
            this.Stop();
            currentVocalizationContext = new(transform);
            StartCoroutine(this.Vocalize(((VocalizerParameters)preset).ModifyWith(modifierLibrary[emote], createClone: true), currentVocalizationContext));
        }

        /// <summary>
        /// Parses text and generates randomized narration
        /// </summary>
        /// <param name="text">Duration of that narration when played uninterrupted</param>
        /// <returns></returns>
        public float SetText(string text, NPCEmotes.Emotes emote)
        {
            if (!(this.text ?? "").Equals(text))
            {
                this.text = text;
                sentences = SentenceVocalizer.Parse(this.text) ?? new();
            }

            return (this as IVocalizer).RandomizeVocalization(
                ((VocalizerParameters)preset).ModifyWith(modifierLibrary[emote], createClone: true), new()
                );
        }

        void IVocalizerComposite<SentenceVocalizer>.PreRandomize(
            VocalizerParameters preset, VocalRandomizationContext context, SentenceVocalizer prior, SentenceVocalizer upcoming, int upcomingIdx) { }
        
        public SentenceVocalizer GetCurrent() => _Current;
        void IVocalizerComposite<SentenceVocalizer>.SetCurrent(SentenceVocalizer value) => _Current = value;
        public VocalizerCompositeStatus GetStatus() => _Status;
        void IVocalizerComposite<SentenceVocalizer>.SetStatus(VocalizerCompositeStatus value) => _Status = value;
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(VocalizableParagraph))]
    public class VocalizerDebuggerEditor : Editor
    {
        string rawText;
        NPCEmotes.Emotes emote;
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var reader = target as VocalizableParagraph;

            if (Application.isPlaying)
            {
                EditorGUILayout.LabelField(reader.GetStatus().ToString());

                GUIStyle textAreaStyle = new(EditorStyles.textArea)
                {
                    wordWrap = true
                };
                rawText = EditorGUILayout.TextArea(rawText, textAreaStyle, GUILayout.MinHeight(100), GUILayout.ExpandHeight(true));

                emote = (NPCEmotes.Emotes) EditorGUILayout.EnumFlagsField(emote);
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Apply"))
                {
                    reader.SetText(rawText, emote);
                    EditorUtility.SetDirty(reader);
                }
                if (reader.Vocalizers != null && reader.Vocalizers.Count > 0 && Application.isPlaying)
                {
                    if (GUILayout.Button("Play"))
                    {
                        reader.StartReadAll(emote);
                    }
                }
                if (reader.GetStatus() == VocalizerCompositeStatus.Playing)
                {
                    if (GUILayout.Button("Stop"))
                    {
                        reader.Stop();
                    }
                }
                EditorGUILayout.EndHorizontal();

                GUIStyle vocalizerPreviewStyle = new(GUI.skin.label)
                {
                    richText = true
                };

                if (reader.Vocalizers != null)
                {
                    foreach (var v in reader.Vocalizers)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(v.punctuation.ToString());
                        if (Application.isPlaying && GUILayout.Button("Read clause"))
                        {
                            reader.StartReadSentence(v, emote);
                        }
                        EditorGUILayout.EndHorizontal();
                        string sentence = "";
                        foreach (var w in v.words)
                        {
                            sentence += w.ToString() + " ";
                        }
                        EditorGUILayout.LabelField(sentence, vocalizerPreviewStyle);
                    }
                }

                // this is to keep the playback up to date
                // sometimes Unity skips repainting the inspector so playback progress lags
                Repaint();
            }
            else
            {
                EditorGUILayout.LabelField("ONLY EDITABLE LIVE DURING PLAY");
            }
        }
    }

#endif
}