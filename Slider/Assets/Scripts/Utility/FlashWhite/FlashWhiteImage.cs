using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class FlashWhiteImage : MonoBehaviour, IFlashWhite
{
    private Image mySprite;
    public Material whiteSpriteMat;
    private Material oldMat;
    
    public float flashTime = 0.25f;

    protected virtual void Awake()
    {
        UpdateRefs();
    }

    private void OnDisable()
    {
        if (oldMat != null)
        {
            mySprite.material = oldMat;
        }
    }

    public void Flash(int n, Action callback = null)
    {
        StopAllCoroutines();
        if (mySprite == null)
        {
            UpdateRefs();
        }
        if (!gameObject.activeInHierarchy)
        {
            return;
        }
        StartCoroutine(_Flash(n, callback));
    }

    private void UpdateRefs()
    {
        mySprite = GetComponent<Image>();
        oldMat = oldMat != null ? oldMat : mySprite.material;
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

        if (callback !=null)
            callback?.Invoke();
    }
}
