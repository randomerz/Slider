using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MilitaryUnitFlagResetter : MonoBehaviour
{
    [SerializeField] private Transform resetTransform;
    [SerializeField] private AnimationCurve resetAnimationCurve;
    [SerializeField] private AnimationCurve resetHeightBonusCurve;
    [SerializeField] private MilitaryUnitFlag item;
    [SerializeField] private ParticleSystem particles;

    private const float RESET_DURATION = 0.75f;

    // private bool isResetting;
    private Coroutine coroutine;

    public void ResetItem(System.Action onFinish=null)
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
        }

        RemoveFromPlayer();
        transform.SetParent(resetTransform);
        item.SetCollider(false);
        particles.Play();

        Vector3 startPos = transform.position;
        
        coroutine = CoroutineUtils.ExecuteEachFrame(
            (x) => {
                Vector3 newPos = Vector3.Lerp(startPos, resetTransform.position, x);
                newPos += Vector3.up * resetHeightBonusCurve.Evaluate(x);
                transform.position = newPos;
            },
            () => {
                StartCoroutine(HoldInNewPosition(() => {
                    item.SetCollider(true);
                    onFinish?.Invoke();
                    transform.position = resetTransform.position;
                    transform.SetParent(resetTransform);
                    particles.Stop();
                    coroutine = null;
                }));
            },
            this,
            RESET_DURATION,
            resetAnimationCurve
        );
    }

    private IEnumerator HoldInNewPosition(System.Action action)
    {
        yield return new WaitUntil(() => !item.attachedUnit.NPCController.hasMoveQueuedOrIsExecuting);
        
        action?.Invoke();
    }

    public void RemoveFromPlayer()
    {
        PlayerInventory.RemoveItem();
        transform.SetParent(resetTransform.parent);
    }

    public void SetResetTransform(Transform t)
    {
        resetTransform = t;
        transform.SetParent(t);
    }
}