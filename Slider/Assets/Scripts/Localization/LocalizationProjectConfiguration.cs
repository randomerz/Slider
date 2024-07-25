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
        public LocalizationFile.Config name;
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
    #if UNITY_EDITOR
    public static LocalizationProjectConfiguration ScriptableObjectSingleton
    {
        get
        {
            string[] guids = AssetDatabase.FindAssets("t:" + typeof(LocalizationProjectConfiguration).Name);
            var configs = guids.Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<LocalizationProjectConfiguration>).ToArray();
            if (configs.Count() > 1)
            {
                throw new Exception("Must have exactly 1 project configuration!");
            }
            if (configs.Count() == 0)
            {
                throw new Exception("No localization project configuration object exists in asset database");
            }

            return configs.First();
        }
    }
    #endif
    
    #if UNITY_EDITOR
    public IEnumerable<GameObject> ScanProjectForPrefabs()
    {
        return AssetDatabase
            .FindAssets("t:prefab")
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(AssetDatabase.LoadAssetAtPath<GameObject>)
            .Where(go => go.GetComponent<LocalizationInjector>() != null);
    }

    public IEnumerable<GameObject> RelevantPrefabs
    {
        get
        {
            if (relevantPrefabs == null)
            {
                relevantPrefabs = ScanProjectForPrefabs();
            }

            return relevantPrefabs;
        }
    }
    
    private IEnumerable<GameObject> relevantPrefabs = null;
    #endif

    public IEnumerable<LocaleConfiguration> InitialLocales => initialLocales;
    
    [SerializeField]
    private List<LocaleConfiguration> initialLocales;

    #if UNITY_EDITOR
    private void OnValidate()
    {
        if (!initialLocales.Any((loc)=>loc.name.Equals(LocalizationFile.DefaultLocale)))
        {
            initialLocales.Insert(0, new LocaleConfiguration
            {
                name = LocalizationFile.DefaultLocale,
                options = new() // will be filled later on!
            });
        }
        
        foreach(var locale in initialLocales)
        {
            // locale.options.RemoveAll(option => !LocalizationFile.defaultConfigs.ContainsKey(option.name));
            // var names = locale.options.Select(o => o.name).ToHashSet();
            // foreach (var (defaultConfigName, defaultVal) in LocalizationFile.defaultConfigs)
            // {
            //     if (!names.Contains(defaultConfigName))
            //     {
            //         locale.options.Add(new LocaleConfiguration.Option
            //         {
            //             name = defaultConfigName,
            //             value = defaultVal.Value
            //         });
            //     }
            // }
            
            locale.options.Sort((a, b) => a.name.CompareTo(b.name));
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

        if (target == null || target is not LocalizationProjectConfiguration)
        {
            return;
        }

        GUILayout.Label($"Project contains following prefabs with the LocalizationInjector component");
        foreach (var prefab in (target as LocalizationProjectConfiguration).RelevantPrefabs)
        {
            GUILayout.Label($" - {prefab.name}");
        }

        if (GUILayout.Button("Generate"))
        {
            var window = EditorWindow.GetWindow<LocalizationSkeletonGenerator>();
            window.Configuration = target as LocalizationProjectConfiguration;
            window.Show();
        }
    }
}

#endif