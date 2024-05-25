using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class JungleDuplicator : JungleSign
{
    private Direction alternateDirection;
    private JungleBox alternateTargetBox;
    public Direction CurrentAlternateDirection { get { return alternateDirection; } }
    
    [SerializeField] protected JungleSignAnimator alternateSignAnimator;
    [SerializeField] protected ParticleTrail alternateParticleTrail;

    private int mySourceId = 0;

    protected override void Awake()
    {
        base.Awake();

        AssignSourceId();
    }

    // Copy from JungleSpawner.cs
    private void AssignSourceId()
    {
        mySourceId = numSourceIds;
        numSourceIds += 1;
    }

    // Kinda yucky, copy paste from JungleBox.cs
    public override void SetDirection(Direction direction)
    {
        JungleBox oldBox = targetBox;
        Direction oldDirection = this.direction;
        JungleBox oldAlternateBox = alternateTargetBox;
        Direction oldAlternateDirection = this.alternateDirection;
        
        this.direction = direction;
        this.alternateDirection = DirectionUtil.Next(direction);
        targetBox = null;
        alternateTargetBox = null;

        // Remove incoming shapes in new direction
        RemoveInput(this.direction);
        RemoveInput(this.alternateDirection);
        
        // Remove outgoing shapes in old direction
        if (oldBox != null)
            oldBox.RemoveInput(DirectionUtil.Inv(oldDirection));
        if (oldAlternateBox != null)
            oldAlternateBox.RemoveInput(DirectionUtil.Inv(oldAlternateDirection));

        if (oldBox != null)
        {
            SetIsSending(false, oldDirection, oldBox, signAnimator);
            oldBox.UpdateBox();
            oldBox.UpdateSendingSprites(propogate: false);
        }
        if (oldAlternateBox != null)
        {
            SetIsSending(false, oldAlternateDirection, oldAlternateBox, alternateSignAnimator);
            oldAlternateBox.UpdateBox();
            oldAlternateBox.UpdateSendingSprites(propogate: false);
        }


        UpdateBox();

        // Raycast to find new box
        JungleBox other = GetBoxInDirection(this.direction);
        targetBox = other;
        JungleBox alternateOther = GetBoxInDirection(this.alternateDirection);
        alternateTargetBox = alternateOther;
        
        if (other != null && particleTrail != null)
        {
            particleTrail.trailTarget = other.transform;
            particleTrail.SpawnParticleTrail(shouldRepeat: false);
        }
        if (alternateOther != null && particleTrail != null)
        {
            alternateParticleTrail.trailTarget = alternateOther.transform;
            alternateParticleTrail.SpawnParticleTrail(shouldRepeat: false);
        }

        TrySendAfterUpdateDirection(this.direction, targetBox, signAnimator);
        TrySendAfterUpdateDirection(this.alternateDirection, alternateTargetBox, alternateSignAnimator);

        UpdateSprites();
        // pathController.UpdateMarchingShape(this.direction, ProducedShape);
        // pathController.UpdateMarchingShape(this.alternateDirection, ProducedShape);
    }

    public override void UpdateSendingSprites(bool propogate = true)
    {
        TrySendAfterUpdateDirection(direction, targetBox, signAnimator, propogate);
        TrySendAfterUpdateDirection(alternateDirection, alternateTargetBox, alternateSignAnimator, propogate);
    }

    public override void CheckDirectionOnMove()
    {
        if (CanSkipCheckDirection(targetBox) && 
            CanSkipCheckDirection(alternateTargetBox))
        {
            return;
        }

        SetDirection(direction);
    }


    public override List<int> GetUsedSourceIds(Direction outputDirection)
    {
        if (outputDirection == direction)
        {
            return base.GetUsedSourceIds(outputDirection);
        }
        else if (outputDirection == alternateDirection)
        {
            return new List<int> { mySourceId };
        }
        else
        {
            Debug.LogWarning($"Tried getting input from direction, {outputDirection}, I wasn't outputting.");
            return base.GetUsedSourceIds(outputDirection);
        }
    }


    public override bool IsValidInput(JungleBox other, Direction fromDirection)
    {
        return (
            base.IsValidInput(other, fromDirection) && 
            IsValidInDirection(other, fromDirection, alternateDirection)
        );
    }

    public override void RemoveInput(Direction fromDirection)
    {
        base.RemoveInput(fromDirection);
        TryRestoreOnRemove(fromDirection, alternateDirection, alternateTargetBox);
    }

    public override bool UpdateBox(int depth = 0)
    {
        bool keepPropogating = base.UpdateBox(depth);
        if (!keepPropogating)
        {
            return false;
        }
        pathController.UpdateMarchingShape(alternateDirection, ProducedShape);
        
        UpdatePropogateInDirection(alternateDirection, alternateTargetBox, alternateSignAnimator, depth);
        return true;
    }

    protected override void UpdateSprites()
    {
        base.UpdateSprites();
    
        alternateSignAnimator.SetDirection(DirectionUtil.D2V(alternateDirection));
    }
}