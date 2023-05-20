using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MountainTileDither : MonoBehaviour
{
    [SerializeField] private Material materialTemplate;
    [SerializeField] private AnimationCurve ditherCurve;
    [SerializeField] private SpriteRenderer spriteRenderer;
    private int numDithers = 7;
    private int islandId;

    private Material materialInstance;

    private void Awake() {
        islandId = GetComponentInParent<STile>().islandId;
    }

    private void Start() {
        materialInstance = new Material(materialTemplate);
        spriteRenderer.material = materialInstance;
        spriteRenderer.enabled = false;
    }
  
    public void AnimateTileDither(float duration) {
        StartCoroutine(DitherCoroutine(duration));
    }

    private IEnumerator DitherCoroutine(float duration) {
        float t = 0;
        spriteRenderer.enabled = true;
        while (t < duration) {
            t += Time.deltaTime;
            materialInstance.SetFloat("_DitherNum", EvaluateCurve(t/duration));
            yield return null;
        }
        materialInstance.SetFloat("_DitherNum", 0);
        spriteRenderer.enabled = false;
    }

    private float EvaluateCurve (float normalizedTime)
    {
        float value = ditherCurve.Evaluate(normalizedTime);
        return Mathf.Clamp(Mathf.Round(value * numDithers), 0, numDithers);
    }
}
