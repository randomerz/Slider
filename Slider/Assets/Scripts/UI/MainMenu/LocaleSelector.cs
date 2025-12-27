using System.Collections.Generic;
using System.Linq;
using Localization;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(SettingRetriever))]
public class LocaleSelector : MonoBehaviour
{
    private SettingRetriever retriever;
    private List<LocaleEntry> entries;
    
    private void Start()
    {
        retriever = GetComponent<SettingRetriever>();
        
        Dropdown.ClearOptions();
        
        bool isDebugMode = (bool)SettingsManager.Setting(Settings.DevConsole).GetCurrentValue();
        entries = LocalizationFile.LocaleList(retriever.ReadSettingValue() as string);
            
        var optionData = entries
            .Where(locale => locale.CanonicalName != "Debug" || Application.isEditor || isDebugMode)
            .Select(locale => {
                TMP_Dropdown.OptionData data = new()
                {
                    text = locale.DisplayName
                };
                return data;
            })
            .ToList();
        
        Dropdown.options.AddRange(optionData);
        Dropdown.value = 0;
        Dropdown.RefreshShownValue();

        // if previously configured locale is no longer valid (not present as a mod, etc.), then the next most prominent
        // locale will be selected instead. this is just English because of how locales are sorted
        if (!optionData[0].text.Equals(retriever.ReadSettingValue() as string))
        {
            retriever.WriteSettingValue(entries[0].CanonicalName);
        }

        if (entries.Count == 1)
        {
            gameObject.SetActive(false);
        }
    }

    [SerializeField]
    private GameObject ShowHide;
    
    [SerializeField]
    private TMP_Dropdown Dropdown;


    public void OnSelectorIconClicked()
    {
        ShowHide.SetActive(false);
        Dropdown.gameObject.SetActive(true);
        Dropdown.Select();
    }

    public void OnSelectionValueChanged()
    {
        var originalLocale = retriever.ReadSettingValue() as string ?? LocalizationFile.DefaultLocale;
        var selection = entries[Dropdown.value];

        // don't change anything if "switching" to the same language
        if (originalLocale.Equals(selection.CanonicalName))
        {
            return;
        }
        
        retriever.WriteSettingValue(selection.CanonicalName);
        
        // non English font does not have outline (TMP has outline but it seems to be UI text only)
        // instead of messing with outlines just force text background to be on, player can toggle
        // it back if they want to
        if (!selection.CanonicalName.Equals(LocalizationFile.DefaultLocale))
        {
            SettingsManager.Setting(Settings.HighContrastTextEnabled).SetCurrentValue(true);
        }

        LocalizationLoader.RefreshLocalization();

        // Don't put anything here... there's a force scene reload in the setting change event (see SettingsManager)
    }
}
