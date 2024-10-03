using TMPro;
using UnityEngine;
using UnityEngine.TextCore;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName="Localization/FontFaceInfoSync")]
public class FontFaceInfoUpdater: ScriptableObject
{
    public FaceInfo faceInfo;
    public TMP_FontAsset[] variants;
}

#if UNITY_EDITOR

[CustomEditor(typeof(FontFaceInfoUpdater))]
public class FontFaceInfoUpdaterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var t = target as FontFaceInfoUpdater;
        
        if (GUILayout.Button("Sync"))
        {
            foreach (var lang in t.variants)
            {
                var fi = lang.faceInfo;

                fi.pointSize = t.faceInfo.pointSize;
                fi.scale = t.faceInfo.scale;
                fi.ascentLine = t.faceInfo.ascentLine;
                fi.capLine = t.faceInfo.capLine;
                fi.meanLine = t.faceInfo.meanLine;
                fi.descentLine = t.faceInfo.descentLine;
                fi.baseline = t.faceInfo.baseline;
                fi.lineHeight = t.faceInfo.lineHeight;
                fi.underlineOffset = t.faceInfo.underlineOffset;
                fi.underlineThickness = t.faceInfo.underlineThickness;

                lang.faceInfo = fi;
                EditorUtility.SetDirty(lang);
            }
        }

        if (GUILayout.Button("Copy face info from index 0"))
        {
            t.faceInfo.pointSize = t.variants[0].faceInfo.pointSize;
            t.faceInfo.scale = t.variants[0].faceInfo.scale;
            t.faceInfo.ascentLine = t.variants[0].faceInfo.ascentLine;
            t.faceInfo.capLine = t.variants[0].faceInfo.capLine;
            t.faceInfo.meanLine = t.variants[0].faceInfo.meanLine;
            t.faceInfo.descentLine = t.variants[0].faceInfo.descentLine;
            t.faceInfo.baseline = t.variants[0].faceInfo.baseline;
            t.faceInfo.lineHeight = t.variants[0].faceInfo.lineHeight;
            t.faceInfo.underlineOffset = t.variants[0].faceInfo.underlineOffset;
            t.faceInfo.underlineThickness = t.variants[0].faceInfo.underlineThickness;
            EditorUtility.SetDirty(t);
        }
    }
}

#endif