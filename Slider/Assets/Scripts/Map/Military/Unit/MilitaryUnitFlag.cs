using System.Collections;
using System.Collections.Generic;
using Localization;
using UnityEngine;

public class MilitaryUnitFlag : Item, IDialogueTableProvider
{
    public MilitaryUnit attachedUnit;
    [SerializeField] private MilitaryUnitFlagResetter resetter;
    [SerializeField] private NPC representativeNPC;

    private bool isDropping;
    private bool cancelOnAfterDropComplete;
    
    #region Localization

    enum MUFStrings
    {
        Canceled,
        Bug,
        CannotLeave,
        NewLocationDistance,
        TileOccupied,
        Walls,
        RandomA,
        RandomB
    }

    public Dictionary<string, LocalizationPair> TranslationTable { get; } = IDialogueTableProvider.InitializeTable(
        new Dictionary<MUFStrings, string>
        {
            { MUFStrings.Canceled, "Move was cancelled." },
            { MUFStrings.Bug, "I am... dead??? Something went wrong!" },
            { MUFStrings.CannotLeave, "We cannot leave the battlefield!" },
            { MUFStrings.NewLocationDistance, "New location was not one tile away!" },
            { MUFStrings.TileOccupied, "Can't move to an occupied tile!" },
            { MUFStrings.Walls, "We cannot move through walls!" },
            { MUFStrings.RandomA, "Let's go, soldiers!" },
            { MUFStrings.RandomB, "Keep moving forward!" }
        });

    #endregion

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
        bool wasSuccessful = TryFinishMove(out LocalizationPair reason);

        if (representativeNPC != null)
        {
            var dialogue = representativeNPC.Conds[0].dialogueChain[0];
            
            dialogue.dialogue = reason.original;
            dialogue.DialogueLocalized = reason.TranslatedFallbackToOriginal;
            
            representativeNPC.TypeCurrentDialogueSafe();
        }

        if (!wasSuccessful)
        {
            if (reason == this.GetLocalized(MUFStrings.Canceled))
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

    private bool TryFinishMove(out LocalizationPair reason)
    {
        if (cancelOnAfterDropComplete)
        {
            reason = this.GetLocalized(MUFStrings.Canceled);
            return false;
        }

        if (attachedUnit == null)
        {
            reason = this.GetLocalized(MUFStrings.Bug);
            Debug.LogError($"Attached Unit was null.");
            return false;
        }

        STile hitStile = SGrid.GetSTileUnderneath(gameObject);

        if (hitStile == null)
        {
            reason = this.GetLocalized(MUFStrings.CannotLeave);
            return false;
        }
        
        Vector2Int newGridPos = new Vector2Int(hitStile.x, hitStile.y);
        Vector2Int direction = newGridPos - attachedUnit.GridPosition;

        if (direction.magnitude == 0)
        {
            reason = this.GetLocalized(MUFStrings.Canceled);
            return false;
        }

        if (direction.magnitude != 1)
        {
            reason = this.GetLocalized(MUFStrings.NewLocationDistance);
            return false;
        }

        foreach (MilitaryUnit unit in MilitaryUnit.ActiveUnits)
        {
            if (unit.GridPosition == newGridPos && unit.UnitStatus == MilitaryUnit.Status.Active && unit.UnitTeam == attachedUnit.UnitTeam)
            {
                reason = this.GetLocalized(MUFStrings.TileOccupied);
                return false;
            }
        }

        foreach (MilitaryUnspawnedAlly unspawnedAlly in MilitaryUnspawnedAlly.AllUnspawnedAllies)
        {
            if (unspawnedAlly.parentStile == hitStile)
            {
                reason = this.GetLocalized(MUFStrings.TileOccupied);
                return false;
            }
        }

        STile originalSTile = attachedUnit.AttachedSTile;
        // If AttachedSTile is null, assume unit is flying
        if (originalSTile != null)
        {
            if (!CanMoveBetweenTiles(originalSTile as MilitarySTile, hitStile as MilitarySTile, direction))
            {
                reason = this.GetLocalized(MUFStrings.Walls);
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

        reason = Random.Range(0, 2) == 0 ? 
            this.GetLocalized(MUFStrings.RandomA)
            : this.GetLocalized(MUFStrings.RandomB);
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
