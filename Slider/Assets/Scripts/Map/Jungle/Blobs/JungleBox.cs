using System;
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
    
    protected bool isSending;

    protected static int numSourceIds; // Used to generate source ids
    protected const int DEPTH_LIMIT = 30;

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
    }

    private void Start()
    {
        SetDirection(direction);
        // UpdateBox();
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
            if (box != null && box != this)
            {
                float dist = Vector2.Distance(hit.centroid, transform.position);
                if (dist < closestBoxDist)
                {
                    closestBoxDist = dist;
                    closestBox = box;
                }
            }
        }

        return closestBox;

    }

    protected virtual void UpdateSprites()
    {
        TEMP_SPRITE.sprite = ProducedShape != null ? ProducedShape.fullSprite : null;

        signAnimator.SetDirection(DirectionUtil.D2V(direction));
    }
}