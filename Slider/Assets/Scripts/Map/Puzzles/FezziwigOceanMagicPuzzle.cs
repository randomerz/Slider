using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FezziwigOceanMagicPuzzle : MonoBehaviour
{
    enum SpellState {
        start,
        jumping,
        casting,
        falling,
        finished
    }

    [SerializeField] private int islandId;
    [SerializeField] private Collider2D npcCollider;
    [SerializeField] private Transform startTransform;
    [SerializeField] private Transform jumpTransform;
    [SerializeField] private AnimationCurve xJumpMotion;
    [SerializeField] private AnimationCurve yJumpMotion;
    [SerializeField] private float jumpDuration;
    [SerializeField] private float fallDuration;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private OceanArtifact oceanArtifact;

    private SpellState state;

    void Start() { 
        transform.localPosition = startTransform.localPosition; 
        state = SpellState.start;
    }

    private void OnEnable() { SGridAnimator.OnSTileMoveEnd += OnTileMoved; }

    private void OnDisable() { SGridAnimator.OnSTileMoveEnd -= OnTileMoved; }

    private void OnTileMoved(object sender, SGridAnimator.OnTileMoveArgs e) {
        if (SGrid.current.GetStile(islandId) != null &&
            SGrid.current.GetStile(islandId).isTileActive &&
            e.stile.islandId == islandId &&
            state == SpellState.casting)
        {
            Fall();
        }
    }

    /// <summary>
    /// Starts the spell jumping process
    /// </summary>
    public void Jump() {
        if (state == SpellState.start) {
            state = SpellState.jumping;
            spriteRenderer.sortingOrder = 5;
            StartCoroutine(AnimateSmoothMove(jumpDuration, startTransform.localPosition, jumpTransform.localPosition));
        }
    }

    /// <summary>
    /// Starts the spell casting process
    /// </summary>
    public void CastSpell() {
        state = SpellState.casting;
        StartCoroutine(RotateTiles());
    }

    private IEnumerator RotateTiles() {
        Vector2Int[] rotateButtons = {
            new Vector2Int(0,0),
            new Vector2Int(1,0),
            new Vector2Int(0,1),
            new Vector2Int(1,1)
        };
        foreach(Vector2Int currButton in rotateButtons) {
            if (state == SpellState.falling) break;
            oceanArtifact.RotateTiles(currButton.x, currButton.y, false);
            yield return new WaitForSeconds(1f);
        }

        if (state == SpellState.falling) {
            state = SpellState.start;
        } else {
            Debug.Log("Spell Complete");
        }
    }

    private void Fall() {
        state = SpellState.falling;
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

    // Methods that check for the current state


}
