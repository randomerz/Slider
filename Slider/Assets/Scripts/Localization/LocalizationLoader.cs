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
    
    [SerializeField]
    private TMP_FontAsset localizationFontNonPixel;

    private static TMP_FontAsset LocalizationFontPixel => _instance.localizationFont;
    private static TMP_FontAsset LocalizationFontNonPixel => _instance.localizationFontNonPixel;

    public static TMP_FontAsset LocalizationFont =>
        SettingsManager.Setting<bool>(Settings.PixelFontEnabled).CurrentValue
            ? LocalizationFontPixel
            : LocalizationFontNonPixel;

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
        => LoadTranslatedString(SpecificTypeHelpers.AreaToDiscordNamePath(area), area.ToString());
    
    public static string LoadAreaDisplayName(Area area)
        => LoadTranslatedString(SpecificTypeHelpers.AreaToDisplayNamePath(area), area.ToString());
    
    public static string LoadJungleShapeTranslation(string shapeName)
        => LoadTranslatedString(SpecificTypeHelpers.JungleShapeToPath(shapeName), shapeName);

    public static string LoadCollectibleTranslation(string name, Area area)
        => LoadTranslatedString(SpecificTypeHelpers.CollectibleToPath(name, area), name);

    private static string LoadTranslatedString(string path, string fallback)
    {
        if (
            _instance?.localeGlobalFile == null 
            || !_instance.localeGlobalFile.records.TryGetValue(
                path, 
                out var translation))
        {
            return fallback;
        }

        return translation.Translated ?? fallback;
    }
    
    #endregion

    /// <summary>
    /// Null on English, for everything else defaults to 'Anonymous' if not specified.
    /// </summary>
    /// <returns></returns>
    private static string GetCreditInformation() =>
        CurrentLocale == LocalizationFile.DefaultLocale ? null : _instance.localeGlobalFile.GetConfigValue(LocalizationFile.Config.Author);
    
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