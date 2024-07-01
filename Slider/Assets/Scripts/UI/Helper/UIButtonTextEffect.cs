using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIButtonTextEffect : MonoBehaviour
{
    public TMP_Text textMeshPro;
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

    public void SetUnderlined(bool underlined)
    {
        if (underlined)
            textMeshPro.fontStyle = FontStyles.Underline;
        else
            textMeshPro.fontStyle = FontStyles.Normal;

    }
}
