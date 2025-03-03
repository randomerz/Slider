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

    ILocalizationTrackable.LocalizationState ILocalizationTrackable.LastLocalizedState => _lastLocalizedState;
    ILocalizationTrackable.LocalizationState _lastLocalizedState = ILocalizationTrackable.DefaultState;

    public void Start()
    {
        Localize();
    }

    public void Localize()
    {
        var w = new System.Diagnostics.Stopwatch();
        w.Start();
        
        var locale = LocalizationLoader.CurrentLocale;
        
        var strategy = (this as ILocalizationTrackable).TrackLocalization(LocalizationLoader.CurrentSetting);

        if (strategy is { ShouldTranslate: false, StyleChange: LocalizableContext.StyleChange.Idle })
        {
            // Debug.Log($"[Localization] Skip localization of {this}");
            _lastLocalizedState = LocalizationLoader.CurrentSetting;
            return;
        }
        
        var prefabAssetPath = LocalizationFile.AssetPath(locale, this);
        var loadedAsset = LocalizationLoader.LoadAssetAndConfigureLocaleDefaults(locale, prefabAssetPath);

        if (loadedAsset.context == null)
        {
            Debug.LogError($"Could not load file at {prefabAssetPath}");
            return;
        }
        
        Ctx.Localize(loadedAsset.context, strategy);
        _lastLocalizedState = LocalizationLoader.CurrentSetting;
        
        w.Stop();
        Debug.Log($"[Localization] Elapsed time {w.Elapsed.TotalMilliseconds}ms");
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