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
    [SerializeField] private bool loadDebugAsset;
    
    private LocalizationFile loadedAsset;
    
    private void OnEnable()
    {
        SceneManager.sceneLoaded += RefreshLocalization;
        
        // TODO: trigger RefreshLocalization on select locale as well
    }

    public void RefreshLocalization(Scene scene, LoadSceneMode mode)
    {
        bool debugAssetLoaded = false;

        LocalizableScene world = new(scene);

        loadedAsset = null;

        if (loadDebugAsset)
        {
            string debugLocalizationPath = LocalizationFile.DebugAssetPath;
            if (File.Exists(debugLocalizationPath))
            {
                loadedAsset = new(new StreamReader(File.OpenRead(debugLocalizationPath)));
                debugAssetLoaded = true;
            }
        }

        if (!debugAssetLoaded)
        {
            string localizationPath = LocalizationFile.LocaleAssetPath("debug", scene); // TODO: use actual locale
            if (File.Exists(localizationPath))
            {
                loadedAsset = new(new StreamReader(File.OpenRead(localizationPath)));
            }
        }
        
        if (loadedAsset == null)
        {
            return;
        }
        
        world.Localize(loadedAsset);
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
            (target as LocalizationLoader).RefreshLocalization(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }
    }
}

#endif