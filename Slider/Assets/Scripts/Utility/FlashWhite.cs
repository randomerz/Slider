using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class FlashWhite : MonoBehaviour
{
    private SpriteRenderer mySprite;
    public Material whiteSpriteMat;
    private Material oldMat;

    // Start is called before the first frame update
    void Awake()
    {
        mySprite = GetComponent<SpriteRenderer>();
        oldMat = mySprite.material;
    }
    
    public void Flash(int n)
    {
        StopAllCoroutines();
        StartCoroutine(_Flash(n));
    }

    public void SetSpriteActive(bool value)
    {
        mySprite.enabled = value;
    }

    private IEnumerator _Flash(int n)
    {
        for (int i = 0; i < n; i++)
        {
            mySprite.material = whiteSpriteMat;

            yield return new WaitForSeconds(0.25f);

            mySprite.material = oldMat;

            yield return new WaitForSeconds(0.25f);
        }
    }
}
