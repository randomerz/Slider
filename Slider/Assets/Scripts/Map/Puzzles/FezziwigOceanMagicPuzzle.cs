using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FezziwigOceanMagicPuzzle : MonoBehaviour
{
    [SerializeField] private int islandId;
    [SerializeField] private Collider2D npcCollider;
    [SerializeField] private Transform startTransform;
    [SerializeField] private Transform jumpTransform;
    [SerializeField] private AnimationCurve xJumpMotion;
    [SerializeField] private AnimationCurve yJumpMotion;
    [SerializeField] private float jumpDuration;
    [SerializeField] private float fallDuration;
    [SerializeField] protected SpriteRenderer spriteRenderer;

    private void OnEnable() { SGridAnimator.OnSTileMoveEnd += OnTileMoved; }

    private void OnDisable() { SGridAnimator.OnSTileMoveEnd -= OnTileMoved; }

    private void OnTileMoved(object sender, SGridAnimator.OnTileMoveArgs e) {
        if (SGrid.current.GetStile(islandId) != null &&
            SGrid.current.GetStile(islandId).isTileActive &&
            e.stile.islandId == islandId)
        {
            Fall();
        }
    }

    private void Jump() {
        spriteRenderer.sortingOrder = 5;
        StartCoroutine(AnimateSmoothMove(jumpDuration, startTransform.localPosition, jumpTransform.localPosition));
    }

    private void Fall() {
        StartCoroutine(AnimateSmoothMove(fallDuration, jumpTransform.localPosition, startTransform.localPosition, -1));
    }

    /// <summary>
    /// Animates Fezziwig's either fall or jump.
    /// Probably overengineered but delegate go brrrr
    /// </summary>
    /// <param name="duration"></param>
    /// <param name="start"></param>
    /// <param name="target"></param>
    /// <param name="direction"></param>
    /// <returns></returns>
    private IEnumerator AnimateSmoothMove(float duration, Vector3 start, Vector3 target, int direction = 1) {
        float t;
        Func<bool> loopParam;
        Func<float, float, Vector3> posCalc;

        if (direction == 1) {
            t = 0;
            loopParam = () => { return t < duration; };
            posCalc = (float x, float y) => { return new Vector3(Mathf.Lerp(start.x, target.x, x),
                                                                Mathf.Lerp(start.y, target.y, y)); };
        } else {
            t = duration;
            loopParam = () => { return t >= 0; };
            posCalc = (float x, float y) => { return new Vector3(Mathf.Lerp(target.x, start.x, x),
                                                                Mathf.Lerp(target.y, start.y, y)); };
        }

        while (loopParam())
        {
            float x = xJumpMotion.Evaluate(t / duration);
            float y = yJumpMotion.Evaluate(t / duration);
            Vector3 pos = new Vector3(Mathf.Lerp(start.x, target.x, x),
                                      Mathf.Lerp(start.y, target.y, y));
            pos = posCalc(x, y);

            transform.localPosition = pos;

            yield return null;
            t += Time.deltaTime * direction;
        }

        transform.localPosition = target;
    }

}
