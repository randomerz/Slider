using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;
using TMPro;
using Sylvan.Data.Csv;
using UnityEngine.SceneManagement;

namespace Localization
{
    public static class SpecificTypeHelpers
    {
        public static string JungleShapeToPath(string shapeName) => $"__JungleShapes/{shapeName}";
        public static string CollectibleToPath(string collectible, Area area) => $"__Collectibles/{area.ToString()}/{collectible}";

        public static string AreaToDiscordNamePath(Area area) => $"__DiscordMessages/{area.ToString()}";

        public static string AreaToDisplayNamePath(Area area) => $"__AreaDisplayName/{area.ToString()}";
    }
    
    public struct LocalizationPair
    {
        public static explicit operator LocalizationPair(string input)
        {
            return new LocalizationPair
            {
                original = input,
                translated = input
            };
        }
        
        //
        // public static implicit operator LocalizationPair((string, string) input)
        // {
        //     return new LocalizationPair
        //     {
        //         original = input.Item1,
        //         translated = input.Item2
        //     };
        // }

        public static implicit operator (string, string)(LocalizationPair self)
        {
            return (self.original, self.translated);
        }

        public string original;
        public string translated;

        public string TranslatedFallbackToOriginal => translated ?? original;

        public static LocalizationPair operator +(LocalizationPair a, LocalizationPair b)
            => new LocalizationPair
            {
                original = a.original + b.original,
                translated = a.translated + b.translated
            };

        public static bool operator ==(LocalizationPair a, LocalizationPair b) => a.original == b.original;

        public static bool operator !=(LocalizationPair a, LocalizationPair b) => !(a == b);

        public static LocalizationPair Join(string separator, IEnumerable<LocalizationPair> pairs)
        {
            var localizationPairs = pairs as LocalizationPair[] ?? pairs.ToArray();
            return new LocalizationPair
            {
                original = string.Join(separator, localizationPairs.Select(pair => pair.original)),
                translated = string.Join(separator, localizationPairs.Select(pair => pair.translated))
            };
        }
        
        public static LocalizationPair Join(char separator, IEnumerable<LocalizationPair> pairs)
        {
            var localizationPairs = pairs as LocalizationPair[] ?? pairs.ToArray();
            return new LocalizationPair
            {
                original = string.Join(separator, localizationPairs.Select(pair => pair.original)),
                translated = string.Join(separator, localizationPairs.Select(pair => pair.translated))
            };
        }
    }
    
    internal abstract class Localizable
    {
        private string[] componentPath;

        public string IndexInComponent => index;
        protected string index;

        // Separates the index string from the hierarchy path
        public static char indexSeparator = '@';
        
        // Separates different digits within an index string
        public static char indexSeparatorSecondary = ':';

        public string FullPath => string.Join('/', componentPath) + (index != null ? indexSeparator + index : "");

        protected Localizable(string[] componentPath)
        {
            this.componentPath = componentPath;
        }

        protected static string[] GetComponentPath(Component current)
        {
            List<string> pathComponents = new();
            for (Transform go = current.transform; go != null; go = go.parent)
            {
                pathComponents.Add(go.name);
            }

            pathComponents.Reverse();
            return pathComponents.ToArray();
        }
    }

    internal struct SerializedLocalizableData
    {
        public string text;
        public string metadata;

        public static implicit operator SerializedLocalizableData(string s)
        {
            return new SerializedLocalizableData
            {
                text = s,
                metadata = null
            };
        }

        public bool IsEmpty => string.IsNullOrWhiteSpace(text) && string.IsNullOrWhiteSpace(metadata);
    }

    internal sealed class ParsedLocalizable : Localizable
    {
        private readonly SerializedLocalizableData _data;
        
        public string Translated => _data.text;
        public string Metadata => _data.metadata;

        private ParsedLocalizable(string hierarchyPath, string index, SerializedLocalizableData data) : base(hierarchyPath.Split('/'))
        {
            this.index = index;
            _data = data;
        }
        
        internal static ParsedLocalizable ParsePath(string fullPath, SerializedLocalizableData data)
        {
            string[] pathAndIndex = fullPath.Split(indexSeparator);

            if (pathAndIndex.Length < 1)
            {
                return null;
            }

            if (pathAndIndex.Length == 1)
            {
                return new ParsedLocalizable(pathAndIndex[0], null, data);
            }

            try
            {
                return new ParsedLocalizable(pathAndIndex[0], pathAndIndex[1], data);
            }
            catch (FormatException)
            {
                return null;
            }
        }
    }

    // Record that tracks a localizable instance in the scene, in the form of a Component + an index within that component
    internal sealed class TrackedLocalizable : Localizable
    {
        // Immediate component that this localizable tracks, several of them can track the same component
        // in the case of different indices (different dialogue numbers of the same NPC, for example)
        public T GetAnchor<T>() where T: class => (_anchor as T);
        private readonly Component _anchor;

        public LocalizableContext.LocalizationStrategy Strategy =
            LocalizableContext.LocalizationStrategy.TranslateTextAndChangeStyle;

        private TrackedLocalizable(Component c) : base(GetComponentPath(c))
        {
            _anchor = c;
        }

        public TrackedLocalizable(TMP_Text tmp) : this(tmp as Component)
        {
        }

        // Track dropdown option
        public TrackedLocalizable(TMP_Dropdown dropdown, int idx) : this(dropdown as Component)
        {
            index = idx.ToString();
        }
        
        // Track dropdown itself, not particular option
        public TrackedLocalizable(TMP_Dropdown dropdown) : this(dropdown as Component)
        {
            index = null;
        }

        public TrackedLocalizable(NPC npc, int cond, int diag) : this(npc as Component)
        {
            index = cond.ToString() + Localizable.indexSeparatorSecondary + diag.ToString();
        }

        public TrackedLocalizable(TMPTextTyper typer) : this(typer as Component)
        {
            index = null;
        }

        public TrackedLocalizable(LocalizationInjector injector) : this(injector as Component)
        {
            index = null;
        }

        public TrackedLocalizable(PlayerActionHints hint, int idx) : this(hint as Component)
        {
            index = idx.ToString();
        }

        public TrackedLocalizable(IDialogueTableProvider dialogueTableProvider, string code) : this(dialogueTableProvider as Component)
        {
            index = code;
        }
        
        public TrackedLocalizable(ArtifactInventoryCollectible collectibleUI) : this(collectibleUI as Component)
        {
            index = null;
        }
    }

    // TODO: try add typed parsing cache
    internal readonly struct LocalizationConfig
    {
        public readonly string Comment;
        public readonly string Value;

        internal LocalizationConfig(string comment, string value)
        {
            Comment = comment;
            Value = value;
        }

        internal LocalizationConfig Override(string value)
        {
            return new LocalizationConfig(Comment, value);
        }
    }

    public class LocalizationFile
    {
        public static string LocalizationRootPath(string root = null) => root ?? Application.streamingAssetsPath;
        
        public static string LocalizationFolderPath(string root = null) => Path.Join(LocalizationRootPath(root), "Localizations");

        public static List<string> LocaleList(string playerPrefLocale, string root = null)
        {
            if (Directory.Exists(LocalizationFolderPath(root)))
            {
                var localeNames = Directory.GetDirectories(LocalizationFolderPath(root))
                    .Select(path => new FileInfo(path).Name)
                    // very expensive check, makes sure that only the legit locales are selected
                    #if DEVELOPMENT_BUILD || UNITY_EDITOR
                    #else
                    .Where(localeName =>
                    {
                        var filePath = LocaleGlobalFilePath(localeName, root);
                        var (parsedGlobalFile, err) = MakeLocalizationFile(localeName, filePath);
                        if (parsedGlobalFile != null)
                        {
                            return true;
                        } else {
                            PrintParserError(err, filePath);
                            return false;
                        }
                    })
                    #endif
                    .ToList();
                    
                    localeNames.Sort(
                    (localeA, localeB) =>
                    {
                        if (localeA.Equals(playerPrefLocale))
                        {
                            return -1;
                        }

                        if (localeB.Equals(playerPrefLocale))
                        {
                            return 1;
                        }
                        
                        if (localeA.Equals(DefaultLocale))
                        {
                            return -1;
                        }

                        if (localeB.Equals(DefaultLocale))
                        {
                            return 1;
                        }

                        // ReSharper disable once StringCompareToIsCultureSpecific
                        return localeA.ToLower().CompareTo(localeB.ToLower());
                    });

                return localeNames;
            }
            else
            {
                return new List<string>();
            }
        }

        private static string LocalizationFileName(Scene scene) => scene.name + "_scene.csv";
        private static string LocalizationFileName(GameObject prefab) => prefab.name + "_prefab.csv";

        private static string LocaleGlobalFileName(string locale) => $"_{locale}_configs.csv";

        public static string LocaleGlobalFilePath(string locale, string root = null) =>
            Path.Join(LocalizationFolderPath(root), locale, LocaleGlobalFileName(locale));
        
        public static string DefaultLocale => "English";
        public static string TestingLanguage => "Debug";
        
        public static string AssetPath(string locale, Scene scene, string root = null) =>
            Path.Join(LocalizationFolderPath(root), locale, LocalizationFileName(scene));
        public static string AssetPath(string locale, GameObject prefab, string root = null) =>
            Path.Join(LocalizationFolderPath(root), locale, LocalizationFileName(prefab));
        
        // AT: This should really be a variable in LocalizableScene, but I'm placing it here
        //     so I don't have to re-type this whole thing as comments to the LocalizationFile
        //     class...
        internal static readonly string explainer = @"Localization files are formatted like CSV so they can be easily edited,
however formatting has strict rules for easy parsing. Most of these rules
are already handled by Excel / Libre / etc., so you don't have to worry
about them in normal usage. However, if your localization file seems to
be corrupted, these rules may be helpful for debugging purposes...
- The first 4 rows can be arbitrarily long
  - The first cell of first row is used to store this block of instruction
    text
  - The first 3 cells of the fourth row is used as headers
  - All other cells in the first and fourth rows are ignored
  - Columns in the second row alternate between property and value (ex.
    ' foo | 1 | bar | 3.5 | ... '
    - Any whitespaces before and after the property name are ignored
  - Columns in the third row are exclusively comments for properties in the
    second row
- Every following line can include any number of cells, but only the
  first four will be parsed (you can use the rest as comments if needed)
  - The fourth column is an optional metadata column
";

        internal static readonly char csvSeparator = ',';

        public enum Config
        {
            IsValid,
            NonDialogueFontScale,
            DialogueFontScale,
            Author
        }

        public static readonly Dictionary<Config, string> ConfigToName = 
            Enum
                .GetValues(typeof(Config))
                .Cast<Config>()
                .ToDictionary(
                config => config,
                config => Enum.GetName(typeof(Config), config));

        private static readonly Dictionary<string, Config> ConfigFromName =
            ConfigToName.ToDictionary(kv => kv.Value, kv => kv.Key);

        internal static SortedDictionary<Config, LocalizationConfig> defaultConfigs = new()
        {
            {
                Config.IsValid, new LocalizationConfig(
                "Set to 1 allow the parser to read this file, otherwise it will be skipped and the default English localization will be used as fallback",
                "1")
            },
            {
                Config.Author, new LocalizationConfig(
                    "The author(s) of this file, will appear in credits", "Anonymous")
            },
            {
                Config.NonDialogueFontScale, new LocalizationConfig(
                    "Float value that scales font size of all non-dialogue text, 1.0 for full size.", "1.0")
            },

            {
                Config.DialogueFontScale, new LocalizationConfig(
                    "Float value that scales font size of all dialogue text, 1.0 for full size.", "1.0")
            },
            
            // { fontOverridePath_name, new LocalizationConfig(
            //     "Relative path to a ttf font file from the folder containing this file, leave empty if no such font is needed", 
            //     "")
            // }
        };

        private SortedDictionary<Config, LocalizationConfig> configs;
        private SortedDictionary<string, ParsedLocalizable> records;

        public string LocaleName => locale;
        private string locale;
        public bool IsDefaultLocale => locale.Equals(DefaultLocale);
        
        // enum ParserState
        // {
        //     Empty,
        //     HasPath,
        //     HasOriginal,
        //     HasTranslation,
        //     Inval
        // }

        public enum ParserError
        {
            NoError,
            FileNotFound,
            ExplicitlyDisabled
        }

        public static void PrintParserError(ParserError error, string path)
        {
            switch (error)
            {
                case ParserError.NoError:
                    Debug.Log($"[Localization] Localization file parser: null file without error thrown at {path}");
                    break;
                case ParserError.FileNotFound:
                    Debug.Log($"[Localization] Localization file parser: file not found {path}");
                    break;
                case ParserError.ExplicitlyDisabled:
                    Debug.LogWarning($"[Localization] Localization file parser: localization file at {path} explicitly disabled");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static (LocalizationFile, ParserError) MakeLocalizationFile(string locale, string filePath,
            LocalizationFile localeConfig = null)
        {
            if (!File.Exists(filePath))
            {
                return (null, ParserError.FileNotFound);
            }

            using var file = File.OpenRead(filePath);
            LocalizationFile parsed = new(locale, filePath, localeConfig);
            
            #if DEVELOPMENT_BUILD || UNITY_EDITOR
            #else
            if (!parsed.TryParseConfigValue(Config.IsValid, out int isValidFlag) || isValidFlag != 1)
            {
                return (null, ParserError.ExplicitlyDisabled);
            }
            #endif
            return (parsed, ParserError.NoError);
        }
        
        private LocalizationFile(string locale, string filePath, LocalizationFile localeConfig = null)
        {
            this.locale = locale;
            
            records = new();

            configs = new();
            foreach (var (k, v) in (localeConfig != null ? localeConfig.configs : defaultConfigs))
            {
                configs.Add(k, new LocalizationConfig(v.Comment, v.Value));
            }

            var opts = new CsvDataReaderOptions
            {
                HasHeaders = false
            };
            
            using var reader = CsvDataReader.Create(filePath, opts);
            for (var currentRow = 0; reader.Read(); currentRow++)
            {
                if (currentRow < 4)
                {
                    // second row of the file are property/value pairs
                    if (currentRow == 1)
                    {
                        ParsePropertiesRow(reader);
                    }

                    continue;
                }
                    
                if (reader.RowFieldCount < 3)
                {
                    switch (reader.RowFieldCount)
                    {
                        case 1:
                            Debug.LogError($"[Localization] Inval: only path {reader.GetString(0)}");
                            break;
                        case 2:
                            Debug.LogError($"[Localization] Inval: Only path orig {reader.GetString(0)} : {reader.GetString(1)}");
                            break;
                        default: break;
                    }
                }
                else
                {
                    ParseRecordRow(reader);
                }
            }
        }
        
        
        private void ParsePropertyValue(string propertyName, string value)
        {
            // property value
            if (ConfigFromName.TryGetValue(propertyName, out var propertyEnum))
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    configs[propertyEnum] = configs[propertyEnum].Override(value);
                    // Debug.Log($"[Localization] Parsed localization config: {property} = {content}");
                }
            }
            else
            {
                Debug.LogError($"[Localization] Unrecognized localization config {propertyName} with value {value}");
            }
        }

        private void ParsePropertiesRow(CsvDataReader reader)
        {
            var propertyName = "";
            for (var i = 0; i < reader.RowFieldCount; i++)
            {
                var cell = reader.GetString(i);
                if (i % 2 == 0)
                {
                    propertyName = cell;
                }
                else
                {
                    ParsePropertyValue(propertyName, cell);
                }
            }
        }

        private void ParseRecordRow(CsvDataReader reader)
        {
            var path = reader.GetString(0);
            var newRecord = ParsedLocalizable.ParsePath(path, 
                new SerializedLocalizableData
                {
                    text = reader.GetString(2),
                    metadata = reader.RowFieldCount > 3 ? reader.GetString(3) : null
                });
            if (newRecord == null)
            {
                Debug.LogError($"[Localization] Null localizable parsed {path}");
            }
            else
            {
                records.Add(path, newRecord);
            }
        }
        
        private Dictionary<Config, int> configParsingCacheInt = new();
        private Dictionary<Config, float> configParsingCacheFloat = new();

        internal string GetConfigValue(Config key)
        {
            if (configs.TryGetValue(key, out LocalizationConfig valStr))
            {
                return valStr.Value;
            }

            return defaultConfigs[key].Value;
        }
        
        internal bool TryParseConfigValue(Config key, out int value)
        {
            // don't try to refactor this with T:IParsable<T>, Unity C# doesn't have it :)
            if (configParsingCacheInt.TryGetValue(key, out var cached))
            {
                value = cached;
                return true;
            }
            if (configs.TryGetValue(key, out LocalizationConfig valStr) && int.TryParse(valStr.Value, out var parsed))
            {
                value = parsed;
                return true;
            }

            value = -1;
            return false;
        }
        
        internal bool TryParseConfigValue(Config key, out float value)
        {
            // don't try to refactor this with T:IParsable<T>, Unity C# doesn't have it :)
            if (configParsingCacheFloat.TryGetValue(key, out var cached))
            {
                value = cached;
                return true;
            }
            if (configs.TryGetValue(key, out LocalizationConfig valStr) && float.TryParse(valStr.Value, out var parsed))
            {
                value = parsed;
                return true;
            }

            value = -1.0f;
            return false;
        }

        internal ParsedLocalizable GetRecord(string path)
        {
            if (!records.TryGetValue(path, out var entry))
            {
                Debug.LogWarning($"[Localization] {path}: NOT FOUND");
            }
            return entry;
        }
    }

    public class LocalizableContext
    {
        private Scene? sceneContext = null;
        private GameObject subcontextAnchor = null;
        Dictionary<Type, List<TrackedLocalizable>> localizables = new();
        private Dictionary<string, string> ImportedGlobalStrings = new();
        public Dictionary<string, string> GlobalStringsToExport = new(); // strings encountered during the context parsing process, but won't be used within the context (rather, for a locale global file)

        private SortedDictionary<LocalizationFile.Config, LocalizationConfig> configs;

        private LocalizableContext()
        {
            configs = new();
            foreach (var (k, v) in LocalizationFile.defaultConfigs)
            {
                configs.Add(k, new LocalizationConfig(v.Comment, v.Value));
            }
            
            ////////////////////// STRING EXPORT FUNCTIONS /////////////////////////////////////////////////////////////
            SelectorFunctionMap.Add(typeof(Collectible), c => ExportCollectibleString(c as Collectible));
            
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////
        }

        // Initializes a null localizable scene for global configuration purposes
        // TODO: add variable substitution support here by passing a list of <var_name>:<var_orig> values here
        private LocalizableContext(LocaleConfiguration localeConfiguration, Dictionary<string, string> globalStrings) : this()
        {
            ImportedGlobalStrings = globalStrings;
            
            foreach (var option in localeConfiguration.options)
            {
                if (configs.ContainsKey(option.name))
                {
                    if (!string.IsNullOrWhiteSpace(option.value))
                    {
                        configs[option.name] = configs[option.name].Override(option.value);
                    }
                }
                else
                {
                    Debug.LogError($"[Localization] Unknown config called {option.name} overriden for locale {localeConfiguration.name}, not sure how to handle this...");
                }
            }
        }
        
        private LocalizableContext(Scene scene) : this()
        {
            sceneContext = scene;
            PopulateLocalizableInstances(scene);
        }

        private LocalizableContext(GameObject prefab) : this()
        {
            subcontextAnchor = prefab;
            PopulateLocalizableInstances(prefab);
        }
        
        /* Following factory methods are just for clearer naming, instead of all uses of this context class being created from same named constructor */
        public static LocalizableContext ForSingleLocale(LocaleConfiguration localeConfiguration, Dictionary<string, string> globalStrings) =>
            new(localeConfiguration, globalStrings);

        public static LocalizableContext ForSingleScene(Scene scene) => new(scene);

        public static LocalizableContext ForSinglePrefab(GameObject prefab) => new(prefab);
        
        /////////////////////////// Localizable Instance Selection  ////////////////////////////////////////////////////
        
        private Dictionary<Type, Func<Component, IEnumerable<TrackedLocalizable>>> SelectorFunctionMap = new()
        {
            // See special handling of LocalizationInjector type in PopulateLocalizableInstances!
            // The type is not included here due the need to call a non-static function, and also in a specific order...
            { typeof(TMP_Text), component => SelectLocalizablesFromTmp(component as TMP_Text) },
            { typeof(TMP_Dropdown), component => SelectLocalizablesFromDropdown(component as TMP_Dropdown) },
            { typeof(NPC), component => SelectLocalizablesFromNpc(component as NPC) },
            { typeof(TMPTextTyper), component => new List<TrackedLocalizable>{ new (component as TMPTextTyper) } },
            { typeof(PlayerActionHints), component => SelectLocalizablesFromPlayerActionHints(component as PlayerActionHints) },
            { typeof(IDialogueTableProvider), component => SelectLocalizablesFromTableProvider(component as IDialogueTableProvider) },
            { typeof(ArtifactInventoryCollectible), component => new List<TrackedLocalizable>{ new (component as ArtifactInventoryCollectible) }},
        };
        
        private void PopulateLocalizableInstances(Scene scene)
        {
            var rgos = scene.GetRootGameObjects();
            
            foreach (GameObject rootObj in rgos)
            {
                PopulateLocalizableInstances(rootObj);
            }
        }

        private void PopulateLocalizableInstances(GameObject rootGameObject)
        {
            var exclude = rootGameObject.GetComponent<ExcludeFromLocalization>();
            if (exclude != null && !exclude.excludeFromTranslationOnly)
            {
                return;
            }

            localizables.TryAdd(typeof(LocalizationInjector), new());
            // Recursively include subcontexts (prefabs) if the current context is not already specifically for that subcontext
            // The current implmentation does not consider nested subcontexts. Currently there's no usage of that in the game in general.
            if (subcontextAnchor == null){
                var query = rootGameObject.GetComponentsInChildren<LocalizationInjector>(includeInactive: true);
                
                foreach (var injector in query)
                {
                    localizables[typeof(LocalizationInjector)].Add(new TrackedLocalizable(injector));
                }
            }
            
            foreach (var type in SelectorFunctionMap.Keys)
            {
                var query = rootGameObject

                    .GetComponentsInChildren(type, includeInactive: true)

                    .Where(c =>
                        !localizables[typeof(LocalizationInjector)]
                            .Any(
                                injectorWrapper =>
                                    c.transform.IsChildOf(injectorWrapper.GetAnchor<Component>().transform)
                            )
                    )

                    .GroupBy<Component, LocalizationStrategy?>(c =>
                    {
                        var exclude = c.GetComponent<ExcludeFromLocalization>();
                        if (exclude == null)
                        {
                            exclude = c.GetComponentInParent<ExcludeFromLocalization>(includeInactive: true);
                        }

                        if (exclude != null)
                        {
                            return exclude.excludeFromTranslationOnly ? LocalizationStrategy.ChangeStyleOnly : null;
                        }

                        return LocalizationStrategy.TranslateTextAndChangeStyle;
                    });
                
                // Do not re-select something that is in a localization injector (i.e. a prefab)
                // this is done because those objects will be selected when localizing that prefab itself
                
                localizables.TryAdd(type, new());
                localizables[type]
                    .AddRange(query
                        .SelectMany(grouping =>
                        {
                            if (grouping.Key == null)
                            {
                                return new List<TrackedLocalizable>();
                            }
                            
                            var many = grouping.SelectMany(c => SelectorFunctionMap[type](c)).ToList();
                            foreach (var t in many)
                            {
                                t.Strategy = grouping.Key.Value; // assigning it here purely to avoid shotgun change in all the constructors of the class...
                            }

                            return many;
                        })
                    );
            }
        }

        private static IEnumerable<TrackedLocalizable> SelectLocalizablesFromTmp(TMP_Text tmp)
        {
            // skip TMP stuff under NPC and dropdown
            if (
                tmp.gameObject.GetComponent<TMPTextTyper>() != null
                || tmp.gameObject.GetComponentInParent<NPC>(includeInactive: true) != null 
                || tmp.gameObject.GetComponentInParent<TMP_Dropdown>(includeInactive: true) != null)
            {
                return new List<TrackedLocalizable>() { };
            }

            return new List<TrackedLocalizable>() { new TrackedLocalizable(tmp) };
        }
        
        private static IEnumerable<TrackedLocalizable> SelectLocalizablesFromDropdown(TMP_Dropdown dropdown)
        {
            return dropdown.options
                .Select((_, idx) => new TrackedLocalizable(dropdown, idx))
                .Append(new TrackedLocalizable(dropdown));
        }

        private static IEnumerable<TrackedLocalizable> SelectLocalizablesFromNpc(NPC npc)
        {
            return npc.Conds.SelectMany((cond, i) =>
            {
                return cond.dialogueChain.Select((_, j) => new TrackedLocalizable(npc, i, j));
            });
        }

        private static IEnumerable<TrackedLocalizable> SelectLocalizablesFromPlayerActionHints(PlayerActionHints hints)
        {
            return hints.hintsList.Select((_, idx) => new TrackedLocalizable(hints, idx));
        }

        private static IEnumerable<TrackedLocalizable> SelectLocalizablesFromTableProvider(IDialogueTableProvider tableProvider)
        {
            var selected = tableProvider
                .TranslationTable.Select((kv) =>
                    new TrackedLocalizable(tableProvider, kv.Key)
                );

            return selected;
        }

        private IEnumerable<TrackedLocalizable> ExportCollectibleString(Collectible collectible)
        {
            // Debug.LogError(SpecificTypeHelpers.CollectibleToPath(collectible.GetCollectibleData().name, collectible.GetCollectibleData().area));
            
            if (!GlobalStringsToExport.TryAdd(
                    
                    SpecificTypeHelpers.CollectibleToPath(
                        collectible.GetCollectibleData().name,
                        collectible.GetCollectibleData().area),
                    
                    collectible.GetCollectibleData().name))
            {
               Debug.LogWarning($"Duplicate collectible: {SpecificTypeHelpers.CollectibleToPath(collectible.GetCollectibleData().name, collectible.GetCollectibleData().area)}"); 
            }

            return new TrackedLocalizable[] { };
        }
        
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        ////////////////////////// Applying Localization ///////////////////////////////////////////////////////////////

        public enum LocalizationStrategy
        {
            ChangeStyleOnly,
            TranslateTextAndChangeStyle
        }
        
        private static readonly Dictionary<Type, Action<TrackedLocalizable, LocalizationFile, LocalizationStrategy>> LocalizationFunctionMap = new()
        {
            { typeof(TMP_Text), LocalizeTmp },
            { typeof(TMP_Dropdown), LocalizeDropdownOption },
            { typeof(NPC), LocalizeNpc },
            { typeof(TMPTextTyper), LocalizeTextTyper },
            { typeof(LocalizationInjector), (loc, _, _) => loc.GetAnchor<LocalizationInjector>().Refresh() },
            { typeof(PlayerActionHints), LocalizePlayerActionHints },
            { typeof(IDialogueTableProvider), LocalizeTableProvider },
            { typeof(ArtifactInventoryCollectible), LocalizeCollectibleUI },
        };

        /// <summary>
        /// No undo, force refresh when text styling needs to be reverted...
        /// </summary>
        /// <param name="file"></param>
        public void Localize(LocalizationFile file, LocalizationStrategy strategy)
        {
            bool isEnglish = file.IsDefaultLocale;
            bool useDefaultPixelFont = SettingsManager.Setting<bool>(Settings.PixelFontEnabled).CurrentValue;

            if (isEnglish && useDefaultPixelFont)
            {
                // Debug.Log("[Localization] Localization strategy: skipping...");
                return;
            }

            string ctxDisplayName = subcontextAnchor == null
                ? "Scene-" + (sceneContext.HasValue ? sceneContext.Value.name + sceneContext.Value.GetHashCode() : "???")
                : "Prefab-" + subcontextAnchor.name + subcontextAnchor.GetHashCode();
            Debug.Log($"[Localization] Localize {ctxDisplayName} with strategy: { Enum.GetName(typeof(LocalizationStrategy), strategy) }");
            
            foreach (var (type, instances) in localizables)
            {
                if (LocalizationFunctionMap.TryGetValue(type, out var localizationFuncion))
                {
                    foreach (var trackedLocalizable in instances)
                    {
                        if (strategy == LocalizationStrategy.TranslateTextAndChangeStyle &&
                            trackedLocalizable.Strategy == LocalizationStrategy.ChangeStyleOnly)
                        {
                            localizationFuncion(trackedLocalizable, file, LocalizationStrategy.ChangeStyleOnly);
                        }
                        else
                        {
                            localizationFuncion(trackedLocalizable, file, strategy);
                        }
                    }
                }
            }
        }
        
        private static void LocalizeTmp(TrackedLocalizable tmp, LocalizationFile file, LocalizationStrategy strategy)
        {
            var tmpCasted = tmp.GetAnchor<TMP_Text>();
            var entry = file.GetRecord(tmp.FullPath);

            if (strategy == LocalizationStrategy.TranslateTextAndChangeStyle && entry != null)
            {
                tmpCasted.text = entry.Translated;
            }

            var metadata = tmpCasted.ParseMetadata(entry?.Metadata);
            tmpCasted.font = LocalizationLoader.LocalizationFont(metadata.family);

            tmpCasted.overflowMode = TextOverflowModes.Overflow;
            tmpCasted.wordSpacing = 0.0f;
            // tmpCasted.ForceConvertMiddleToMidline();
            tmpCasted.enableWordWrapping = false;
            
            if (file.TryParseConfigValue(LocalizationFile.Config.NonDialogueFontScale, out float scale))
            {
                tmpCasted.fontSize = metadata.size * scale;
            }
            
        }
        
        private static void LocalizeDropdownOption(TrackedLocalizable dropdownOption, LocalizationFile file, LocalizationStrategy strategy)
        {
            var dropdown = dropdownOption.GetAnchor<TMP_Dropdown>();
            var entry = file.GetRecord(dropdownOption.FullPath);

            // particular option in dropdown, change text as well as size modifications (using rich text)
            if (dropdownOption.IndexInComponent != null)
            {
                try
                {
                    int idx = int.Parse(dropdownOption
                        .IndexInComponent); // possibly malformed path, consider logging error instead

                    if (strategy == LocalizationStrategy.TranslateTextAndChangeStyle && entry != null)
                    {
                        
                        dropdown.options[idx].text = entry.Translated;
                    }
                }
                catch (IndexOutOfRangeException)
                {
                    Debug.LogError($"[Localization] {dropdownOption.FullPath}: dropdown option index out of range");
                }
            }
            
            // entire dropdown itself, change font info
            else
            {
                void ApplyStyle(TMP_Text txt)
                {
                    var metadata = txt.ParseMetadata(entry?.Metadata);
                    txt.font = LocalizationLoader.LocalizationFont(metadata.family);
                    txt.overflowMode = TextOverflowModes.Overflow;
                    if (file.TryParseConfigValue(LocalizationFile.Config.NonDialogueFontScale, out float scale))
                    {
                        txt.fontSize = metadata.size * scale;
                        txt.enableWordWrapping = false;
                        // txt.ForceConvertMiddleToMidline();
                        // txt.fontWeight = FontWeight.SemiBold;
                    }
                }
                
                ApplyStyle(dropdown.itemText);
                ApplyStyle(dropdown.captionText);
            }
        }

        private static void LocalizeNpc(TrackedLocalizable npc, LocalizationFile file, LocalizationStrategy strategy)
        {
            if (strategy == LocalizationStrategy.ChangeStyleOnly)
            {
                return;
            }
            
            string path = npc.FullPath;
            try
            {
                var idx = npc.IndexInComponent.Split(Localizable.indexSeparatorSecondary);
                int idxCond = int.Parse(idx[0]);
                int idxDiag = int.Parse(idx[1]);
                var npcCasted = npc.GetAnchor<NPC>();
                
                if (!string.IsNullOrWhiteSpace(npcCasted.Conds[idxCond].dialogueChain[idxDiag].dialogue))
                {
                    var entry = file.GetRecord(path);
                    npcCasted.Conds[idxCond].dialogueChain[idxDiag].DialogueLocalized = entry.Translated;
                }

            }
            catch (IndexOutOfRangeException)
            {
                Debug.LogError($"[Localization] {path}: NPC dialogue out of bounds");
            }
        }

        private static void LocalizeTextTyper(TrackedLocalizable typer, LocalizationFile file, LocalizationStrategy strategy)
        {
            if (!file.TryParseConfigValue(LocalizationFile.Config.DialogueFontScale, out float adjFlt))
            {
                return;
            }
            
            var tmp = typer.GetAnchor<TMPTextTyper>().TextMeshPro;
            var entry = file.GetRecord(typer.FullPath);
            var metadata = tmp.ParseMetadata(entry?.Metadata);
            tmp.font = LocalizationLoader.LocalizationFont(metadata.family);
            tmp.fontSize = tmp.ParseMetadata(entry?.Metadata).size * adjFlt;
            tmp.wordSpacing = 0;
            
            tmp.SetLayoutDirty();
        }

        private static void LocalizePlayerActionHints(TrackedLocalizable hints, LocalizationFile file,
            LocalizationStrategy strategy)
        {
            if (strategy == LocalizationStrategy.ChangeStyleOnly)
            {
                return;
            }

            var path = hints.FullPath;
            var entry = file.GetRecord(path);
            if (entry != null)
            {
                try
                {
                    var idx = int.Parse(hints.IndexInComponent);
                    hints.GetAnchor<PlayerActionHints>().hintsList[idx].hintData.hintText = entry.Translated;
                }
                catch (IndexOutOfRangeException)
                {
                    Debug.LogError($"[Localization] {path}: Player action hint out of bounds");
                }
            }
            else
            {
                Debug.LogWarning($"[Localization] {path}: NOT FOUND");
            }
        }

        private static void LocalizeTableProvider(TrackedLocalizable tableProviderEntry, LocalizationFile file, LocalizationStrategy strategy)
        {
            if (strategy == LocalizationStrategy.ChangeStyleOnly)
            {
                return;
            }
            
            var entry = file.GetRecord(tableProviderEntry.FullPath);
            if (entry == null) return;
            
            // AT: cursed Unity editor crashing bug if I call any interface function,
            // so instead just rewrite the function here...
            // For some reason running during play mode is fine, but non-play mode won't
            // allow it. Probably some Unity Mono issue...
            var table = tableProviderEntry.GetAnchor<IDialogueTableProvider>().TranslationTable;
            table[tableProviderEntry.IndexInComponent] = new LocalizationPair() {
                original = table[tableProviderEntry.IndexInComponent].original,
                translated = entry.Translated
            };
        }
        
        private static void LocalizeCollectibleUI(TrackedLocalizable collectible, LocalizationFile file, LocalizationStrategy strategy)
        {
            if (strategy == LocalizationStrategy.ChangeStyleOnly)
            {
                return;
            }

            var entry = file.GetRecord(collectible.FullPath);
            if (entry != null)
            {
                collectible.GetAnchor<ArtifactInventoryCollectible>().displayName = entry.Translated;
            }
        }
        
        /////////////////////////// Serialization //////////////////////////////////////////////////////////////////////
        private static readonly Dictionary<Type, Func<TrackedLocalizable, SerializedLocalizableData>> SerializationFunctionMap = new()
        {
            { typeof(TMP_Text), SerializeTmp },
            { typeof(TMP_Dropdown), SerializeDropdownOption },
            { typeof(NPC), SerializeNpc },
            { typeof(TMPTextTyper), SerializeTextTyper },
            { typeof(PlayerActionHints), SerializePlayerActionHints },
            { typeof(IDialogueTableProvider), SerializeTableProvider },
            { typeof(ArtifactInventoryCollectible), SerializeCollectibleUI },
        };
        
        public void Serialize(
            bool serializeConfigurationDefaults, 
            TextWriter tw, 
            LocalizationFile referenceFile, 
            string autoPadTranslated = null)
        {
            // AT: currently not using Sylvan CSV Writer bc their interface is for rigid db dumping instead of custom (and variable) schema writing
            autoPadTranslated = autoPadTranslated ?? "";
            
            var sep = LocalizationFile.csvSeparator;

            void SerializeCells(params string[] cells)
                => tw.Write(
                    string.Join(
                        string.Empty,
                        cells.Select(c => '"' + c.Replace("\"", "\"\"") + '"' + LocalizationFile.csvSeparator))
                    );
            
            // file format explainer
            SerializeCells(
                $"Slider.v{Application.version}",
                $"Unity.v{Application.unityVersion}",
                "(file format explainer)->",
                LocalizationFile.explainer);
            tw.WriteLine();
            
            // properties and values
            foreach (var (name, defaults) in configs)
            {
                var nameStr = LocalizationFile.ConfigToName[name];
                SerializeCells(nameStr, serializeConfigurationDefaults ? defaults.Value : "");
            }
            tw.WriteLine();
            
            // property comments
            foreach (var (name, defaults) in configs)
            {
                SerializeCells(defaults.Comment, "");
            }
            tw.WriteLine();
            
            // headers 
            SerializeCells("Path", "Orig", "Translation", "Metadata");
            tw.WriteLine();

            SortedDictionary<string, SerializedLocalizableData> data = SerializeTrackedLocalizables();

            foreach (var kv in ImportedGlobalStrings)
            {
                data.Add(kv.Key, kv.Value);
            }
            
            foreach (var (path, orig) in data)
            {
                // use double double-quotes to escape double-quotes

                string translated = orig.text;

                if (!string.IsNullOrWhiteSpace(translated))
                {
                    translated = referenceFile?.GetRecord(path)?.Translated ?? translated;
                    translated = autoPadTranslated + translated + autoPadTranslated;
                }
                else
                {
                    translated = null;
                }
                
                // AT: metadata is freshly updated each time, no referencing!
                
                SerializeCells(path, orig.text ?? "", translated ?? "", orig.metadata ?? ""); // for skeleton file, just use original as the translation
                tw.WriteLine();
            }
        }
        
        private SortedDictionary<string, SerializedLocalizableData> SerializeTrackedLocalizables()
        {
            SortedDictionary<string, SerializedLocalizableData> result = new();
            
            Dictionary<Type, Action<TrackedLocalizable>> serializationMappingAdaptor = new();
            foreach (var (type, serialize) in SerializationFunctionMap)
            {
                serializationMappingAdaptor.Add(type, localizable =>
                {
                    var data = serialize(localizable);
                    if (data.IsEmpty)
                    {
                        return;
                    }
                    var path = localizable.FullPath;
                    if (!result.TryAdd(path, data))
                    {
                        Debug.LogError($"[Localization] Duplicate path: {path}");
                    }
                });
            }

            foreach (var (type, instances) in localizables)
            {
                if (serializationMappingAdaptor.TryGetValue(type, out var serializationFunction))
                {
                    foreach (TrackedLocalizable localizable in instances)
                    {
                        serializationFunction(localizable);
                    }
                }
            }

            return result;
        }

        private static SerializedLocalizableData SerializeTmp(TrackedLocalizable tmp)
        {
            var t = tmp.GetAnchor<TMP_Text>();
            return new SerializedLocalizableData
            {
                text = t.text,
                metadata = t.GetMetadata()
            };
        }
        
        private static SerializedLocalizableData SerializeDropdownOption(TrackedLocalizable dropdownOption)
        {
            var d = dropdownOption.GetAnchor<TMP_Dropdown>();
            if (dropdownOption.IndexInComponent != null)
            {
                return d.options[int.Parse(dropdownOption.IndexInComponent)].text;
            }
            // for dropdown itself, not particular options
            else
            {
                return new SerializedLocalizableData
                {
                    text = null,
                    metadata = d.itemText.GetMetadata().ToString()
                };
            }
        }
        
        private static SerializedLocalizableData SerializeNpc(TrackedLocalizable npc)
        {
            if (npc.IndexInComponent == null)
            {
                return null;
            }
            
            var idx = npc.IndexInComponent.Split(Localizable.indexSeparatorSecondary);

            try
            {
                int idxCond = int.Parse(idx[0]);
                int idxDiag = int.Parse(idx[1]);

                NPC npcCasted = npc.GetAnchor<NPC>();

                return npcCasted.Conds[idxCond].dialogueChain[idxDiag].dialogue;
            }
            catch (FormatException)
            {
                Debug.LogError(
                    $"[Localization] {npc.FullPath} corrupted: requires one condition index and one dialogue index within condition");
            }
            catch (IndexOutOfRangeException)
            {
                Debug.LogError(
                    $"[Localization] {npc.FullPath} corrupted: requires one condition index and one dialogue index within condition");
            }

            return null;
        }

        private static SerializedLocalizableData SerializeTextTyper(TrackedLocalizable typer)
        {
            var t = typer.GetAnchor<TMPTextTyper>();
            return new SerializedLocalizableData
            {
                text = null,
                metadata = t.TextMeshPro.GetMetadata()
            };
        }

        private static SerializedLocalizableData SerializePlayerActionHints(TrackedLocalizable hint)
        {
            if (!int.TryParse(hint.IndexInComponent, out int idx))
            {
                return null;
            }
            try
            {
                return hint.GetAnchor<PlayerActionHints>().hintsList[idx].hintData.hintText;
            }
            catch (IndexOutOfRangeException)
            { 
                Debug.LogError($"[Localization] {hint.FullPath}: Player action hint out of bounds");
                return null;
            }
        }

        private static SerializedLocalizableData SerializeTableProvider(TrackedLocalizable tableProvider)
        {
            // AT: cursed Unity editor crashing bug if I call any interface function,
            // so instead just rewrite the function here...
            // For some reason running during play mode is fine, but non-play mode won't
            // allow it. Probably some Unity Mono issue...
            return tableProvider.GetAnchor<IDialogueTableProvider>().TranslationTable[tableProvider.IndexInComponent].original;
        }
        
        private static SerializedLocalizableData SerializeCollectibleUI(TrackedLocalizable collectible)
        {
            return collectible.GetAnchor<ArtifactInventoryCollectible>().displayName;
        }
    }

    struct FontMetadata
    {
        public string family;
        public float size;

        public override string ToString() => $"{family};{size}";
        public static implicit operator string(FontMetadata self) => self.ToString();
    }

    internal static class TmpTextExtensions
    {
        // // pixel one alignment is really fucking weird expecially when UI height is LESS than font height for some fuckin reason
        // internal static void ForceConvertMiddleToMidline(this TMP_Text txt)
        // {
        //     // pixel one alignment is really fucking weird expecially when UI height is LESS than font height for some fuckin reason
        //     if (txt.verticalAlignment == VerticalAlignmentOptions.Middle)
        //     {
        //         txt.verticalAlignment = VerticalAlignmentOptions.Geometry;
        //         txt.SetLayoutDirty();
        //     }
        // }

        internal static FontMetadata GetMetadata(this TMP_Text txt) => new FontMetadata
        {
            family = txt.font.faceInfo.familyName,
            size = txt.fontSize
        };

        internal static FontMetadata ParseMetadata(this TMP_Text txt, string metadata)
        {
            if (string.IsNullOrWhiteSpace(metadata))
            {
                // fallback to current state
                return txt.GetMetadata();
            }

            var cols = metadata.Split(';');
            var l = cols.Length;

            if (l == 0)
            {
                return txt.GetMetadata();
            }

            var family = cols[0];

            if (l > 1 && float.TryParse(cols[1], out var size))
            {
                return new FontMetadata
                {
                    family = family,
                    size = size
                };
            }

            return new FontMetadata
            {
                family = family,
                size = float.NaN
            };
        }
    }
}