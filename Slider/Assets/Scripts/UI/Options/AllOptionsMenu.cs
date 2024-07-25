using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class AllOptionsMenu : MonoBehaviour
{
    public enum MenuType {
        OPTIONS,
        SOUND,
        DISPLAY,
        CONTROLS,
        MISC,
    }

    public List<GameObject> panels;

    private MenuType currentSubMenu;

    private void OnEnable()
    {
        OpenMenu(MenuType.OPTIONS);
    }

    // Unity doesn't serialize enums in UnityEvents
    public void OpenMenu(int menuType) => OpenMenu((MenuType)menuType);

    public void OpenMenu(MenuType menuType)
    {
        SelectableSet currentSubMenuSelectableSet = panels[(int)currentSubMenu].GetComponent<SelectableSet>();
        currentSubMenuSelectableSet.gameObject.SetActive(false);
        GetComponent<SelectableSet>().RemoveSubSelectableSet(currentSubMenuSelectableSet);

        currentSubMenu = menuType;

        currentSubMenuSelectableSet = panels[(int)currentSubMenu].GetComponent<SelectableSet>();
        panels[(int)currentSubMenu].SetActive(true);
        GetComponent<SelectableSet>().AddSubSelectableSet(currentSubMenuSelectableSet);
    }

    public void SelectBestButtonInCurrentSubMenu()
    {
        SelectableSet currentSubMenuSelectableSet = panels[(int)currentSubMenu].GetComponent<SelectableSet>();
        Selectable bestSelectableInCurrentMenu = currentSubMenuSelectableSet.Selectables.Where(selectable => selectable.IsInteractable()).First();
        CoroutineUtils.ExecuteAfterEndOfFrame(() => bestSelectableInCurrentMenu.Select(), this);
    }

    public void SelectSideNavButtonForCurrentSubMenu()
    {
        CoroutineUtils.ExecuteAfterEndOfFrame(() => GetComponent<SelectableSet>().Selectables[(int)currentSubMenu].Select(), this);
    }
}
