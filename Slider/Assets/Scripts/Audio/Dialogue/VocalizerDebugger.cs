using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class VocalizerDebugger : MonoBehaviour { }

#if UNITY_EDITOR

[CustomEditor(typeof(VocalizerDebugger))]
public class VocalizerDebuggerEditor : Editor
{
    string paragraph = "";
    List<SentenceVocalizer> vocalizers = new List<SentenceVocalizer>();
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        paragraph = EditorGUILayout.TextArea(paragraph, GUILayout.MinHeight(100), GUILayout.ExpandHeight(true));

        if (GUILayout.Button("Parse"))
        {
            vocalizers = SentenceVocalizer.Parse(paragraph);
        }

        foreach (var v in vocalizers)
        {
            var style = new GUIStyle(GUI.skin.label);
            style.richText = true;
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField(v.punctuation.ToString());
            foreach (var w in v.words) {
                EditorGUILayout.LabelField("\t"+w.ToString(), style);
            }
        }
    }
}

#endif