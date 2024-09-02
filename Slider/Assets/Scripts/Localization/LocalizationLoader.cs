using System;
using Localization;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

using LocalizationFile = Localization.LocalizationFile;

public class LocalizationLoader : Singleton<LocalizationLoader>
{
    [SerializeField]
    private TMP_FontAsset localizationFont;

    public static TMP_FontAsset LocalizationFont => _instance.localizationFont;

    private LocalizationFile localeGlobalFile;
    
    private void Awake()
    {
        InitializeSingleton();
    }

    private void Start()
    {
        RefreshLocalization(SceneManager.GetActiveScene());

        SceneManager.activeSceneChanged += (_, to) =>
        {
            RefreshLocalization(to);
        };
    }

    public static void RefreshLocalization()
    {
        if (_instance != null)
        {
            _instance.RefreshLocalization(SceneManager.GetActiveScene()); // do not use GameObject.Scene since it will return the non destructable scene instead!
        }
    }
    
    public static string CurrentLocale => _instance == null ? LocalizationFile.DefaultLocale : SettingsManager.Setting<string>(Settings.Locale).CurrentValue;

    #region SpecificTypes

    public static string LoadAreaDiscordTranslation(Area area)
    {
        if (
            _instance?.localeGlobalFile == null 
            || !_instance.localeGlobalFile.records.TryGetValue(
                SpecificTypeHelpers.AreaToDiscordNamePath(area), 
                out var translation))
        {
            return area.ToString();
        }
        
        return translation.Translated ?? area.ToString();
    }
    
    public static string LoadAreaDisplayName(Area area)
    {
        if (
            _instance?.localeGlobalFile == null 
            || !_instance.localeGlobalFile.records.TryGetValue(
                SpecificTypeHelpers.AreaToDisplayNamePath(area), 
                out var translation))
        {
            return area.ToString();
        }
        
        return translation.Translated ?? area.ToString();
    }
    
    public static string LoadJungleShapeTranslation(string shapeName)
    {
        if (
            _instance?.localeGlobalFile == null 
            || !_instance.localeGlobalFile.records.TryGetValue(
                SpecificTypeHelpers.JungleShapeToPath(shapeName), 
                out var translation))
        {
            return shapeName;
        }

        return translation.Translated ?? shapeName;
    }
    
    #endregion
    
    private static (LocalizationFile global, LocalizationFile context) LoadAssetAndConfigureLocaleDefaults(string locale, string sceneLocalizationFilePath, LocalizationFile existingGlobal = null)
    {
        LocalizationFile globalFile;
        
        if ((existingGlobal?.LocaleName ?? "") != locale)
        {
            string localeConfigFilePath = LocalizationFile.LocaleGlobalFilePath(locale);
            var (localeConfigFile, errorLocale) = LocalizationFile.MakeLocalizationFile(locale, localeConfigFilePath);
            if (localeConfigFile == null)
            {
                LocalizationFile.PrintParserError(errorLocale, localeConfigFilePath);
                return (null, null);
            }
            globalFile = localeConfigFile;
        }
        else
        {
            globalFile = existingGlobal;
        }
        
        var (sceneLocalizationFile, errorScene) = LocalizationFile.MakeLocalizationFile(locale, sceneLocalizationFilePath, globalFile);
        if (sceneLocalizationFile == null)
        {
            LocalizationFile.PrintParserError(errorScene, sceneLocalizationFilePath);
        }
        return (globalFile, sceneLocalizationFile ?? globalFile); // AT: this is purely for stylistics like non-pixel in dev scenes, won't be hit since all build scenes will have CSV
    }
    
    private void RefreshLocalization(Scene scene)
    {
        var locale = CurrentLocale;
        var loadedAsset = LoadAssetAndConfigureLocaleDefaults(locale, LocalizationFile.AssetPath(locale, scene), localeGlobalFile);

        localeGlobalFile = loadedAsset.global;
        
        if (loadedAsset.context != null)
        {
            LocalizableContext loaded = LocalizableContext.ForSingleScene(scene);
            LocalizableContext persistent = LocalizableContext.ForSingleScene(GameManager.instance.gameObject.scene);
            
            loaded.Localize(loadedAsset.context);
            persistent.Localize(loadedAsset.context);
        }
    }

    public static void LocalizePrefab(GameObject prefab)
    {
        if (_instance == null)
        {
            Debug.LogWarning($"Attempting to localize prefab {prefab} without a localization loader singleton");
            // return;
        }
        
        var locale = CurrentLocale;
        var loadedAsset = LoadAssetAndConfigureLocaleDefaults(locale, LocalizationFile.AssetPath(locale, prefab), _instance?.localeGlobalFile);

        if (loadedAsset.context != null)
        {
            LocalizableContext.ForSinglePrefab(prefab).Localize(loadedAsset.context);
        }
    }
}