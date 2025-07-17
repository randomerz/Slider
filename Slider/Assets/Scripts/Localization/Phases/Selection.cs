using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Localization
{
    public partial class LocalizableContext
    {
        private readonly Dictionary<Type, Func<Component, IEnumerable<TrackedLocalizable>>> _selectorFunctionMap = new()
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
        
        private static TranslationExclusionMode GetExclusionMode(Transform t, TranslationExclusionMode defaultLocalizationShouldTranslate)
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
            foreach (var t in _selectorFunctionMap.Keys)
            {
                localizables.TryAdd((Type)t, new ());
            }

            var rootIsInj = localizationRoot is LocalizationInjector;

            if (rootIsInj)
            {
                todo.Enqueue((rootGameObject.transform, topLevelExclusionMode, (localizationRoot as LocalizationInjector)?.prefabName));
            }
            else
            {
                todo.Enqueue((rootGameObject.transform, topLevelExclusionMode, rootGameObject.name));
            }
            
            while (todo.TryDequeue(out var curr))
            {
                var inj = curr.t.GetComponent<LocalizationInjector>();
                if (inj != null)
                {
                    if (rootIsInj && inj.gameObject == rootGameObject)
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
                
                foreach (var (type, selector) in _selectorFunctionMap)
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
        
        public static string JungleShapeToPath(string shapeName) => $"__JungleShapes/{shapeName}";

        public static string CollectibleToPath(string collectible, Area area) =>
            $"__Collectibles/{collectible}";

        public static string SpecialItemToPath(string specialItem) => $"__SpecialItem/{specialItem}";

        public static string AreaToDiscordNamePath(Area area) => $"__DiscordMessages/{area.ToString()}";

        public static string AreaToDisplayNamePath(Area area) => $"__AreaDisplayName/{area.ToString()}";
        
        private IEnumerable<TrackedLocalizable> ExportCollectibleString(Collectible collectible)
        {
            // Debug.LogError(SpecificTypeHelpers.CollectibleToPath(collectible.GetCollectibleData().name, collectible.GetCollectibleData().area));

            if (!GlobalStringsToExport.TryAdd(
                    CollectibleToPath(
                        collectible.GetCollectibleData().name,
                        collectible.GetCollectibleData().area),
                    collectible.GetCollectibleData().name))
            {
                // Debug.LogWarning(
                //     $"Duplicate collectible: {SpecificTypeHelpers.CollectibleToPath(collectible.GetCollectibleData().name, collectible.GetCollectibleData().area)}");
            }

            return new TrackedLocalizable[] { };
        }

        private IEnumerable<TrackedLocalizable> ExportAICString(ArtifactInventoryCollectible aic)
        {
            if (aic.isSpecialItem && !GlobalStringsToExport.TryAdd(
                    SpecialItemToPath(aic.collectibleName),
                    aic.collectibleName))
            {
                // Debug.LogWarning(
                //     $"Duplicate collectible: {SpecificTypeHelpers.CollectibleToPath(collectible.GetCollectibleData().name, collectible.GetCollectibleData().area)}");
            }

            return new TrackedLocalizable[] { };
        }
    }
}