using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class FlashWhiteUI : MonoBehaviour
{
    private Image mySprite;
    public Material whiteSpriteMat;
    private Material oldMat;

    protected virtual void Awake()
    {
        UpdateRefs();
    }

    private void OnDisable()
    {
        if(oldMat != null)
            mySprite.material = oldMat;
    }

    public void Flash(int n, Action callback = null)
    {
        StopAllCoroutines();
        if(mySprite == null)
            UpdateRefs();
        StartCoroutine(_Flash(n, callback));
    }

    private void UpdateRefs()
    {
        mySprite = GetComponent<Image>();
        oldMat ??= mySprite.material;
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

            yield return new WaitForSeconds(0.25f);

            mySprite.material = oldMat;

            yield return new WaitForSeconds(0.25f);
        }

        if(callback !=null)
            callback?.Invoke();
    }
}
