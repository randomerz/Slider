using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Localization;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using FuckCSharpWhyDoIHaveToRelayEverything = DialogueTableProviderExtensions;

public interface IDialogueTableProvider
{
    public Dictionary<string, LocalizationPair> TranslationTable { get; }

    protected static Dictionary<string, LocalizationPair> InitializeTable(
        Dictionary<string, string> input)
    {
        return input.ToDictionary(kv => LocalizationKey(kv.Key), kv => new LocalizationPair()
        {
            original = kv.Value,
            translated = null
        });
    }

    /// <summary>
    /// Initialization helper to shorten code size for describing the initial value of the translation table implementation
    /// Supports 1:1 mapping from enum value to string, will contain a default integer index of 0 for each entry
    /// </summary>
    /// <typeparam name="T">Enum type to be used by the implementer</typeparam>
    /// <returns></returns>
    protected static Dictionary<string, LocalizationPair> InitializeTable<T>(Dictionary<T, string> input) where T: Enum
    {
        return input.ToDictionary(kv => LocalizationKey(kv.Key) , kv => new LocalizationPair
        {
            original = kv.Value,
            translated = null
        } );
    }
    
    /// <summary>
    /// Initialization helper to shorten code size for describing the initial value of the translation table implementation
    /// Supports 1:X mapping from enum value to string, where each mapping is additionally described by an integer index (Such as `ShopDialogue.Default:2`)
    /// </summary>
    /// <typeparam name="T">Enum type to be used by the implementer</typeparam>
    /// <typeparam name="V">Any enumerable string container, such as array, list, etc.</typeparam>
    /// <returns></returns>
    protected static Dictionary<string, LocalizationPair> InitializeTable<T, V>(Dictionary<T, V> input) where T: Enum where V: IEnumerable<string>
    {
        return input
            .SelectMany(kv => kv.Value.Select((str, idx) => new KeyValuePair<string, string>(LocalizationKey(kv.Key, idx), str)))
            .ToDictionary(kv => kv.Key, kv => new LocalizationPair
            {
                original = kv.Value,
                translated = null
            });
    }
    
    /// <summary>
    /// Exposed translation logic from entry index specification to a serialized string (seen in localization CSV file paths)
    /// </summary>
    /// <param name="key">Enum value</param>
    /// <param name="i">Integer index in case enum value is mapped to multiple consecutive entries</param>
    /// <typeparam name="T">Enum type to be used by the implementer</typeparam>
    /// <returns></returns>
    protected static string LocalizationKey<T>(T key, int i = 0) where T: Enum
    {
        return Enum.GetName(typeof(T), key) + Localizable.indexSeparatorSecondary + i.ToString();
    }

    protected static string LocalizationKey(string key, int i = 0)
    {
        return key + Localizable.indexSeparatorSecondary + i.ToString();
    }

    /// <summary>
    /// Retrieves localization from table, translation is inserted by the LocalizationLoader at runtime
    /// </summary>
    /// <param name="key">Enum value</param>
    /// <param name="i">Integer index in case enum value is mapped to multiple consecutive entries</param>
    /// <typeparam name="T">Enum type to be used by the implementer</typeparam>
    /// <returns>Pair of (original, translated) strings</returns>
    public LocalizationPair GetLocalized<T>(T key, int i = 0) where T: Enum
    {
        string index = LocalizationKey(key, i);
        return GetLocalizedInternal(index);
    }

    public LocalizationPair GetLocalized(string key, int i = 0 )
    {
        string index = LocalizationKey(key, i);
        return GetLocalizedInternal(index);
    }

    /// <summary>
    /// Retrieves localization from table, translation is inserted by the LocalizationLoader at runtime
    /// </summary>
    /// <param name="formattedIndex">Serialized result of LocalizationKey, please do not come up with your own string to put here :)</param>
    /// <returns>Pair of (original, translated) strings</returns>
    private LocalizationPair GetLocalizedInternal(string formattedIndex)
    {
        if (TranslationTable.TryGetValue(formattedIndex, out var val))
        {
            return val;
        }

        return new LocalizationPair
        {
            original = "ERROR: NOT FOUND",
            translated = null
        };
    }
    
    /// <summary>
    /// In a string, find self-closed tags labeled with some key. Then, substitute that tag using the dictionary entry
    /// corresponding to the key
    /// *ex.* "<item/> Acquired!>" + Dict{"item":"Breadge"} = "Breadge Acquired!"
    /// 
    /// This function is preferred compared to string interpolation to better accommodate for different language SVO orders
    /// and other intricacies. *ex.* [获得][物品]="[acquired][item]" for CN is reverse of default ordering.
    /// </summary>
    /// <returns></returns>
    public string Interpolate(string format, Dictionary<string, string> substitutions)
    {
        foreach (var (k, v) in substitutions)
        {
            format = format.Replace($"<{k}/>", v);
        }

        return format;
    }
}

public static class DialogueTableProviderExtensions
{
    /// <summary>
    /// Retrieves localization from table, translation is inserted by the LocalizationLoader at runtime
    /// </summary>
    /// <param name="key">Enum value</param>
    /// <param name="i">Integer index in case enum value is mapped to multiple consecutive entries</param>
    /// <typeparam name="T">Enum type to be used by the implementer</typeparam>
    /// <returns>Pair of (original, translated) strings</returns>
    public static LocalizationPair GetLocalized<I, T>(this I self,T key, int i = 0) where I: MonoBehaviour, IDialogueTableProvider where T : Enum
    {
        return self.GetLocalized(key, i);
    }

    public static LocalizationPair GetLocalized<I>(this I self, string key, int i = 0)
        where I : MonoBehaviour, IDialogueTableProvider
    {
        return self.GetLocalized(key, i);
    }
    
    public static string GetLocalizedSingle<I, T>(this I self,T key, int i = 0) where I: MonoBehaviour, IDialogueTableProvider where T : Enum
    {
        var pair = self.GetLocalized(key, i);
        return pair.TranslatedFallbackToOriginal;
    }

    public static string GetLocalizedSingle<I, T>(this I self, string key, int i = 0)
        where I : MonoBehaviour, IDialogueTableProvider
    {
        var pair = self.GetLocalized(key, i);
        return pair.TranslatedFallbackToOriginal;
    }

    public static string Interpolate<I>(this I self, string format, Dictionary<string, string> substitutions)
    {
        return self.Interpolate(format, substitutions);
    }
}