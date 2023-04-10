using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;

namespace SliderVocalization
{
    public class VocalizableParagraph : MonoBehaviour, IVocalizerComposite<SentenceVocalizer>
    {
        public VocalizerPreset preset;
        [SerializeField, HideInInspector] private string text;
        [SerializeField, HideInInspector] private List<SentenceVocalizer> sentences;
        public VocalizationContext currentVocalizationContext;

        public string Text => text;
        public List<SentenceVocalizer> Vocalizers => sentences;

        private SentenceVocalizer _Current;
        private VocalizerCompositeStatus _Status;

        public WaitUntil WaitUntilCanPlay() => new WaitUntil(() => _Status == VocalizerCompositeStatus.CanPlay);

        internal void StartReadSentence(SentenceVocalizer voc)
        {
            this.Stop();
            voc.Stop();
            currentVocalizationContext = new(transform);
            StartCoroutine(voc.Vocalize(preset, currentVocalizationContext));
        }

        public void StartReadAll()
        {
            this.Stop();
            currentVocalizationContext = new(transform);
            StartCoroutine(this.Vocalize(preset, currentVocalizationContext));
        }

        IEnumerator IVocalizerComposite<SentenceVocalizer>.Prevocalize(
            VocalizerPreset preset, VocalizationContext context, SentenceVocalizer prior, SentenceVocalizer upcoming, int upcomingIdx)
            => null;

        IEnumerator IVocalizerComposite<SentenceVocalizer>.Postvocalize(
            VocalizerPreset preset, VocalizationContext context, SentenceVocalizer completed, SentenceVocalizer upcoming, int upcomingIdx)
            => null;

        public void SetText(string text)
        {
            if (!(this.text ?? "").Equals(text))
            {
                this.text = text;
                sentences = SentenceVocalizer.Parse(this.text) ?? new();
            }
        }

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

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Apply"))
                {
                    reader.SetText(rawText);
                    EditorUtility.SetDirty(reader);
                }
                if (reader.Vocalizers != null && reader.Vocalizers.Count > 0 && Application.isPlaying)
                {
                    if (GUILayout.Button("Play"))
                    {
                        reader.StartReadAll();
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

                GUIStyle vocalizerPreviewStyle = new(GUI.skin.label);
                vocalizerPreviewStyle.richText = true;

                if (reader.Vocalizers != null)
                {
                    foreach (var v in reader.Vocalizers)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(v.punctuation.ToString());
                        if (Application.isPlaying && GUILayout.Button("Read clause"))
                        {
                            reader.StartReadSentence(v);
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