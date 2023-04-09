using FMODUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security;
using UnityEditor;
using UnityEngine;
using Status = IVocalizerComposite<SentenceVocalizer>.PlayStatus;

public class VocalizableParagraph : MonoBehaviour, IVocalizerComposite<SentenceVocalizer> {
    [SerializeField] private VocalizerPreset preset;
    [SerializeField, HideInInspector] private string text;
    [SerializeField, HideInInspector] private List<SentenceVocalizer> sentences;
    public VocalizationContext debugContext;

    public string Text => text;
    public List<SentenceVocalizer> Vocalizers => sentences;

    SentenceVocalizer IVocalizerComposite<SentenceVocalizer>.Current { get => _Current; set => _Current = value; }
    Status IVocalizerComposite<SentenceVocalizer>.Status { get => _Status; set => _Status = value; }
    private SentenceVocalizer _Current;
    private Status _Status;

    public void StartRead(SentenceVocalizer voc)
    {
        var vc = (this as IVocalizerComposite<SentenceVocalizer>);
        vc.Stop();
        debugContext = new(transform);
        StartCoroutine((voc as IVocalizerComposite<WordVocalizer>).Vocalize(preset, debugContext));
    }

    public void StartReadAll()
    {
        var vc = (this as IVocalizerComposite<SentenceVocalizer>);
        vc.Stop();
        debugContext = new(transform);
        StartCoroutine(vc.Vocalize(preset, debugContext));
    }

    public bool IsEmpty => sentences.Count == 0;

    public IEnumerator Postvocalize(VocalizerPreset preset, VocalizationContext context, SentenceVocalizer completed, SentenceVocalizer upcoming, int upcomingIdx)
    {
        yield return new WaitForSecondsRealtime(preset.clauseGap);
    }

    public IEnumerator Prevocalize(VocalizerPreset preset, VocalizationContext context, SentenceVocalizer prior, SentenceVocalizer upcoming, int upcomingIdx)
    {
        yield return null;
    }

    public void SetText(string text)
    {
        this.text = text;
        sentences = SentenceVocalizer.Parse(this.text) ?? new();
    }
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
            if (reader.Vocalizers.Count > 0 && Application.isPlaying)
            {
                if (GUILayout.Button("Play"))
                {
                    reader.StartReadAll();
                }
            }
            if ((reader as IVocalizerComposite<SentenceVocalizer>).Status == Status.Playing)
            {
                if (GUILayout.Button("Stop"))
                {
                    (reader as IVocalizerComposite<SentenceVocalizer>).Stop();
                }
            }
            EditorGUILayout.EndHorizontal();

            GUIStyle vocalizerPreviewStyle = new(GUI.skin.label);
            vocalizerPreviewStyle.richText = true;
            foreach (var v in reader.Vocalizers)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(v.punctuation.ToString());
                if (Application.isPlaying && GUILayout.Button("Read clause"))
                {
                    reader.StartRead(v);
                }
                EditorGUILayout.EndHorizontal();
                string sentence = "";
                foreach (var w in v.words)
                {
                    sentence += w.ToString() + " ";
                }
                EditorGUILayout.LabelField(sentence, vocalizerPreviewStyle);
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