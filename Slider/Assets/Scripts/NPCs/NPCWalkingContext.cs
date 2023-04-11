using System.Collections;
using System.Collections.Generic;

using UnityEngine;

internal class NPCWalkingContext : MonoBehaviourContextProvider<NPC>
{

    public bool isWalking { get; private set; }
    private NPCWalkData currWalk;
    private List<STileCrossing> remainingStileCrossings;
    private List<Transform> remainingPath;
    private Coroutine walkCoroutine;

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private bool spriteDefaultFacingLeft;

    public NPCWalkingContext(NPC context, Animator animator, SpriteRenderer sr, bool spriteDefaultFacingLeft) : base(context)
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
            Debug.LogError($"Tried to start a walk event for NPC {context.gameObject.name} that did not exist.");
        }
        else
        {
            StartWalkAtIndex(walkInd);
        }
    }

    public void TryStartValidWalk()
    {
        context.StartCoroutine(WaitForTilesToStopMoving(() =>
        {
            int validWalkIndex = FindValidWalkIndex();

            if (validWalkIndex < 0)
            {
                Debug.LogError($"Valid Walk Not Found For {context.gameObject.name}");
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
        if (currWalk == null)
        {
            currWalk = context.CurrCond.walks[walkInd];

            if (currWalk.path.Count == 0)
            {
                Debug.LogError($"You forgot to set a path for walk for {context.gameObject.name} at cond {context.CurrCondIndex} walk index {walkInd}");
                return;
            }

            if (context.transform.position != currWalk.path[0].position)
            {
                Debug.LogWarning("NPC transform does not match start of walk, this will cause rubberbanding. (You're might be calling it multiple times in an inspector event!)");
                //return;
            }
            remainingStileCrossings = new List<STileCrossing>(currWalk.stileCrossings);
            remainingPath = new List<Transform>(currWalk.path);

            isWalking = true;
            animator.SetBool("isWalking", true);
            currWalk.onPathStarted?.Invoke();

            walkCoroutine = context.StartCoroutine(DoCurrentWalk());
        } else
        {
            Debug.LogWarning("Did not start NPC walk because one was already occurring!");
        }
    }

    private void ResumeWalk()
    {
        currWalk.onPathResumed?.Invoke();
        SetPathStartToCurrNPCPos();

        walkCoroutine = context.StartCoroutine(DoCurrentWalk());
    }

    private void InterruptWalk()
    {
        context.StopCoroutine(walkCoroutine);
        walkCoroutine = null;
        isWalking = false;
        animator.SetBool("isWalking", false);
        currWalk.onPathBroken?.Invoke();

        if (currWalk.teleportToEndIfInterrupted)
        {
            context.Teleport(remainingPath[remainingPath.Count - 1], true);
            FinishWalk();
        }
    }

    private IEnumerator DoCurrentWalk()
    {
        float scaledSpeed;
        float t;
        while (remainingPath.Count >= 2)
        {
            scaledSpeed = context.speed / Vector3.Distance(remainingPath[0].position, remainingPath[1].position);
            t = 0;
            while (t < 1f)
            {
                context.transform.position = Vector3.Lerp(remainingPath[0].position, remainingPath[1].position, t);
                t += scaledSpeed * Time.deltaTime;

                UpdateSpriteFacingDir();
                if (remainingStileCrossings.Count > 0 && remainingStileCrossings[0].to == context.CurrentSTileUnderneath)
                {
                    UpdateSTileCrossings();
                    t = 0;
                    scaledSpeed = context.speed / Vector3.Distance(remainingPath[0].position, remainingPath[1].position);
                }
                yield return new WaitForEndOfFrame();
            }
            context.transform.position = remainingPath[1].position;
            context.transform.SetParent(remainingPath[1]);
            remainingPath.RemoveAt(0);
        }

        FinishWalk();
    }

    private IEnumerator WaitForTilesToStopMoving(System.Action callback = null)
    {
        if (SGrid.Current.TilesMoving())
        {
            yield return new WaitUntil(() => !SGrid.Current.TilesMoving());
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
       
            remainingStileCrossings.RemoveAt(0);
            context.transform.SetParent(context.CurrentSTileUnderneath == null ? null : context.CurrentSTileUnderneath.transform);
            SetPathStartToCurrNPCPos();
    }
    
    private void FinishWalk()
    {
        if (currWalk != null)
        {
            if (currWalk.turnAroundAfterWalking)
            {
                spriteRenderer.flipX = !spriteRenderer.flipX;
            }

            NPCWalkData oldWalk = currWalk;
            isWalking = false;
            currWalk = null;
            walkCoroutine = null;
            animator.SetBool("isWalking", false);
            oldWalk.onPathFinished?.Invoke();   //This needs to be called after we've safely handled finishing the previous walk.
        } else
        {
            Debug.LogError("Current Walk was null? This should never happen. Report bug to Logan immediately.");
        }
    }

    public void CancelWalk()
    {
        if (walkCoroutine != null)
            context.StopCoroutine(walkCoroutine);

        NPCWalkData oldWalk = currWalk;
        isWalking = false;
        currWalk = null;
        walkCoroutine = null;
        animator.SetBool("isWalking", false);
    }

    private void SetPathStartToCurrNPCPos()
    {
        Transform trans = context.transform;
        GameObject objectAtNPCPos = Object.Instantiate(remainingPath[0].gameObject, trans.position, trans.rotation, trans.parent);
        remainingPath[0] = objectAtNPCPos.transform;
    }

    private int FindValidWalkIndex()
    {
        for (int i = 0; i < context.CurrCond.walks.Count; i++)
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
            NPCWalkData walk = context.CurrCond.walks[walkInd];
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
        return walkInd >= 0 && walkInd < context.CurrCond.walks.Count;
    }
}