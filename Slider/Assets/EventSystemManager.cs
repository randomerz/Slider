using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is cancer. I hate Unity. The only decent way to deselect a button is to have *this* script attached to the EventSystem
/// and call a method to clear the selected UI element.....
/// </summary>
public class EventSystemManager : MonoBehaviour
{
    private static EventSystemManager _instance;

    private UnityEngine.EventSystems.EventSystem eventSystem;

    /// <summary>
    /// Call this to deslect the currently select button. No, there isn't a better approach to this. Yes, that drives me insane.
    /// </summary>
    public static void ClearSelectable()
    {
        _instance.eventSystem.SetSelectedGameObject(null);
        // Debug.Log(_instance.eventSystem.currentSelectedGameObject);
    }

    private void Awake()
    {
        _instance = this;
        _instance.eventSystem = GetComponent<UnityEngine.EventSystems.EventSystem>();
    }
}
