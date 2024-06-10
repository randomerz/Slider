using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class MilitaryNPCController : MonoBehaviour
{
    private const float MOVE_DURATION = 2f;
    private const string FLAG_ITEM_NAME = "MilitaryFlag";

    public MilitaryUnit militaryUnit;
    public List<NPC> myNPCs;
    public List<FlashWhiteSprite> myFlashWhites;
    public MilitaryUnitFlag myFlag;
    
    private System.Action moveFinishCallback;
    public bool hasMoveQueuedOrIsExecuting;
    private bool isWalking;
    private bool isFacingRight = true;
    
    private void Start()
    {
        if (myNPCs == null || myNPCs.Count == 0 || myNPCs.Count != gameObject.GetComponentsInChildren<NPC>().Length)
        {
            Debug.LogWarning($"Did you forget to assign NPCs?");
        }

        UpdateSpriteTypes();
    }

    private void Update()
    {
        Item held = PlayerInventory.GetCurrentItem();
        
        if (held == null || held.itemName != FLAG_ITEM_NAME)
        {
            return;
        }

        if (isWalking)
        {
            return;
        }

        float deltaX = Player.GetPosition().x - transform.position.x;
        if (isFacingRight && deltaX < -2)
        {
            // face left
            SetNPCFacingDirection(false);
        }
        else if (!isFacingRight && deltaX > 2)
        {
            // face right
            SetNPCFacingDirection(true);
        }
    }

    public void UpdateSpriteTypes()
    {
        MilitarySpriteTable militarySpriteTable = (SGrid.Current as MilitaryGrid).militarySpriteTable;

        Sprite mySprite = militarySpriteTable.GetSpriteForUnit(militaryUnit);
        RuntimeAnimatorController myAnimatorController = militarySpriteTable.GetAnimatorControllerForUnit(militaryUnit);

        foreach (NPC npc in myNPCs)
        {
            npc.sr.sprite = mySprite;
            npc.animator.runtimeAnimatorController = myAnimatorController;
        }

        if (myFlag != null)
        {
            myFlag.spriteRenderer.sprite = militarySpriteTable.GetFlagSpriteForUnit(militaryUnit);
        }

        // MilitaryUITrackerManager.RemoveUnitTracker(militaryUnit);
        // MilitaryUITrackerManager.AddUnitTracker(militaryUnit);
    }

    public void SetPosition(Vector3 position)
    {
        transform.position = position;
    }

    // public void SetParentSTile(STile stile)
    // {
    //     militaryUnit.AttachedSTile = stile;
    // }

    public void AnimateMove(MGMove move, bool skipAnimation, System.Action finishCallback)
    {
        if (moveFinishCallback != null)
        {
            moveFinishCallback?.Invoke();
            Debug.LogWarning($"Might have called AnimateMove while another move was animating");
        }
        moveFinishCallback = finishCallback;
        Vector3 startPos, targetPos;
        startPos = MilitaryUnit.GridPositionToWorldPosition(move.startCoords);
        targetPos = MilitaryUnit.GridPositionToWorldPosition(move.endCoords);

        if (startPos.x < targetPos.x)
        {
            SetNPCFacingDirection(true);
        }
        else if (startPos.x > targetPos.x)
        {
            SetNPCFacingDirection(false);
        }

        if (skipAnimation)
        {
            FinishAnimation(targetPos, move.endStile);
        }
        else
        {
            SetNPCWalking(true);
            CoroutineUtils.ExecuteEachFrame(
                (x) => {
                    Vector3 newPos = Vector3.Lerp(startPos, targetPos, x);
                    SetPosition(newPos);
                    // if (x >= 0.5f && militaryUnit.AttachedSTile != move.endStile)
                    // {
                    //     militaryUnit.AttachedSTile = move.endStile;
                    // }
                },
                () => {
                    SetNPCWalking(false);
                    FinishAnimation(targetPos, move.endStile);
                },
                this,
                MOVE_DURATION,
                null
            );
        }
    }

    private void FinishAnimation(Vector3 targetPos, STile endStile)
    {
        hasMoveQueuedOrIsExecuting = false;
        SetPosition(targetPos);
        // militaryUnit.AttachedSTile = endStile;
        System.Action callback = moveFinishCallback;
        moveFinishCallback = null;
        callback?.Invoke(); // this might update moveFinishCallback so we cant change it after this
    }

    public void AnimateDeath()
    {
        if (moveFinishCallback != null)
        {
            Debug.LogWarning("'MoveFinishCallback' was not null when unit died.");
            moveFinishCallback?.Invoke();
        }

        // TODO: on death animator?

        FlashWhite(1);
        
        foreach (NPC npc in myNPCs)
        {
            ParticleManager.SpawnParticle(ParticleType.SmokePoof, npc.transform.position);
        }
    }

    private void SetNPCFacingDirection(bool faceRight)
    {
        isFacingRight = faceRight;
        foreach (NPC npc in myNPCs)
        {
            npc.SetFacingRight(faceRight);
        }
    }

    private void SetNPCWalking(bool isWalking)
    {
        this.isWalking = isWalking;
        foreach (NPC npc in myNPCs)
        {
            npc.animator.SetBool("isWalking", isWalking);
        }
    }

    public void FlashForDuration(float seconds)
    {
        FlashWhite((int)(seconds / (myFlashWhites[0].flashTime * 2)));
    }

    private void FlashWhite(int num, System.Action callback=null)
    {
        foreach (FlashWhiteSprite f in myFlashWhites)
        {
            f.Flash(num, callback);
        }
    }
}