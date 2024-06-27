using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;

/// <summary>
/// Handles everything related to UI keyboard navigation. This should be attached to the EventSystem in every scene.
/// Make sure to properly update CurrentMenu based on which menu is currently active to keep navigation working properly.
/// </summary>
/// <remarks>Author: Travis</remarks>
public class UINavigationManager : Singleton<UINavigationManager>
{
    private UnityEngine.EventSystems.EventSystem eventSystem;

    /// <summary>
    /// This should be set to a GameObject inside of buttonSets or a GameObject which has a SelectableSet component. Make sure this matches the currently active menu panel
    /// or navigation won't work properly.
    /// </summary>
    public static GameObject CurrentMenu
    { 
        get => _instance._currentMenu;
        set
        {
            _instance._currentMenu = value;
            if (value != null)
            {
                if (value.GetComponent<SelectableSet>() == null)
                {
                    LogSelectableNotFoundError();
                }
            }
        }
    }
    private GameObject _currentMenu;

    /// <summary>
    /// Making a keyboard input switches to keyboard mode and clicking with the mouse switches to mouse mode. The key
    /// distinction is that buttons will not be put into the Selected state when we are in mouse mode.
    /// </summary>
    private static bool _inMouseControlMode = true;
    public static bool InMouseControlMode
    {
        get { return _inMouseControlMode; }
        set
        {
            _inMouseControlMode = value;
            if (_inMouseControlMode) { ClearSelectable(); }
            else
            {
                SelectBestButtonInCurrentMenu();
            }
        }
    }

    /// <summary>
    /// Call this to deslect the currently selected button. No, there isn't a better approach to this. Yes, that drives me insane.
    /// </summary>
    public static void ClearSelectable()
    {
        _instance.eventSystem.SetSelectedGameObject(null);
    }

    private void Awake()
    {
        InitializeSingleton(this);

        _instance.eventSystem = GetComponent<UnityEngine.EventSystems.EventSystem>();

        Controls.RegisterBindingBehavior(this, Controls.Bindings.UI.Navigate,
            context =>
            {
                if (CurrentMenu != null)
                {
                    if (InMouseControlMode)
                    {
                        InMouseControlMode = false;
                    }
                    if (!ButtonInCurrentMenuIsSelected())
                    {
                        SelectBestButtonInCurrentMenu();
                    }
                }
                
            }
        );
        Controls.RegisterBindingBehavior(this, Controls.Bindings.UI.Submit,
            context =>
            {
                if (CurrentMenu != null && InMouseControlMode)
                {
                    InMouseControlMode = false;
                }
            }
        );
        Controls.RegisterBindingBehavior(this, Controls.Bindings.UI.Click,
            context =>
            {
                if (CurrentMenu != null && !InMouseControlMode)
                {
                    InMouseControlMode = true;
                }
            }
        );
        Controls.RegisterBindingBehavior(this, Controls.Bindings.UI.Cancel, (_) =>
        {
            if (CurrentMenu != null)
            {
                UIMenu menu = CurrentMenu.GetComponent<UIMenu>();
                if (menu != null)
                {
                    menu.MoveToParentMenu();
                }
            }
        });

        // Have you tried turning the EventSystem off and on again?
        SceneManager.sceneLoaded += (Scene s1, LoadSceneMode mode) => {
            if (this != null && gameObject != null)
            {
                gameObject.SetActive(false);
                gameObject.SetActive(true);
            }
        };
    }

    /// <summary>
    /// Use this to check if a button in the current menu panel is selected (such as for selecting a default button when a
    /// navigation key is pressed and none is currently selected.)
    /// </summary>
    /// <returns>
    /// True if the EventSystem's currentSelectedGameObject is inside of the currentMenu's buttons list.
    /// False if currentMenu is null or the currentMenu is not in the buttonSets array.
    /// </returns>
    public static bool ButtonInCurrentMenuIsSelected()
    {
        if (_instance._currentMenu == null)
        {
            return false;
        }

        foreach (Selectable selectable in AllSelectablesInCurrentMenu())
        {
            if (selectable != null && _instance.eventSystem.currentSelectedGameObject == selectable.gameObject)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Selects the first selectable button inside of the buttons array for the currentMenu in buttonSets 
    /// based on the ordering in the inspector. Higher-up buttons are chosen first. Does nothing if the
    /// UI is currently in mouse control mode.
    /// </summary>
    public static void SelectBestButtonInCurrentMenu()
    {
        if (_inMouseControlMode || _instance._currentMenu == null)
        {
            return;
        }
        foreach (Selectable selectable in AllSelectablesInCurrentMenu())
        {
            if (selectable.interactable)
            {
                CoroutineUtils.ExecuteAfterEndOfFrame(() => selectable.Select(), _instance);
                break;
            }
        }
    }

    /// <summary>
    /// Apply a temporary lockout to all selectables in the current menu panel, making them
    /// un-interactable for the passed in duration. This has no effect if currentMenu
    /// is null.
    /// </summary>
    public static void LockoutSelectablesInCurrentMenu(System.Action callback = null, float duration = 1)
    {
        if (_instance._currentMenu == null)
        {
            return;
        }

        _instance.StartCoroutine(ILockoutSelectablesInCurrentMenu(callback, duration));
    }

    public static void SetCurrentSelectable(Selectable selectable)
    {
        selectable.Select();
    }

    public static GameObject GetCurrentlySelectedGameObject()
    {
        return _instance.eventSystem.currentSelectedGameObject;
    }

    private static IEnumerator ILockoutSelectablesInCurrentMenu(System.Action callback, float duration)
    {
        // We need to track all of the ones we disable and then re-enable them, otherwise we would
        // re-activate ones that were disabled before this method was called.
        List<Selectable> selectablesToReactivate = new List<Selectable>();

        // Disable all interactables in set
        foreach (Selectable selectable in AllSelectablesInCurrentMenu())
        {
            if (selectable.interactable)
            {
                selectable.interactable = false;
                selectablesToReactivate.Add(selectable);
            }
        }

        yield return new WaitForSeconds(duration);

        // Re-enable
        foreach (Selectable selectable in selectablesToReactivate)
        {
            selectable.interactable = true;
        }
        callback?.Invoke();
    }

    private static List<Selectable> AllSelectablesInCurrentMenu()
    {
        List<Selectable> allSelectables = new();

        SelectableSet selectableSet = CurrentMenu.GetComponent<SelectableSet>();

        if (selectableSet == null)
        {
            LogSelectableNotFoundError();
        }

        Queue<SelectableSet> selectableSetsToAdd = new();
        selectableSetsToAdd.Enqueue(selectableSet);

        while (selectableSetsToAdd.Count > 0)
        {
            SelectableSet current = selectableSetsToAdd.Dequeue();
            allSelectables.AddRange(current.Selectables);
            current.SubSelectableSets.ToList().ForEach(subSet => selectableSetsToAdd.Enqueue(subSet));
        }

        return allSelectables;
    }

    private static void LogSelectableNotFoundError()
    {
        Debug.LogError($"UINavigationManager could not find a SelectableSet for {_instance._currentMenu.name} ({_instance._currentMenu.GetInstanceID()}). " +
                $"Make sure this menu object has a SelectableSet component that is properly setup with its child selectables.");
    }
}

/// <summary>
/// Maps a menu panel GameObject to a set of navigatable buttons inside of it.
/// This is used inside of UINavigationManager.
/// </summary>
[System.Serializable]
public struct ButtonSet
{
    public GameObject menu;
    public Selectable[] selectables;
}