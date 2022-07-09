using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class NPC : MonoBehaviour
{
    private readonly string poofParticleName = "SmokePoof Variant";

    public float speed;
    [SerializeField] private NPCAnimatorController animator;
    [SerializeField] private string characterName;
    [FormerlySerializedAs("dconds")] [SerializeField] private List<NPCConditionals> conds;

    [SerializeField] private DialogueDisplay dialogueDisplay;
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private bool spriteDefaultFacingLeft;

    //Dconds
    private int currCondIndex;
    private Dictionary<NPCConditionals, int> condToCurrDchainIndex;

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
    NPCWalkController walkController;

    private GameObject poofParticles;

    public NPCConditionals CurrCond => conds[currCondIndex];
    public STile CurrentSTileUnderneath => currentStileUnderneath;
    private bool DialogueEnabled => dialogueEnabledAllNPC && canGiveDialogue;
    private int CurrDchainIndex {
        get {
            return condToCurrDchainIndex[CurrCond];
        }

        set
        {
            condToCurrDchainIndex[CurrCond] = value;
        }
    }

    private void Awake()
    {
        canGiveDialogue = true;

        SetCondPrioritiesToArrayPos();

        InitializeCondControllers();
        InitializeCurrDchainIndices();
    }

    private void OnEnable()
    {
        walkController.OnEnable();
        PlayerAction.OnAction += OnPlayerAction;
        poofParticles = Resources.Load<GameObject>(poofParticleName);
    }

    private void OnDisable()
    {
        walkController.OnDisable();
        PlayerAction.OnAction -= OnPlayerAction;
    }

    void Update()
    {
        CheckAllConditionals();

        if (CanUpdateConditionals())
        {
            PollForNewConditional();
        }

        UpdateCondControllers();

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

    public void AddNewConditionals(NPCConditionals cond)
    {
        conds.Add(cond);
    }

    private void InitializeCondControllers()
    {
        walkController = new NPCWalkController(this, animator, sr, spriteDefaultFacingLeft);
    }

    private void UpdateCondControllers()
    {
        walkController.Update();
    }

    #region Dialogue
    public void DialogueTriggerEnter()
    {
        playerInDialogueTrigger = true;

        bool shouldStartTypingDialogue = !CurrDchainIsEmpty() && !NPCGivingDontInterruptDialogue();
        if (shouldStartTypingDialogue)
        {
            if (CurrCond.alwaysStartFromBeginning)
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
            dialogueDisplay.DisplaySentence(CurrCond.GetDialogueString(CurrDchainIndex));

            isTypingDialogue = true;
            dialogueBoxIsActive = true;

            CurrCond.OnDialogueChainStart(CurrDchainIndex);
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

    private void PollForNewConditional()
    {
        int maxPrioCond = GetCondIndexWithMaxPriority();
        bool condIsNew = currCondIndex != maxPrioCond;
        if (condIsNew)
        {
            ChangeCurrentConditional(maxPrioCond);

            if (dialogueBoxIsActive && playerInDialogueTrigger)
            {
                TypeCurrentDialogue();
            }
        }
    }

    private void ChangeCurrentConditional(int newDialogue)
    {
        currCondIndex = newDialogue;
        currDchainExhausted = false;

        dialogueDisplay.SetMessagePing(!CurrDchainIsEmpty());

        CurrCond.onConditionalEnter?.Invoke();
    }

    private void HandleDialogueFinished()
    {
        isTypingDialogue = false;
        CurrCond.OnDialogueChainEnd(CurrDchainIndex);

        if (CurrentDialogue().waitUntilPlayerAction)
        {
            waitingForPlayerAction = true;
        }
        else
        {
            float delay = CurrentDialogue().delayAfterFinishedTyping;
            delayBeforeNextDialogueCoroutine = StartCoroutine(SetNextDialogueInChainAfterDelay(delay));
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
        var dChain = CurrCond.dialogueChain;
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
            CurrCond.OnDialogueChainExhausted();
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

    private int GetCondIndexWithMaxPriority()
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

    private void SetCondPrioritiesToArrayPos()
    {
        for (int i = 0; i < conds.Count; i++)
        {
            conds[i].priority = i + 1;
        }
    }

    private void InitializeCurrDchainIndices()
    {
        condToCurrDchainIndex = new Dictionary<NPCConditionals, int>();
        foreach (var dcond in conds)
        {
            condToCurrDchainIndex[dcond] = 0;
        }
    }

    private void CheckAllConditionals()
    {
        foreach (NPCConditionals d in conds)
        {
            d.CheckConditions();
        }
    }

    private bool CanUpdateConditionals()
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

    private DialogueData CurrentDialogue()
    {
        if (CurrDchainIndex < 0 || CurrDchainIndex >= CurrCond.dialogueChain.Count)
        {
            Debug.LogError($"Attempted to Access Dialogue at invalid index: {CurrDchainIndex}");
            return null;
        }

        return CurrCond.dialogueChain[CurrDchainIndex];
    }

    private bool CurrDchainIsEmpty()
    {
        return CurrCond.dialogueChain.Count == 0;
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
    public void StartWalkAtIndex(int walkInd)
    {
        walkController.TryStartWalkAtIndex(walkInd);
    }

    public void StartValidWalk()
    {
        walkController.TryStartValidWalk();
    }
    #endregion
}