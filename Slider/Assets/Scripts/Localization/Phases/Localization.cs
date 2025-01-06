using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Localization
{
    public partial class LocalizableContext
    {
        public enum StyleChange
        {
            Idle,
            DefaultPixel,
            LocalizedPixel,
            NonPixel
        }

        public struct LocalizationAction
        {
            public bool ShouldTranslate;
            public StyleChange StyleChange;

        }
        
        private readonly Dictionary<Type, Action<TrackedLocalizable, LocalizationFile, LocalizationAction>>
            _localizationFunctionMap = new()
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
        
        public void Localize(LocalizationFile file, LocalizationAction strategy)
        {
            Debug.Log($"[Localization] Localize {(MonoBehaviour) localizationRoot} with shouldTranslate {strategy.ShouldTranslate}, styleChange {strategy.StyleChange}");
            
            foreach (var (type, instances) in localizables)
            {
                if (!_localizationFunctionMap.TryGetValue(type, out var func))
                {
                    continue;
                }
                
                foreach (var i in instances)
                {
                    func(i, file, strategy);
                }
            }
        }

        private static void InjectLocalizations(TrackedLocalizable inj, LocalizationFile file, LocalizationAction strategy)
        {
            var injector = inj.GetAnchor<LocalizationInjector>();
            injector.Localize();
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

        private static void LocalizeTmp_Stylize(TMP_Text tmp, ParsedLocalizable entry, LocalizationFile file, StyleChange strategy)
        {
            if (strategy == StyleChange.Idle)
            {
                return;
            }
            
            var metadata = tmp.ParseMetadata(entry?.Metadata);

            if (strategy != StyleChange.DefaultPixel)
            {
                StylizeTmpMetadata(ref metadata, file);   
            }
            
            if (LocalizationLoader.TryLoadFont(metadata.family, strategy, out var newFont))
            {
                tmp.font = newFont;
            }
            tmp.DeserializeMetadataExceptFont(metadata);
        }

        private static void LocalizeTmp(TrackedLocalizable tmp, LocalizationFile file, LocalizationAction strategy)
        {
            var tmpCasted = tmp.GetAnchor<TMP_Text>();

            if (file.TryGetRecord(tmp.FullPath, out var entry)
                && tmp.shouldTranslate 
                && strategy.ShouldTranslate 
                && entry.TryGetTranslated(out var translated))
            {
                tmpCasted.text = translated;
            }
            
            LocalizeTmp_Stylize(tmpCasted, entry, file, strategy.StyleChange);
        }

        private static void LocalizeDropdownOption(TrackedLocalizable dropdownOption, LocalizationFile file, LocalizationAction strategy)
        {
            var dropdown = dropdownOption.GetAnchor<TMP_Dropdown>();
            var hasEntry = file.TryGetRecord(dropdownOption.FullPath, out var entry);
            var didTranslate = hasEntry && dropdownOption.shouldTranslate && strategy.ShouldTranslate;

            // particular option in dropdown, change text as well as size modifications (using rich text)
            if (dropdownOption.IndexInComponent != null)
            {
                if (!didTranslate || !entry.TryGetTranslated(out var translated))
                {
                    return;
                }
                
                try
                {
                    if (int.TryParse(dropdownOption.IndexInComponent, out var idx))
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
                LocalizeTmp_Stylize(dropdown.itemText, entry, file, strategy.StyleChange);
                LocalizeTmp_Stylize(dropdown.captionText, entry, file, strategy.StyleChange);
            }
        }

        private static void LocalizeBigText(TrackedLocalizable big, LocalizationFile file, LocalizationAction strategy)
        {
            var b = big.GetAnchor<UIBigText>();

            if (
                file.TryGetRecord(big.FullPath, out var entry)
                && big.shouldTranslate
                && strategy.ShouldTranslate
                && entry.TryGetTranslated(out var translated))
            {
                foreach (var txt in b.texts)
                {
                    txt.text = translated;
                }
            }
            
            foreach (var txt in b.texts)
            {
                LocalizeTmp_Stylize(txt, entry, file, strategy.StyleChange);
            }
        }

        private static void LocalizeNpc(TrackedLocalizable npc, LocalizationFile file, LocalizationAction strategy)
        {
            if (!npc.shouldTranslate 
                || !strategy.ShouldTranslate 
                || !file.TryGetRecord(npc.FullPath, out var entry)
                || !entry.TryGetTranslated(out var translated))
            {
                return;
            }
            
            try
            {
                var idx = npc.IndexInComponent.Split(Localizable.indexSeparatorSecondary);
                int idxCond = int.Parse(idx[0]);
                int idxDiag = int.Parse(idx[1]);
                var npcCasted = npc.GetAnchor<NPC>();
                npcCasted.Conds[idxCond].dialogueChain[idxDiag].DialogueLocalized = translated;
            }
            catch (IndexOutOfRangeException)
            {
                Debug.LogError($"[Localization] {npc.FullPath}: NPC dialogue out of bounds");
            }
        }

        private static void LocalizeTextTyper(TrackedLocalizable typer, LocalizationFile file, LocalizationAction strategy)
        {
            var t = typer.GetAnchor<TMPTextTyper>();
            var tmp = t.TextMeshPro;
            
            if (
                file.TryGetRecord(typer.FullPath, out var entry)
                && t.localizeText
                && strategy.ShouldTranslate
                && typer.shouldTranslate
                && entry.TryGetTranslated(out var translation))
            {
                tmp.text = translation;
            }

            if (strategy.StyleChange == StyleChange.Idle)
            {
                return;
            }

            var metadata = tmp.ParseMetadata(entry?.Metadata);
            if (strategy.StyleChange != StyleChange.DefaultPixel)
            {
                if (LocalizationLoader.TryLoadFont(metadata.family, strategy.StyleChange, out var font))
                {
                    tmp.font = font;
                }
            
                if (!file.TryParseConfigValue(LocalizationFile.Config.DialogueFontScale, out float adjFlt))
                {
                    metadata.fontSize *= adjFlt;
                }
                metadata.wordSpacing = 0;
            }
            
            tmp.DeserializeMetadataExceptFont(metadata);
        }

        private static void LocalizePlayerActionHints(TrackedLocalizable hints, LocalizationFile file, LocalizationAction strategy)
        {
            if (!hints.shouldTranslate 
                || !strategy.ShouldTranslate 
                || !file.TryGetRecord(hints.FullPath, out var entry)
                || !entry.TryGetTranslated(out var translated))
            {
                return;
            }
            
            try
            {
                var idx = int.Parse(hints.IndexInComponent);
                hints.GetAnchor<PlayerActionHints>().hintsList[idx].hintData.hintText = translated;
            }
            catch (IndexOutOfRangeException)
            {
                Debug.LogError($"[Localization] {hints.FullPath}: Player action hint out of bounds");
            }
        }

        private static void LocalizeTableProvider(TrackedLocalizable tableProviderEntry, LocalizationFile file, LocalizationAction strategy)
        {
            if (!tableProviderEntry.shouldTranslate 
                || !strategy.ShouldTranslate 
                || !file.TryGetRecord(tableProviderEntry.FullPath, out var entry)
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
    }
}