using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BartenderPanickedSearching : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;

    private void OnEnable()
    {
        StartCoroutine(FlipLoop());
    }

    private IEnumerator FlipLoop()
    {
        while (true)
        {
            spriteRenderer.flipX = !spriteRenderer.flipX;
            yield return new WaitForSeconds(1);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        StopAllCoroutines();
    }

    private void OnTriggerExit(Collider other)
    {
        StartCoroutine(FlipLoop());
    }
}
