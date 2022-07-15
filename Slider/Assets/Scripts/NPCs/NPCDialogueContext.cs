using System.Collections;
using System.Collections.Generic;

using UnityEngine;

internal class NPCDialogueContext : MonoBehaviourContextProvider<NPC>
{
    public static bool dialogueEnabledAllNPC = true;

    private bool canGiveDialogue;
    private bool dialogueBoxIsActive;
    private bool currDchainExhausted;
    private bool playerInDialogueTrigger;
    private bool isTypingDialogue;
    private bool waitingForPlayerAction;
    private Coroutine delayBeforeNextDialogueCoroutine;

    private DialogueDisplay display;

    public bool DialogueEnabled => dialogueEnabledAllNPC && canGiveDialogue;

    private Dictionary<int, int> condIndexToCurrDchainIndex;

    private List<DialogueData> CurrDchain
    {
        get
        {
            if (context.CurrCond == null)
            {
                return null;
            }
            return context.CurrCond.dialogueChain;
        }
    }

    private int CurrDchainIndex
    {
        get
        {
            return condIndexToCurrDchainIndex[context.CurrCondIndex];
        }

        set
        {
            condIndexToCurrDchainIndex[context.CurrCondIndex] = value;
        }
    }

    public NPCDialogueContext(NPC context, DialogueDisplay display) : base(context)
    {
        this.context = context;
        this.display = display;
    }

    public override void Awake()
    {
        base.Awake();
        canGiveDialogue = true;
        InitializeCurrDchainIndices();
    }

    public override void OnEnable()
    {
        base.OnEnable();
        PlayerAction.OnAction += OnPlayerAction;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        PlayerAction.OnAction -= OnPlayerAction;
    }

    public override void Start()
    {
        base.Start();
        display.SetMessagePing(!CurrDchainIsEmpty());
    }

    public override void Update()
    {
        base.Update();

        bool finishedTypingCurrDialogue = isTypingDialogue && display.textTyperText.finishedTyping;
        if (finishedTypingCurrDialogue)
        {
            HandleDialogueFinished();
        }

        if (DialogueShouldDeactivate())
        {
            DeactivateDialogueBox();
        }
    }

    public void SetDialogueEnabled(bool value)
    {
        canGiveDialogue = value;
    }

    public void OnDialogueTriggerEnter()
    {
        playerInDialogueTrigger = true;

        bool shouldStartTypingDialogue = !CurrDchainIsEmpty() && !NPCGivingDontInterruptDialogue();
        if (shouldStartTypingDialogue)
        {
            if (context.CurrCond.alwaysStartFromBeginning)
            {
                CurrDchainIndex = 0;
                currDchainExhausted = false;
            }

            TypeCurrentDialogue();
        }
    }

    public void OnDialogueTriggerExit()
    {
        playerInDialogueTrigger = false;

        if (DialogueShouldDeactivate())
        {
            DeactivateDialogueBox();
        }
    }

    public void OnConditionalsChanged()
    {
        currDchainExhausted = false;
        display.SetMessagePing(!CurrDchainIsEmpty());
        if (dialogueBoxIsActive && playerInDialogueTrigger)
        {
            TypeCurrentDialogue();
        }
    }

    public void TypeCurrentDialogue()
    {
        if (DialogueEnabled && !CurrDchainIsEmpty())
        {
            display.DisplaySentence(context.CurrCond.GetDialogueString(CurrDchainIndex));

            isTypingDialogue = true;
            dialogueBoxIsActive = true;

            context.CurrCond.OnDialogueChainStart(CurrDchainIndex);
        }
    }

    public void OnPlayerAction(object sender, System.EventArgs e)
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

    public bool NPCGivingDontInterruptDialogue()
    {
        if (CurrDchainIsEmpty())
        {
            return false;
        }

        bool givingDialogue = dialogueBoxIsActive && !currDchainExhausted;
        return givingDialogue && CurrentDialogue().dontInterrupt;
    }

    private void InitializeCurrDchainIndices()
    {
        condIndexToCurrDchainIndex = new Dictionary<int, int>();
        for (int i = 0; i < context.Conds.Count; i++)
        {
            condIndexToCurrDchainIndex[i] = 0;
        }
    }

    private IEnumerator SetNextDialogueInChainAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SetNextDialogueInChain(true);
    }

    private void SetNextDialogueInChain(bool typeNextDialogue = false)
    {
        if (CurrDchainIndex < CurrDchain.Count - 1)
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
            context.CurrCond.OnDialogueChainExhausted();
        }
    }

    private void HandleDialogueFinished()
    {
        isTypingDialogue = false;
        context.CurrCond.OnDialogueChainEnd(CurrDchainIndex);

        if (CurrentDialogue().waitUntilPlayerAction)
        {
            waitingForPlayerAction = true;
        }
        else
        {
            float delay = CurrentDialogue().delayAfterFinishedTyping;
            delayBeforeNextDialogueCoroutine = context.StartCoroutine(SetNextDialogueInChainAfterDelay(delay));
        }
    }

    private void DeactivateDialogueBox()
    {
        display.FadeAwayDialogue();

        DontAllowDialogueToContinue();

        if (!CurrDchainIsEmpty() && CurrentDialogue().doNotRepeatAfterTriggered)
        {
            SetNextDialogueInChain();
        }

        dialogueBoxIsActive = false;
        isTypingDialogue = false;
    }

    private void DontAllowDialogueToContinue()
    {
        waitingForPlayerAction = false;

        if (delayBeforeNextDialogueCoroutine != null)
        {
            context.StopCoroutine(delayBeforeNextDialogueCoroutine);
            delayBeforeNextDialogueCoroutine = null;
        }
    }

    private void SkipText()
    {
        display.textTyperText.TrySkipText();
        display.textTyperBG.TrySkipText();
    }

    private bool DialogueShouldDeactivate()
    {
        return dialogueBoxIsActive && !playerInDialogueTrigger && !NPCGivingDontInterruptDialogue();
    }

    private DialogueData CurrentDialogue()
    {
        if (CurrDchain == null || CurrDchainIndex < 0 || CurrDchainIndex >= CurrDchain.Count)
        {
            Debug.LogError($"Attempted to Access Dialogue at invalid index: {CurrDchainIndex}");
            return null;
        }

        return CurrDchain[CurrDchainIndex];
    }

    private bool CurrDchainIsEmpty()
    {
        return CurrDchain == null || CurrDchain.Count == 0;
    }
}