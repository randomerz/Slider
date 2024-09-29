using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MilitaryUnitFlag : Item
{
    public MilitaryUnit attachedUnit;
    [SerializeField] private MilitaryUnitFlagResetter resetter;
    [SerializeField] private NPC representativeNPC;
    [SerializeField] private NPC[] otherNPCs;

    private bool isDropping;
    private bool cancelOnAfterDropComplete;

    public override void Awake()
    {
        base.Awake();
        attachedUnit.OnDeath.AddListener(() => gameObject.SetActive(false));

        if (resetter == null)
        {
            Debug.LogError($"Resetter was null on awake. Check your build!");
        }
    }
    
    public override void Start()
    {
        base.Start();

        representativeNPC.Conds[0].dialogueChain[0].dialogue = MilitaryAllyDialogue.GetLocalizedOf(MilitaryAllyDialogue.MilitaryAllyDialogueCode.Intro1);
        foreach (NPC npc in otherNPCs)
        {
            npc.Conds[0].dialogueChain[0].dialogue = MilitaryAllyDialogue.GetLocalizedOf(MilitaryAllyDialogue.MilitaryAllyDialogueCode.Intro2);
        }
    }

    private void OnEnable()
    {
        MilitaryTurnAnimator.AfterCheckQueue += DoFightChecks;
        MilitaryGrid.OnRestartMilitary += CheckPlayerHoldingOnRestartMilitary;
    }

    private void OnDisable()
    {
        MilitaryTurnAnimator.AfterCheckQueue -= DoFightChecks;
        MilitaryGrid.OnRestartMilitary -= CheckPlayerHoldingOnRestartMilitary;
        MilitaryTurnManager.OnPlayerEndTurn -= ResetOnPlayerEndTurn;
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
            if (reason == MilitaryAllyDialogue.GetLocalizedOf(MilitaryAllyDialogue.MilitaryAllyDialogueCode.MoveCancelled))
            {
                AudioManager.Play("UI Click");
            }
            else
            {
                representativeNPC.RefreshEmoteOnEnter();
                AudioManager.Play("Hurt");
            }
            resetter.ResetItem(onFinish: null);
        }
    }

    private bool TryFinishMove(out string reason)
    {
        if (cancelOnAfterDropComplete)
        {
            reason = MilitaryAllyDialogue.GetLocalizedOf(MilitaryAllyDialogue.MilitaryAllyDialogueCode.MoveCancelled);
            return false;
        }

        if (attachedUnit == null)
        {
            reason = MilitaryAllyDialogue.GetLocalizedOf(MilitaryAllyDialogue.MilitaryAllyDialogueCode.MoveError1);
            Debug.LogError($"Attached Unit was null.");
            return false;
        }

        STile hitStile = SGrid.GetSTileUnderneath(gameObject);

        if (hitStile == null)
        {
            reason = MilitaryAllyDialogue.GetLocalizedOf(MilitaryAllyDialogue.MilitaryAllyDialogueCode.MoveOffMap);
            return false;
        }
        
        Vector2Int newGridPos = new Vector2Int(hitStile.x, hitStile.y);
        Vector2Int direction = newGridPos - attachedUnit.GridPosition;

        if (direction.magnitude == 0)
        {
            reason = MilitaryAllyDialogue.GetLocalizedOf(MilitaryAllyDialogue.MilitaryAllyDialogueCode.MoveCancelled);
            return false;
        }

        if (direction.magnitude != 1)
        {
            reason = MilitaryAllyDialogue.GetLocalizedOf(MilitaryAllyDialogue.MilitaryAllyDialogueCode.MoveTooFar);
            return false;
        }

        foreach (MilitaryUnit unit in MilitaryUnit.ActiveUnits)
        {
            if (unit.GridPosition == newGridPos && unit.UnitStatus == MilitaryUnit.Status.Active && unit.UnitTeam == attachedUnit.UnitTeam)
            {
                reason = MilitaryAllyDialogue.GetLocalizedOf(MilitaryAllyDialogue.MilitaryAllyDialogueCode.MoveOccupied);
                return false;
            }
        }

        foreach (MilitaryUnspawnedAlly unspawnedAlly in MilitaryUnspawnedAlly.AllUnspawnedAllies)
        {
            if (unspawnedAlly.parentStile == hitStile)
            {
                reason = MilitaryAllyDialogue.GetLocalizedOf(MilitaryAllyDialogue.MilitaryAllyDialogueCode.MoveOccupied);
                return false;
            }
        }

        STile originalSTile = attachedUnit.AttachedSTile;
        // If AttachedSTile is null, assume unit is flying
        if (originalSTile != null)
        {
            if (!CanMoveBetweenTiles(originalSTile as MilitarySTile, hitStile as MilitarySTile, direction))
            {
                reason = MilitaryAllyDialogue.GetLocalizedOf(MilitaryAllyDialogue.MilitaryAllyDialogueCode.MoveThroughWalls);
                return false;
            }
        }

        attachedUnit.CreateAndQueueMove(newGridPos, hitStile);
        attachedUnit.GridPosition = newGridPos;
        attachedUnit.AttachedSTile = hitStile;

        if (resetter == null)
        {
            Debug.LogError($"[Military] FlagResetter was dead! Am I dead?! Aborting!");
            reason = MilitaryAllyDialogue.GetLocalizedOf(MilitaryAllyDialogue.MilitaryAllyDialogueCode.MoveError2);
            return false;
        }

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

        reason = Random.Range(0, 2) == 0 ? 
            MilitaryAllyDialogue.GetLocalizedOf(MilitaryAllyDialogue.MilitaryAllyDialogueCode.MoveSuccess1) : 
            MilitaryAllyDialogue.GetLocalizedOf(MilitaryAllyDialogue.MilitaryAllyDialogueCode.MoveSuccess2);
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
        MilitaryTurnManager.OnPlayerEndTurn -= ResetOnPlayerEndTurn;
        resetter.ResetItem(onFinish: null);
    }

    private void CheckPlayerHoldingOnRestartMilitary(object sender, System.EventArgs e)
    {
        if (PlayerInventory.GetCurrentItem() == this)
        {
            Debug.Log($"[Military] Did player reset while holding the flag?");
            resetter.RemoveFromPlayer();
            resetter.gameObject.SetActive(false);
        }
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
