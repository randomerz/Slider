using System.Collections;
using System.Collections.Generic;

using UnityEngine;

internal class NPCDialogueContext : MonoBehaviourContextProvider<NPC>
{
    public class DialogueEventFlags
    {
        public bool startInvoked;
        public bool endInvoked;

        public DialogueEventFlags(bool start, bool end)
        {
            startInvoked = start;
            endInvoked = end;
        }
    }

    public static bool dialogueEnabledAllNPC = true;

    //L: This should be a state machine now that I think about it.
    private bool canGiveDialogue;
    private bool dialogueBoxIsActive;
    private bool currDchainExhausted;
    private bool playerInDialogueTrigger;
    private bool isTypingDialogue;
    private bool waitingForPlayerAction;

    private Coroutine delayBeforeNextDialogueCoroutine;

    private DialogueDisplay display;

    public bool DialogueEnabled => dialogueEnabledAllNPC && canGiveDialogue;

    private Dictionary<int, int> cachedDchainIndices;    //(CondIndex, CurrDchainIndex)

    private Dictionary<int, Dictionary<int, DialogueEventFlags>> cachedEventFlags; //(CondIndex, DchainIndex, flags)

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
            if (!cachedDchainIndices.ContainsKey(context.CurrCondIndex))
            {
                cachedDchainIndices[context.CurrCondIndex] = 0;
            }
            return cachedDchainIndices[context.CurrCondIndex];
        }

        set
        {
            cachedDchainIndices[context.CurrCondIndex] = value;
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
        cachedDchainIndices = new Dictionary<int, int>();
        cachedEventFlags = new Dictionary<int, Dictionary<int, DialogueEventFlags>>();
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
        display.SetMessagePing(!CurrDchainIsEmpty());
        if (dialogueBoxIsActive && playerInDialogueTrigger)
        {
            if (context.CurrCond.alwaysStartFromBeginning)
            {
                CurrDchainIndex = 0;
                cachedDchainIndices[context.CurrCondIndex] = 0;
            }

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

            OnDialogueStart();
        }
    }

    public void OnPlayerAction(object sender, System.EventArgs e)
    {
        if (dialogueBoxIsActive)
        {
            if (waitingForPlayerAction)
            {
                waitingForPlayerAction = false;
                TypeNextDialogueInChain();
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

        return dialogueBoxIsActive && CurrentDialogue().dontInterrupt;
    }

    public void TypeNextDialogueInChain()
    {
        if (CurrDchainIndex < CurrDchain.Count - 1)
        {
            CurrDchainIndex++;
            TypeCurrentDialogue();
        }
        else   
        {
            //Dialogue Chain Ended
            if (CurrDchainIndex == CurrDchain.Count - 1 && CurrDchain[CurrDchainIndex].dontInterrupt)
            {
                //This is so the dialogue automatically ends after the delay or whatever for don't interrupt's so that it doesn't just stay there until the end of time. 
                DeactivateDialogueBox();
            }
            context.CurrCond.OnDialogueChainExhausted();
        }
    }

    private IEnumerator SetNextDialogueInChainAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        TypeNextDialogueInChain();
    }

    private void HandleDialogueFinished()
    {
        isTypingDialogue = false;

        if (!CurrDchainIsEmpty())
        {
            if (CurrentDialogue().waitUntilPlayerAction)
            {
                waitingForPlayerAction = true;
            }
            else if (!CurrentDialogue().advanceDialogueManually && CurrentDialogue().delayAfterFinishedTyping != 0)
            {
                float delay = CurrentDialogue().delayAfterFinishedTyping;
                delayBeforeNextDialogueCoroutine = context.StartCoroutine(SetNextDialogueInChainAfterDelay(delay));
            }
            OnDialogueEnd();
        }
    }

    private void DeactivateDialogueBox()
    {
        display.FadeAwayDialogue();

        DontAllowDialogueToContinue();

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

    private void OnDialogueStart()
    {
        //Handle event caching (only cache doNotRepeatEvents)
        if (!CurrDchainIsEmpty() && CurrDchain[CurrDchainIndex].doNotRepeatEvents)
        {
            if (CheckEventInCacheAndUpdate(true, false))
            {
                return;
            }
        }

        context.CurrCond.OnDialogueChainStart(CurrDchainIndex);
    }

    private void OnDialogueEnd()
    {
        //Handle event caching (only cache doNotRepeatEvents)
        if (!CurrDchainIsEmpty() && CurrDchain[CurrDchainIndex].doNotRepeatEvents)
        {
            if (CheckEventInCacheAndUpdate(false, true))
            {
                return;
            }
        }

        context.CurrCond.OnDialogueChainEnd(CurrDchainIndex);
    }

    //Returns if the cache already had the value
    private bool CheckEventInCacheAndUpdate(bool startInvoked, bool endInvoked)
    {
        //L: Dictionary nested if logic pain.
        if (CachedFlagsContainsCurrDialogue())
        {
            bool startAlreadyIn = startInvoked && cachedEventFlags[context.CurrCondIndex][CurrDchainIndex].startInvoked;
            bool endAlreadyIn = endInvoked && cachedEventFlags[context.CurrCondIndex][CurrDchainIndex].endInvoked;
            if (startAlreadyIn || endAlreadyIn)
            {
                return true;
            }
            cachedEventFlags[context.CurrCondIndex][CurrDchainIndex].startInvoked = startInvoked || cachedEventFlags[context.CurrCondIndex][CurrDchainIndex].startInvoked;
            cachedEventFlags[context.CurrCondIndex][CurrDchainIndex].endInvoked = endInvoked || cachedEventFlags[context.CurrCondIndex][CurrDchainIndex].endInvoked;
        }
        else
        {
            if (!cachedEventFlags.ContainsKey(context.CurrCondIndex))
            {
                cachedEventFlags[context.CurrCondIndex] = new Dictionary<int, DialogueEventFlags>();
            }
            cachedEventFlags[context.CurrCondIndex][CurrDchainIndex] = new DialogueEventFlags(startInvoked, endInvoked);
        }

        return false;
    }

    private bool CachedFlagsContainsCurrDialogue()
    {
        return cachedEventFlags.ContainsKey(context.CurrCondIndex) && cachedEventFlags[context.CurrCondIndex].ContainsKey(CurrDchainIndex);
    }

    private bool DialogueShouldDeactivate()
    {
        return dialogueBoxIsActive && !playerInDialogueTrigger && !NPCGivingDontInterruptDialogue();
    }

    private DialogueData CurrentDialogue()
    {
        if (CurrDchain == null)
        {
            Debug.Log("CuurDchain is null");
            return null;
        }
        if (CurrDchainIndex < 0 || CurrDchainIndex >= CurrDchain.Count)
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