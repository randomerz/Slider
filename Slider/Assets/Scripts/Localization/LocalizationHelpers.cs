using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;
using System.Text;
using TMPro;
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

    internal sealed class ParsedLocalizable : Localizable
    {
        public string Translated { get; }

        private ParsedLocalizable(string hierarchyPath, string index, string translated) : base(hierarchyPath.Split('/'))
        {
            this.index = index;
            Translated = translated;
        }
        
        internal static ParsedLocalizable ParsePath(string fullPath, string translated = null)
        {
            string[] pathAndIndex = fullPath.Split(indexSeparator);

            if (pathAndIndex.Length < 1)
            {
                return null;
            }

            if (pathAndIndex.Length == 1)
            {
                return new ParsedLocalizable(pathAndIndex[0], null, translated);
            }

            try
            {
                return new ParsedLocalizable(pathAndIndex[0], pathAndIndex[1], translated);
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

        public TrackedLocalizable(DialogueDisplay display) : this(display as Component)
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
        public static string GoofyAhLanguage => "Piratese";
        
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
- In general, any consecutive string of \r and \n (in any order) will be
  treated as one SINGLE line break, except within an enclosed context
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
  first three will be parsed (you can use the rest as comments if needed)
- The file must end in a sequence of \r and \n's
- Every cell containing a ^, \r, or \n must be enclosed in ""
  - If a "" is encountered right after a non-enclosed separator, \r, or \n,
    then the parser is said to enter an ""enclosed context"", such context
    ends immediately when a non-escaped "" that is not followed by \r, \n,
    or a separator is encountered
  - "" within each cell should be escaped using another "" in front of it,
  - If a "" is encountered in a non-enclosed context, and it is not right
    after a separator, \r, or \n, then it is entirely ignored
  - If a non-escaped "" is encountered in an enclosed context...
    - If it is followed by a separator, \r, or \n, then it is considered
      the closing quote of the current context
    - Otherwise, it is ignored
  - If a separator, \r, or \n is encountered while in an enclosed context,
    it will be preserved
";

        internal static readonly char csvSeparator = ',';

        public enum Config
        {
            IsValid,
            NonDialogueFontScale,
            DialogueFontScale
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
                Config.NonDialogueFontScale, new LocalizationConfig(
                "Float value that scales font size of all dialogue text, this is an option separate because the English font of this game is *really* tiny, like about 1/3 of regular font when under the same font size.","1.0")
            },

            {
                Config.DialogueFontScale, new LocalizationConfig(
                "Float value that scales font size of all dialogue text, this is an option separate because the English font of this game is *really* tiny, like about 1/3 of regular font when under the same font size. Recommended setting is around 0.3~0.6.",
                "1.0")
            },
            
            // { fontOverridePath_name, new LocalizationConfig(
            //     "Relative path to a ttf font file from the folder containing this file, leave empty if no such font is needed", 
            //     "")
            // }
        };

        internal SortedDictionary<Config, LocalizationConfig> configs;
        internal SortedDictionary<string, ParsedLocalizable> records;

        public string LocaleName => locale;
        private string locale;
        public bool IsDefaultLocale => locale.Equals(DefaultLocale);
        public static bool SupportsPixelFont(string locale) => locale.Equals(DefaultLocale); // TODO: change when pixel font adds Spanish supports or something...
        
        enum ParserState
        {
            Empty,
            HasPath,
            HasOriginal,
            HasTranslation,
            Inval
        }

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
                case LocalizationFile.ParserError.NoError:
                    Debug.Log($"[Localization] Localization file parser: null file without error thrown at {path}");
                    break;
                case LocalizationFile.ParserError.FileNotFound:
                    Debug.Log($"[Localization] Localization file parser: file not found {path}");
                    break;
                case LocalizationFile.ParserError.ExplicitlyDisabled:
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
            LocalizationFile parsed = new(locale, new StreamReader(file), localeConfig);
            
            #if !UNITY_EDITOR
            if (!parsed.TryParseConfigValue(Config.IsValid, out int isValidFlag) || isValidFlag != 1)
            {
                return (null, ParserError.ExplicitlyDisabled);
            }
            #endif
            return (parsed, ParserError.NoError);
        }
        
        private LocalizationFile(string locale, StreamReader reader, LocalizationFile localeConfig = null)
        {
            this.locale = locale;
            
            records = new();

            int currentRow = 0; // first row is row 0
            int currentCol = 0;

            ParserState cellState = ParserState.Empty;

            string path = null;
            string orig = null;
            string trans = null;

            string property = null;

            configs = new();
            foreach (var (k, v) in (localeConfig != null ? localeConfig.configs : defaultConfigs))
            {
                configs.Add(k, new LocalizationConfig(v.Comment, v.Value));
            }

            while (true)
            {
                var (content, isNewLineStart, isEof) = ParseCell(reader);
                
                if (isNewLineStart)
                {
                    currentRow++;
                    // Debug.Log($"New line {currentRow} starts with {content}");
                    currentCol = 0;
                }
                else
                {
                    currentCol++;
                }

                if (!isEof && currentRow < 4)
                {
                    // parameter row
                    if (currentRow == 1)
                    {
                        if (currentCol % 2 == 0)
                        {
                            property = content.Trim();
                        }
                        else if (property != null)
                        {
                            if (ConfigFromName.TryGetValue(property, out var propertyEnum))
                            {
                                if (!string.IsNullOrWhiteSpace(content))
                                {
                                    configs[propertyEnum] = configs[propertyEnum].Override(content);
                                    // Debug.Log($"[Localization] Parsed localization config: {property} = {content}");
                                }
                            }
                            else
                            {
                                Debug.LogError($"[Localization] Unrecognized localization config {property} with value {content}");
                            }
                            
                            // reset property
                            property = null;
                        }
                    }
                    
                    continue;
                }

                // on new line, commit the last entry
                if (isNewLineStart || isEof)
                {
                    if (path != null && cellState == ParserState.HasTranslation) // this should always be true but JetBrains kept giving warning, so this check is added
                    {
                        var newRecord = ParsedLocalizable.ParsePath(path, trans);
                        if (newRecord == null)
                        {
                            Debug.LogError($"[Localization] Null localizable parsed {path}");
                        }
                        else
                        {
                            records.Add(path, newRecord);
                        }
                    }
                    else
                    {
                        switch (cellState)
                        {
                            case ParserState.HasPath:
                                Debug.LogError($"[Localization] Inval: only path {path}");
                                break;
                            case ParserState.HasOriginal:
                                Debug.LogError($"[Localization] Inval: Only path orig {path} : {orig}");
                                break;
                            default:
                                break;
                        }
                    }

                    cellState = ParserState.Empty;
                }

                if (isEof)
                {
                    break;
                }

                switch (cellState)
                {
                    case ParserState.Empty:
                        if (string.IsNullOrWhiteSpace(content))
                        {
                            cellState = ParserState.Inval;
                            break;
                        }

                        path = content;
                        cellState = ParserState.HasPath;
                        break;
                    case ParserState.HasPath:
                        cellState = ParserState.HasOriginal;
                        orig = content;
                        break;
                    case ParserState.HasOriginal:
                        if (string.IsNullOrWhiteSpace(content))
                        {
                            cellState = ParserState.Inval;
                            break;
                        }

                        trans = content;
                        cellState = ParserState.HasTranslation;
                        break;
                    case ParserState.HasTranslation:
                        break;
                    case ParserState.Inval:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            
            reader.Close();
        }
        
        private (string content, bool isNewLineStart, bool isEOF) ParseCell(StreamReader reader)
        {
            bool isNewLineStart = false;
            StringBuilder builder = new();

            bool isEnclosed = false;

            // read initial \r \n sequence before a cell begins
            // then, read character right after it only if it is a quote (in which case it is interpreted as an opening
            // quote of an enclosed cell)
            // if the reader starts at EOF, or encounters EOF after a string of \r and \n, then the file is considered
            // terminated
            while (true)
            {
                if (reader.EndOfStream)
                {
                    return (null, isNewLineStart, true);
                }

                char c = (char)reader.Peek();

                if (c is '\r' or '\n')
                {
                    isNewLineStart = true;
                    reader.Read();
                }
                else
                {
                    if (c == '"')
                    {
                        isEnclosed = true;
                        reader.Read();
                    }

                    break;
                }
            }

            bool isLastCharacterEscapingQuote = false;
            while (true)
            {
                char c = (char)reader.Peek();
                bool isThisCharacterEscapingQuote = false;

                if (c == '\r' || c == '\n' || c == csvSeparator)
                {
                    if (!isEnclosed)
                    {
                        // only read in the character if it is a separator. if it is \r or \n, leave the character for
                        // the next parser run. This is useful when there is only one such break. If it is read in
                        // instead of preserved, the next parser run will start at a non \r or \n character, and
                        // mistake the cell as on the same row as this cell
                        if (c == csvSeparator)
                        {
                            reader.Read();
                        }

                        break;
                    }
                    else
                    {
                        reader.Read();
                        builder.Append(c);
                    }
                }
                else if (c == '"')
                {
                    reader.Read(); // always read in quotes, this leaves Peek available for examining what comes after!
                    char nextAfterQuote = (char)reader.Peek();

                    if (isEnclosed)
                    {
                        // if this quote is escaped, read it and don't consider it as an escaping quote
                        if (isLastCharacterEscapingQuote)
                        {
                            isThisCharacterEscapingQuote = false;
                            builder.Append(c);
                        }
                        // if a non-escaped quote is followed by \r, \n, or separator, then it is considered the closing
                        // quote of this enclosed context
                        else if (nextAfterQuote == '\r' || nextAfterQuote == '\n' || nextAfterQuote == csvSeparator)
                        {
                            // only read the next character in if it is a separator, same reasoning as above
                            if (nextAfterQuote == csvSeparator)
                            {
                                reader.Read();
                            }
                            break;
                        }
                        // otherwise, consider this quote as an escaping quote for the next quote
                        else
                        {
                            isThisCharacterEscapingQuote = true;
                        }
                    }
                    else
                    {
                        // Having a quote in a non-enclosed cell
                        builder.Append(c);
                    }
                }
                else
                {
                    builder.Append(c);
                    reader.Read();
                }

                isLastCharacterEscapingQuote = isThisCharacterEscapingQuote;
            }

            return (builder.ToString(), isNewLineStart, false);
        }
        
        private Dictionary<Config, int> configParsingCacheInt = new();
        private Dictionary<Config, float> configParsingCacheFloat = new();
        
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
    }

    public class LocalizableContext
    {
        private Scene? sceneContext = null;
        private GameObject subcontextAnchor = null;
        Dictionary<Type, List<TrackedLocalizable>> localizables = new();
        private Dictionary<string, string> AdditionalStrings = new();
        public Dictionary<string, string> AdditionalExportedStrings = new(); // strings encountered during the context parsing process, but won't be used within the context (rather, for a locale global file)

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
            AdditionalStrings = globalStrings;
            
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
            { typeof(DialogueDisplay), component => new List<TrackedLocalizable>{ new (component as DialogueDisplay) } },
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
            if (rootGameObject.GetComponent<ExcludeFromLocalization>() != null)
            {
                return;
            }

            localizables.TryAdd(typeof(LocalizationInjector), new());
            // Recursively include subcontexts (prefabs) if the current context is not already specifically for that subcontext
            // The current implmentation does not consider nested subcontexts. Currently there's no usage of that in the game in general.
            if (subcontextAnchor == null){
                var query = rootGameObject.GetComponentsInChildren(typeof(LocalizationInjector), includeInactive: true);
                
                foreach (var injector in query)
                {
                    localizables[typeof(LocalizationInjector)].Add(new TrackedLocalizable(injector as LocalizationInjector));
                }
            }
            
            foreach (var type in SelectorFunctionMap.Keys)
            {
                var query = rootGameObject
                        
                    .GetComponentsInChildren(type, includeInactive: true)
                    
                    .Where(c => c.GetComponentInParent<ExcludeFromLocalization>() == null)
                    
                    // Do not re-select something that is in a localization injector (i.e. a prefab)
                    // this is done because those objects will be selected when localizing that prefab itself
                    .Where(c => 
                        !localizables[typeof(LocalizationInjector)]
                            .Any(
                                injectorWrapper => c.transform.IsChildOf(injectorWrapper.GetAnchor<Component>().transform)
                                )
                        );
                
                localizables.TryAdd(type, new());
                localizables[type]
                    .AddRange(query
                        .SelectMany(component => SelectorFunctionMap[type](component)));
            }
        }

        private static IEnumerable<TrackedLocalizable> SelectLocalizablesFromTmp(TMP_Text tmp)
        {
            // skip TMP stuff under NPC and dropdown
            if (tmp.gameObject.GetComponentInParent<NPC>(includeInactive: true) != null || tmp.gameObject.GetComponentInParent<TMP_Dropdown>(includeInactive: true) != null)
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
            if (!AdditionalExportedStrings.TryAdd(
                    SpecificTypeHelpers.CollectibleToPath(collectible.GetCollectibleData().name,
                        collectible.GetCollectibleData().area),
                    collectible.GetCollectibleData().name))
            {
               Debug.LogWarning($"Duplicate collectible: {SpecificTypeHelpers.CollectibleToPath(collectible.GetCollectibleData().name, collectible.GetCollectibleData().area)}"); 
            }

            return new TrackedLocalizable[] { };
        }
        
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        ////////////////////////// Applying Localization ///////////////////////////////////////////////////////////////

        private enum LocalizationStrategy
        {
            ChangeStyleOnly,
            TranslateTextAndChangeStyle
        }
        
        private static readonly Dictionary<Type, Action<TrackedLocalizable, LocalizationFile, LocalizationStrategy>> LocalizationFunctionMap = new()
        {
            { typeof(TMP_Text), LocalizeTmp },
            { typeof(TMP_Dropdown), LocalizeDropdownOption },
            { typeof(NPC), LocalizeNpc },
            { typeof(DialogueDisplay), LocalizeDialogueDisplay },
            { typeof(LocalizationInjector), (loc, _, _) => loc.GetAnchor<LocalizationInjector>().Refresh() },
            { typeof(PlayerActionHints), LocalizePlayerActionHints },
            { typeof(IDialogueTableProvider), LocalizeTableProvider },
            { typeof(ArtifactInventoryCollectible), LocalizeCollectibleUI },
        };

        /// <summary>
        /// No undo, force refresh when text styling needs to be reverted...
        /// </summary>
        /// <param name="file"></param>
        public void Localize(LocalizationFile file)
        {
            bool isEnglish = file.IsDefaultLocale;
            bool canUsePixelFont = SettingsManager.Setting<bool>(Settings.PixelFontEnabled).CurrentValue;

            if (isEnglish && canUsePixelFont)
            {
                // Debug.Log("[Localization] Localization strategy: skipping...");
                return;
            }

            var strategy = isEnglish
                ? LocalizationStrategy.ChangeStyleOnly
                : LocalizationStrategy.TranslateTextAndChangeStyle;

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
                        localizationFuncion(trackedLocalizable, file, strategy);
                    }
                }
            }
        }
        
        private static void LocalizeTmp(TrackedLocalizable tmp, LocalizationFile file, LocalizationStrategy strategy)
        {
            var tmpCasted = tmp.GetAnchor<TMP_Text>();
            var path = tmp.FullPath;

            if (strategy == LocalizationStrategy.TranslateTextAndChangeStyle)
            {
                if (file.records.TryGetValue(path, out var entry))
                {
                    tmpCasted.text = entry.Translated;
                }
                else
                {
                    Debug.LogWarning($"[Localization] {path}: NOT FOUND");
                }
            }
            
            tmpCasted.font = LocalizationLoader.LocalizationFont;
            tmpCasted.overflowMode = TextOverflowModes.Overflow;
            tmpCasted.wordSpacing = 0.0f;
            // tmpCasted.fontWeight = FontWeight.SemiBold; // for some reason some characters aren't bolded

            if (tmpCasted.GetComponent<TMPTextTyper>())
            {
                // is dialogue
            }
            else
            {
                // is not dialogue
                tmpCasted.enableWordWrapping = false;
                if (file.TryParseConfigValue(LocalizationFile.Config.NonDialogueFontScale, out float scale))
                {
                    tmpCasted.fontSize *= scale;
                }
            }
            
        }
        
        private static void LocalizeDropdownOption(TrackedLocalizable dropdownOption, LocalizationFile file, LocalizationStrategy strategy)
        {
            TMP_Dropdown dropdown = dropdownOption.GetAnchor<TMP_Dropdown>();

            // particular option in dropdown, change text as well as size modifications (using rich text)
            if (dropdownOption.IndexInComponent != null)
            {
                try
                {
                    int idx = int.Parse(dropdownOption
                        .IndexInComponent); // possibly malformed path, consider logging error instead

                    string path = dropdownOption.FullPath;

                    if (strategy == LocalizationStrategy.TranslateTextAndChangeStyle)
                    {
                        if (file.records.TryGetValue(path, out var entry))
                        {
                            dropdown.options[idx].text = entry.Translated;
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"[Localization] {path}: NOT FOUND");
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
                    txt.font = LocalizationLoader.LocalizationFont;
                    txt.overflowMode = TextOverflowModes.Overflow;
                    if (file.TryParseConfigValue(LocalizationFile.Config.NonDialogueFontScale, out float scale))
                    {
                        txt.fontSize *= scale;
                        txt.enableWordWrapping = false;
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
            if (file.records.TryGetValue(path, out var entry))
            {
                try
                {
                    var idx = npc.IndexInComponent.Split(Localizable.indexSeparatorSecondary);
                    int idxCond = int.Parse(idx[0]);
                    int idxDiag = int.Parse(idx[1]);

                    NPC npcCasted = npc.GetAnchor<NPC>();
                    npcCasted.Conds[idxCond].dialogueChain[idxDiag].DialogueLocalized = entry.Translated;
                }
                catch (IndexOutOfRangeException)
                {
                    Debug.LogError($"[Localization] {path}: NPC dialogue out of bounds");
                }
            }
            else
            {
                Debug.LogWarning($"[Localization] {path}: NOT FOUND");
            }
        }

        private static void LocalizeDialogueDisplay(TrackedLocalizable display, LocalizationFile file, LocalizationStrategy strategy)
        {
            if (file.TryParseConfigValue(LocalizationFile.Config.DialogueFontScale, out float adjFlt))
            {
                display.GetAnchor<DialogueDisplay>().SetFont(LocalizationLoader.LocalizationFont, adjFlt, true);
            }
        }

        private static void LocalizePlayerActionHints(TrackedLocalizable hints, LocalizationFile file,
            LocalizationStrategy strategy)
        {
            if (strategy == LocalizationStrategy.ChangeStyleOnly)
            {
                return;
            }

            var path = hints.FullPath;
            if (file.records.TryGetValue(path, out var entry))
            {
                try
                {
                    int idx = int.Parse(hints.IndexInComponent);
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
            
            var path = tableProviderEntry.FullPath;
            if (file.records.TryGetValue(path, out var entry))
            {
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
            else
            {
                Debug.LogWarning($"[Localization] {path}: NOT FOUND");
            }
        }
        
        private static void LocalizeCollectibleUI(TrackedLocalizable collectible, LocalizationFile file, LocalizationStrategy strategy)
        {
            if (strategy == LocalizationStrategy.ChangeStyleOnly)
            {
                return;
            }
            
            var path = collectible.FullPath;
            if (file.records.TryGetValue(path, out var entry))
            {
                collectible.GetAnchor<ArtifactInventoryCollectible>().displayName = entry.Translated;
            }
            else
            {
                Debug.LogWarning($"[Localization] {path}: NOT FOUND");
            }
        }
        
        /////////////////////////// Serialization //////////////////////////////////////////////////////////////////////
        private static readonly Dictionary<Type, Func<TrackedLocalizable, string>> SerializationFunctionMap = new()
        {
            { typeof(TMP_Text), SerializeTmp },
            { typeof(TMP_Dropdown), SerializeDropdownOption },
            { typeof(NPC), SerializeNpc },
            { typeof(PlayerActionHints), SerializePlayerActionHints },
            { typeof(IDialogueTableProvider), SerializeTableProvider },
        };
        
        public string Serialize(bool serializeConfigurationDefaults, LocalizationFile referenceFile, Func<string> autoPadTranslated = null)
        {
            StringBuilder builder = new StringBuilder();

            char sep = LocalizationFile.csvSeparator;
            
            // file format explainer
            builder.Append($"\"{LocalizationFile.explainer.Replace("\"", "\"\"")}\"{sep}\r\n");
            
            // properties and values
            foreach (var (name, defaults) in configs)
            {
                var nameStr = LocalizationFile.ConfigToName[name];
                
                if (serializeConfigurationDefaults)
                {
                    builder.Append($"\"{nameStr.Replace("\"", "\"\"")}\"{sep}\"{defaults.Value.Replace("\"", "\"\"")}\"{sep}");
                }
                else
                {
                    builder.Append($"\"{nameStr.Replace("\"", "\"\"")}\"{sep}\" \"{sep}");
                }
            }
            builder.Append("\r\n");
            
            // property comments
            foreach (var (name, defaults) in configs)
            {
                builder.Append($"\"{defaults.Comment.Replace("\"", "\"\"")}\"{sep}\" \"{sep}");
            }
            builder.Append("\r\n");
            
            // headers 
            builder.Append($"\"Path\"{sep}\"Orig\"{sep}\"Translation\"{sep}\r\n");

            SortedDictionary<string, string> data = SerializeTrackedLocalizables();

            foreach (var kv in AdditionalStrings)
            {
                data.Add(kv.Key, kv.Value);
            }
            
            foreach (var (_path, _orig) in data)
            {
                // use double double-quotes to escape double-quotes

                string path = _path.Replace("\"", "\"\"");
                string orig = _orig.Replace("\"", "\"\"");

                string translated = orig;
                
                if (referenceFile != null)
                {
                    if (referenceFile.records.TryGetValue(_path, out var referenceTranslation))
                    {
                        translated = referenceTranslation.Translated;
                    }
                    else
                    {
                        Debug.LogWarning($"[Localization] No existing translation at {_path}");
                    }
                }

                if (autoPadTranslated != null)
                {
                    string pad = autoPadTranslated();
                    translated = pad + translated + pad;
                }
                
                builder.Append($"\"{path}\"{sep}\"{orig}\"{sep}\"{translated}\"{sep}\r\n"); // for skeleton file, just use original as the translation
            }

            return builder.ToString();
        }
        
        private SortedDictionary<string, string> SerializeTrackedLocalizables()
        {
            SortedDictionary<string, string> result = new();
            
            Dictionary<Type, Action<TrackedLocalizable>> serializationMappingAdaptor = new();
            foreach (var (type, serialize) in SerializationFunctionMap)
            {
                serializationMappingAdaptor.Add(type, localizable =>
                {
                    string orig = serialize(localizable);
                    if (string.IsNullOrWhiteSpace(orig))
                    {
                        return;
                    }
                    string path = localizable.FullPath;
                    if (!result.TryAdd(path, orig))
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

        private static string SerializeTmp(TrackedLocalizable tmp)
        {
            return tmp.GetAnchor<TMP_Text>().text;
        }
        
        private static string SerializeDropdownOption(TrackedLocalizable dropdownOption)
        {
            if (dropdownOption.IndexInComponent != null)
            {
                return dropdownOption.GetAnchor<TMP_Dropdown>().options[int.Parse(dropdownOption.IndexInComponent)].text;
            }
            // for dropdown itself, not particular options
            else
            {
                return null;
            }
        }
        
        private static string SerializeNpc(TrackedLocalizable npc)
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

        private static string SerializePlayerActionHints(TrackedLocalizable hint)
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

        private static string SerializeTableProvider(TrackedLocalizable tableProvider)
        {
            // AT: cursed Unity editor crashing bug if I call any interface function,
            // so instead just rewrite the function here...
            // For some reason running during play mode is fine, but non-play mode won't
            // allow it. Probably some Unity Mono issue...
            return tableProvider.GetAnchor<IDialogueTableProvider>().TranslationTable[tableProvider.IndexInComponent].original;
        }
    }
}