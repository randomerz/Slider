using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class FlashWhiteText : MonoBehaviour, IFlashWhite
{
    private TextMeshProUGUI text;
    public Color whiteColor = GameSettings.white;
    private Color oldColor;
    
    public float flashTime = 0.25f;

    void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
        oldColor = text.color;
    }
    
    public void Flash(int n, Action callback = null)
    {
        StopAllCoroutines();
        StartCoroutine(_Flash(n, callback));
    }

    private IEnumerator _Flash(int n, Action callback)
    {
        for (int i = 0; i < n; i++)
        {
            text.color = whiteColor;

            yield return new WaitForSeconds(flashTime);

            text.color = oldColor;

            yield return new WaitForSeconds(flashTime);
        }

        callback?.Invoke();
    }
}
