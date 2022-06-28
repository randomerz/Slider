using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

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

    public string characterName;
    [SerializeField] private float speed;
    public NPCAnimatorController animator;
    public List<DialogueConditionals> dconds;

    [SerializeField] private DialogueDisplay dialogueDisplay;
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private bool spriteDefaultFacingLeft;


    //Dialogue
    public bool DialogueEnabled => gDialogueEnabled && dialogueEnabled;

    public static bool gDialogueEnabled = true; //Dialogue Enabling for all NPCs.

    private bool dialogueEnabled;   //The NPC can give dialogue
    private bool dialogueActive;    //The NPC is in the process of giving dialogue (regardless of if it's finished)
    private bool playerInTrigger;   //The player is in the dialogue trigger.
    private bool startedTyping;     //The NPC is in the middle of typing the dialogue
    private bool waitingForPlayerContinue;  //The NPC is waiting for the player to press e to continue its chain.
    private bool dChainExhausted;

    private int currDconds;    //indices to get the right dialogue
    private int currDialogueInChain;

    private Dictionary<int, int> currDialogueInDconds;  //Keeps track of each dialogue in the given dconds.



    private Coroutine waitNextDialogueCoroutine;

    //Walking
    private STile currentStileUnderneath;
    private bool walking;   //NPC is in the process of following a path.
    private NPCWalk currWalk;   //Current walk the NPC is performing, null otherwise.
    private List<STileCrossing> remainingStileCrossings;
    private List<Transform> remainingPath;
    private Coroutine walkCoroutine;

    private GameObject poofParticles;

    private void Awake()
    {
        dialogueEnabled = true;
        waitNextDialogueCoroutine = null;

        for(int i = 0; i < dconds.Count; i++)
        {
            dconds[i].priority = i+1;
        }

        currDialogueInDconds = new Dictionary<int, int>();
        for (int i = 0; i < dconds.Count; i++)
        {
            currDialogueInDconds[i] = 0;
        }
    }

    private void OnEnable()
    {
        PlayerAction.OnAction += OnPlayerAction;
        SGridAnimator.OnSTileMoveStart += OnSTileMoveStart;
        SGridAnimator.OnSTileMoveEnd += OnSTileMoveEnd;

        poofParticles = Resources.Load<GameObject>("SmokePoof Variant");
    }

    private void OnDisable()
    {
        PlayerAction.OnAction -= OnPlayerAction;
        SGridAnimator.OnSTileMoveStart -= OnSTileMoveStart;
        SGridAnimator.OnSTileMoveEnd -= OnSTileMoveEnd;
    }

    // might need optimizing
    void Update()
    {
        foreach (DialogueConditionals d in dconds)
        {
            d.CheckConditions();
        }

        bool lastDialogueFinished = dconds[currDconds].dialogueChain.Count > 0 && dconds[currDconds].dialogueChain.Count - 1 == currDialogueInChain && dialogueDisplay.textTyperText.finishedTyping;
        bool canUpdateDialogue = (dconds[currDconds].dialogueChain.Count > 0 && !dconds[currDconds].dialogueChain[currDialogueInChain].dontInterrupt) || lastDialogueFinished;

        if (dChainExhausted && !playerInTrigger)
        {
            dChainExhausted = false;
            DeactivateDialogue();
        }

        //Poll for the new dialogue, and update it if it is different.
        int newDialogue = CurrentDialogue();
        if (currDconds != newDialogue && DialogueEnabled && canUpdateDialogue)
        {
            ChangeDialogue(newDialogue);
            if (dialogueActive)
            {
                if (playerInTrigger)
                {
                    TypeNextDialogue();
                } else
                {
                    DeactivateDialogue();
                }
            }
        }

        if (startedTyping && dialogueDisplay.textTyperText.finishedTyping)
        {
            startedTyping = false;
            FinishDialogue();
        }

        if (dialogueActive && !playerInTrigger)
        {
            if (canUpdateDialogue)
            {
                //Keep going until the player reaches the first non-don't interrupt (or last dialogue) before disabling dialogue.
                DeactivateDialogue();
            }
        }
    }

    private void FixedUpdate()
    {
        // updating childing
        currentStileUnderneath = STile.GetSTileUnderneath(transform, currentStileUnderneath);
        // Debug.Log("Currently on: " + currentStileUnderneath);

        /*  Don't reparent right away bc this causes some problems with NPCs that are childs of other moving objects (boats, etc.)
        if (currentStileUnderneath != null)
        {
            transform.SetParent(currentStileUnderneath.transform);
        }
        else
        {
            transform.SetParent(null);
        }
        */
    }

    #region Dialogue

    //OnTriggerEnter and OnTriggerExit handlers.
    public void DialogueTriggerEnter()
    {
        playerInTrigger = true;

        var dChain = dconds[currDconds].dialogueChain;
        if (dChain.Count > 0 && dChain[currDialogueInChain].dontInterrupt && dialogueActive)
        {
            //Don't retype the don't interrupt dialogue if it's already been typed.
            return;
        }

        if (dconds[currDconds].alwaysStartFromBeginning)
        {
            currDialogueInChain = 0;
            currDialogueInDconds[currDconds] = currDialogueInChain;
        }

        TypeNextDialogue();
    }

    public void DialogueTriggerExit()
    {
        playerInTrigger = false;

        var dChain = dconds[currDconds].dialogueChain;
        if (!(dChain.Count > 0 && dChain[currDialogueInChain].dontInterrupt))
        {
            DeactivateDialogue();
        }
    }

    //Player entering the trigger and also from moving to the next dialogue in the chain.
    public void TypeNextDialogue()
    {
        if (DialogueEnabled && dconds[currDconds].dialogueChain.Count > 0)
        {
            dconds[currDconds].OnDialogueChainStart(currDialogueInChain);
            dialogueDisplay.DisplaySentence(dconds[currDconds].GetDialogue(currDialogueInChain));

            startedTyping = true;
            dialogueActive = true;
        }
    }

    public void DeactivateDialogue()
    {
        dialogueDisplay.FadeAwayDialogue();

        //Don't allow player to continue conversation.
        if (waitNextDialogueCoroutine != null)
        {
            StopCoroutine(waitNextDialogueCoroutine);
            waitNextDialogueCoroutine = null;
        }

        //Dialogue that doesn't repeat should be skipped now.
        var dChain = dconds[currDconds].dialogueChain;
        if (dChain.Count > 0)
        {
            if (dChain[currDialogueInChain].doNotRepeatAfterTriggered)
            {
                SetNextDialogueInChain();
            }
        }

        waitingForPlayerContinue = false;
        dialogueActive = false;
        startedTyping = false;
    }

    public int CurrentDialogue()
    {
        int curr = -1;
        int max = 0;
        for (int i = 0; i< dconds.Count; i++)
        {
            if (dconds[i].GetPrio() > max)
            {
                curr = i;
                max = dconds[i].GetPrio();
            }
        }
        if (curr == -1)
        {
            Debug.LogError("No suitable dialogue can be displayed!");
        }
        return curr;
    }

    public void ClearDialogue()
    {
        dconds[currDconds].KillDialogue();
    }

    public void SetNextDialogue()
    {
        if (currDconds < dconds.Count - 1)
        {
            dconds[currDconds + 1].SetPrio(dconds[currDconds].GetPrio());
        }
    }

    public void SetDialogueEnabled(bool value)
    {
        dialogueEnabled = value;
    }

    private void ChangeDialogue(int newDialogue)
    {
        currDconds = newDialogue;
        currDialogueInChain = currDialogueInDconds[currDconds];
        if (dconds[newDialogue].dialogueChain.Count > 0)
        {
            //Show new message ping if the dialogue actually exists.
            dialogueDisplay.NewMessagePing();
        } else
        {
            dialogueDisplay.ReadMessagePing();
        }
        
        dconds[currDconds].onDialogueChanged?.Invoke();
    }

    private void FinishDialogue()
    {
        dconds[currDconds].OnDialogueChainEnd(currDialogueInChain);
        waitNextDialogueCoroutine = StartCoroutine(WaitForNextDialogue());
    }

    private IEnumerator WaitForNextDialogue()
    {
        DialogueConditionals.Dialogue curr = dconds[currDconds].dialogueChain[currDialogueInChain];
        if (curr.waitUntilPlayerAction)
        {
            waitingForPlayerContinue = true;
            yield return null;
        }
        else
        {
            yield return new WaitForSeconds(curr.delayAfterFinishedTyping);
            SetNextDialogueInChain(true);
        }
    }

    private void SetNextDialogueInChain(bool triggerNext = false)
    {
        //The dialogue will just chill on the last line if it's already been exhausted (could maybe customize to repeat the last line or start from the beginning).
        var dChain = dconds[currDconds].dialogueChain;
        if (currDialogueInChain < dChain.Count - 1)
        {
            currDialogueInChain++;
            currDialogueInDconds[currDconds] = currDialogueInChain;
            if (triggerNext)
            {
                TypeNextDialogue();
            }
        }
        else
        {
            dconds[currDconds].OnDialogueChainExhausted();
            dChainExhausted = true;
        }
    }

    private void OnPlayerAction(object sender, System.EventArgs e)
    {
        if (dialogueActive)
        {
            if (waitingForPlayerContinue)
            {
                //Player triggered next dialogue.
                SetNextDialogueInChain(true);
                waitingForPlayerContinue = false;
            }
            else if (startedTyping && !dialogueDisplay.textTyperText.finishedTyping)
            {
                //Player skipped through text.
                dialogueDisplay.textTyperText.TrySkipText();
                dialogueDisplay.textTyperBG.TrySkipText();
            }
        }
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
        if (walkInd < 0 || walkInd >= dconds[currDconds].walks.Count)
        {
            Debug.LogError($"Tried to start a walk event for NPC {gameObject.name} that did not exist.");
            return;
        }

        if (currWalk == null)
        {
            currWalk = dconds[currDconds].walks[walkInd];
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
        for (int i = 0; i < dconds[currDconds].walks.Count; i++)
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
        return (walkInd < 0 || walkInd >= dconds[currDconds].walks.Count) ? false : PathExistsAndValid(dconds[currDconds].walks[walkInd].path, dconds[currDconds].walks[walkInd].stileCrossings);
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

    /*  Old Walking
    public void WalkTo(Transform trans)
    {
        //NPCs can't talkie while they walkie (under normal circumstances)
        dialogueEnabled = false;
        nav.SetDestination(TileUtil.WorldToTileCoords(trans.position), null, (pos) =>
        {
            dialogueEnabled = true;
        });
    }
    */
    #endregion
}