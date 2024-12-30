using System;
using System.Collections.Generic;
using System.Linq;
using Localization;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

using LocalizationFile = Localization.LocalizationFile;

public class LocalizationLoader : Singleton<LocalizationLoader>
{
    [SerializeField]
    private TMP_FontAsset localizationFontPixelBig;
    [SerializeField]
    private TMP_FontAsset localizationFontPixelSmall;
    [SerializeField]
    private TMP_FontAsset localizationFontNonPixelBig;
    [SerializeField]
    private TMP_FontAsset localizationFontNonPixelSmall;
    
    private static TMP_FontAsset LocalizationFontPixelBig => _instance.localizationFontPixelBig;
    private static TMP_FontAsset LocalizationFontPixelSmall => _instance.localizationFontPixelSmall;
    private static TMP_FontAsset LocalizationFontNonPixelBig => _instance.localizationFontNonPixelBig;
    private static TMP_FontAsset LocalizationFontNonPixelSmall => _instance.localizationFontNonPixelSmall;
    
    public static TMP_FontAsset LocalizationFont(string originalFamilyName)
    {
        var big = BigFontFamilyNames.Contains(originalFamilyName);
        if (UsePixelFont)
        {
            if (big)
            {
                return LocalizationFontPixelBig;
            }
            else
            {
                return LocalizationFontPixelSmall;
            }
        }
        else
        {
            if (big)
            {
                return LocalizationFontNonPixelBig;
            }
            else
            {
                return LocalizationFontNonPixelSmall;
            }
        }
    }
    
    [SerializeField]
    private TMP_FontAsset[] BigFonts;

    private static HashSet<string> BigFontFamilyNames
    {
        get
        {
            if (_instance._bigFontFamilyNames == null)
            {
                _instance._bigFontFamilyNames = _instance.BigFonts.Select(f => f.faceInfo.familyName).ToHashSet();
            }

            return _instance._bigFontFamilyNames;
        }
    }
    
    private HashSet<string> _bigFontFamilyNames = null;

    public static bool UsePixelFont => SettingsManager.Setting<bool>(Settings.PixelFontEnabled).CurrentValue;

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
        var file = _instance.localeGlobalFile;
        if (file == null)
        {
            return fallback;
        }

        if (file.TryGetRecord(path, out var entry) && entry.TryGetTranslated(out var translated))
        {
            return translated;
        }

        return fallback;
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
            
            var shouldTranslate = !loadedAsset.context.IsDefaultLocale;
            loaded.Localize(loadedAsset.context, shouldTranslate);
            persistent.Localize(loadedAsset.context, shouldTranslate);
        }
    }

    public static void LocalizePrefab(LocalizationInjector injector)
    {
        Debug.Log($"Localize prefab {injector.gameObject} (instance of {injector.prefabName})");
        
        if (_instance == null)
        {
            Debug.LogWarning($"Code ordering: attempting to localize prefab without localization loader instance setup");
            // return;
        }
        
        var locale = CurrentLocale;
        var loadedAsset = LoadAssetAndConfigureLocaleDefaults(locale, LocalizationFile.AssetPath(locale, injector), _instance?.localeGlobalFile);

        if (loadedAsset.context != null)
        {
            var shouldTranslate = !loadedAsset.context.IsDefaultLocale;
            LocalizableContext.ForInjector(injector).Localize(loadedAsset.context, shouldTranslate);
        }
    }
}