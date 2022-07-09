using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Handles everything related to UI keyboard navigation. This should be attached to the EventSystem in every scene.
/// Make sure to setup buttonSets properly — each menu should be matched with all of the navigatable buttons inside of it. Also make sure to
/// properly update CurrentMenu based on which menu is currently active to keep navigation working properly.
/// <para/>
/// <b>Note: It is recommended that we transition away from setting up our button sets on this component and instead use the <see cref="SelectableSet"/>
/// component attached to UI GameObjects. This will be far more robust, especially in regards to switching scenes and in scenes where there
/// are lots of different unique UI elements such as the Ocean Shop.</b>
/// </summary>
/// <remarks>Author: Travis</remarks>
public class UINavigationManager : Singleton<UINavigationManager>
{
    private UnityEngine.EventSystems.EventSystem eventSystem;

    [Tooltip("Match each UI panel GameObject with all the navigatable buttons inside of it.")]
    [SerializeField] private ButtonSet[] selectableSets;

    // We convert our buttonSets into a dictionary at start (dictionaries are not serializable and therefore invisible in the inspector)
    private Dictionary<GameObject, Selectable[]> selectableSetDictionary;

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
            if (value != null && !_instance.selectableSetDictionary.ContainsKey(value))
            {
                SelectableSet selectableSet = value.GetComponent<SelectableSet>();
                if (selectableSet == null)
                {
                    LogSelectableNotFoundError();
                } else
                {
                    _instance.selectableSetDictionary[value] = value.GetComponent<SelectableSet>().Selectables;
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
        InitializeSingleton();

        _instance.eventSystem = GetComponent<UnityEngine.EventSystems.EventSystem>();

        if (selectableSets.Length > 0)
        {
            Debug.LogWarning("UINavigationManager's ButtonSets field is deprecated. You should be attaching SelectableSet components to " +
                "your menu objects instead.");
        }
        selectableSetDictionary = new Dictionary<GameObject, Selectable[]>();
        foreach (ButtonSet set in selectableSets)
        {
            selectableSetDictionary[set.menu] = set.selectables;
        }


        Controls.RegisterBindingBehavior(this, Controls.Bindings.UI.Navigate,
            context =>
            {
                if (CurrentMenu != null && InMouseControlMode)
                {
                    InMouseControlMode = false;
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
        if (!_instance.selectableSetDictionary.ContainsKey(_instance._currentMenu))
        {
            LogSelectableNotFoundError();
            return false;
        }

        foreach (Selectable selectable in _instance.selectableSetDictionary[_instance._currentMenu])
        {
            if (_instance.eventSystem.currentSelectedGameObject == selectable.gameObject)
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
        if (!_instance.selectableSetDictionary.ContainsKey(_instance._currentMenu))
        {
            LogSelectableNotFoundError();
            return;
        }
        foreach (Selectable selectable in _instance.selectableSetDictionary[_instance._currentMenu])
        {
            if (selectable.interactable)
            {
                selectable.Select();
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
    private static IEnumerator ILockoutSelectablesInCurrentMenu(System.Action callback, float duration)
    {
        // We need to track all of the ones we disable and then re-enable them, otherwise we would
        // re-activate ones that were disabled before this method was called.
        List<Selectable> selectablesToReactivate = new List<Selectable>();

        // Disable all interactables in set
        foreach (Selectable selectable in _instance.selectableSetDictionary[_instance._currentMenu])
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