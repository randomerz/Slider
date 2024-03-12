using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public abstract class JungleBox : MonoBehaviour
{
    protected Dictionary<Direction, JungleBox> inputs = new();
    protected Direction direction;
    protected JungleBox targetBox;

    public Shape ProducedShape { get; protected set; }
    public List<int> UsedSourceIds { get; protected set; }
    public STile ParentSTile { get; protected set; }
    
    protected bool isSending;

    protected static int numSourceIds; // Used to generate source ids
    protected const int DEPTH_LIMIT = 30;
    protected const string WORLD_COLLIDER_TAG = "WorldMapCollider"; // Does not include collider tilemap, etc

    [Header("References")]
    public SpriteRenderer TEMP_SPRITE;
    [SerializeField] private LayerMask raycastLayerMask;
    [SerializeField] private JungleSignAnimator signAnimator;

    protected virtual void Awake()
    {
        foreach (Direction d in DirectionUtil.Directions)
        {
            inputs[d] = null;
        }
        UsedSourceIds = new();

        if (raycastLayerMask == 0)
        {
            Debug.LogWarning("Warning: Jungle Box's layer mask has nothing selected. It should probably have 'Default' and 'JungleSigns'.");
        }

        if (ParentSTile == null)
        {
            ParentSTile = GetComponentInParent<STile>();
            if (ParentSTile == null)
            {
                Debug.LogError("Jungle Box couldn't find parent STile.");
            }
        }
    }

    private void Start()
    {
        SetDirection(direction);
        // UpdateBox();
    }

    private void OnEnable()
    {
        SGridAnimator.OnSTileMoveStart += CheckDirectionEndOfFrame;
        SGridAnimator.OnSTileMoveEnd += CheckDirectionEndOfFrame;
        SGrid.OnSTileEnabled += CheckDirectionEndOfFrame;
        SGrid.OnGridSet += CheckDirectionEndOfFrame;
    }

    private void OnDisable()
    {
        SGridAnimator.OnSTileMoveStart -= CheckDirectionEndOfFrame;
        SGridAnimator.OnSTileMoveEnd -= CheckDirectionEndOfFrame;
        SGrid.OnSTileEnabled -= CheckDirectionEndOfFrame;
        SGrid.OnGridSet -= CheckDirectionEndOfFrame;
    }

    private void OnDestroy()
    {
        numSourceIds = 0;
    }

    /// <summary>
    /// Check if a box and direction is valid for 'AddInput()'.
    /// </summary>
    /// <param name="fromDirection">The direction shapes are coming from, 
    /// from my perspective.</param>
    /// <returns>If the input is being used or not.</returns>
    public abstract bool IsValidInput(JungleBox other, Direction fromDirection);

    /// <summary>
    /// Called by another box to start tracking the other. JungleSigns will 
    /// only produce using the shapes added to inputs.
    /// </summary>
    /// <param name="fromDirection">The direction shapes are coming from, 
    /// from my perspective.</param>
    /// <returns>If the input is being used or not.</returns>
    public abstract bool AddInput(JungleBox other, Direction fromDirection);

    /// <summary>
    /// Called by another box to remove this from the inputs.
    /// </summary>
    /// <param name="fromDirection">The direction shapes are coming from, 
    /// from my perspective.</param>
    public abstract void RemoveInput(Direction fromDirection);

    /// <summary>
    /// Update and propogate the box graph. Signs will craft, Spawners 
    /// send, etc.
    /// </summary>
    public abstract void UpdateBox(int depth=0);

    /// <summary>
    /// Called whenever the direction of this JungleBox should be updated.
    /// </summary>
    public void SetDirection(Direction direction)
    {
        JungleBox oldBox = targetBox;
        Direction oldDirection = this.direction;
        
        this.direction = direction;
        targetBox = null;

        // Remove incoming shapes in new direction
        RemoveInput(direction);
        
        // Remove outgoing shapes in old direction
        if (oldBox != null)
        {
            oldBox.RemoveInput(DirectionUtil.Inv(oldDirection));
            oldBox.UpdateBox();
        }

        UpdateBox();

        // Raycast to find new box
        JungleBox other = GetBoxInDirection(direction);
        targetBox = other;
        
        if (targetBox == null)
        {
            SetIsSending(false);
            return;
        }

        bool canSend = targetBox.IsValidInput(this, DirectionUtil.Inv(direction));
        if (canSend)
        {
            targetBox.AddInput(this, DirectionUtil.Inv(direction));
            targetBox.UpdateBox();
            SetIsSending(true);
        }
        else
        {
            // Could be two signs pointing same direction,
            // a sign pointing towards spawner, etc...
            SetIsSending(false);
        }
    }

    protected void CheckDirectionEndOfFrame(object sender, SGridAnimator.OnTileMoveArgs e) => StartCoroutine(CheckDirectionEndOfFrame());
    protected void CheckDirectionEndOfFrame(object sender, SGrid.OnSTileEnabledArgs e) => StartCoroutine(CheckDirectionEndOfFrame());
    protected void CheckDirectionEndOfFrame(object sender, SGrid.OnGridMoveArgs e) => StartCoroutine(CheckDirectionEndOfFrame());
    
    public IEnumerator CheckDirectionEndOfFrame()
    {
        // TileMoveEnd is invoked when the move ends, but before colliders are restored, 
        // so we are waiting until the end of the frame.
        yield return new WaitForEndOfFrame();

        CheckDirectionOnMove();
    }

    public void CheckDirectionOnMove()
    {
        if (targetBox != null)
        {
            // If neither is moving do nothing
            if (!ParentSTile.IsMoving() && !targetBox.ParentSTile.IsMoving())
            {
                return;
            }

            // If both on the same tile do nothing
            if (ParentSTile.islandId == targetBox.ParentSTile.islandId)
            {
                return; 
            }
        }

        SetDirection(direction);
    }


    public void IncrementDirection()
    {
        SetDirection(DirectionUtil.Next(direction));
    }

    protected void SetIsSending(bool isSending)
    {
        this.isSending = isSending;
        UpdateSprites();
    }

    protected JungleBox GetBoxInDirection(Direction direction)
    {
        Vector2 directionVector = DirectionUtil.D2V(direction);

        RaycastHit2D[] hits = new RaycastHit2D[DEPTH_LIMIT];
        ContactFilter2D contactFilter2D = new ContactFilter2D();
        contactFilter2D.SetLayerMask(raycastLayerMask);
        contactFilter2D.useTriggers = false;

        int numHits = Physics2D.Raycast(
            transform.position,
            directionVector.normalized, 
            contactFilter2D,
            hits,
            100 // Max distance
        );

        JungleBox closestBox = null;
        float closestBoxDist = float.MaxValue;

        for (int i = 0; i < numHits; i++)
        {
            RaycastHit2D hit = hits[i];
            JungleBox box = hit.collider.GetComponent<JungleBox>();
            // float dist = Vector2.Distance(hit.centroid, transform.position);
            float dist = hit.distance;

            if (box != null)
            {
                if (box == this)
                {
                    continue;
                }

                if (dist < closestBoxDist)
                {
                    closestBoxDist = dist;
                    closestBox = box;
                }
            }
            else if (hit.collider.CompareTag(WORLD_COLLIDER_TAG)) // hit something else
            {
                if (dist < closestBoxDist)
                {
                    closestBoxDist = dist;
                    closestBox = null;
                }
            }
        }

        return closestBox;

    }

    protected virtual void UpdateSprites()
    {
        TEMP_SPRITE.sprite = ProducedShape != null ? ProducedShape.fullSprite : null;

        if (signAnimator != null)
        {
            signAnimator.SetDirection(DirectionUtil.D2V(direction));
        }
    }
}