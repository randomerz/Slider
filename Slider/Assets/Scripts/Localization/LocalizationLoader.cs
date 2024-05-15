using System;
using System.IO;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using Object = UnityEngine.Object;

public class LocalizationLoader : Singleton<LocalizationLoader>
{
    private string localizationCtxPath;
    private FileInfo loadedAsset;
    
    private void OnEnable()
    {
        SceneManager.sceneLoaded += RefreshLocalization;
        
        // TODO: trigger RefreshLocalization on select locale as well
    }

    private void RefreshLocalization(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("[LOC] Scene change");

        localizationCtxPath = scene.name;
        Debug.Log(Application.streamingAssetsPath);
        
        // TODO: switch to streamingAssetPath/locale/scene.csv
        var localizationPath = Path.Join(Application.streamingAssetsPath, scene.name + ".csv");
        loadedAsset = File.Exists(localizationPath) ? new FileInfo(localizationPath) : null;

        Dictionary<Type, Action<Object>> localizationMapping = new()
        {
            { typeof(TMP_Text), o => LocalizeTextMeshProGUI(o as TMP_Text) }
        };

        foreach (var (locType, locAction) in localizationMapping)
        {
            var instances = FindObjectsByType(locType, FindObjectsSortMode.None);
            foreach (var instance in instances)
            {
                locAction(instance);
            }
        }
    }

    private void LocalizeTextMeshProGUI(TMP_Text tmp)
    {
        if (tmp == null)
        {
            return;
        }

        if (loadedAsset != null)
        {
            tmp.text = loadedAsset.Name;
        }
        else
        {
            tmp.text = GetPath(tmp.gameObject);
        }
    }

    private string TryLocalizeText(string orig)
    {
        
        
        return orig;
    }
    
    public static string GetPath(GameObject current)
    {
        return string.Join('/', current.GetComponentsInParent<Transform>().Select(t => t.name).Reverse());
    }
}
