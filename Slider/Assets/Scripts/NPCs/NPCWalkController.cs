using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class NPCWalkController : NPCCondsController
{

    private bool isWalking;
    private NPCWalkData currWalk;
    private List<STileCrossing> remainingStileCrossings;
    private List<Transform> remainingPath;
    private Coroutine walkCoroutine;

    private NPCAnimatorController animator; 
    private SpriteRenderer spriteRenderer;
    private bool spriteDefaultFacingLeft;

    public NPCWalkController(NPC context, NPCAnimatorController animator, SpriteRenderer sr, bool spriteDefaultFacingLeft) : base(context)
    {
        this.animator = animator;
        this.spriteRenderer = sr;
        this.spriteDefaultFacingLeft = spriteDefaultFacingLeft;
    }

    public override void OnEnable()
    {
        base.OnEnable();
        SGridAnimator.OnSTileMoveStart += OnSTileMoveStart;
        SGridAnimator.OnSTileMoveEnd += OnSTileMoveEnd;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        SGridAnimator.OnSTileMoveStart -= OnSTileMoveStart;
        SGridAnimator.OnSTileMoveEnd -= OnSTileMoveEnd;
    }

    public void TryStartWalkAtIndex(int walkInd)
    {
        if (!WalkIndexInBounds(walkInd))
        {
            Debug.LogError($"Tried to start a walk event for NPC {npcContext.gameObject.name} that did not exist.");
        }
        else
        {
            StartWalkAtIndex(walkInd);
        }
    }

    public void TryStartValidWalk()
    {
        npcContext.StartCoroutine(WaitForTilesToStopMoving(() =>
        {
            int validWalkIndex = FindValidWalkIndex();

            if (validWalkIndex < 0)
            {
                Debug.LogError($"Valid Walk Not Found For {npcContext.gameObject.name}");
            }
            else
            {
                StartWalkAtIndex(validWalkIndex);
            }
        }));
    }

    private void OnSTileMoveStart(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        if (isWalking && PathBrokenAfterSTileMoved(e.stile))
        {
            InterruptWalk();
        }
    }

    private void OnSTileMoveEnd(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        bool walkStopped = currWalk != null && remainingPath.Count > 0 && !isWalking;
        if (walkStopped && PathExistsAndValid(remainingPath, remainingStileCrossings))
        {
            ResumeWalk();
        }
    }

    private void StartWalkAtIndex(int walkInd)
    {
        if (walkCoroutine != null)
        {
            npcContext.StopCoroutine(walkCoroutine);
        }

        currWalk = npcContext.CurrCond.walks[walkInd];
        remainingStileCrossings = new List<STileCrossing>(currWalk.stileCrossings);
        remainingPath = new List<Transform>(currWalk.path);

        isWalking = true;
        animator.SetBoolToTrue("isWalking");
        currWalk.onPathStarted?.Invoke();

        walkCoroutine = npcContext.StartCoroutine(DoCurrentWalk());
    }

    private void ResumeWalk()
    {
        currWalk.onPathResumed?.Invoke();
        SetPathStartToCurrNPCPos();

        walkCoroutine = npcContext.StartCoroutine(DoCurrentWalk());
    }

    private void InterruptWalk()
    {
        npcContext.StopCoroutine(walkCoroutine);
        walkCoroutine = null;
        isWalking = false;
        animator.SetBoolToFalse("isWalking");
        currWalk.onPathBroken?.Invoke();
    }

    private IEnumerator DoCurrentWalk()
    {
        float scaledSpeed;
        float t;
        while (remainingPath.Count >= 2)
        {
            scaledSpeed = npcContext.speed / Vector3.Distance(remainingPath[0].position, remainingPath[1].position);
            t = 0;
            while (t < 1f)
            {
                npcContext.transform.position = Vector3.Lerp(remainingPath[0].position, remainingPath[1].position, t);
                t += scaledSpeed * Time.deltaTime;

                UpdateSpriteFacingDir();
                UpdateSTileCrossings();

                yield return new WaitForEndOfFrame();
            }
            npcContext.transform.position = remainingPath[1].position;
            remainingPath.RemoveAt(0);
        }

        FinishWalk();
    }

    private IEnumerator WaitForTilesToStopMoving(System.Action callback = null)
    {
        if (SGrid.current.TilesMoving())
        {
            yield return new WaitUntil(() => !SGrid.current.TilesMoving());
        }

        callback();
    }

    private void UpdateSpriteFacingDir()
    {
        //dot > 0 if player is moving in their default direction, so we want to flip it if this is not the case.
        float dot = Vector2.Dot((remainingPath[1].position - remainingPath[0].position), spriteDefaultFacingLeft ? Vector2.left : Vector2.right);
        spriteRenderer.flipX = spriteRenderer.flipX ? dot <= 0 : dot < 0;   //Don't change directions if dot == 0
    }

    private void UpdateSTileCrossings()
    {
        if (remainingStileCrossings.Count > 0 && remainingStileCrossings[0].to == npcContext.CurrentSTileUnderneath)
        {
            remainingStileCrossings.RemoveAt(0);
            npcContext.transform.SetParent(npcContext.CurrentSTileUnderneath == null ? null : npcContext.CurrentSTileUnderneath.transform);
        }
    }

    private void FinishWalk()
    {
        if (currWalk.turnAroundAfterWalking)
        {
            spriteRenderer.flipX = !spriteRenderer.flipX;
        }

        animator.SetBoolToFalse("isWalking");
        currWalk.onPathFinished?.Invoke();

        isWalking = false;
        currWalk = null;
        walkCoroutine = null;
    }

    private void SetPathStartToCurrNPCPos()
    {
        Transform trans = npcContext.transform;
        GameObject objectAtNPCPos = Object.Instantiate(remainingPath[0].gameObject, trans.position, trans.rotation, trans.parent);
        remainingPath[0] = objectAtNPCPos.transform;
    }

    private int FindValidWalkIndex()
    {
        for (int i = 0; i < npcContext.CurrCond.walks.Count; i++)
        {
            if (PathExistsAndValid(i))
            {
                return i;
            }
        }

        return -1;
    }

    private bool PathBrokenAfterSTileMoved(STile movedStile)
    {
        foreach (STileCrossing cross in remainingStileCrossings)
        {
            if (movedStile == cross.from || movedStile == cross.to)
            {
                return true;
            }
        }

        return false;
    }

    private bool PathExistsAndValid(int walkInd)
    {
        if (WalkIndexInBounds(walkInd))
        {
            NPCWalkData walk = npcContext.CurrCond.walks[walkInd];
            return PathExistsAndValid(walk.path, walk.stileCrossings);
        }
        return false;
    }

    private bool PathExistsAndValid(List<Transform> path, List<STileCrossing> stileCrossings)
    {
        return path.Count > 0 && AllSTileCrossingsValid(stileCrossings);
    }

    private bool AllSTileCrossingsValid(List<STileCrossing> stileCrossings)
    {
        foreach (STileCrossing cross in stileCrossings)
        {
            if (!cross.CrossingIsValid())
            {
                return false;
            }
        }

        return true;
    }

    private bool WalkIndexInBounds(int walkInd)
    {
        return walkInd >= 0 && walkInd < npcContext.CurrCond.walks.Count;
    }
}

