using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogAnimationController : MonoBehaviour
{
    [SerializeField] private List<SpriteRenderer> spriteRenderers = new List<SpriteRenderer>();
    [SerializeField] private float startRange = 1;
    [SerializeField] private float animationDuration = 2;

    [SerializeField] private bool isVisible = true;
    private List<Coroutine> coroutines = new List<Coroutine>();

    [Header("Tools")]
    [SerializeField] private Vector3 minPosition;
    [SerializeField] private Vector3 maxPosition;

    public void SetIsVisible(bool value)
    {
        if (value != isVisible)
        {
            isVisible = value;
            StopCoroutines();
            AnimateAllAlphas(isVisible ? 0 : 1, isVisible ? 1 : 0);
        }
    }

    private void StopCoroutines()
    {
        foreach (Coroutine c in coroutines)
        {
            StopCoroutine(c);
        }
        coroutines.Clear();
    }

    private void AnimateAllAlphas(float from, float to)
    {
        foreach (SpriteRenderer sr in spriteRenderers)
        {
            coroutines.Add(StartCoroutine(AnimateAlpha(sr, from, to, Random.Range(0f, startRange))));
        }
    }

    private IEnumerator AnimateAlpha(SpriteRenderer spriteRenderer, float from, float to, float initialDelay)
    {
        yield return new WaitForSeconds(initialDelay);

        Color c = spriteRenderer.color;
        float t = 0;
        while (t < animationDuration)
        {
            c.a = Mathf.Lerp(from, to, t / animationDuration);
            spriteRenderer.color = c;

            yield return null;
            t += Time.deltaTime;
        }

        c.a = to;
        spriteRenderer.color = c;
    }


    // Tools
    public void RandomizeFogPositions()
    {
        foreach (SpriteRenderer sr in spriteRenderers)
        {
            sr.transform.position = new Vector3(
                Mathf.Round(Random.Range(minPosition.x, maxPosition.x)), 
                Mathf.Round(Random.Range(minPosition.y, maxPosition.y))
            );
        }
    }
}
