using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class NPC : MonoBehaviour
{
    [System.Serializable]
    public class STileCrossing
    {
        public STile from;
        public STile to;

        //Use if from or to is null.
        public Vector2Int fromPos;
        public Vector2Int toPos;

        public Vector2Int[] dirs;  //Check from + dir = to in order to check the crossing is valid.
    }

    [System.Serializable]
    public class NPCWalk
    {
        //public bool usePathfinding;
        public List<Transform> path;
        public List<STileCrossing> stileCrossings;
        public bool turnAroundAfterWalking;

        public UnityEvent onPathStarted;
        public UnityEvent onPathFinished;
        public UnityEvent onPathBroken;
        public UnityEvent onPathResumed;
    }

    private readonly string poofParticleName = "SmokePoof Variant";

    public string characterName;
    [SerializeField] private float speed;
    public NPCAnimatorController animator;
    [FormerlySerializedAs("dconds")]
    public List<NPCConditionals> conds;

    [SerializeField] private DialogueDisplay dialogueDisplay;
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private bool spriteDefaultFacingLeft;

    //Dconds
    private int currDcondIndex;
    private Dictionary<NPCConditionals, int> dcondToCurrDchainIndex;

    //Dialogue
    public static bool dialogueEnabledAllNPC = true;

    private bool canGiveDialogue;
    private bool dialogueBoxIsActive;
    private bool currDchainExhausted;
    private bool playerInDialogueTrigger;
    private bool isTypingDialogue;
    private bool waitingForPlayerAction;

    private Coroutine delayBeforeNextDialogueCoroutine;

    //Walking
    private STile currentStileUnderneath;
    private bool walking;   //NPC is in the process of following a path.
    private NPCWalk currWalk;   //Current walk the NPC is performing, null otherwise.
    private List<STileCrossing> remainingStileCrossings;
    private List<Transform> remainingPath;
    private Coroutine walkCoroutine;
    private GameObject poofParticles;

    private bool DialogueEnabled => dialogueEnabledAllNPC && canGiveDialogue;

    private NPCConditionals CurrDcond => conds[currDcondIndex];

    private int CurrDchainIndex {
        get {
            return dcondToCurrDchainIndex[CurrDcond];
        }

        set
        {
            dcondToCurrDchainIndex[CurrDcond] = value;
        }
    }

    private void Awake()
    {
        canGiveDialogue = true;

        SetDcondPrioritiesToArrayPos();

        InitializeCurrDchainIndices();
    }

    private void OnEnable()
    {
        PlayerAction.OnAction += OnPlayerAction;
        SGridAnimator.OnSTileMoveStart += OnSTileMoveStart;
        SGridAnimator.OnSTileMoveEnd += OnSTileMoveEnd;

        poofParticles = Resources.Load<GameObject>();
    }

    private void OnDisable()
    {
        PlayerAction.OnAction -= OnPlayerAction;
        SGridAnimator.OnSTileMoveStart -= OnSTileMoveStart;
        SGridAnimator.OnSTileMoveEnd -= OnSTileMoveEnd;
    }

    void Update()
    {
        CheckAllDconds();

        if (CanUpdateDialogue())
        {
            PollForNewDialogue();
        }

        bool finishedTypingCurrDialogue = isTypingDialogue && dialogueDisplay.textTyperText.finishedTyping;
        if (finishedTypingCurrDialogue)
        {
            HandleDialogueFinished();
        }

        if (DialogueShouldDeactivate())
        {
            DeactivateDialogueBox();
        }
    }

    private void FixedUpdate()
    {
        currentStileUnderneath = STile.GetSTileUnderneath(transform, currentStileUnderneath);
    }

    #region Dialogue
    public void DialogueTriggerEnter()
    {
        playerInDialogueTrigger = true;

        bool shouldStartTypingDialogue = !CurrDchainIsEmpty() && !NPCGivingDontInterruptDialogue();
        if (shouldStartTypingDialogue)
        {
            if (CurrDcond.alwaysStartFromBeginning)
            {
                CurrDchainIndex = 0;
                currDchainExhausted = false;
            }

            TypeCurrentDialogue();
        }
    }

    public void DialogueTriggerExit()
    {
        playerInDialogueTrigger = false;

        if (DialogueShouldDeactivate())
        {
            DeactivateDialogueBox();
        }
    }

    public void SetDialogueEnabled(bool value)
    {
        canGiveDialogue = value;
    }

    public void TypeCurrentDialogue()
    {
        if (DialogueEnabled && !CurrDchainIsEmpty())
        {
            dialogueDisplay.DisplaySentence(CurrDcond.GetDialogueString(CurrDchainIndex));

            isTypingDialogue = true;
            dialogueBoxIsActive = true;

            CurrDcond.OnDialogueChainStart(CurrDchainIndex);
        }
    }

    private void OnPlayerAction(object sender, System.EventArgs e)
    {
        if (dialogueBoxIsActive)
        {
            if (waitingForPlayerAction)
            {
                waitingForPlayerAction = false;
                SetNextDialogueInChain(true);
            }
            else if (isTypingDialogue)
            {
                SkipText();
            }
        }
    }

    private void DeactivateDialogueBox()
    {
        dialogueDisplay.FadeAwayDialogue();

        DontAllowDialogueToContinue();

        if (!CurrDchainIsEmpty() && CurrentDialogue().doNotRepeatAfterTriggered)
        {
            SetNextDialogueInChain();
        }

        dialogueBoxIsActive = false;
        isTypingDialogue = false;
    }

    private void PollForNewDialogue()
    {
        int maxPrioDialogue = GetDcondIndexWithMaxPriority();
        bool dialogueIsNew = currDcondIndex != maxPrioDialogue;
        if (dialogueIsNew)
        {
            ChangeDialogue(maxPrioDialogue);

            if (dialogueBoxIsActive && playerInDialogueTrigger)
            {
                TypeCurrentDialogue();
            }
        }
    }

    private void ChangeDialogue(int newDialogue)
    {
        currDcondIndex = newDialogue;
        currDchainExhausted = false;

        dialogueDisplay.SetMessagePing(!CurrDchainIsEmpty());

        CurrDcond.onDialogueChanged?.Invoke();
    }

    private void HandleDialogueFinished()
    {
        isTypingDialogue = false;
        CurrDcond.OnDialogueChainEnd(CurrDchainIndex);

        if (CurrentDialogue().waitUntilPlayerAction)
        {
            waitingForPlayerAction = true;
        }
        else
        {
            delayBeforeNextDialogueCoroutine = StartCoroutine(SetNextDialogueInChainAfterDelay(CurrentDialogue().delayAfterFinishedTyping));
            SetNextDialogueInChain(true);
        } 
    }

    private IEnumerator SetNextDialogueInChainAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SetNextDialogueInChain(true);
    }

    private void SetNextDialogueInChain(bool typeNextDialogue = false)
    {
        var dChain = CurrDcond.dialogueChain;
        if (CurrDchainIndex < dChain.Count - 1)
        {
            CurrDchainIndex++;
            if (typeNextDialogue)
            {
                TypeCurrentDialogue();
            }
        }
        else
        {
            currDchainExhausted = true;
            CurrDcond.OnDialogueChainExhausted();
        }
    }

    private void DontAllowDialogueToContinue()
    {
        waitingForPlayerAction = false;

        if (delayBeforeNextDialogueCoroutine != null)
        {
            StopCoroutine(delayBeforeNextDialogueCoroutine);
            delayBeforeNextDialogueCoroutine = null;
        }
    }

    private void SkipText()
    {
        dialogueDisplay.textTyperText.TrySkipText();
        dialogueDisplay.textTyperBG.TrySkipText();
    }

    private int GetDcondIndexWithMaxPriority()
    {
        int maxPrioIndex = -1;
        int maxPrio = 0;
        for (int i = 0; i < conds.Count; i++)
        {
            if (conds[i].GetPrio() > maxPrio)
            {
                maxPrioIndex = i;
                maxPrio = conds[i].GetPrio();
            }
        }
        if (maxPrioIndex == -1)
        {
            Debug.LogError("No suitable dialogue can be displayed!");
        }
        return maxPrioIndex;
    }

    private void SetDcondPrioritiesToArrayPos()
    {
        for (int i = 0; i < conds.Count; i++)
        {
            conds[i].priority = i + 1;
        }
    }

    private void InitializeCurrDchainIndices()
    {
        dcondToCurrDchainIndex = new Dictionary<NPCConditionals, int>();
        foreach (var dcond in conds)
        {
            dcondToCurrDchainIndex[dcond] = 0;
        }
    }

    private void CheckAllDconds()
    {
        foreach (NPCConditionals d in conds)
        {
            d.CheckConditions();
        }
    }

    private bool CanUpdateDialogue()
    {
        return DialogueEnabled && !NPCGivingDontInterruptDialogue();
    }

    private bool DialogueShouldDeactivate()
    {
        return dialogueBoxIsActive && !playerInDialogueTrigger && !NPCGivingDontInterruptDialogue();
    }

    private bool NPCGivingDontInterruptDialogue()
    {
        if (CurrDchainIsEmpty())
        {
            return false;
        }

        bool givingDialogue = dialogueBoxIsActive && !currDchainExhausted;
        return givingDialogue && CurrentDialogue().dontInterrupt;
    }

    private NPCConditionals.Dialogue CurrentDialogue()
    {
        if (CurrDchainIsEmpty())
        {
            return null;
        }

        return CurrDcond.dialogueChain[CurrDchainIndex];
    }

    private bool CurrDchainIsEmpty()
    {
        return CurrDcond.dialogueChain.Count == 0;
    }
    #endregion Dialogue

    public void Teleport(Transform trans)
    {
        if (transform.position != trans.position)
        {
            Instantiate(poofParticles, transform.position, Quaternion.identity);
            transform.position = trans.position;
            transform.parent = trans.parent;
        }
    }

    #region Walking

    public void StartCurrentWalk(int walkInd)
    {
        if (walkInd < 0 || walkInd >= CurrDcond.walks.Count)
        {
            Debug.LogError($"Tried to start a walk event for NPC {gameObject.name} that did not exist.");
            return;
        }

        if (currWalk == null)
        {
            currWalk = CurrDcond.walks[walkInd];
            remainingStileCrossings = new List<STileCrossing>(currWalk.stileCrossings);
            remainingPath = new List<Transform>(currWalk.path);
            walkCoroutine = StartCoroutine(DoCurrentWalk(false));
        }
    }

    public void StartValidWalk()
    {
        if (currWalk == null)
        {
            StartCoroutine(WaitThenCheckValidWalk());
        }
    }

    private IEnumerator WaitThenCheckValidWalk()
    {
        if (SGrid.current.TilesMoving())
        {
            yield return new WaitUntil(() => !SGrid.current.TilesMoving());
        }

        bool validWalkFound = false;
        for (int i = 0; i < CurrDcond.walks.Count; i++)
        {
            if (CurrentPathExistsAndValid(i))
            {
                validWalkFound = true;
                StartCurrentWalk(i);
                break;
            }
        }

        if (!validWalkFound)
        {
            Debug.LogError($"Valid Walk Not Found For {gameObject.name}");
        }
    }

    public IEnumerator DoCurrentWalk(bool resumed)
    {
        walking = true;
        animator.SetBoolToTrue("isWalking");
        if (resumed)
        {
            currWalk.onPathResumed?.Invoke();
            //Create a dummy transform that matches the NPC's current position
            remainingPath[0] = ((GameObject) Instantiate(remainingPath[0].gameObject, transform.position, transform.rotation, transform.parent)).transform;
        } else
        {
            currWalk.onPathStarted?.Invoke();
        }

        if (SGrid.current.TilesMoving())
        {

        }

        //Lerp positions until we go through the whole path.
        float s;
        float t;
        while(remainingPath.Count >= 2)
        {
            s = speed / Vector3.Distance(remainingPath[0].position, remainingPath[1].position);    //This factor ensures that the speed traveled is scaled properly to the distance btw points.
            t = 0;
            while (t < 1f)
            {
                transform.position = Vector3.Lerp(remainingPath[0].position, remainingPath[1].position, t);
                t += s * Time.deltaTime;

                //dot > 0 if player is moving in their default direction, so we want to flip it if this is not the case.`
                float dot = Vector2.Dot((remainingPath[1].position - remainingPath[0].position), spriteDefaultFacingLeft ? Vector2.left : Vector2.right);
                sr.flipX = sr.flipX ? dot <= 0 : dot < 0;   //Don't change directions if dot == 0

                if (remainingStileCrossings.Count > 0 && remainingStileCrossings[0].to == currentStileUnderneath)
                {
                    remainingStileCrossings.RemoveAt(0);
                    transform.SetParent(currentStileUnderneath == null ? null : currentStileUnderneath.transform);
                }
                yield return new WaitForEndOfFrame();
            }
            transform.position = remainingPath[1].position;
            remainingPath.RemoveAt(0);
        }

        if (currWalk.turnAroundAfterWalking)
        {
            sr.flipX = !sr.flipX;
        }

        walking = false;
        animator.SetBoolToFalse("isWalking");
        currWalk.onPathFinished?.Invoke();
        currWalk = null;
    }

    //Check if the path is broken.
    private void OnSTileMoveStart(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        if (currWalk != null && walking)
        {
            //Handle STile moved while the NPC was walking
            foreach (STileCrossing cross in remainingStileCrossings)
            {
                if (e.stile == cross.from || e.stile == cross.to)
                {
                    //Stop the routine if a tile is moved that is part of the remaining path.
                    StopCoroutine(walkCoroutine);
                    walkCoroutine = null;
                    walking = false;
                    animator.SetBoolToFalse("isWalking");
                    currWalk.onPathBroken?.Invoke();
                }
            }
        }
    }

    //Check if the path can be resumed.
    private void OnSTileMoveEnd(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        if (currWalk != null && remainingPath.Count > 0 && !walking)
        {
            if (PathExistsAndValid(remainingPath, remainingStileCrossings))
            {
                walkCoroutine = StartCoroutine(DoCurrentWalk(true));
            }
        }
    }

    public bool CurrentPathExistsAndValid(int walkInd)
    {
        return (walkInd < 0 || walkInd >= CurrDcond.walks.Count) ? false : PathExistsAndValid(CurrDcond.walks[walkInd].path, CurrDcond.walks[walkInd].stileCrossings);
    }

    private bool PathExistsAndValid(List<Transform> path, List<STileCrossing> stileCrossings)
    {
        if (path.Count <= 0)
        {
            return false;
        }

        //If all the crossings are valid, the path is valid.
        foreach (STileCrossing cross in stileCrossings)
        {
            bool crossGood = false;
            Vector2Int from = cross.from ? new Vector2Int(cross.from.x, cross.from.y) : cross.fromPos;
            Vector2Int to = cross.to ? new Vector2Int(cross.to.x, cross.to.y) : cross.toPos;

            //If any of the directions are good, the crossing is good.
            foreach (Vector2Int dir in cross.dirs)
            {
                if ((to - from).Equals(dir))
                {
                    crossGood = true;
                }
            }

            if (!crossGood)
            {
                return false;
            }
        }

        return true;
    }
    #endregion
}