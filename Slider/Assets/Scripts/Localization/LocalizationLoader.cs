using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
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

    internal static LocalizationLoader Instance => _instance;
    
    private static TMP_FontAsset LocalizationFontPixelBig => _instance.localizationFontPixelBig;
    private static TMP_FontAsset LocalizationFontPixelSmall => _instance.localizationFontPixelSmall;
    private static TMP_FontAsset LocalizationFontNonPixelBig => _instance.localizationFontNonPixelBig;
    private static TMP_FontAsset LocalizationFontNonPixelSmall => _instance.localizationFontNonPixelSmall;
    
    public static bool TryLoadFont(string originalFamilyName, bool localized, out TMP_FontAsset font)
    {
        if (originalFamilyName == null || !OriginalFonts.TryGetValue(originalFamilyName, out var desc))
        {
            font = null;
            return false;
        }

        if (!localized)
        {
            font = desc.tmpFont;
            return true;
        }

        if (desc.isBig)
        {
            font = UsePixelFont ? LocalizationFontPixelBig : LocalizationFontNonPixelBig;
        }
        else
        {
            font = UsePixelFont ? LocalizationFontPixelSmall : LocalizationFontNonPixelSmall;
        }

        return true;
    }

    [System.Serializable]
    public struct SliderFontDescriptor
    {
        public TMP_FontAsset tmpFont;
        public bool isBig;
    }

    [SerializeField] public SliderFontDescriptor[] originalFonts;

    private static Dictionary<string, SliderFontDescriptor> OriginalFonts => m_originalFonts.Value;
    private static Lazy<Dictionary<string, SliderFontDescriptor>> m_originalFonts = new (() =>
    {
        return _instance.originalFonts.ToDictionary(f => f.tmpFont.name, f=>f);
    });

    public static bool UsePixelFont => SettingsManager.Setting<bool>(Settings.PixelFontEnabled).CurrentValue;

    internal LocalizationFile LocaleGlobalFile;
    
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
        var file = _instance.LocaleGlobalFile;
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
    internal static string GetCreditInformation() =>
        CurrentLocale == LocalizationFile.DefaultLocale ? null : _instance.LocaleGlobalFile.GetConfigValue(LocalizationFile.Config.Author);
    
    internal static (LocalizationFile global, LocalizationFile context) LoadAssetAndConfigureLocaleDefaults(string locale, string sceneLocalizationFilePath, LocalizationFile existingGlobal = null)
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
        var loadedAsset = LoadAssetAndConfigureLocaleDefaults(locale, LocalizationFile.AssetPath(locale, scene), LocaleGlobalFile);
        if (loadedAsset.context == null)
        {
            return;
        }

        LocaleGlobalFile = loadedAsset.global;
        
        LocalizableContext loaded = LocalizableContext.ForSingleScene(scene);
        LocalizableContext persistent = LocalizableContext.ForSingleScene(GameManager.instance.gameObject.scene);
        loaded.Localize(loadedAsset.context, UsePixelFont, locale);
        persistent.Localize(loadedAsset.context, UsePixelFont, locale);
    }

    public static void ForceReload()
    {
        _instance?.RefreshLocalization(SceneManager.GetActiveScene());
        // TODO: implement
    }
}

// defining this here due to lots of shared logic with regular localization loader usage
public static class LocalizationInjectorExtensions
{
    public static void InjectLocalization(this LocalizationInjector injector)
    {
            
        Debug.Log($"Localize prefab {injector.gameObject} (instance of {injector.prefabName})");
        
        var locale = LocalizationLoader.CurrentLocale;

        var prefabAssetPath = LocalizationFile.AssetPath(locale, injector);
        var loadedAsset = LocalizationLoader.LoadAssetAndConfigureLocaleDefaults(locale, prefabAssetPath, LocalizationLoader.Instance?.LocaleGlobalFile);

        if (loadedAsset.context == null)
        {
            return;
        }
        
        LocalizableContext.ForInjector(injector).Localize(loadedAsset.context, LocalizationLoader.UsePixelFont, locale);
    }
}