using System;
using System.Linq;
using Localization;
using TMPro;
using UnityEngine;

public class LocaleSelector : MonoBehaviour
{
    private void Start()
    {
        Dropdown.ClearOptions();
        
        var sortedOptions = LocalizationFile.LocaleList.Select(locale =>
        {
            TMP_Dropdown.OptionData data = new();
            data.text = locale;
            return data;
        }).ToList();
        
        Dropdown.options.AddRange(sortedOptions);
        Dropdown.value = 0;
        Dropdown.RefreshShownValue();
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
        LocalizationLoader.Refresh(Dropdown.options[Dropdown.value].text);
    }
}
