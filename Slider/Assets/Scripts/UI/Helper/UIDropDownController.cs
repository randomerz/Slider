using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIDropDownController : TMP_Dropdown
{
    public UnityEvent onDropdownCreate;
    public UnityEvent onDropdownDestroy;

    public SelectableSet mySelectableSet;
    private List<Selectable> selectables = new();

    protected override GameObject CreateDropdownList(GameObject template)
    {
        GameObject dropdownList = base.CreateDropdownList(template);
        
        UINavigationManager.CurrentDropdownOpen = this;
        onDropdownCreate?.Invoke();

        return dropdownList;
    }

    protected override void DestroyDropdownList(GameObject dropdownList)
    {
        base.DestroyDropdownList(dropdownList);
        UINavigationManager.CurrentDropdownOpen = null;
        onDropdownDestroy?.Invoke();
    }

    public void RegisterItem(UIDropDownButton dropDownButton)
    {
        selectables.Add(dropDownButton.myToggle);
        mySelectableSet.SetSelectebles(selectables.ToArray());
    }

    public void UnregisterItem(UIDropDownButton dropDownButton)
    {
        selectables.Remove(dropDownButton.myToggle);
        mySelectableSet.SetSelectebles(selectables.ToArray());
    }
}