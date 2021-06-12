using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIButtonTextEffect : MonoBehaviour
{
    public RectTransform target;
    public int amount;

    private Vector3 defaultPos;

    private void Awake()
    {
        defaultPos = target.anchoredPosition;
    }

    public void MoveDown()
    {
        target.anchoredPosition = defaultPos - amount * Vector3.up;
    }

    public void MoveDefault()
    {
        target.anchoredPosition = defaultPos;
    }
}
