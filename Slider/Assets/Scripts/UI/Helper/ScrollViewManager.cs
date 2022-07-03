using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollViewManager : MonoBehaviour
{
    [SerializeField] private List<ScrollViewElement> scrollViewElements;
    [SerializeField] private Scrollbar scrollbar;

    void Start()
    {
        foreach (ScrollViewElement scrollViewElement in scrollViewElements)
        {
            scrollViewElement.ScrollViewManager = this;
        }
    }

    public void UpdateScrollPositionBasedOnSelectedElement(ScrollViewElement selectedElement)
    {
        // 1 means the scrollbar is at the top, 0 means it is at the bottom. This might be different
        // depending on your scrollbar implementation. If so, introduce some variables for better customization
        int indexOfSelectElement = scrollViewElements.IndexOf(selectedElement);
        if (indexOfSelectElement == scrollViewElements.Count - 1)
        {
            // We special case the last element so we end up at the very bottom when it is selected
            scrollbar.value = 0;
        } else
        {
            scrollbar.value = 1 - (float) scrollViewElements.IndexOf(selectedElement) / scrollViewElements.Count;
        }
    }
}
