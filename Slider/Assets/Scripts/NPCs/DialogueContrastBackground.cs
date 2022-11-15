using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueContrastBackground : MonoBehaviour
{
    public float borderWidth = 0.5f;
    private Vector3 offset = new Vector3(0, 1.5f);

    public TextMeshProUGUI text;
    public RectTransform backgroundRectTransform;

    private void Update() 
    {
        Bounds bounds = text.bounds;
        backgroundRectTransform.anchoredPosition = bounds.center + offset;
        backgroundRectTransform.sizeDelta = 2 * (bounds.extents + new Vector3(borderWidth, borderWidth));
    }
}
