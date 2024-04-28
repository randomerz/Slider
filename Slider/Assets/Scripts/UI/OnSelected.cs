using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OnSelected : Selectable
{
    [SerializeField] private UnityEvent onSelected;

    public override void OnSelect(BaseEventData eventData)
    {
        base.OnSelect(eventData);
        onSelected?.Invoke();
    }
}
