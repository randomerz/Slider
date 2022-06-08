using System.Collections;
using System.Collections.Generic;
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



    //Dialogue
    private int currDconds;    //indices to get the right dialogue
    private int currDialogueInChain;
    private bool dialogueEnabled;   //The NPC can give dialogue
    private bool dialogueActive;    //The NPC is in the process of giving dialogue (regardless of if it's finished)
    private bool startedTyping;     //The NPC is in the middle of typing the dialogue
    private bool waitingForPlayerContinue;  //The NPC is waiting for the player to press e to continue its chain.
    private Coroutine waitNextDialogueCoroutine;

    //Walking
    private STile currentStileUnderneath;
    private WorldNavAgent nav;
    private bool walking;   //NPC is in the process of following a path.
    private NPCWalk currWalk;   //Current walk the NPC is performing, null otherwise.
    private List<STileCrossing> remainingStileCrossings;
    private List<Transform> remainingPath;
    private Coroutine walkCoroutine;



    private void Awake()
    {
        nav = GetComponent<WorldNavAgent>();
        dialogueEnabled = true;
        waitNextDialogueCoroutine = null;
    }

    private void OnEnable()
    {
        PlayerAction.OnAction += OnPlayerAction;
        SGridAnimator.OnSTileMoveStart += OnSTileMoveStart;
        SGridAnimator.OnSTileMoveEnd += OnSTileMoveEnd;
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

        //Poll for the new dialogue, and update it if it is different.
        int newDialogue = CurrentDialogue();
        if (currDconds != newDialogue && dialogueEnabled)
        {
            StartCoroutine(WaitThenChangeDialogue());
        }

        if (startedTyping && dialogueDisplay.textTyperText.finishedTyping)
        {
            startedTyping = false;
            FinishDialogue();
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

    public void DialogueTriggerEnter()
    {
        var dChain = dconds[currDconds].dialogueChain;
        if (dChain.Count > 0 && dChain[currDialogueInChain].dontInterrupt && dialogueActive)
        {
            //Don't retype the don't interrupt dialogue if it's already been typed.
            return;
        }
        
        TypeNextDialogue();
    }

    public void DialogueTriggerExit()
    {
        var dChain = dconds[currDconds].dialogueChain;
        if (dChain.Count > 0 && dChain[currDialogueInChain].dontInterrupt)
        {
            //Dialogue keeps playing even if the player exits
            return;
        }

        FadeDialogue();
    }

    //Player entering the trigger and also from moving to the next dialogue in the chain.
    public void TypeNextDialogue()
    {
        if (dialogueEnabled)
        {
            if (dconds[currDconds].dialogueChain.Count == 0)
            {
                dconds[currDconds].OnDialogueStart();
                dialogueDisplay.DisplaySentence(dconds[currDconds].GetDialogue());
            }
            else
            {
                dconds[currDconds].OnDialogueChainStart(currDialogueInChain);
                dialogueDisplay.DisplaySentence(dconds[currDconds].GetDialogueChain(currDialogueInChain));
            }

            startedTyping = true;
            dialogueActive = true;
        }
    }

    public void FadeDialogue()
    {
        dialogueDisplay.FadeAwayDialogue();

        //Don't allow player to continue conversation.
        if (waitNextDialogueCoroutine != null)
        {
            waitingForPlayerContinue = false;
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

        dialogueActive = false;
    }

    private void ChangeDialogue(int newDialogue, bool triggerOnChange = false)
    {
        currDconds = newDialogue;
        currDialogueInChain = 0;
        if (!dconds[newDialogue].dialogue.Equals("") || dconds[newDialogue].dialogueChain.Count > 0)
        {
            //Show new message ping if the dialogue actually exists.
            dialogueDisplay.NewMessagePing();
        } else
        {
            dialogueDisplay.ReadMessagePing();
        }

        if (dialogueActive)
        {
            //If there was dialogue playing before, get rid of it.
            FadeDialogue();
        }

        
        if (triggerOnChange)    //Use this if we immediately want to go into the next dialogue.
        {
            TypeNextDialogue();
        }
        
        dconds[currDconds].onDialogueChanged?.Invoke();
    }

    //Waits until the player has exited the trigger before retrieving the next dialogue (idk if this is necessary, might be annoying/confusing to the player).
    private IEnumerator WaitThenChangeDialogue()
    {
        yield return new WaitUntil(() =>
        {
            var dChain = dconds[currDconds].dialogueChain;
            if (dChain.Count > 0 && dChain[currDialogueInChain].dontInterrupt && dialogueDisplay.textTyperText.finishedTyping)
            {
                return true;
            }
            return !dialogueActive;
        });

        //Make sure the dialogue didn't change while we were waiting
        int newDialogue = CurrentDialogue();
        if (currDconds != newDialogue && dialogueEnabled)
        {
            ChangeDialogue(newDialogue);
        }
    }

    private void FinishDialogue()
    {
        if (dconds[currDconds].dialogueChain.Count == 0)
        {
            dconds[currDconds].OnDialogueEnd();
        } else
        {
            dconds[currDconds].OnDialogueChainEnd(currDialogueInChain);
            waitNextDialogueCoroutine = StartCoroutine(WaitForNextDialogue());
        }
    }

    public void ClearDialogue()
    {
        dconds[currDconds].KillDialogue();
    }

    public void SetNextDialogue()
    {
        if (currDconds < dconds.Count - 1)
        {
            dconds[currDconds+1].SetPrio(dconds[currDconds].GetPrio());
        }
    }

    public void DialogueEnabled(bool value)
    {
        dialogueEnabled = value;
    }

    private void SetNextDialogueInChain(bool triggerNext = false)
    {
        //The dialogue will just chill on the last line if it's already been exhausted (could maybe customize to repeat the last line or start from the beginning).
        if (currDialogueInChain < dconds[currDconds].dialogueChain.Count - 1)
        {
            currDialogueInChain++;
            if (triggerNext)
            {
                TypeNextDialogue();
            }
        }
        else
        {
            dconds[currDconds].OnDialogueChainExhausted();
        }
    }

    private void OnPlayerAction(object sender, System.EventArgs e)
    {
        if (waitingForPlayerContinue)
        {
            //Player triggered next dialogue.
            SetNextDialogueInChain(true);
            waitingForPlayerContinue = false;
        } else if (startedTyping && !dialogueDisplay.textTyperText.finishedTyping)
        {
            //Player skipped through text.
            dialogueDisplay.textTyperText.TrySkipText();
            dialogueDisplay.textTyperBG.TrySkipText();
        }  
    }

    private IEnumerator WaitForNextDialogue()
    {
        DialogueConditionals.Dialogue curr = dconds[currDconds].dialogueChain[currDialogueInChain];
        if (curr.waitUntilPlayerAction)
        {
            //Might want to loop an animation here of the ... per this card: https://trello.com/c/MMGJR8Ra/143-new-npc-dialogue-features
            waitingForPlayerContinue = true;
            yield return null;
        } else
        {
            yield return new WaitForSeconds(curr.delayAfterFinishedTyping);
            SetNextDialogueInChain(true);
        }
    }
    #endregion Dialogue

    public void Teleport(Transform trans)
    {
        transform.position = trans.position;
        transform.parent = trans.parent;
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