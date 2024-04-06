using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blob : MonoBehaviour
{
    public Direction Direction { get; private set; }

    private float targetDistanceTraveled = 10;
    private float distanceTraveled = 0;
    private STile currentSTileUnder = null;

    private bool isJumping = false;
    private float timeSinceJump = 0;
    private Vector2 jumpStartPos;
    private Vector2 jumpTargetPos;

    private float targetSpriteAlpha;
    private Action onTargetAlphaReached;

    JungleBlobPathController parentPath = null;

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private SpriteRenderer shapeSpriteRenderer;

    public const float MARCH_SPEED = 0.75f;
    private const float JUMP_SPEED = 1.25f;
    private const float JUMP_DURATION = 2.2f;

    public void InitializeParameters(Direction direction, 
                                     float targetDistanceTraveled, 
                                     float initDistanceTraveled, 
                                     Shape shape, 
                                     JungleBlobPathController owner=null)
    {
        if (direction == Direction.LEFT)
        {
            spriteRenderer.flipX = false;
        }
        else if (direction == Direction.RIGHT)
        {
            spriteRenderer.flipX = true;
        }
        else
        {
            spriteRenderer.flipX = UnityEngine.Random.Range(0f, 1f) > 0.5f;
        }

        if (shape != null)
        {
            shapeSpriteRenderer.sprite = shape.fullSprite;
        }
        else
        {
            Debug.LogWarning("Tried to start a slime with no shape.");
        }

        this.Direction = direction;
        this.distanceTraveled = initDistanceTraveled;
        this.targetDistanceTraveled = targetDistanceTraveled;
        parentPath = owner;

        currentSTileUnder = SGrid.GetSTileUnderneath(gameObject);
        if (currentSTileUnder == null)
        {
            Debug.LogError($"Blob was spawned at {transform.position} and couldn't find current STile underneath -- {currentSTileUnder}");
        }
        else
        {
            transform.SetParent(currentSTileUnder.transform);
        }
        
        ResetJump();
    }

    private void Update()
    {
        HandleAlpha();
    }

    void FixedUpdate()
    {
        if (isJumping)
        {
            HandleJump();
        }
        else
        {
            HandleMarch();
        }
    }

    private void HandleMarch()
    {
        Vector3 deltaPosition = DirectionUtil.D2V(Direction) * (MARCH_SPEED * Time.deltaTime);
        distanceTraveled += Mathf.Abs(deltaPosition.magnitude);
        transform.position = transform.position + deltaPosition;

        // I made it!
        if (distanceTraveled >= targetDistanceTraveled)
        {
            RemoveBlob();
        }

        // Check for reparenting
        STile under = SGrid.GetSTileUnderneath(gameObject);

        // I'm not on a tile? Hopefully it's when tiles are shifting around
        if (under == null)
        {
            // maybe add a check/warning to make sure i am fading away

            // Undo moves
            distanceTraveled -= Mathf.Abs(deltaPosition.magnitude);
            transform.position = transform.position - deltaPosition;
            return;
        }

        if (currentSTileUnder == null)
        {
            Debug.LogWarning("Current STile under was not properly set up for blob! Attempting to rectify.");
            currentSTileUnder = under;
            transform.SetParent(currentSTileUnder.transform);
            return;
        }

        if (under != currentSTileUnder)
        {
            // If either is moving, undo moves
            if (under.IsMoving() || currentSTileUnder.IsMoving())
            {
                // Undo moves
                distanceTraveled -= Mathf.Abs(deltaPosition.magnitude);
                transform.position = transform.position - deltaPosition;
                return;
            }

            // Otherwise reassign me
            currentSTileUnder = under;
            transform.SetParent(currentSTileUnder.transform);
        }
    }

    private void HandleJump()
    {
        timeSinceJump += Time.deltaTime;

        if (Direction == Direction.RIGHT || Direction == Direction.LEFT)
        {
            float dist = Mathf.Abs(jumpTargetPos.x - jumpStartPos.x);
            float nextX = Mathf.MoveTowards(transform.position.x, jumpTargetPos.x, JUMP_SPEED * Time.deltaTime);
            float nextY = Mathf.Lerp(jumpStartPos.y, jumpTargetPos.y, Mathf.Abs(nextX - jumpStartPos.x) / dist);
            float heightBonus = -6 * (nextX - jumpStartPos.x) * (nextX - jumpTargetPos.x) / Mathf.Pow(dist, 2);

            transform.position = new Vector3(nextX, nextY + heightBonus);
        }
        else
        {
            float time01 = timeSinceJump / JUMP_DURATION;
            float nextY = -4 * Mathf.Pow(time01, 2) + 3 * time01 + jumpStartPos.y;
            transform.position = new Vector3(transform.position.x, nextY);
        }
    }

    public void JumpIntoBin()
    {
        if (isJumping)
            return;

        isJumping = true;
        animator.SetBool("isJumping", true);
        timeSinceJump = 0;

        int xOffset = Direction == Direction.RIGHT ? 3 : -3;

        jumpStartPos = transform.position;
        jumpTargetPos = new Vector2(transform.position.x + xOffset, transform.position.y - 3);

        StartCoroutine(DoJumpCoroutine());
    }

    private IEnumerator DoJumpCoroutine()
    {
        yield return new WaitForSeconds(JUMP_DURATION / 2);

        spriteRenderer.sortingOrder = -2;
        shapeSpriteRenderer.sortingOrder = -2;

        yield return new WaitForSeconds(JUMP_DURATION / 2);

        RemoveBlob();
    }

    private void ResetJump()
    {
        isJumping = false;
        animator.SetBool("isJumping", false);
        timeSinceJump = 0;

        spriteRenderer.sortingOrder = 0;
        shapeSpriteRenderer.sortingOrder = 0;
    }

    private void HandleAlpha()
    {
        if (spriteRenderer.color.a != targetSpriteAlpha)
        {
            float newAlpha = Mathf.MoveTowards(spriteRenderer.color.a, targetSpriteAlpha, 2 * Time.deltaTime);
            SetAlpha(newAlpha);

            if (newAlpha == targetSpriteAlpha)
            {
                onTargetAlphaReached?.Invoke();
                onTargetAlphaReached = null;
            }
        }
    }

    public void SetAlpha(float alpha)
    {
        Color c = spriteRenderer.color;
        c.a = alpha;
        spriteRenderer.color = c;
        shapeSpriteRenderer.color = c;
    }

    public bool IsFadingIn()
    {
        return targetSpriteAlpha == 1 && spriteRenderer.color.a < 0.1f;
    }

    public void SetTargetAlpha(float alpha, Action onTargetAlphaReached=null)
    {
        targetSpriteAlpha = alpha;
        if (onTargetAlphaReached != null)
        {
            this.onTargetAlphaReached = onTargetAlphaReached;
        }
    }

    public void SetShape(Shape shape)
    {
        if (shapeSpriteRenderer.sprite != shape.fullSprite)
        {
            shapeSpriteRenderer.sprite = shape.fullSprite;
            Vector3 pos = shapeSpriteRenderer.transform.position + new Vector3(0, 0.5f);
            ParticleManager.SpawnParticle(ParticleType.MiniSparkle, pos, shapeSpriteRenderer.transform);
        }
    }

    public void RemoveBlob()
    {
        if (parentPath != null)
        {
            parentPath.RemoveBlob(this);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
}
