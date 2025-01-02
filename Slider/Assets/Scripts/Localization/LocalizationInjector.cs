using System;
using System.Collections.Generic;
using Localization;
using UnityEngine;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class LocalizationInjector : MonoBehaviour, ILocalizationTrackable
{
    public string prefabName;

    private LocalizableContext Ctx => _ctx ??= LocalizableContext.ForInjector(this);
    private LocalizableContext _ctx;

    private Dictionary<string, LocalizationFile> loadedFiles = new();

    ILocalizationTrackable.LocalizationState ILocalizationTrackable.LastLocalizedState => _lastLocalizedState;
    ILocalizationTrackable.LocalizationState _lastLocalizedState = ILocalizationTrackable.DefaultState;

    public void Start()
    {
        Localize();
    }

    public void Localize()
    {
        var locale = LocalizationLoader.CurrentLocale;

        if (!loadedFiles.ContainsKey(locale))
        {
            var prefabAssetPath = LocalizationFile.AssetPath(locale, this);
            var loadedAsset = LocalizationLoader.LoadAssetAndConfigureLocaleDefaults(locale, prefabAssetPath, LocalizationLoader.Instance?.LocaleGlobalFile);

            if (loadedAsset.context == null)
            {
                Debug.LogError($"Could not load file at {prefabAssetPath}");
                return;
            }
            
            loadedFiles.Add(locale, loadedAsset.context);
        }
        
        Ctx.Localize(loadedFiles[locale], LocalizationLoader.CurrentSetting);
        _lastLocalizedState = LocalizationLoader.CurrentSetting;
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