using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class JungleBox : MonoBehaviour, ISavable
{
    protected Dictionary<Direction, JungleBox> inputs = new();
    protected Direction direction;
    protected JungleBox targetBox;

    public Shape ProducedShape { get; protected set; }
    protected List<int> usedSourceIds;
    public STile ParentSTile { get; protected set; }
    public Direction CurrentDirection { get { return direction; } }


    protected static int numSourceIds; // Used to generate source ids
    protected const int DEPTH_LIMIT = 30;
    // includes BGFrame colliders, stile colliders, collider tilemap, etc
    protected const string WORLD_COLLIDER_TAG = "WorldMapCollider"; 

    [SerializeField] protected string saveString = "";

    [Header("References")]
    [SerializeField] protected JungleBlobPathController pathController;
    [SerializeField] private SpriteRenderer debugSpriteRenderer;
    [SerializeField] private LayerMask raycastLayerMask;
    [SerializeField] protected JungleSignAnimator signAnimator;
    [SerializeField] protected ParticleTrail particleTrail;

    protected virtual void Awake()
    {
        foreach (Direction d in DirectionUtil.Directions)
        {
            inputs[d] = null;
        }
        usedSourceIds = new();

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

        if (signAnimator != null)
        {
            signAnimator.SetIsGray(true);
        }
    }

    private void OnEnable()
    {
        SGridAnimator.OnSTileMoveStart += CheckDirectionEndOfFrame;
        SGridAnimator.OnSTileMoveEnd += CheckDirectionEndOfFrame;
        SGrid.OnSTileEnabled += CheckDirectionEndOfFrame;
        SGrid.OnGridSet += UpdateDirectionEndOfFrame;
    }

    private void OnDisable()
    {
        SGridAnimator.OnSTileMoveStart -= CheckDirectionEndOfFrame;
        SGridAnimator.OnSTileMoveEnd -= CheckDirectionEndOfFrame;
        SGrid.OnSTileEnabled -= CheckDirectionEndOfFrame;
        SGrid.OnGridSet -= UpdateDirectionEndOfFrame;
    }

    private void OnDestroy()
    {
        numSourceIds = 0;
    }

    private void Start()
    {
        // STile movement + colliders aren't refreshed fast enough on scene Load() if we do it in Start()
        StartCoroutine(LateStart(() => SetDirection(direction)));
    }

    private IEnumerator LateStart(System.Action action)
    {
        yield return new WaitForEndOfFrame();

        action.Invoke();
    }

    public virtual void Save()
    {
        if (!IsSaveStringOkay())
        {
            Debug.LogWarning($"Save string was not set for jungle box: {name}. Skipping saving.");
            return;
        }

        SaveSystem.Current.SetInt(saveString, (int)direction);
    }

    protected bool IsSaveStringOkay()
    {
        if (saveString == null || saveString == "")
        {
            return false;
        }
        return true;
    }

    public virtual void Load(SaveProfile profile)
    {
        if (!IsSaveStringOkay())
        {
            Debug.LogWarning($"Save string was not set for jungle box: {name}. Skipping loading.");
            return;
        }

        direction = (Direction)profile.GetInt(saveString);
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
    /// <returns>If the update should keep propogating</returns>
    public abstract bool UpdateBox(int depth=0);

    /// <summary>
    /// Called whenever the direction of this JungleBox should be updated.
    /// Warning! If you change this method, you may have to apply the same changes 
    /// to JungleDuplicator.SetDirection() as well! Sorry :(
    /// </summary>
    public virtual void SetDirection(Direction direction)
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
            SetIsSending(false, oldDirection, oldBox, signAnimator);
            oldBox.UpdateBox();
            oldBox.UpdateSendingSprites(propogate: false);
        }

        UpdateBox();

        // Raycast to find new box
        JungleBox other = GetBoxInDirection(direction);
        targetBox = other;
        if (other != null && particleTrail != null)
        {
            particleTrail.trailTarget = other.transform;
            particleTrail.SpawnParticleTrail(shouldRepeat: false);
        }

        TrySendAfterUpdateDirection(direction, targetBox, signAnimator);

        UpdateSprites();
    }

    public virtual void UpdateSendingSprites(bool propogate=true)
    {
        TrySendAfterUpdateDirection(direction, targetBox, signAnimator, propogate);
    }

    protected void TrySendAfterUpdateDirection(Direction myDirection, JungleBox myTargetBox, JungleSignAnimator mySignAnimator, bool propogate=true)
    {
        if (myTargetBox == null)
        {
            SetIsSending(false, myDirection, myTargetBox, mySignAnimator);
            return;
        }

        bool canSend = myTargetBox.IsValidInput(this, DirectionUtil.Inv(myDirection));
        if (canSend)
        {
            if (propogate)
            {
                myTargetBox.AddInput(this, DirectionUtil.Inv(myDirection));
                myTargetBox.UpdateBox();
            }

            SetIsSending(ProducedShape != null, myDirection, myTargetBox, mySignAnimator);
        }
        else
        {
            // Could be two signs pointing same direction,
            // a sign pointing towards spawner, etc...
            SetIsSending(false, myDirection, myTargetBox, mySignAnimator);
            if (propogate)
            {
                myTargetBox.UpdateSendingSprites(propogate: false);
            }
        }
    }

    protected void CheckDirectionEndOfFrame(object sender, SGridAnimator.OnTileMoveArgs e) => StartCoroutine(CheckDirectionEndOfFrame());
    protected void CheckDirectionEndOfFrame(object sender, SGrid.OnSTileEnabledArgs e) => StartCoroutine(CheckDirectionEndOfFrame());
    protected void CheckDirectionEndOfFrame(object sender, SGrid.OnGridMoveArgs e) => StartCoroutine(CheckDirectionEndOfFrame());
    protected void UpdateDirectionEndOfFrame(object sender, SGrid.OnGridMoveArgs e) => StartCoroutine(UpdateDirectionEndOfFrame());
    

    public IEnumerator CheckDirectionEndOfFrame()
    {
        // TileMoveEnd is invoked when the move ends, but before colliders are restored, 
        // so we are waiting until the end of the frame.
        yield return new WaitForEndOfFrame();

        CheckDirectionOnMove();
    }

    public virtual void CheckDirectionOnMove()
    {
        if (CanSkipCheckDirection(targetBox))
            return;

        SetDirection(direction);
    }

    public IEnumerator UpdateDirectionEndOfFrame()
    {
        // For scroll set grid
        yield return new WaitForEndOfFrame();
        yield return null;

        SetDirection(direction);
    }

    protected bool CanSkipCheckDirection(JungleBox myTargetBox)
    {
        if (myTargetBox != null)
        {
            // If neither is moving do nothing
            if (!ParentSTile.IsMoving() && !myTargetBox.ParentSTile.IsMoving())
            {
                return true;
            }

            // If both on the same tile do nothing
            if (ParentSTile.islandId == myTargetBox.ParentSTile.islandId)
            {
                return true; 
            }
        }
        return false;
    }


    public void IncrementDirection()
    {
        SetDirection(DirectionUtil.Prev(direction));
    }

    /// <summary>
    /// Get the list of shape ids that are a part of the produced shape.
    /// The ids are tracked from where they source from, and usedful to
    /// make sure there are no loops in the graph.
    /// </summary>
    /// <param name="outputDirection">The output direction of my box. Used for duplicator.</param>
    /// <returns>List of shape ids.</returns>
    public virtual List<int> GetUsedSourceIds(Direction outputDirection)
    {
        return usedSourceIds;
    }

    protected virtual void SetIsSending(bool isSending, Direction sendingDirection, JungleBox target, JungleSignAnimator mySignAnimator)
    {
        if (mySignAnimator != null)
        {
            mySignAnimator.SetIsGray(!isSending);
        }
            
        if (isSending)
        {
            pathController.EnableMarching(sendingDirection, ProducedShape, target);
        }
        else
        {
            pathController.DisableMarching(sendingDirection);
        }
    }

    protected JungleBox GetBoxInDirection(Direction direction)
    {
        Vector2 directionVector = DirectionUtil.D2V(direction);

        RaycastHit2D[] hits = new RaycastHit2D[DEPTH_LIMIT];
        ContactFilter2D contactFilter2D = new ContactFilter2D();
        contactFilter2D.SetLayerMask(raycastLayerMask);
        contactFilter2D.useTriggers = true;

        int numHits = Physics2D.Raycast(
            transform.position,
            directionVector.normalized, 
            contactFilter2D,
            hits,
            100 // Max distance
        );

        if (numHits == DEPTH_LIMIT)
        {
            Debug.LogError("Warning: GetBoxInDirection hit the maximum amount of colliders. " +
                           "This may cause the box to miss another.");
        }

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
            else if (hit.collider.CompareTag(WORLD_COLLIDER_TAG) && !hit.collider.isTrigger) // hit something else
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
        debugSpriteRenderer.sprite = ProducedShape != null ? ProducedShape.fullSprite : null;

        if (signAnimator != null)
        {
            signAnimator.SetDirection(DirectionUtil.D2V(direction));
        }
    }

    public void TurnOnDebugShape()
    {
        debugSpriteRenderer.gameObject.SetActive(true);
    }

    public void IsDirectionRight(Condition c) => c.SetSpec(direction == Direction.RIGHT);
    public void IsDirectionUp(Condition c) => c.SetSpec(direction == Direction.UP);
    public void IsDirectionLeft(Condition c) => c.SetSpec(direction == Direction.LEFT);
    public void IsDirectionDown(Condition c) => c.SetSpec(direction == Direction.DOWN);
}