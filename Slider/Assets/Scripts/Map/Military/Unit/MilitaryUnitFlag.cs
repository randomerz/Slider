using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MilitaryUnitFlag : Item
{
    public MilitaryUnit attachedUnit;
    [SerializeField] private MilitaryUnitFlagResetter resetter;
    [SerializeField] private NPC representativeNPC;

    private bool isDropping;
    private bool cancelOnAfterDropComplete;

    public override void Awake()
    {
        base.Awake();
        attachedUnit.OnDeath.AddListener(() => gameObject.SetActive(false));
    }

    private void OnEnable()
    {
        MilitaryTurnAnimator.AfterCheckQueue += DoFightChecks;
    }

    private void OnDisable()
    {
        MilitaryTurnAnimator.AfterCheckQueue -= DoFightChecks;
    }

    public override void PickUpItem(Transform pickLocation, System.Action callback = null)
    {
        MilitaryTurnManager.OnPlayerEndTurn += ResetOnPlayerEndTurn;
        base.PickUpItem(pickLocation, callback);
    }

    public override STile DropItem(Vector3 dropLocation, System.Action callback = null)
    {
        isDropping = true;

        return base.DropItem(dropLocation, () => {
            isDropping = false;
            MilitaryTurnManager.OnPlayerEndTurn -= ResetOnPlayerEndTurn;
            AfterDropComplete();
            callback.Invoke();
        });
    }

    private void AfterDropComplete()
    {
        bool wasSuccessful = TryFinishMove(out string reason);

        if (representativeNPC != null)
        {
            representativeNPC.Conds[0].dialogueChain[0].dialogue = reason;
            representativeNPC.TypeCurrentDialogueSafe();
        }

        if (!wasSuccessful)
        {
            if (reason == "Move was cancelled.")
            {
                AudioManager.Play("UI Click");
            }
            else
            {
                AudioManager.Play("Hurt");
            }
            resetter.ResetItem(onFinish: null);
        }
    }

    private bool TryFinishMove(out string reason)
    {
        if (cancelOnAfterDropComplete)
        {
            reason = "Move was cancelled.";
            return false;
        }

        if (attachedUnit == null)
        {
            reason = "I am... dead??? Something went wrong!";
            Debug.LogError($"Attached Unit was null.");
            return false;
        }

        STile hitStile = SGrid.GetSTileUnderneath(gameObject);

        if (hitStile == null)
        {
            reason = "We cannot leave the battlefield!";
            return false;
        }
        
        Vector2Int newGridPos = new Vector2Int(hitStile.x, hitStile.y);
        Vector2Int direction = newGridPos - attachedUnit.GridPosition;

        if (direction.magnitude == 0)
        {
            reason = "Move was cancelled.";
            return false;
        }

        if (direction.magnitude != 1)
        {
            reason = "New location was not one tile away!";
            return false;
        }

        foreach (MilitaryUnit unit in MilitaryUnit.ActiveUnits)
        {
            if (unit.GridPosition == newGridPos && unit.UnitStatus == MilitaryUnit.Status.Active && unit.UnitTeam == attachedUnit.UnitTeam)
            {
                reason = "Can't move to an occupied tile!";
                return false;
            }
        }

        foreach (MilitaryUnspawnedAlly unspawnedAlly in MilitaryUnspawnedAlly.AllUnspawnedAllies)
        {
            if (unspawnedAlly.parentStile == hitStile)
            {
                reason = "Can't move to an occupied tile!";
                return false;
            }
        }

        STile originalSTile = attachedUnit.AttachedSTile;
        // If AttachedSTile is null, assume unit is flying
        if (originalSTile != null)
        {
            if (!CanMoveBetweenTiles(originalSTile as MilitarySTile, hitStile as MilitarySTile, direction))
            {
                reason = "We cannot move through walls!";
                return false;
            }
        }

        attachedUnit.CreateAndQueueMove(newGridPos, hitStile);
        attachedUnit.GridPosition = newGridPos;
        attachedUnit.AttachedSTile = hitStile;

        resetter.SetResetTransform(hitStile.transform);
        resetter.ResetItem(onFinish: () => { 
            resetter.SetResetTransform(attachedUnit.NPCController.transform);
        });

        MilitaryTurnManager.EndPlayerTurn();

        AudioManager.PlayWithVolume("Hat Click", 0.75f);
        CoroutineUtils.ExecuteAfterDelay(
            () => AudioManager.PlayWithVolume("Hat Click", 0.75f),
            this, 0.1f
        );

        reason = Random.Range(0, 2) == 0 ? "Let's go, soliders!" : "Keep moving forward!";
        return true;
    }

    private bool CanMoveBetweenTiles(MilitarySTile stile1, MilitarySTile stile2, Vector2Int direction)
    {
        return stile1.CanMoveToDirection(direction) && stile2.CanMoveFromDirection(direction);
    }

    private void ResetOnPlayerEndTurn(object sender, System.EventArgs e)
    {
        if (isDropping)
        {
            cancelOnAfterDropComplete = true;
            return;
        }

        // Reset the flag
        resetter.ResetItem(onFinish: null);
    }

    private void DoFightChecks(object sender, System.EventArgs e) => DoFightChecks();

    private void DoFightChecks()
    {
        if (MilitaryTurnAnimator.IsUnitInActiveOrQueue(attachedUnit))
        {
            if (PlayerInventory.GetCurrentItem() == this)
            {
                resetter.ResetItem(onFinish: null);
            }
            SetCollider(false);
        }
        else
        {
            SetCollider(true);
        }
    }
}
