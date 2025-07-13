using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Localization
{
    partial class LocalizableContext
    {
        private readonly Dictionary<Type, Func<TrackedLocalizable, SerializedLocalizableData>>
            _serializationFunctionMap = new()
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
                        if (!entry.original.Normalize().Replace("\r\n", "\n").Equals(orig.text.Normalize().Replace("\r\n", "\n")))
                        {
                            Debug.LogWarning($"Original text differs at key={path.key}, which may indicate index shift / out of sync.\nReference original: {entry.original}\nSerialized origional: {orig.text}");
                        }
                        
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
            foreach (var (type, serialize) in _serializationFunctionMap)
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
                if (!serializationMappingAdaptor.TryGetValue(type, out var serializationFunction))
                {
                    continue;
                }
                
                foreach (var i in instances)
                {
                    serializationFunction(i);
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
                text = t.localizeText ? t.TextMeshPro.text : null,
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
}