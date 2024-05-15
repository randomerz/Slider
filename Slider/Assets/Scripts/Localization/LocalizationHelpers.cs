using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public class LocalizationHelpers
{
    public struct Localizable
    {
        private string path;
        private Object instance;

        public Localizable(string path, Object instance)
        {
            this.path = path;
            this.instance = instance;
        }

        public string Path => path;
        public Object Instance => instance;
    }

    public class LocalizationDirectory
    {
        // TODO: locale-based constructor, auto loading behavior, etc.
    }
    
    // Localization files are formatted like CSV so they can be easily edited,
    // however formatting has strict rules for easy parsing:
    // - First line must be exactly "Sep=^"^\n
    // - Every cell must be double quoted
    // - Every cell must end with the separator ^, even if it is the last cell
    //   of the row (in which case it must end with ^\n)
    
    // During parsing, the parser splits everything by ^, if a cell starts
    // with \n then it is the first cell of a new row
    
    // Additionally...
    // - The second line is reserved for headers
    // - The very last separator (i.e. the last cell of the last row) must
    //   be followed with \n
    public class LocalizationFile
    {
        private readonly char csvSeparator = '^';
        private SortedDictionary<string, (string, string)> Entries;
        
        public LocalizationFile()
        {
            Entries = new();
        }

        enum ParserState
        {
            Empty,
            HasPath,
            HasOriginal,
            HasTranslation,
            Inval
        }

        public LocalizationFile(string Csv)
        {
            Entries = new();

            var cells = Csv.Split(csvSeparator, StringSplitOptions.RemoveEmptyEntries);

            const int skipCount = 4; // first row (1 column), header row (3 columns)

            ParserState cellState = ParserState.Empty;
            
            string path = null;
            string orig = null;
            string trans = null;
            
            for (int i = skipCount; i < cells.Length; i++)
            {
                var (content, isNewLineStart) = ParseCell(cells[i]);

                // on new line, commit the last entry
                if (isNewLineStart)
                {
                    switch (cellState)
                    {
                        case ParserState.Empty:
                            break;
                        case ParserState.HasPath:
                            break;
                        case ParserState.HasOriginal:
                            Entries.Add(path, (orig, orig));
                            break;
                        case ParserState.HasTranslation:
                            Entries.Add(path, (orig, trans));
                            break;
                        case ParserState.Inval:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
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
                        if (string.IsNullOrWhiteSpace(content))
                        {
                            cellState = ParserState.Inval;
                            break;
                        }

                        orig = content;
                        cellState = ParserState.HasOriginal;
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
        }

        private static (string content, bool isNewLineStart) ParseCell(string cell)
        {
            bool isNewLineStart = false;
            StringBuilder builder = new();
            
            int lastQuote = cell.LastIndexOf('"');

            if (lastQuote < 0)
            {
                return ("", true);
            }
            
            int i = 0;
            for(i = 0; i < lastQuote; i++)
            {
                char c = cell[i];
                if (c == '\r' || c == '\n')
                {
                    isNewLineStart = true;
                }
                if (c == '"')
                {
                    i++; // skip this quote!
                    break;
                }
            }

            bool isLastCharacterQuote = false;
            for (; i < lastQuote; i++)
            {
                char c = cell[i];
                bool isThisCharacterQuote = c == '"';

                if (isLastCharacterQuote && isThisCharacterQuote)
                {
                    builder.Append(c);
                    // consecutive quotes are escaped, meaning this quote should not be counted in the next step...
                    isLastCharacterQuote = false;
                }
                else
                {
                    // this is not a consecutive quote, so the state machine keeps progressing
                    isLastCharacterQuote = isThisCharacterQuote;

                    // For every other character, just copy directly into content
                    if (!isThisCharacterQuote)
                    {
                        builder.Append(c);
                    }
                }
            }

            return (builder.ToString(), isNewLineStart);
        }

        public string Serialize()
        {
            StringBuilder builder = new StringBuilder();

            builder.Append($"\"Sep={csvSeparator}\"{csvSeparator}\r\n");
            builder.Append($"\"Path\"{csvSeparator}\"Orig\"{csvSeparator}\"Translation\"{csvSeparator}\r\n");

            foreach (var (_path, (_orig, _trans)) in Entries)
            {
                // use double double-quotes to escape double-quotes
                
                string path = _path.Replace("\"", "\"\"");
                string orig = _orig.Replace("\"", "\"\"");
                string trans = _trans.Replace("\"", "\"\"");
                
                builder.Append($"\"{path}\"{csvSeparator}\"{orig}\"{csvSeparator}\"{trans}\"{csvSeparator}\r\n");
            }
            
            return builder.ToString();
        }

        public void AddEntryTmp(Localizable tmp)
        {
            TMP_Text tmpCasted = tmp.Instance as TMP_Text;
            AddEntry(tmp.Path, tmpCasted.text);
        }

        private void AddEntry(string path, string orig)
        {
            if (string.IsNullOrEmpty(orig))
            {
                return;
            }

            if (Entries.ContainsKey(path))
            {
                throw new Exception("lmao");
            }
            else
            {
                Entries.Add(path, (orig, orig));
            }
        }
        
    }

    private static Dictionary<Type, Func<Component, IEnumerable<Localizable>>> selectors = new()
    {
        { typeof(TMP_Text), SelectTMPText }
    };
    
    public static void IterateLocalizableTypes(Scene scene, Dictionary<Type, Action<Localizable>> forTypeDo)
    {
        Dictionary<Type, List<Localizable>> components = new();
        
        foreach (GameObject rootObj in scene.GetRootGameObjects())
        {
            foreach (Type type in forTypeDo.Keys)
            {
                Component[] query = rootObj.GetComponentsInChildren(type, includeInactive: true);
                if (!components.ContainsKey(type))
                {
                    components.Add(type, new());
                }

                components[type].AddRange(query.SelectMany(component => selectors[type](component)));
            }
        }

        foreach (var (type, instances) in components)
        {
            foreach (Localizable localizable in instances)
            {
                forTypeDo[type](localizable);
            }
        }
    }

    private static IEnumerable<Localizable> SelectTMPText(Component tmp)
    {
        // skip TMP stuff under NPC, but not other places!
        if (tmp.gameObject.GetComponentInParent<NPC>(includeInactive: true) != null)
        {
            return new Localizable[] { };
        }

        string path = GetPath(tmp);

        if (string.IsNullOrEmpty(path))
        {
            throw new Exception("Empty path");
        }
        
        return new List<Localizable>
        {
            new (path, tmp)
        };
    }
    
    private static string GetPath(Component current)
    {
        List<string> pathComponents = new();
        for (Transform go = current.transform; go.parent != null; go = go.parent)
        {
            pathComponents.Add(go.name + go.GetSiblingIndex());
        }
        pathComponents.Reverse();
        return string.Join('/', pathComponents);
    }
}