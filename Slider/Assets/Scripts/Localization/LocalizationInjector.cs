using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class LocalizationInjector : MonoBehaviour
{
    public string prefabName;
    private int numLocalizations = 0; // turn public for debugging

    public void Start()
    {
        Refresh();
    }
    
    void Refresh()
    {
        if (numLocalizations++ == 0 && prefabName != null)
        {
            LocalizationLoader.InjectLocalization(this);
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