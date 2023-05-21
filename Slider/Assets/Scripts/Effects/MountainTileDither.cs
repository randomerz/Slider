using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MountainTileDither : MonoBehaviour
{
    [SerializeField] private Material tileMaterialTemplate;
    [SerializeField] private Material borderMaterialTemplate;
    [SerializeField] private AnimationCurve ditherCurve;
    [SerializeField] private SpriteRenderer tileSpriteRenderer;
    [SerializeField] private SpriteRenderer borderSpriteRenderer;

    private int numDithers = 7;
    private int islandId;

    private Material tileMaterialInstance;
    private Material borderMaterialInstance;


    private void Awake() {
        islandId = GetComponentInParent<STile>().islandId;
    }

    private void Start() {
        tileMaterialInstance = new Material(tileMaterialTemplate);
        tileSpriteRenderer.material = tileMaterialInstance;
        tileSpriteRenderer.enabled = false;
        borderMaterialInstance = new Material(borderMaterialTemplate);
        borderSpriteRenderer.material = borderMaterialInstance;
        borderSpriteRenderer.enabled = false;
    }
  
    public void AnimateTileDither(float duration) {
        StartCoroutine(DitherCoroutine(duration, tileSpriteRenderer, tileMaterialInstance));
    }

    public void AnimateBorderTileDither(float duration) {
        StartCoroutine(DitherCoroutine(duration, borderSpriteRenderer, borderMaterialInstance));
    }

    private IEnumerator DitherCoroutine(float duration, SpriteRenderer spriteRenderer, Material material) {
        float t = 0;
        spriteRenderer.enabled = true;
        while (t < duration) {
            t += Time.deltaTime;
            material.SetFloat("_DitherNum", EvaluateCurve(t/duration));
            yield return null;
        }
        material.SetFloat("_DitherNum", 0);
        spriteRenderer.enabled = false;
    }

    private float EvaluateCurve (float normalizedTime)
    {
        float value = ditherCurve.Evaluate(normalizedTime);
        return Mathf.Clamp(Mathf.Round(value * numDithers), 0, numDithers);
    }
}
