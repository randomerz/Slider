using System;
using System.IO;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LocalizationLoader : Singleton<LocalizationLoader>
{
    private LocalizationHelpers.LocalizationFile loadedAsset;
    
    private void OnEnable()
    {
        SceneManager.sceneLoaded += RefreshLocalization;
        
        // TODO: trigger RefreshLocalization on select locale as well
    }

    private void RefreshLocalization(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("[LOC] Scene change");

        string localizationCtxPath = scene.name;
        Debug.Log(Application.streamingAssetsPath);
        
        // TODO: switch to streamingAssetPath/locale/scene.csv
        var localizationPath = Path.Join(Application.streamingAssetsPath, scene.name + ".csv");
        loadedAsset = File.Exists(localizationPath) ? new LocalizationHelpers.LocalizationFile(File.ReadAllText(localizationPath)) : null;

        if (loadedAsset == null)
        {
            return;
        }
        
        Dictionary<Type, Action<LocalizationHelpers.Localizable>> localizationMapping = new()
        {
            { typeof(TMP_Text), localizable => loadedAsset.AddEntryTmp(localizable) }
        };
        
        LocalizationHelpers.IterateLocalizableTypes(scene, localizationMapping);
    }
}
