using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Vocalizer : MonoBehaviour {
    [SerializeField] private VocalizerPreset preset;

    public void StartRead(SentenceVocalizer voc)
    {
        StopAllCoroutines();
        StartCoroutine((voc as IVocalizerComposite<WordVocalizer>).Vocalize(preset, new(transform)));
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

        paragraph = EditorGUILayout.TextArea(paragraph, GUILayout.MinHeight(100), GUILayout.ExpandHeight(true));

        if (GUILayout.Button("Parse"))
        {
            vocalizers = SentenceVocalizer.Parse(paragraph);
        }

        foreach (var v in vocalizers)
        {
            var style = new GUIStyle(GUI.skin.label);
            style.richText = true;
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
                EditorGUILayout.LabelField("\t"+w.ToString(), style);
            }
        }
    }
}

#endif