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
        
        LocalizationLoader.RefreshSilent(sortedOptions[0].text); // assume English will always be in there :)
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
        
        retriever.WriteSettingValue(Dropdown.options[Dropdown.value].text);
        
        // Refresh is done 
        // LocalizationLoader.Refresh(Dropdown.options[Dropdown.value].text);
    }
}
