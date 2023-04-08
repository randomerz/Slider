using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using System.Security;
using UnityEditor;
using UnityEngine;

public class Vocalizer : MonoBehaviour, IVocalizerComposite<SentenceVocalizer> {
    [SerializeField] private VocalizerPreset preset;

    private List<SentenceVocalizer> _Vocalizers;
    public List<SentenceVocalizer> Vocalizers => _Vocalizers;

    public void StartRead(SentenceVocalizer voc)
    {
        StopAllCoroutines();
        StartCoroutine((voc as IVocalizerComposite<WordVocalizer>).Vocalize(preset, new(transform)));
    }

    public void StartReadAll()
    {
        StopAllCoroutines();
        StartCoroutine((this as IVocalizerComposite<SentenceVocalizer>).Vocalize(preset, new(transform)));
    }

    public bool IsEmpty => _Vocalizers.Count > 0;

    public IEnumerator Postvocalize(VocalizerPreset preset, VocalizationContext context, SentenceVocalizer completed, SentenceVocalizer upcoming, int upcomingIdx)
    {
        yield return new WaitForSecondsRealtime(preset.secondsBetweenSentences);
    }

    public IEnumerator Prevocalize(VocalizerPreset preset, VocalizationContext context, SentenceVocalizer prior, SentenceVocalizer upcoming, int upcomingIdx)
    {
        yield return null;
    }

    public void SetSentences(List<SentenceVocalizer> vocalizers)
    {
        _Vocalizers = vocalizers;
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(Vocalizer))]
public class VocalizerDebuggerEditor : Editor
{
    string paragraph = "";
    List<SentenceVocalizer> vocalizers = new List<SentenceVocalizer>();
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var reader = target as Vocalizer;
        GUIStyle textAreaStyle = new(EditorStyles.textArea);
        textAreaStyle.wordWrap = true;
        string text = EditorGUILayout.TextArea(paragraph, textAreaStyle, GUILayout.MinHeight(100), GUILayout.ExpandHeight(true));
        if (!text.Equals(paragraph))
        {
            vocalizers.Clear();
            reader.SetSentences(new());
            paragraph = text;
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Parse"))
        {
            vocalizers = SentenceVocalizer.Parse(paragraph);
            reader.SetSentences(vocalizers);
        }

        if (vocalizers.Count > 0)
        {
            if (GUILayout.Button("Play"))
            {
                reader.StartReadAll();
            }
        }
        EditorGUILayout.EndHorizontal();

        GUIStyle vocalizerPreviewStyle = new(GUI.skin.label);
        vocalizerPreviewStyle.richText = true;
        foreach (var v in vocalizers)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(v.punctuation.ToString());
            if (GUILayout.Button("Read clause"))
            {
                if (Application.isPlaying)
                {
                    reader.StartRead(v);
                } else
                {
                    Debug.Log("Cannot read in editor mode, FMOD does not support it");
                }
            }
            EditorGUILayout.EndHorizontal();
            foreach (var w in v.words) {
                EditorGUILayout.LabelField("\t"+w.ToString(), vocalizerPreviewStyle);
            }
        }

    }
}

#endif