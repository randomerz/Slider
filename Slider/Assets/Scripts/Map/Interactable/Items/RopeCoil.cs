using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RopeCoil : Item
{
    [Header("Rope Parameters")]
    public Sprite[] ropeSprites = new Sprite[6];
    public float minDist; // min dist until the sprite starts uncoiling
    public float maxDist;
    public float maxSlowMultiplier;

    private const float ROPE_LINE_RENDERER_START_OFFSET = 0.5625f;
    public Transform ropeLineRendererTransform;
    public Transform treeLineRendererTransform;

    public Transform resetTransform;
    public AnimationCurve resetAnimationCurve;
    public AnimationCurve resetHeightBonusCurve;
    private bool isResetting;
    public float resetDuration = 1;
    
    private int distanceIndex; // 0 is closest, 5 is furthest
    private bool isPicked;
    private bool ignoreDistChecks;


    private void OnDisable()
    {
        if (Player.GetInstance() != null)
        {
            Player.SetMoveSpeedMultiplier(1f);
        }
    }

    public override void Update() 
    {
        base.Update();

        // update index
        float distance = Vector3.Distance(transform.position, treeLineRendererTransform.position);
        float unlerp = Mathf.InverseLerp(minDist, maxDist, distance);
        distanceIndex = Mathf.Clamp(Mathf.FloorToInt(unlerp * ropeSprites.Length), 0, ropeSprites.Length - 1);

        // Debug.Log($"distance {distance}, index {distanceIndex}");

        UpdateSprite();
        UpdatePlayerSpeed();

        if (
            !isResetting && 
            !ignoreDistChecks && 
            (
                distance > maxDist || 
                (PlayerInventory.GetCurrentItem() == this && Player.GetInstance().GetSTileUnderneath().islandId == 5)
            )
        )
        {
            ResetItem();
        }
    }

    public void SetRopeCoilActive(bool value)
    {
        gameObject.SetActive(value);

        if (value)
        {
            transform.position = resetTransform.position;
            SetCollider(true);
        }
    }

    private void UpdateSprite()
    {
        spriteRenderer.sprite = ropeSprites[distanceIndex];
        ropeLineRendererTransform.localPosition = 
            new Vector3(0, ROPE_LINE_RENDERER_START_OFFSET - (distanceIndex / 16f));
    }

    private void UpdatePlayerSpeed()
    {
        if (!isPicked)
            return;

        Player.SetMoveSpeedMultiplier(
            Mathf.Lerp(1, maxSlowMultiplier, distanceIndex * 1.0f / ropeSprites.Length)
        );
    }

    public override void PickUpItem(Transform pickLocation, System.Action callback = null) // pickLocation may be moving
    {
        ignoreDistChecks = true;
        base.PickUpItem(pickLocation, () => {
            ignoreDistChecks = false;
            callback?.Invoke();
        });
        isPicked = true;
        UpdatePlayerSpeed();
    }
    
    public override STile DropItem(Vector3 dropLocation, System.Action callback = null)
    {
        ignoreDistChecks = true;
        STile hitTile = base.DropItem(dropLocation, () => {
            ignoreDistChecks = false;
            callback?.Invoke();
        });
        isPicked = false;
        Player.SetMoveSpeedMultiplier(1f);

        return hitTile;
    }

    private void ResetItem()
    {
        StartCoroutine(IResetItem());
    }

    public void RemoveRopeFromPlayer()
    {
        PlayerInventory.RemoveItem();
        transform.SetParent(resetTransform.parent);
        Player.SetMoveSpeedMultiplier(1f);
    }

    private IEnumerator IResetItem()
    {
        RemoveRopeFromPlayer();
        SetCollider(false);
        isResetting = true;

        Vector3 startPos = transform.position;
        float t = 0;
        while (t < resetDuration)
        {
            float x = resetAnimationCurve.Evaluate(t / resetDuration);
            Vector3 newPos = Vector3.Lerp(startPos, resetTransform.position, x);
            newPos += Vector3.up * resetHeightBonusCurve.Evaluate(t);
            transform.position = newPos;

            yield return null;
            t += Time.deltaTime;
        }

        transform.position = resetTransform.position;
        SetCollider(true);
        isResetting = false;
    }
}
