using System;
using System.IO;
using Localization;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

using LocalizationFile = Localization.LocalizationFile;

public class LocalizationLoader : Singleton<LocalizationLoader>
{
    private string locale = "English";
    private LocalizationFile loadedAsset;

    private void Awake()
    {
        InitializeSingleton();
    }

    private void OnEnable()
    {
        SceneManager.activeSceneChanged += (_, scene) => RefreshLocalization(scene);

        // TODO: trigger RefreshLocalization on select locale as well
    }

    public static void RefreshSilent(string locale)
    {
        if (_instance != null)
        {
            _instance.locale = locale;
        }
    }

    public static void Refresh(string locale)
    {
        if (_instance != null)
        {
            _instance.locale = locale;
            _instance.RefreshLocalization(SceneManager.GetActiveScene());
        }
        else
        {
            Debug.LogWarning("No active localization loader instance in scene");
        }
    }

    public void RefreshLocalization(Scene scene)
    {
        LocalizableScene loaded = new(scene);
        LocalizableScene persistent = new(gameObject.scene);

        loadedAsset = null;

        string localizationPath = LocalizationFile.LocaleAssetPath(locale, scene); // TODO: use actual locale
        if (File.Exists(localizationPath))
        {
            loadedAsset = new(new StreamReader(File.OpenRead(localizationPath)));
        }
        
        if (loadedAsset == null)
        {
            Debug.LogError($"Locale file does not exist {locale}");
            return;
        }
        
        loaded.Localize(loadedAsset);
        persistent.Localize(loadedAsset);
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(LocalizationLoader))]
public class LocalizationLoaderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (Application.isPlaying && GUILayout.Button("Refresh localization"))
        {
            (target as LocalizationLoader).RefreshLocalization(SceneManager.GetActiveScene());
        }
    }
}

#endif