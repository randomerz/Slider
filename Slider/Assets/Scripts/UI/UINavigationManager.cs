using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Singleton component that handles everything related to UI keyboard navigation. This should be attached to the EventSystem in every scene.
/// Make sure to setup buttonSets properly — each menu should be matched with all of the navigatable buttons inside of it. Also make sure to
/// properly update CurrentMenu based on which menu is currently active to keep navigation working properly.
/// </summary>
public class UINavigationManager : MonoBehaviour
{
    private static UINavigationManager _instance;

    private UnityEngine.EventSystems.EventSystem eventSystem;

    [Tooltip("Match each UI panel GameObject with all the navigatable buttons inside of it.")]
    [SerializeField] private ButtonSet[] selectableSets;

    // We convert our buttonSets into a dictionary at start (dictionaries are not serializable and therefore invis in the inspector)
    private Dictionary<GameObject, Selectable[]> selectableSetDictionary;

    /// <summary>
    /// This should be set to a GameObject inside of buttonSets. Make sure this matches the currently active menu panel 
    /// or navigation won't work properly.
    /// </summary>
    public static GameObject CurrentMenu { get => _instance._currentMenu; set => _instance._currentMenu = value; }
    [SerializeField] private GameObject _currentMenu;

    /// <summary>
    /// Call this to deslect the currently selected button. No, there isn't a better approach to this. Yes, that drives me insane.
    /// </summary>
    public static void ClearSelectable()
    {
        _instance.eventSystem.SetSelectedGameObject(null);
    }

    private void Awake()
    {
        _instance = this;
        _instance.eventSystem = GetComponent<UnityEngine.EventSystems.EventSystem>();

        selectableSetDictionary = new Dictionary<GameObject, Selectable[]>();
        foreach (ButtonSet set in selectableSets)
        {
            selectableSetDictionary[set.menu] = set.selectables;
        }
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
            Debug.LogError($"EventSystemManager could not find ButtonSet for Menu Object {_instance._currentMenu}. Did you remember to setup this menu in the Button Sets?");
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
    /// based on the ordering in the inspector. Higher-up buttons are chosen first.
    /// </summary>
    public static void SelectBestButtonInCurrentMenu()
    {
        if (_instance._currentMenu == null)
        {
            return;
        }
        if (!_instance.selectableSetDictionary.ContainsKey(_instance._currentMenu))
        {
            Debug.LogError($"EventSystemManager could not find ButtonSet for Menu Object {_instance._currentMenu}");
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
