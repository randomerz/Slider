using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class ChadJump : MonoBehaviour
{
    enum JumpState
    {
        STANDING,
        JUMPING,
        JUMPED,
        FALLING,
        FELL
    }

    public UnityEvent OnJumpFinish;

    [Header("Chad Animation stuff")]
    [SerializeField] private SpriteRenderer npcRenderer;
    [SerializeField] private Animator npcAnimator;
    [SerializeField] private Collider2D npcCollider; // non-trigger collider
    [SerializeField] [FormerlySerializedAs("flashlightItem")] private Item artifactItem;
    [SerializeField] private int islandId;

    [SerializeField] private Transform startTransform;
    [SerializeField] private Transform jumpTransform;
    [SerializeField] private Transform fallTransform;

    [SerializeField] private AnimationCurve xJumpMotion;
    [SerializeField] private AnimationCurve yJumpMotion;
    [SerializeField] private float jumpDuration;
    [SerializeField] private float fallDuration;

    private JumpState jumpState;
    
    void Awake()
    {
        // VillageGrid.Start() want to overwrite this to be true in some cases, so we put this in awake
        if (SGrid.Current is VillageGrid)
        {
            artifactItem.SetCollider(PlayerInventory.Contains("Slider 3", Area.Caves));
        }
        else if (artifactItem != null)
        {
            artifactItem.SetCollider(false);
        }
    }

    private void OnEnable()
    {
        SGridAnimator.OnSTileMoveEnd += OnTileMoved;
    }

    private void OnDisable()
    {
        SGridAnimator.OnSTileMoveEnd -= OnTileMoved;
    }

    // Mini-Puzzle - Chad Flashlight
    public void OnTileMoved(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        if (SGrid.Current.GetStile(islandId) != null &&
            SGrid.Current.GetStile(islandId).isTileActive &&
            e.stile.islandId == islandId && 
            jumpState == JumpState.JUMPED)
        {
            // Falling!
            StartCoroutine(Fall());
        }
    }

    // So that the dcond can call after dialogue ends
    public void JumpStarter()
    {
        if (jumpState != JumpState.JUMPING)
        {
            StartCoroutine(Jump());
        }
    }

    // Animates Chad Jumping
    private IEnumerator Jump()
    {
        jumpState = JumpState.JUMPING;
        npcAnimator.SetBool("isJumping", true);
        npcAnimator.SetBool("isFallen", false);
        npcAnimator.SetBool("isTipping", false);
        npcCollider.enabled = false;
        npcRenderer.sortingOrder = 1;

        Vector3 start = startTransform.localPosition;
        Vector3 target = jumpTransform.localPosition;

        float t = 0;
        while (t < jumpDuration)
        {
            float x = xJumpMotion.Evaluate(t / jumpDuration);
            float y = yJumpMotion.Evaluate(t / jumpDuration);
            Vector3 pos = new Vector3(Mathf.Lerp(start.x, target.x, x),
                                      Mathf.Lerp(start.y, target.y, y));

            transform.localPosition = pos;

            yield return null;
            t += Time.deltaTime;
        }

        transform.localPosition = target;

        jumpState = JumpState.JUMPED;
        npcAnimator.SetBool("isJumping", false);

        OnJumpFinish?.Invoke();
    }

    public void EnableCollider()
    {
        npcCollider.enabled = true;
    }

    // Animates Chad Falling
    private IEnumerator Fall()
    {
        jumpState = JumpState.FALLING;
        
        foreach(AnimatorControllerParameter p in npcAnimator.parameters)
        {
            npcAnimator.SetBool(p.name, false);
        }

        npcAnimator.SetBool("isTipping", true);

        yield return new WaitForSeconds(0.5f);

        npcAnimator.SetBool("isFallen", true);
        npcAnimator.SetBool("isTipping", false);
        AudioManager.Play("Fall");

        Vector3 start = jumpTransform.localPosition;
        Vector3 target = fallTransform.localPosition;

        float t = 0;
        while (t < fallDuration)
        {
            transform.localPosition = Vector3.Lerp(start, target, t / fallDuration);

            yield return null;
            t += Time.deltaTime;
        }

        transform.localPosition = target;

        jumpState = JumpState.FELL;
        AudioManager.Play("Hurt");
        npcRenderer.sortingOrder = 0;

        if (artifactItem != null)
        {
            artifactItem.transform.parent = SGrid.Current.GetStile(islandId).transform;
            artifactItem.DropItem(transform.position + (Vector3.right * 1f), callback: FinishFall);
            artifactItem.SetCollider(false);
        }
    }

    public void ResetJump()
    {
        jumpState = JumpState.STANDING;

        foreach (AnimatorControllerParameter p in npcAnimator.parameters)
        {
            npcAnimator.SetBool(p.name, false);
        }

    }

    public void FinishFall()
    {
        npcCollider.enabled = true;
        
        if (artifactItem != null)
        {
            artifactItem?.SetCollider(true);
            artifactItem.spriteRenderer.sortingOrder = 0;
        }
    }

    public void ChadFell(Condition cond)
    {
        cond.SetSpec(ChadFell());
    }

    public bool ChadFell()
    {
        return jumpState == JumpState.FELL;
    }

    public bool ChadFalling()
    {
        return jumpState == JumpState.FALLING;
    }

    public bool ChadJumped()
    {
        return jumpState == JumpState.JUMPED;
    }
}
