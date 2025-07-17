using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Localization;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LocalizationLoader : Singleton<LocalizationLoader>, ILocalizationTrackable
{
    internal static bool UsePixelFont => SettingsManager.Setting<bool>(Settings.PixelFontEnabled).CurrentValue;
    internal static string CurrentLocale => _instance == null ? LocalizationFile.DefaultLocale : SettingsManager.Setting<string>(Settings.Locale).CurrentValue;

    internal static ILocalizationTrackable.LocalizationState CurrentSetting => new ILocalizationTrackable.LocalizationState
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

    public static bool TryLoadFont(string originalFamilyName, LocalizableContext.StyleChange target,
        out TMP_FontAsset font) => _instance.TryLoadFont_Impl(originalFamilyName, target, out font);

    private bool TryLoadFont_Impl(string originalFamilyName, LocalizableContext.StyleChange target, out TMP_FontAsset font)
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
            font = target == LocalizableContext.StyleChange.LocalizedPixel ? localizationFontPixelBig : localizationFontNonPixelBig;
        }
        else
        {
            font = target == LocalizableContext.StyleChange.LocalizedPixel ? localizationFontPixelSmall : localizationFontNonPixelSmall;
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

    private Dictionary<string, SliderFontDescriptor> OriginalFonts => m_originalFonts.Value;
    private Lazy<Dictionary<string, SliderFontDescriptor>> m_originalFonts = new (() =>
    {
        return _instance.originalFonts.ToDictionary(f => f.tmpFont.name, f=>f);
    });

    internal LocalizationFile LocaleGlobalFile;

    ILocalizationTrackable.LocalizationState ILocalizationTrackable.LastLocalizedState => _lastLocalizedState;
    private ILocalizationTrackable.LocalizationState _lastLocalizedState = ILocalizationTrackable.DefaultState;

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
            _lastLocalizedState = ILocalizationTrackable.DefaultState;
            RefreshLocalization();
        };
    }

    #region SpecificTypes

    public static string LoadAreaDiscordTranslation(Area area)
        => _instance.LoadTranslatedString(LocalizableContext.AreaToDiscordNamePath(area), area.ToString());
    
    public static string LoadAreaDisplayName(Area area)
        => _instance.LoadTranslatedString(LocalizableContext.AreaToDisplayNamePath(area), area.ToString());
    
    public static string LoadJungleShapeTranslation(string shapeName)
        => _instance.LoadTranslatedString(LocalizableContext.JungleShapeToPath(shapeName), shapeName);

    public static string LoadCollectibleTranslation(string name, Area area)
        => _instance.LoadTranslatedString(LocalizableContext.CollectibleToPath(name, area), name);

    public static string LoadSpecialItemTranslation(string name) =>
        _instance.LoadTranslatedString(LocalizableContext.SpecialItemToPath(name), name);

    private string LoadTranslatedString(string path, string fallback)
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
    internal string GetCreditInformation() =>
        CurrentLocale == LocalizationFile.DefaultLocale ? null : _instance.LocaleGlobalFile.GetConfigValue(LocalizationFile.Config.Author);

    public static (LocalizationFile global, LocalizationFile context)
        LoadAssetAndConfigureLocaleDefaults(string locale, string sceneLocalizationFilePath) =>
        _instance.LoadAssetAndConfigureLocaleDefaults_Impl(locale, sceneLocalizationFilePath);
    
    private (LocalizationFile global, LocalizationFile context) 
        LoadAssetAndConfigureLocaleDefaults_Impl(string locale, string sceneLocalizationFilePath)
    {
        string localeConfigFilePath = LocalizationFile.LocaleGlobalFilePath(locale);
        if (!assetCache.TryGetValue(localeConfigFilePath, out var globalFile))
        {
            globalFile = LoadGlobalFile(locale, localeConfigFilePath).Result;
        }

        if (!assetCache.TryGetValue(sceneLocalizationFilePath, out var unitFile))
        {
            unitFile = LoadUnitFile(locale, sceneLocalizationFilePath, globalFile).Result;
        }
        return (globalFile, unitFile ?? globalFile); // AT: this is purely for stylistics like non-pixel in dev scenes, won't be hit since all build scenes will have CSV
    }

    public static void RefreshLocalization() => _instance.RefreshLocalization_Impl();
    
    private void RefreshLocalization_Impl()
    {
        StopAllCoroutines();
        StartCoroutine(LoadAllLocaleFilesNextFrame(CurrentSetting.locale));
        
        // var w = new System.Diagnostics.Stopwatch();
        // w.Start();
        
        var scene = SceneManager.GetActiveScene();
        var strategy = (this as ILocalizationTrackable).TrackLocalization(CurrentSetting);
        if (strategy is { ShouldTranslate: false, StyleChange: LocalizableContext.StyleChange.Idle })
        {
            Debug.Log($"[Localization] Skip localization of {this}");
            _lastLocalizedState = CurrentSetting;
            return;
        }
        
        var locale = CurrentLocale;
        var loadedAsset = LoadAssetAndConfigureLocaleDefaults_Impl(locale, LocalizationFile.AssetPath(locale, scene));
        if (loadedAsset.context == null)
        {
            return;
        }
        LocaleGlobalFile = loadedAsset.global;
        
        SceneCtx(scene).Localize(loadedAsset.context, strategy);
        PersistentCtx(GameManager.instance.gameObject.scene).Localize(loadedAsset.context, strategy);
        _lastLocalizedState = CurrentSetting;
        
        // w.Stop();
        // Debug.Log($"[Localization] Elapsed time {w.Elapsed.TotalMilliseconds}ms");
    }

    private ConcurrentDictionary<string, LocalizationFile> assetCache = new();

    async Task<LocalizationFile> LoadGlobalFile(string locale, string fullPath)
    {
        var (localeConfigFile, errorLocale) = await LocalizationFile.MakeLocalizationFile(locale, fullPath);
        if (localeConfigFile == null)
        {
            LocalizationFile.PrintParserError(errorLocale, fullPath);
        }
        else
        {
            Debug.Log($"[Localization] ... cache miss: load file {fullPath}");
            assetCache.TryAdd(fullPath, localeConfigFile);
        }
        return localeConfigFile;
    }

    async Task<LocalizationFile> LoadUnitFile(string locale, string fullPath, LocalizationFile localeConfigFile)
    {
        var (sceneLocalizationFile, errorScene) = await LocalizationFile.MakeLocalizationFile(locale, fullPath, localeConfigFile);
        if (sceneLocalizationFile == null)
        {
            LocalizationFile.PrintParserError(errorScene, fullPath);
        }
        else
        {
            Debug.Log($"[Localization] on frame {Time.frameCount} cache miss: load file {fullPath}");
            assetCache.TryAdd(fullPath, sceneLocalizationFile);
        }

        return sceneLocalizationFile ?? localeConfigFile;
    }

    private IEnumerator LoadAllLocaleFilesNextFrame(string locale)
    {
        Debug.Log($"[Localization] on frame {Time.frameCount} start background CSV loading for locale {locale}");
        yield return new WaitForFixedUpdate();
        
        string localeConfigFilePath = LocalizationFile.LocaleGlobalFilePath(locale);
        if (!assetCache.TryGetValue(localeConfigFilePath, out var localeConfigFile))
        {
            var t = LoadGlobalFile(locale, localeConfigFilePath);
            yield return new WaitUntil(() => t.IsCompleted);
            localeConfigFile = t.Result;
        }
        
        foreach (var f in Directory.GetFiles(Path.GetDirectoryName(localeConfigFilePath)!, "*.csv",
                     SearchOption.TopDirectoryOnly))
        {
            if (assetCache.ContainsKey(f))
            {
                continue;
            }
            var t = LoadUnitFile(locale, f, localeConfigFile);
            yield return new WaitUntil(() => t.IsCompleted);
        }
    }
}
