using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using Sylvan.Data.Csv;
using UnityEngine.SceneManagement;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace Localization
{
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
        public string original;
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

        private ParsedLocalizable(string hierarchyPath, string index, string original, SerializedLocalizableData data)
        {
            this.index = index;
            this.hierarchyPath = hierarchyPath;
            this.original = original;
            _data = data;
        }

        internal static ParsedLocalizable ParsePath(string fullPath, string original, SerializedLocalizableData data)
        {
            string[] pathAndIndex = fullPath.Split(indexSeparator);

            if (pathAndIndex.Length < 1)
            {
                return null;
            }

            if (pathAndIndex.Length == 1)
            {
                return new ParsedLocalizable(pathAndIndex[0], null, original, data);
            }

            try
            {
                return new ParsedLocalizable(pathAndIndex[0], pathAndIndex[1], original, data);
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
                    .Select(localeNameCanonical =>
                    {
                        var filePath = LocaleGlobalFilePath(localeNameCanonical, root);
                        var (parsedGlobalFile, err) = MakeLocalizationFile(localeNameCanonical, filePath).Result;
                        if (parsedGlobalFile != null)
                        {
                            if (parsedGlobalFile.configs.TryGetValue(Config.DisplayName, out var displayName) && !displayName.Value.Equals(""))
                            {
                                return displayName.Value;
                            }
                            return localeNameCanonical;
                        } else {
                            PrintParserError(err, filePath);
                            return null;
                        }
                    })
                    .Where(localeDisplayName => localeDisplayName != null)
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

        public static string DefaultLocale => defaultLocale;
        private static string defaultLocale = "English";
        public static string TestingLanguage => "Debug";

        // Unity's GetStartupLocale should only be called in Start() or Awake() according to docs
        public static void UpdateDefaultLocaleOnAwake() =>
                defaultLocale = LocaleUtils.LocaleShortToLong(GetDefaultLocale().Identifier.Code);
        
        private static Locale GetDefaultLocale()
        {
            foreach (var selector in LocalizationSettings.StartupLocaleSelectors)
            {
                var locale = selector.GetStartupLocale(LocalizationSettings.AvailableLocales);
                if (locale != null)
                    return locale;
            }
            return LocalizationSettings.AvailableLocales.Locales[0]; // english
        }

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
            Author,
            DisplayName
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
        
        public static async Task<(LocalizationFile, ParserError)> MakeLocalizationFile(string locale, string filePath,
            LocalizationFile localeConfig = null)
        {
            if (!File.Exists(filePath))
            {
                return (null, ParserError.FileNotFound);
            }

            await using var file = File.OpenRead(filePath);
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
                reader.GetString(1),
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

    public partial class LocalizableContext
    {
        private ILocalizationTrackable localizationRoot = null;
        
        Dictionary<Type, List<TrackedLocalizable>> localizables = new();
        private Dictionary<string, string> ImportedGlobalStrings = new();

        public Dictionary<string, string> GlobalStringsToExport =
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
            _selectorFunctionMap.TryAdd(typeof(Collectible), c => ExportCollectibleString(c as Collectible));
            _selectorFunctionMap.TryAdd(typeof(MilitaryCollectibleController),
                c => ExportMilitaryCollectibleString(c as MilitaryCollectibleController));
            _selectorFunctionMap.TryAdd(typeof(ArtifactInventoryCollectible),
                c => ExportAICString(c as ArtifactInventoryCollectible));

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
            localizationRoot = null;
            var rgos = scene.GetRootGameObjects();

            foreach (GameObject rootObj in rgos)
            {
                PopulateLocalizableInstances(rootObj);
            }
        }

        private LocalizableContext(LocalizationInjector injector) : this()
        {
            localizationRoot = injector;
            PopulateLocalizableInstances(injector.gameObject);
        }

        /* Following factory methods are just for clearer naming, instead of all uses of this context class being created from same named constructor */
        public static LocalizableContext ForSingleLocale(LocaleConfiguration localeConfiguration,
            Dictionary<string, string> globalStrings) =>
            new(localeConfiguration, globalStrings);

        public static LocalizableContext ForSingleScene(Scene scene) => new(scene);

        public static LocalizableContext ForInjector(LocalizationInjector injector) => new(injector);
    }

    public interface ILocalizationTrackable
    {
        public struct LocalizationState
        {
            internal string locale;
            internal bool usePixelFont;
        }

        internal static LocalizationState DefaultState = new()
        {
            locale = LocalizationFile.DefaultLocale,
            usePixelFont = true
        };
        
        internal LocalizationState LastLocalizedState { get; }

        public LocalizableContext.LocalizationAction TrackLocalization(LocalizationState newLocalizationStatus)
        {
            if ((LastLocalizedState.locale == newLocalizationStatus.locale) && (LastLocalizedState.usePixelFont == newLocalizationStatus.usePixelFont))
            {
                return new LocalizableContext.LocalizationAction
                {
                    ShouldTranslate = false, 
                    StyleChange = LocalizableContext.StyleChange.Idle
                };
            }
            
            var action = new LocalizableContext.LocalizationAction
            {
                ShouldTranslate = LastLocalizedState.locale != newLocalizationStatus.locale
            };

            var isAlreadyEnglish = (LastLocalizedState.locale == LocalizationFile.DefaultLocale);
            var willBeInEnglish = (newLocalizationStatus.locale == LocalizationFile.DefaultLocale);
            var isUsingPixelFont = LastLocalizedState.usePixelFont;
            var willBeUsingPixelFont = newLocalizationStatus.usePixelFont;

            // localizing from * to english locale
            if (willBeInEnglish)
            {
                // any change to english pixel will need to stylize to 
                action.StyleChange = willBeUsingPixelFont ? LocalizableContext.StyleChange.DefaultPixel : LocalizableContext.StyleChange.NonPixel;
                goto ret;
            }
            
            // localizing from english to non-english locale
            if (isAlreadyEnglish)
            {
                action.StyleChange = willBeUsingPixelFont ? LocalizableContext.StyleChange.LocalizedPixel : LocalizableContext.StyleChange.NonPixel;
                goto ret;
            }
            
            // localizing from non-english to non-english locale
            
            // in this case translation has no direct relation with stylization, so idle stylization can
            // be detected by equality
            if (isUsingPixelFont == willBeUsingPixelFont)
            {
                action.StyleChange = LocalizableContext.StyleChange.Idle;
                goto ret;
            }
            
            action.StyleChange = willBeUsingPixelFont ? LocalizableContext.StyleChange.LocalizedPixel : LocalizableContext.StyleChange.NonPixel;

            ret:
                return action;
        }
    }
}