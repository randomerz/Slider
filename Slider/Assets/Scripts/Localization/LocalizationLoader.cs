using System;
using System.IO;
using Localization;
using UnityEngine;
using UnityEngine.SceneManagement;

using LocalizationFile = Localization.LocalizationFile;

public static class LocalizationLoader
{
    public static void RefreshLocalization(Scene scene, string locale)
    {
        LocalizableScene loaded = new(scene);
        LocalizableScene persistent = new(GameManager.instance.gameObject.scene);

        LocalizationFile loadedAsset = null;

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