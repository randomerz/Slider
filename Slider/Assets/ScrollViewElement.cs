using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Attach this to selectable UI elements inside of a scroll view to automatically scroll up and down
/// in the view when using keyboard navigation.
/// </summary>
[RequireComponent(typeof(Selectable))]
public class ScrollViewElement : MonoBehaviour, ISelectHandler
{
    public ScrollViewManager ScrollViewManager { get; set; }

    public void OnSelect(BaseEventData eventData)
    {
        ScrollViewManager.UpdateScrollPositionBasedOnSelectedElement(this);
    }
}
