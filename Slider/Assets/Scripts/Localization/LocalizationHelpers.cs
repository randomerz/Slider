using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;
using System.Reflection;
using System.Text;
using TMPro;
using Sylvan.Data.Csv;
using UnityEngine.SceneManagement;

namespace Localization
{
    public static class SpecificTypeHelpers
    {
        public static string JungleShapeToPath(string shapeName) => $"__JungleShapes/{shapeName}";

        public static string CollectibleToPath(string collectible, Area area) =>
            $"__Collectibles/{area.ToString()}/{collectible}";

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

        public string TranslatedOrFallback => TranslatedFallbackToOriginal;
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
        internal string hierarchyPath;
        
        public string IndexInComponent => index;
        protected string index = null;

        // Separates the index string from the hierarchy path
        public static char indexSeparator = '@';

        // Separates different digits within an index string
        public static char indexSeparatorSecondary = ':';

        public string FullPath => hierarchyPath + (index != null ? indexSeparator + index : "");

        protected Localizable()
        {
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
        
        public bool TryGetTranslated(out string translated)
        {
            if (!string.IsNullOrWhiteSpace(_data.text))
            {
                translated = _data.text;
                return true;
            }
        
            translated = null;
        
            return false;
        }

        public string Metadata => _data.metadata;

        private ParsedLocalizable(string hierarchyPath, string index, SerializedLocalizableData data)
        {
            this.index = index;
            this.hierarchyPath = hierarchyPath;
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
        public T GetAnchor<T>() where T : class => (_anchor as T);
        
        private readonly Component _anchor;

        public bool shouldTranslate = true;

        public TrackedLocalizable(Component c) : base()
        {
            _anchor = c;
        }

        public override string ToString()
        {
            return $"{_anchor.gameObject.name} << {_anchor} >> ";
        }

        // Track dropdown option
        public TrackedLocalizable(TMP_Dropdown dropdown, int idx) : this(dropdown as Component)
        {
            index = idx.ToString();
        }

        public TrackedLocalizable(NPC npc, int cond, int diag) : this(npc as Component)
        {
            index = cond.ToString() + Localizable.indexSeparatorSecondary + diag.ToString();
        }

        public TrackedLocalizable(PlayerActionHints hint, int idx) : this(hint as Component)
        {
            index = idx.ToString();
        }

        public TrackedLocalizable(IDialogueTableProvider dialogueTableProvider, string code) : this(
            dialogueTableProvider as Component)
        {
            index = code;
        }
    }
    
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

        public static string LocalizationFolderPath(string root = null) =>
            Path.Join(LocalizationRootPath(root), "Localizations");

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
        private static string LocalizationFileName(LocalizationInjector injector) => injector.prefabName + "_prefab.csv";

        private static string LocaleGlobalFileName(string locale) => $"_{locale}_configs.csv";

        public static string LocaleGlobalFilePath(string locale, string root = null) =>
            Path.Join(LocalizationFolderPath(root), locale, LocaleGlobalFileName(locale));

        public static string DefaultLocale => "English";
        public static string TestingLanguage => "Debug";

        public static string AssetPath(string locale, Scene scene, string root = null) =>
            Path.Join(LocalizationFolderPath(root), locale, LocalizationFileName(scene));

        public static string AssetPath(string locale, LocalizationInjector injector, string root = null) =>
            Path.Join(LocalizationFolderPath(root), locale, LocalizationFileName(injector));

        // AT: This should really be a variable in LocalizableScene, but I'm placing it here
        //     so I don't have to re-type this whole thing as comments to the LocalizationFile
        //     class...
        internal static readonly string explainer =
            @"Localization files are formatted like CSV so they can be easily edited,
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

        public enum ParserError
        {
            NoError,
            FileNotFound,
            ExplicitlyDisabled
        }
        
        #if UNITY_EDITOR
        private string _debug_path;
        #endif

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
                    Debug.LogWarning(
                        $"[Localization] Localization file parser: localization file at {path} explicitly disabled");
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
                  
#if UNITY_EDITOR
            _debug_path = filePath;
#endif

            records = new();
            configs = new();

            foreach (var (k, v) in (localeConfig != null ? localeConfig.configs : defaultConfigs))
            {
                configs.Add(k, new LocalizationConfig(v.Comment, v.Value));
            }

            var opts = new CsvDataReaderOptions
            {
                CsvStyle = CsvStyle.Standard,
                HasHeaders = false
            };

            using var reader = CsvDataReader.Create(filePath, opts);

            for (var currentRow = 0; SafeRead(reader); currentRow++)
            {
                // second row of the file are property/value pairs
                if (currentRow == 1)
                {
                    ParsePropertiesRow(reader);
                }
                else if (currentRow < 4)
                {
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
                            Debug.LogError(
                                $"[Localization] Inval: Only path orig {reader.GetString(0)} : {reader.GetString(1)}");
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

        private static bool SafeRead(CsvDataReader reader)
        {
            try
            {
                return reader.Read();
            }
            catch (CsvFormatException e)
            {
                Debug.LogError($"[Localization] CSV format error on row {e.RowNumber}: {e}");
                return false;
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

        internal bool TryGetRecord(string path, out ParsedLocalizable entry)
        {
            if (!records.TryGetValue(path, out entry))
            {
                Debug.LogWarning($"[Localization] {path}: NOT FOUND");
                entry = null;
                return false;
            }

            return true;
        }
    }

    public class LocalizableContext
    {
        private LocalizationInjector root = null;
        
        Dictionary<Type, List<TrackedLocalizable>> localizables = new();
        private Dictionary<string, string> ImportedGlobalStrings = new();

        public readonly Dictionary<string, string> GlobalStringsToExport =
                new(); // strings encountered during the context parsing process, but won't be used within the context (rather, for a locale global file)

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
        private LocalizableContext(LocaleConfiguration localeConfiguration, Dictionary<string, string> globalStrings) :
            this()
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
                    Debug.LogError(
                        $"[Localization] Unknown config called {option.name} overriden for locale {localeConfiguration.name}, not sure how to handle this...");
                }
            }
        }

        private LocalizableContext(Scene scene) : this()
        {
            var rgos = scene.GetRootGameObjects();

            foreach (GameObject rootObj in rgos)
            {
                PopulateLocalizableInstances(rootObj);
            }
        }

        private LocalizableContext(LocalizationInjector injector) : this()
        {
            root = injector;
            PopulateLocalizableInstances(injector.gameObject);
        }

        /* Following factory methods are just for clearer naming, instead of all uses of this context class being created from same named constructor */
        public static LocalizableContext ForSingleLocale(LocaleConfiguration localeConfiguration,
            Dictionary<string, string> globalStrings) =>
            new(localeConfiguration, globalStrings);

        public static LocalizableContext ForSingleScene(Scene scene) => new(scene);

        public static LocalizableContext ForInjector(LocalizationInjector injector) => new(injector);

        /////////////////////////// Localizable Instance Selection  ////////////////////////////////////////////////////

        private Dictionary<Type, Func<Component, IEnumerable<TrackedLocalizable>>> SelectorFunctionMap = new()
        {
            // See special handling of LocalizationInjector type in PopulateLocalizableInstances!
            // The type is not included here due the need to call a non-static function, and also in a specific order...
            { typeof(TMP_Text), component => SelectLocalizablesFromTmp(component as TMP_Text) },
            { typeof(TMP_Dropdown), component => SelectLocalizablesFromDropdown(component as TMP_Dropdown) },
            { typeof(UIBigText), SelectSelf },
            { typeof(NPC), component => SelectLocalizablesFromNpc(component as NPC) },
            { typeof(TMPTextTyper), SelectSelf },
            {
                typeof(PlayerActionHints),
                component => SelectLocalizablesFromPlayerActionHints(component as PlayerActionHints)
            },
            {
                typeof(IDialogueTableProvider),
                component => SelectLocalizablesFromTableProvider(component as IDialogueTableProvider)
            },
        };

        private enum TranslationExclusionMode
        {
            Skip,
            NoTranslation,
            None
        }
        
        private TranslationExclusionMode GetExclusionMode(Transform t, TranslationExclusionMode defaultLocalizationShouldTranslate)
        {
            TranslationExclusionMode mode;
            var e = t.GetComponent<ExcludeFromLocalization>();
            if (e != null)
            {
                if (e.excludeFromTranslationOnly)
                {
                    mode = TranslationExclusionMode.NoTranslation;
                }
                else
                {
                    mode = TranslationExclusionMode.Skip;
                }
            } else
            {
                mode = TranslationExclusionMode.None;
            }

            if (defaultLocalizationShouldTranslate < mode)
            {
                return defaultLocalizationShouldTranslate;
            }
            return mode;
        }

        private void PopulateLocalizableInstances(GameObject rootGameObject)
        {
            // treat bool as shouldTranslate
            Queue<(Transform t, TranslationExclusionMode parentMode, string path)> todo = new();
            var topLevelExclusionMode = GetExclusionMode(rootGameObject.transform, TranslationExclusionMode.None);

            if (topLevelExclusionMode == TranslationExclusionMode.Skip)
            {
                return;
            }

            localizables.TryAdd(typeof(LocalizationInjector), new());
            foreach (var t in SelectorFunctionMap.Keys)
            {
                localizables.TryAdd(t, new ());
            }
            
            todo.Enqueue((rootGameObject.transform, topLevelExclusionMode, root?.prefabName ?? rootGameObject.name));
            while (todo.TryDequeue(out var curr))
            {
                var inj = curr.t.GetComponent<LocalizationInjector>();
                if (inj != null)
                {
                    if (root != null && inj.gameObject == rootGameObject)
                    {
                        // the root of an actual injector-rooted object should be allowed
                        // (not added to localizables to prevent self recursion)
                    }
                    else
                    {
                        // in any other case, the localizable should be delegated to the injector
                        // and not the current context
                        // this includes:
                        // - Scene -> injector on root obj
                        // - Scene -> ... -> injector on non-root obj
                        // - Injector -> ... -> injector on non-root-injector obj
                        localizables[typeof(LocalizationInjector)].Add(new TrackedLocalizable(inj));
                        continue;   
                    }
                }

                var mode = GetExclusionMode(curr.t, curr.parentMode);

                if (mode == TranslationExclusionMode.Skip)
                {
                    continue;
                }
                
                foreach (var (type, selector) in SelectorFunctionMap)
                {
                    var inst = curr.t.GetComponents(type);
                    foreach (var t in inst.SelectMany(selector))
                    {
                        t.shouldTranslate = mode == TranslationExclusionMode.None;
                        t.hierarchyPath = curr.path;
                        localizables[type].Add(t);
                    }
                }
                
                for (int i = 0; i < curr.t.childCount; i++)
                {
                    var child = curr.t.GetChild(i);
                    todo.Enqueue((child, mode, curr.path + "/" + child.name));
                }
            }
        }

        private static IEnumerable<TrackedLocalizable> SelectSelf(Component c)
        {
            return new List<TrackedLocalizable> { new (c) };
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

        private static IEnumerable<TrackedLocalizable> SelectLocalizablesFromTableProvider(
            IDialogueTableProvider tableProvider)
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
                // Debug.LogWarning(
                //     $"Duplicate collectible: {SpecificTypeHelpers.CollectibleToPath(collectible.GetCollectibleData().name, collectible.GetCollectibleData().area)}");
            }

            return new TrackedLocalizable[] { };
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        ////////////////////////// Applying Localization ///////////////////////////////////////////////////////////////
        
        private enum Strategy
        {
            Reset,
            StylizeOnly,
            TranslateAndStyle,
        }
        
        private static readonly Dictionary<Type, Action<TrackedLocalizable, LocalizationFile, Strategy>>
            LocalizationFunctionMap = new()
            {
                { typeof(TMP_Text), LocalizeTmp },
                { typeof(TMP_Dropdown), LocalizeDropdownOption },
                { typeof(NPC), LocalizeNpc },
                { typeof(TMPTextTyper), LocalizeTextTyper },
                { typeof(UIBigText), LocalizeBigText },
                { typeof(LocalizationInjector), InjectLocalizations },
                { typeof(PlayerActionHints), LocalizePlayerActionHints },
                { typeof(IDialogueTableProvider), LocalizeTableProvider },
            };

        /// <summary>
        /// No undo, force refresh when text styling needs to be reverted...
        /// </summary>
        public void Localize(LocalizationFile file, bool usePixelFont, string locale)
        {
            Strategy strategy;
            
            if (!locale.Equals(LocalizationFile.DefaultLocale))
            {
                strategy = Strategy.TranslateAndStyle;
            }
            else
            {
                if (usePixelFont)
                {
                    strategy = Strategy.Reset; // use English pixel fonts
                }
                else
                {
                    strategy = Strategy.StylizeOnly; // apply non-pixel font and do other adjustments to make it work
                }
            }
            
            foreach (var (type, instances) in localizables)
            {
                if (LocalizationFunctionMap.TryGetValue(type, out var localizationFunction))
                {
                    foreach (var trackedLocalizable in instances)
                    {
                        // some instances do not need translation
                        var instStrategy = strategy == Strategy.TranslateAndStyle && !trackedLocalizable.shouldTranslate
                            ? Strategy.StylizeOnly
                            : strategy;
                        
                        localizationFunction(
                            trackedLocalizable, file,
                            instStrategy);
                    }
                }
            }
        }

        private static void InjectLocalizations(TrackedLocalizable inj, LocalizationFile file, Strategy strategy)
        {
            var injector = inj.GetAnchor<LocalizationInjector>();
            injector.Refresh();
        }
        
        private static void StylizeTmpMetadata(ref FontMetadata metadata, LocalizationFile file)
        {
            if (!file.IsDefaultLocale)
            {
                metadata.overflowMode = TextOverflowModes.Overflow;
            }

            metadata.wordSpacing = 0;
            metadata.lineSpacing = 0;
            metadata.enableWordWrapping = false;
            metadata.extraPadding = false; // does not work with CN font for some reason

            if (file.TryParseConfigValue(LocalizationFile.Config.NonDialogueFontScale, out float scale))
            {
                metadata.fontSize *= scale;
            }
        }

        private static void LocalizeTmp_Stylize(TMP_Text tmp, ParsedLocalizable entry, LocalizationFile file, Strategy strategy)
        {
            var metadata = tmp.ParseMetadata(entry?.Metadata);

            if (strategy != Strategy.Reset)
            {
                StylizeTmpMetadata(ref metadata, file);   
            }
            
            if (LocalizationLoader.TryLoadFont(metadata.family, strategy != Strategy.Reset, out var newFont))
            {
                tmp.font = newFont;
            }
            tmp.DeserializeMetadataExceptFont(metadata);
        }

        private static void LocalizeTmp(TrackedLocalizable tmp, LocalizationFile file, Strategy strategy)
        {
            var tmpCasted = tmp.GetAnchor<TMP_Text>();

            if (file.TryGetRecord(tmp.FullPath, out var entry))
            {
                if (strategy == Strategy.TranslateAndStyle && entry.TryGetTranslated(out var translated))
                {
                    tmpCasted.text = translated;
                }
            }
            
            LocalizeTmp_Stylize(tmpCasted, entry, file, strategy);
        }

        private static void LocalizeDropdownOption(TrackedLocalizable dropdownOption, LocalizationFile file, Strategy strategy)
        {
            var dropdown = dropdownOption.GetAnchor<TMP_Dropdown>();
            var hasEntry = file.TryGetRecord(dropdownOption.FullPath, out var entry);

            // particular option in dropdown, change text as well as size modifications (using rich text)
            if (dropdownOption.IndexInComponent != null)
            {
                if (!hasEntry || strategy != Strategy.TranslateAndStyle)
                {
                    return;
                }

                try
                {
                    if (int.TryParse(dropdownOption.IndexInComponent, out var idx) && entry.TryGetTranslated(out var translated))
                    {
                        dropdown.options[idx].text = translated;
                    }
                    else
                    {
                        Debug.LogError($"[Localization] malformed index in {dropdownOption.FullPath}");
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
                LocalizeTmp_Stylize(dropdown.itemText, entry, file, strategy);
                LocalizeTmp_Stylize(dropdown.captionText, entry, file, strategy);
            }
        }

        private static void LocalizeBigText(TrackedLocalizable big, LocalizationFile file, Strategy strategy)
        {
            var b = big.GetAnchor<UIBigText>();

            string translated = null;
            var hasTranslated =
                    file.TryGetRecord(big.FullPath, out var entry)
                    && strategy == Strategy.TranslateAndStyle
                    && entry.TryGetTranslated(out translated);
                
            foreach (var txt in b.texts)
            {
                if (hasTranslated)
                {
                    txt.text = translated;
                }
                LocalizeTmp_Stylize(txt, entry, file, strategy);
            }
        }

        private static void LocalizeNpc(TrackedLocalizable npc, LocalizationFile file, Strategy strategy)
        {
            if (strategy != Strategy.TranslateAndStyle)
            {
                return; // leave stylization to texttyper localization
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
                    if (file.TryGetRecord(path, out var entry) && entry.TryGetTranslated(out var translated))
                    {
                        npcCasted.Conds[idxCond].dialogueChain[idxDiag].DialogueLocalized = translated;
                    }
                }
            }
            catch (IndexOutOfRangeException)
            {
                Debug.LogError($"[Localization] {path}: NPC dialogue out of bounds");
            }
        }

        private static void LocalizeTextTyper(TrackedLocalizable typer, LocalizationFile file, Strategy strategy)
        {
            var t = typer.GetAnchor<TMPTextTyper>();
            var tmp = t.TextMeshPro;
            
            if (file.TryGetRecord(typer.FullPath, out var entry))
            {
                if (t.localizeText && entry.TryGetTranslated(out var translation))
                {
                    tmp.text = translation;
                }
            }

            var metadata = tmp.ParseMetadata(entry.Metadata);
            if (LocalizationLoader.TryLoadFont(metadata.family, strategy == Strategy.TranslateAndStyle, out var font))
            {
                tmp.font = font;
            }
            
            if (!file.TryParseConfigValue(LocalizationFile.Config.DialogueFontScale, out float adjFlt))
            {
                metadata.fontSize *= adjFlt;
            }
            metadata.wordSpacing = 0;
            
            tmp.DeserializeMetadataExceptFont(metadata);
        }

        private static void LocalizePlayerActionHints(TrackedLocalizable hints, LocalizationFile file, Strategy strategy)
        {
            if (strategy != Strategy.TranslateAndStyle)
            {
                return;
            }

            var path = hints.FullPath;
            if (file.TryGetRecord(path, out var entry) && entry.TryGetTranslated(out var translated))
            {
                try
                {
                    var idx = int.Parse(hints.IndexInComponent);
                    hints.GetAnchor<PlayerActionHints>().hintsList[idx].hintData.hintText = translated;
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

        private static void LocalizeTableProvider(TrackedLocalizable tableProviderEntry, LocalizationFile file, Strategy strategy)
        {
            if (strategy != Strategy.TranslateAndStyle)
            {
                return;
            }

            if (!file.TryGetRecord(tableProviderEntry.FullPath, out var entry)
                || !entry.TryGetTranslated(out var translated))
            {
                return;
            }

            // AT: cursed Unity editor crashing bug if I call any interface function,
            // so instead just rewrite the function here...
            // For some reason running during play mode is fine, but non-play mode won't
            // allow it. Probably some Unity Mono issue...
            var table = tableProviderEntry.GetAnchor<IDialogueTableProvider>().TranslationTable;
            table[tableProviderEntry.IndexInComponent] = new LocalizationPair()
            {
                original = table[tableProviderEntry.IndexInComponent].original,
                translated = translated
            };
        }

        /////////////////////////// Serialization //////////////////////////////////////////////////////////////////////
        private static readonly Dictionary<Type, Func<TrackedLocalizable, SerializedLocalizableData>>
            SerializationFunctionMap = new()
            {
                { typeof(TMP_Text), SerializeTmp },
                { typeof(TMP_Dropdown), SerializeDropdownOption },
                { typeof(UIBigText), SerializeBigText },
                { typeof(NPC), SerializeNpc },
                { typeof(TMPTextTyper), SerializeTextTyper },
                { typeof(PlayerActionHints), SerializePlayerActionHints },
                { typeof(IDialogueTableProvider), SerializeTableProvider },
            };

        internal struct NullableKey : IComparable<NullableKey>
        {
            internal bool valueIsNull;
            internal string key;

            public int CompareTo(NullableKey other)
            {
                if (other.valueIsNull == valueIsNull)
                {
                    return String.Compare(key, other.key, StringComparison.Ordinal);
                }
                else
                {
                    // null value comes after non-null value
                    return valueIsNull ? 1 : -1;
                }
            }
        }

        public void Serialize(
            bool serializeConfigurationDefaults,
            TextWriter tw,
            LocalizationFile referenceFile,
            string autoPadTranslated = null)
        {
            // AT: currently not using Sylvan CSV Writer bc their interface is for rigid db dumping instead of custom (and variable) schema writing
            autoPadTranslated ??= "";

            void SerializeCells(params string[] cells)
                => tw.Write(
                    string.Join(
                        string.Empty,
                        cells.Select(c => '"' + (c??"").Replace("\"", "\"\"") + '"' + LocalizationFile.csvSeparator))
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

            SortedDictionary<NullableKey, SerializedLocalizableData> data = SerializeTrackedLocalizables();

            foreach (var kv in ImportedGlobalStrings)
            {
                data.Add(new NullableKey()
                {
                    key = kv.Key,
                    valueIsNull = string.IsNullOrWhiteSpace(kv.Value)
                }, kv.Value);
            }

            foreach (var (path, orig) in data)
            {
                // use double double-quotes to escape double-quotes

                string text = orig.text;

                if (!string.IsNullOrWhiteSpace(text))
                {
                    if (referenceFile != null 
                        && referenceFile.TryGetRecord(path.key, out var entry) 
                        && entry.TryGetTranslated(out var translated)
                        )
                    {
                        text = translated;
                    }

                    text = autoPadTranslated + text + autoPadTranslated;
                }

                // AT: metadata is freshly updated each time, no referencing!

                SerializeCells(path.key, orig.text, text, orig.metadata); // for skeleton file, just use original as the translation
                tw.WriteLine();
            }
        }

        private SortedDictionary<NullableKey, SerializedLocalizableData> SerializeTrackedLocalizables()
        {
            SortedDictionary<NullableKey, SerializedLocalizableData> result = new();
            
            Dictionary<NullableKey, TrackedLocalizable> debugHistory = new();
            
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
                    var key = new NullableKey()
                    {
                        key = path,
                        valueIsNull = string.IsNullOrWhiteSpace(data.text)
                    };
                    
                    if (!result.TryAdd(key, data))
                    {
                        if (debugHistory[key].GetAnchor<Component>() != localizable.GetAnchor<Component>())
                        {
                            throw new Exception($"Different objects sharing the same path: {debugHistory[key]} vs. {localizable}");
                        }
                    }
                    else
                    {
                        debugHistory.Add(key, localizable);
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
                text = tmp.shouldTranslate ? t.text : null,
                metadata = t.SerializeMetadata()
            };
        }

        private static SerializedLocalizableData SerializeDropdownOption(TrackedLocalizable dropdownOption)
        {
            var d = dropdownOption.GetAnchor<TMP_Dropdown>();
            if (dropdownOption.IndexInComponent != null)
            {
                return dropdownOption.shouldTranslate
                    ? d.options[int.Parse(dropdownOption.IndexInComponent)].text
                    : null;
            }
            // for dropdown itself, not particular options
            else
            {
                return new SerializedLocalizableData
                {
                    text = null,
                    metadata = d.itemText.SerializeMetadata()
                };
            }
        }

        private static SerializedLocalizableData SerializeBigText(TrackedLocalizable big)
        {
            var b = big.GetAnchor<UIBigText>();
            return new SerializedLocalizableData
            {
                text = big.shouldTranslate ? b.texts[0].text : null,
                metadata = b.texts[0].SerializeMetadata()
            };
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

                return npc.shouldTranslate ? npcCasted.Conds[idxCond].dialogueChain[idxDiag].dialogue : null;
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
                text = typer.shouldTranslate && t.localizeText ? t.TextMeshPro.text : null,
                metadata = t.TextMeshPro.SerializeMetadata()
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
                return hint.shouldTranslate
                    ? hint.GetAnchor<PlayerActionHints>().hintsList[idx].hintData.hintText
                    : null;
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
            return tableProvider.shouldTranslate
                ? tableProvider.GetAnchor<IDialogueTableProvider>().TranslationTable[tableProvider.IndexInComponent]
                    .original
                : null;
        }
    }
    
    partial struct FontMetadata
    {
        public string family;
        public float fontSize;
        public TextOverflowModes overflowMode;
        public float wordSpacing;
        public float lineSpacing;
        public bool enableWordWrapping;
        public bool extraPadding;
    }

    internal static class TmpTextExtensions
    {
        internal static FontMetadata SerializeMetadata(this TMP_Text t) => new()
        {
            family = t.font?.name,
            fontSize = t.fontSize,
            overflowMode = t.overflowMode,
            wordSpacing = t.wordSpacing,
            lineSpacing = t.lineSpacing,
            enableWordWrapping = t.enableWordWrapping,
            extraPadding = t.extraPadding,
        };

        // see font logic in localization loader
        internal static void DeserializeMetadataExceptFont(this TMP_Text t, FontMetadata metadata)
        {
            t.fontSize = metadata.fontSize;
            t.overflowMode = metadata.overflowMode;
            t.wordSpacing = metadata.wordSpacing;
            t.lineSpacing = metadata.lineSpacing;
            t.enableWordWrapping = metadata.enableWordWrapping;
            t.extraPadding = metadata.extraPadding;
            
            t.SetAllDirty();
        }

        internal static FontMetadata ParseMetadata(this TMP_Text txt, string metadata)
        {
            if (string.IsNullOrWhiteSpace(metadata))
            {
                // fallback to current state
                return txt.SerializeMetadata();
            }

            var data = txt.SerializeMetadata();
            
            foreach(
                var (f, v) in metadata
                    .Split(';')
                    .Where(s => s.Contains(':'))
                    .Select(s => s.Split(":"))
                    .Where(kv => kv.Length == 2 && FontMetadata.fields.ContainsKey(kv[0]))
                    .Select(kv => (FontMetadata.fields[kv[0]], kv[1])))
            {
                if (f.FieldType == typeof(string))
                {
                    f.SetValue(data, v);
                }
                else if (f.FieldType == typeof(float))
                {
                    if (float.TryParse(v, out var value))
                    {
                        f.SetValue(data, value);
                    }
                }
                else if (f.FieldType.IsEnum)
                {
                    if (Enum.TryParse(f.FieldType, v, out var value))
                    {
                        f.SetValue(data, value);
                    }
                }
                else if (f.FieldType == typeof(bool))
                {
                    if (int.TryParse(v, out var value))
                    {
                        f.SetValue(data, value == 1);
                    }
                }
            }

            return data;
        }
    }

    partial struct FontMetadata
    {
        
        internal static SortedDictionary<string, FieldInfo> fields =
            new(typeof(FontMetadata).GetFields().ToDictionary(f => f.Name, f => f));

        public override string ToString()
        {
            StringBuilder sb = new();

            foreach (var (k, f) in fields)
            {
                sb.Append(k);
                sb.Append(":");
                
                // TODO: modify
                if (f.FieldType == typeof(bool))
                {
                    var val = f.GetValue(this);
                    if ((bool)val)
                    {
                        sb.Append("1");
                    }
                    else
                    {
                        sb.Append("0");
                    }
                }
                else
                {
                    sb.Append(f.GetValue(this));
                }
                
                sb.Append(";");
            }

            return sb.ToString();
        }

        public static implicit operator string(FontMetadata self) => self.ToString();
    }
}