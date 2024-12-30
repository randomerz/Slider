using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class LocalizationInjector : MonoBehaviour
{
    public string prefabName;
    public bool autoRefreshOnSpawn = false;

    public void Start()
    {
        if (autoRefreshOnSpawn)
        {
            Refresh();
        }
    }
    
    public void Refresh()
    {
        if (prefabName != null)
        {
            LocalizationLoader.LocalizePrefab(this);
        }
        else
        {
            Debug.LogError($"Empty prefab variant parent for injector: {this}");
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(LocalizationInjector))]
public class LocalizationInjectorEditor: Editor
{
    private Object parent;
    
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (target == null || target is not LocalizationInjector targetCasted)
        {
            return;
        }

        if (GUILayout.Button("Use self name"))
        {
            targetCasted.prefabName = targetCasted.name;
            EditorUtility.SetDirty(target);
        }

        GUILayout.BeginHorizontal();
        parent = EditorGUILayout.ObjectField(parent, typeof(GameObject), allowSceneObjects: false);

        if (GUILayout.Button("Use parent prefab name"))
        {
            targetCasted.prefabName = parent?.name;
            EditorUtility.SetDirty(target);
        }
        GUILayout.EndHorizontal();
    }

}

#endif