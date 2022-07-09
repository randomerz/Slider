using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChadJump : MonoBehaviour
{
    enum JumpState
    {
        standing,
        jumping,
        jumped,
        falling,
        fell
    }

    [Header("Chad Animation stuff")]
    [SerializeField] private SpriteRenderer npcRenderer;
    [SerializeField] private Animator npcAnimator;
    [SerializeField] private Collider2D npcCollider; // non-trigger collider
    [SerializeField] private Item flashlightItem;
    [SerializeField] private SpriteRenderer flashRenderer;
    [SerializeField] private int islandId;

    [SerializeField] private Transform startTransform;
    [SerializeField] private Transform jumpTransform;
    [SerializeField] private Transform fallTransform;

    [SerializeField] private AnimationCurve xJumpMotion;
    [SerializeField] private AnimationCurve yJumpMotion;
    [SerializeField] private float jumpDuration;
    [SerializeField] private float fallDuration;

    private JumpState jumpState;
    
    void Start()
    {
        flashlightItem.SetCollider(false);
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
        if (SGrid.current.GetStile(islandId) != null &&
            SGrid.current.GetStile(islandId).isTileActive &&
            e.stile.islandId == islandId && 
            jumpState == JumpState.jumped)
        {
            // Falling!
            StartCoroutine(Fall());
        }
    }

    // So that the dcond can call after dialogue ends
    public void JumpStarter()
    {
        if (jumpState != JumpState.jumping)
        {
            StartCoroutine(Jump());
        }
    }

    // Animates Chad Jumping
    public IEnumerator Jump()
    {
        jumpState = JumpState.jumping;
        npcAnimator.SetBool("isJumping", true);
        npcCollider.enabled = false;

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

        jumpState = JumpState.jumped;
        npcAnimator.SetBool("isJumping", false);
    }

    // Animates Chad Falling
    private IEnumerator Fall()
    {
        jumpState = JumpState.falling;
        npcAnimator.SetBool("isTipping", true);

        yield return new WaitForSeconds(0.5f);

        npcAnimator.SetBool("isFallen", true);
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

        jumpState = JumpState.fell;
        npcAnimator.SetBool("isTipping", false);
        AudioManager.Play("Hurt");

        flashlightItem.transform.parent = SGrid.current.GetStile(islandId).transform;
        flashlightItem.DropItem(transform.position + (Vector3.right * 1f), callback: FinishFall);
        flashlightItem.SetCollider(false);
    }

    private void FinishFall()
    {
        npcCollider.enabled = true;
        flashlightItem.SetCollider(true);
    }

    public void ChadFell(Conditionals.Condition cond)
    {
        cond.SetSpec(jumpState == JumpState.fell || PlayerInventory.Contains("Flashlight"));
    }
}
