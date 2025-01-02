using System;
using System.Collections.Generic;
using System.Linq;
using Localization;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LocalizationLoader : Singleton<LocalizationLoader>, ILocalizationTrackable
{
    internal static bool UsePixelFont => SettingsManager.Setting<bool>(Settings.PixelFontEnabled).CurrentValue;
    internal static string CurrentLocale => _instance == null ? LocalizationFile.DefaultLocale : SettingsManager.Setting<string>(Settings.Locale).CurrentValue;

    internal static ILocalizationTrackable.LocalizationState ToCurrentSetting => new ILocalizationTrackable.LocalizationState
    {
        locale = CurrentLocale,
        usePixelFont = UsePixelFont
    };
    
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
    
    public static bool TryLoadFont(string originalFamilyName, LocalizableContext.StyleChange target, out TMP_FontAsset font)
    {
        if (originalFamilyName == null || !OriginalFonts.TryGetValue(originalFamilyName, out var desc))
        {
            font = null;
            return false;
        }

        if (target == LocalizableContext.StyleChange.Idle)
        {
            throw new Exception("Nonsense code path");
        }

        if (target == LocalizableContext.StyleChange.DefaultPixel)
        {
            font = desc.tmpFont;
            return true;
        }

        if (desc.isBig)
        {
            font = (target == LocalizableContext.StyleChange.LocalizedPixel) ? LocalizationFontPixelBig : LocalizationFontNonPixelBig;
        }
        else
        {
            font = (target == LocalizableContext.StyleChange.LocalizedPixel) ? LocalizationFontPixelSmall : LocalizationFontNonPixelSmall;
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

    internal LocalizationFile LocaleGlobalFile;

    ILocalizationTrackable.LocalizationState ILocalizationTrackable.Record { get; set; } = ILocalizationTrackable.DefaultRecord;

    private void Awake()
    {
        InitializeSingleton();
    }

    private LocalizableContext SceneCtx(Scene current)
    {
        if (current != _scene)
        {
            _scene = current;
            _sceneCtx = LocalizableContext.ForSingleScene(current);
        }

        return _sceneCtx;
    }
    private Scene? _scene;
    private LocalizableContext _sceneCtx;

    private LocalizableContext PersistentCtx(Scene current)
    {
        if (current != _persistent)
        {
            _persistent = current;
            _persistentCtx = LocalizableContext.ForSingleScene(current);
        }

        return _persistentCtx;
    }
    private Scene? _persistent;
    private LocalizableContext _persistentCtx;

    private void Start()
    {
        RefreshLocalization();
        SceneManager.activeSceneChanged += (_, to) =>
        {
            RefreshLocalization();
        };
    }

    #region SpecificTypes

    public static string LoadAreaDiscordTranslation(Area area)
        => LoadTranslatedString(LocalizableContext.AreaToDiscordNamePath(area), area.ToString());
    
    public static string LoadAreaDisplayName(Area area)
        => LoadTranslatedString(LocalizableContext.AreaToDisplayNamePath(area), area.ToString());
    
    public static string LoadJungleShapeTranslation(string shapeName)
        => LoadTranslatedString(LocalizableContext.JungleShapeToPath(shapeName), shapeName);

    public static string LoadCollectibleTranslation(string name, Area area)
        => LoadTranslatedString(LocalizableContext.CollectibleToPath(name, area), name);

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
    
    internal static (LocalizationFile global, LocalizationFile context) 
        LoadAssetAndConfigureLocaleDefaults(string locale, string sceneLocalizationFilePath, LocalizationFile existingGlobal = null)
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

    public static void RefreshLocalization() => _instance.RefreshLocalizationImpl();
    
    private void RefreshLocalizationImpl()
    {
        var scene = SceneManager.GetActiveScene();
        
        var locale = CurrentLocale;
        var loadedAsset = LoadAssetAndConfigureLocaleDefaults(locale, LocalizationFile.AssetPath(locale, scene), LocaleGlobalFile);
        if (loadedAsset.context == null)
        {
            return;
        }

        LocaleGlobalFile = loadedAsset.global;
        SceneCtx(scene).Localize(loadedAsset.context, ToCurrentSetting);
        PersistentCtx(GameManager.instance.gameObject.scene).Localize(loadedAsset.context, ToCurrentSetting);
    }
}
