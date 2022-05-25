using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroySelf : MonoBehaviour
{
    public bool destroyInsteadOfDisable;
    public float timeToDestroy = 5;

    void Start()
    {
        StartCoroutine(DestroyMe(timeToDestroy));
    }
    
    private IEnumerator DestroyMe(float time)
    {
        yield return new WaitForSeconds(time);

        if (destroyInsteadOfDisable)
            Destroy(gameObject);
        else
            gameObject.SetActive(false);
    }
}
