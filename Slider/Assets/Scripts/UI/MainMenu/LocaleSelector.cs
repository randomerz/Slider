using System;
using System.Linq;
using Localization;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(SettingRetriever))]
public class LocaleSelector : MonoBehaviour
{
    private SettingRetriever retriever;
    
    private void Start()
    {
        retriever = GetComponent<SettingRetriever>();
        
        Dropdown.ClearOptions();
        
        var sortedOptions = LocalizationFile.LocaleList(retriever.ReadSettingValue() as string).Select(locale =>
        {
            TMP_Dropdown.OptionData data = new()
            {
                text = locale
            };
            return data;
        }).ToList();
        
        Dropdown.options.AddRange(sortedOptions);
        Dropdown.value = 0;
        Dropdown.RefreshShownValue();

        // if previously configured locale is no longer valid (not present as a mod, etc.), then the next most prominent
        // locale will be selected instead. this is just English because of how locales are sorted
        if (!sortedOptions[0].text.Equals(retriever.ReadSettingValue() as string))
        {
            retriever.WriteSettingValue(sortedOptions[0].text);
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
    }

    public void OnSelectionValueChanged()
    {
        Dropdown.gameObject.SetActive(false);
        ShowHide.SetActive(true);

        string selection = Dropdown.options[Dropdown.value].text;
        retriever.WriteSettingValue(selection);

        // non English font does not have outline (TMP has outline but it seems to be UI text only)
        // instead of messing with outlines just force text background to be on, player can toggle
        // it back if they want to
        if (!selection.Equals(LocalizationFile.DefaultLocale))
        {
            SettingsManager.Setting(Settings.HighContrastTextEnabled).SetCurrentValue(true);
        }
        
        // Refresh is done through SettingsManager
        // LocalizationLoader.Refresh(Dropdown.options[Dropdown.value].text);
    }
}