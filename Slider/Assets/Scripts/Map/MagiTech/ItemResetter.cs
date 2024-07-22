using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemResetter : MonoBehaviour
{
    [SerializeField] private Transform resetTransform;
    [SerializeField] private AnimationCurve resetAnimationCurve;
    [SerializeField] private AnimationCurve resetHeightBonusCurve;
    [SerializeField] private ParticleSystem particlesToAttachToItem;

    private const float RESET_DURATION = 0.75f;

    private Coroutine coroutine;

    public void ResetItem(Item item, System.Action onFinish=null)
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
        }

        RemoveFromPlayer(item);
        item.transform.SetParent(resetTransform);
        item.SetCollider(false);
        particlesToAttachToItem.Play();
        particlesToAttachToItem.transform.SetParent(item.transform);
        particlesToAttachToItem.transform.position = item.transform.position;

        Vector3 startPos = item.transform.position;
        
        coroutine = CoroutineUtils.ExecuteEachFrame(
            (x) => {
                Vector3 newPos = Vector3.Lerp(startPos, resetTransform.position, x);
                newPos += Vector3.up * resetHeightBonusCurve.Evaluate(x);
                item.transform.position = newPos;
            },
            () => {
                item.SetCollider(true);
                onFinish?.Invoke();
                item.transform.position = resetTransform.position;
                item.transform.SetParent(resetTransform);
                particlesToAttachToItem.Stop();
                particlesToAttachToItem.transform.SetParent(transform);
                coroutine = null;
            },
            this,
            RESET_DURATION,
            resetAnimationCurve
        );
    }

    public void RemoveFromPlayer(Item item)
    {
        if (PlayerInventory.GetCurrentItem() == item)
        {
            PlayerInventory.RemoveItem();
            transform.SetParent(resetTransform.parent);
        }
    }

    public void SetResetTransform(Transform t)
    {
        resetTransform = t;
        transform.SetParent(t);
    }
}