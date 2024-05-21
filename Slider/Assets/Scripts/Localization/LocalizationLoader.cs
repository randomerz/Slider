using Localization;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

using LocalizationFile = Localization.LocalizationFile;

[RequireComponent(typeof(SettingRetriever))]
public class LocalizationLoader : Singleton<LocalizationLoader>
{
    [SerializeField]
    private TMP_FontAsset localizationFont;
    
    [SerializeField]
    private TMP_FontAsset defaultUiFont;

    public static TMP_FontAsset LocalizationFont => _instance.localizationFont;
    public static TMP_FontAsset DefaultUiFont => _instance.defaultUiFont;
    
    private void Awake()
    {
        InitializeSingleton();
    }

    private void Start()
    {
        RefreshLocalization(SceneManager.GetActiveScene());
    }

    public static void RefreshLocalization()
    {
        if (_instance != null)
        {
            RefreshLocalization(SceneManager.GetActiveScene()); // do not use GameObject.Scene since it will return the non destructable scene instead!
        }
    }
    
    private static string CurrentLocale => _instance == null ? LocalizationFile.DefaultLocale : _instance.GetComponent<SettingRetriever>().ReadSettingValue() as string;

    private static LocalizationFile LoadAssetAndConfigureLocaleDefaults(string locale, string assetPath) 
    {
        string localeConfigsPath = LocalizationFile.LocaleGlobalFilePath(locale);
        var localeConfig = LocalizationFile.MakeLocalizationFile(locale, localeConfigsPath);
        if (localeConfig == null)
        {
            Debug.LogError($"{locale} missing global configuration file, check for {localeConfigsPath}");
        }
        return LocalizationFile.MakeLocalizationFile(locale, assetPath, localeConfig);
    }
    
    private static void RefreshLocalization(Scene scene)
    {
        var locale = CurrentLocale;
        
        LocalizableContext loaded = LocalizableContext.ForSingleScene(scene);
        LocalizableContext persistent = LocalizableContext.ForSingleScene(GameManager.instance.gameObject.scene);

        var loadedAsset = LoadAssetAndConfigureLocaleDefaults(locale, LocalizationFile.AssetPath(locale, scene));
        loaded.Localize(loadedAsset);
        persistent.Localize(loadedAsset);
    }

    public static void LocalizePrefab(GameObject prefab)
    {
        var locale = CurrentLocale;
        LocalizableContext ctx = LocalizableContext.ForSinglePrefab(prefab);
        var loadedAsset = LoadAssetAndConfigureLocaleDefaults(locale, LocalizationFile.AssetPath(locale, prefab));
        ctx.Localize(loadedAsset);
    }
}