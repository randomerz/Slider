using System;
using System.Collections.Generic;
using System.Linq;
using Localization;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public struct LocaleConfiguration
{
    [System.Serializable]
    public struct Option
    {
        public string name;
        public string value;
    }
    
    public string name;
    public List<Option> options;

    public static LocaleConfiguration Default
    {
        get
        {
            LocaleConfiguration defaults = new LocaleConfiguration
            {
                name = LocalizationFile.DefaultLocale,
                options = LocalizationFile.defaultConfigs.Select((kv) =>
                {
                    var (k, v) = kv;
                    return new Option
                    {
                        name = k,
                        value = v.Value
                    };
                }).ToList()
            };

            return defaults;
        }
    }
}

[CreateAssetMenu]
public class LocalizationProjectConfiguration : ScriptableObject
{
    public GameObject[] RelevantPrefabs;
    
    [SerializeField]
    private LocaleConfiguration[] InitialAdditionalLocales;

    public IEnumerable<LocaleConfiguration> InitialLocales =>
        InitialAdditionalLocales.Append(LocaleConfiguration.Default);

    #if UNITY_EDITOR
    private void OnValidate()
    {
        foreach(var locale in InitialAdditionalLocales)
        {
            locale.options.RemoveAll(option => !LocalizationFile.defaultConfigs.ContainsKey(option.name));
            var names = locale.options.Select(o => o.name).ToHashSet();
            foreach (var (defaultConfigName, defaultVal) in LocalizationFile.defaultConfigs)
            {
                if (!names.Contains(defaultConfigName))
                {
                    locale.options.Add(new LocaleConfiguration.Option
                    {
                        name = defaultConfigName,
                        value = defaultVal.Value
                    });
                }
            }
            
            locale.options.Sort((a, b) => string.Compare(a.name, b.name, StringComparison.Ordinal));
        }
    }
    #endif
}

#if UNITY_EDITOR

[CustomEditor(typeof(LocalizationProjectConfiguration))]
public class LocalizationProjectConfigurationEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Generate"))
        {
            var window = EditorWindow.GetWindow<LocalizationSkeletonGenerator>();
            window.Configuration = target as LocalizationProjectConfiguration;
            window.Show();
        }
    }
}

#endif