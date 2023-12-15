using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(SpriteRenderer))]
public class FlashWhiteSprite : MonoBehaviour, IFlashWhite
{
    private SpriteRenderer mySprite;
    public Material whiteSpriteMat;
    private Material oldMat;

    public float flashTime = 0.25f;

    void Awake()
    {
        mySprite = GetComponent<SpriteRenderer>();
        oldMat = mySprite.material;
    }
    
    public void Flash(int n, Action callback = null)
    {
        StopAllCoroutines();
        StartCoroutine(_Flash(n, callback));
    }

    public void SetSpriteActive(bool value)
    {
        mySprite.enabled = value;
    }

    private IEnumerator _Flash(int n, Action callback)
    {
        for (int i = 0; i < n; i++)
        {
            mySprite.material = whiteSpriteMat;

            yield return new WaitForSeconds(flashTime);

            mySprite.material = oldMat;

            yield return new WaitForSeconds(flashTime);
        }

        callback?.Invoke();
    }
}
