using System.Collections;
using System.Collections.Generic;
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

    private void OnEnable()
    {
        OpenMenu(MenuType.OPTIONS);
    }

    private void CloseAllMenus()
    {
        foreach (GameObject menu in panels)
        {
            menu.SetActive(false);
        }
    }


    public void OpenMenu(MenuType menuType) => OpenMenu((int)menuType);

    // Unity doesn't serialize enums in UnityEvents
    public void OpenMenu(int menuType)
    {
        CloseAllMenus();
        panels[menuType].SetActive(true);
    }
}
