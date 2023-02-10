using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class NPC : MonoBehaviourContextSubscriber<NPC>
{
    private readonly string poofParticleName = "SmokePoof Variant";

    [SerializeField] private string characterName;
    [SerializeField] private List<NPCConditionals> conds;
    public float speed;

    public NPCEmotes emoteController;// { get; private set; }
    public Animator animator { 
        get => npcAnimatorController.myAnimator; 
        private set => npcAnimatorController.myAnimator = value;
    }
    public NPCAnimatorController npcAnimatorController;
    [SerializeField] private DialogueDisplay dialogueDisplay;
    [SerializeField] private SpriteRenderer sr;
    [Tooltip("This is for NPC walks")]
    [SerializeField] private bool spriteDefaultFacingLeft;

    private int currCondIndex;
    private NPCDialogueContext dialogueCtx;
    private NPCWalkingContext walkingCtx;
    private STile currentStileUnderneath;
    private GameObject poofParticles;

    // For editor
    [HideInInspector] public bool autoSetWaitUntilPlayerAction = true;

    public List<NPCConditionals> Conds => conds;
    public int CurrCondIndex => currCondIndex;
    public NPCConditionals CurrCond {
        get {
            if (conds.Count == 0) {
                return null;
            }
            return conds[currCondIndex];
        }
    }
    public STile CurrentSTileUnderneath => currentStileUnderneath;

    private new void Awake()
    {
        base.Awake();
        SetCondPrioritiesToArrayPos();
    }

    private new void Start() 
    {
        base.Start();
        
        npcAnimatorController.Play(CurrCond.animationOnEnter);
        emoteController.SetEmote(CurrCond.emoteOnEnter);
        CurrCond.onConditionalEnter?.Invoke();
    }

    private new void OnEnable()
    {
        base.OnEnable();
        poofParticles = Resources.Load<GameObject>(poofParticleName);
    }

    private new void Update()
    {
        base.Update();
        CheckAllConditionals();

        if (CanUpdateConditionals())
        {
            PollForNewConditional();
        }
    }

    private void FixedUpdate()
    {
        currentStileUnderneath = SGrid.GetSTileUnderneath(transform, currentStileUnderneath);
    }

    public void AddNewConditionals(NPCConditionals cond)
    {
        conds.Add(cond);
    }

    public void ModifyConditional(NPCConditionals cond, int index)
    {
        Debug.Log(conds.Count);
        if (index < 0 || index >= conds.Count) throw new ArgumentOutOfRangeException("Index " + index + " is out of range of conds list!");
        conds[index] = cond;
    }

    

    //These are all interfaces to the various contexts to be used in inspector events and such. Implementation details are in NPCDialogueContext/NPCWalkingCOntext
    #region Dialogue
    public void OnDialogueTriggerEnter()
    {
        dialogueCtx.OnDialogueTriggerEnter();
    }

    public void OnDialogueTriggerExit()
    {
        dialogueCtx.OnDialogueTriggerExit();
    }

    public void TypeCurrentDialogue()
    {
        dialogueCtx.TypeCurrentDialogue();
    }

    public void AdvanceDialogueChain()
    {
        dialogueCtx.TypeNextDialogueInChain();
    }
    #endregion Dialogue

    #region Walking
    public void StartWalkAtIndex(int walkInd)
    {
        walkingCtx.TryStartWalkAtIndex(walkInd);
    }

    public void StartValidWalk()
    {
        walkingCtx.TryStartValidWalk();
    }
    #endregion

    #region Teleportation

    public void Teleport(Transform trans,bool poof=false)
    {
        if (transform.position != trans.position)
        {
            if(poof)
                Instantiate(poofParticles, transform.position, Quaternion.identity);
            transform.position = trans.position;
            transform.parent = trans;
        }
    }
    #endregion

    #region MonoBehaviourContextSubscriber
    protected override void InitializeContexts()
    {
        dialogueCtx = new NPCDialogueContext(this, dialogueDisplay);
        walkingCtx = new NPCWalkingContext(this, animator, sr, spriteDefaultFacingLeft);

        RegisterProvider(dialogueCtx);
        RegisterProvider(walkingCtx);
    }
    #endregion

    private void PollForNewConditional()
    {
        int maxPrioIndex = GetCondIndexWithMaxPriority();
        if (maxPrioIndex == -1)
        {
            Debug.LogError("No suitable dialogue can be displayed!");
            return;
        }

        bool condIsNew = currCondIndex != maxPrioIndex;
        if (condIsNew)
        {
            ChangeCurrentConditional(maxPrioIndex);
        }
    }

    private void ChangeCurrentConditional(int newCond)
    {
        currCondIndex = newCond;

        npcAnimatorController.Play(CurrCond.animationOnEnter);
        emoteController.SetEmote(CurrCond.emoteOnEnter);
        CurrCond.onConditionalEnter?.Invoke();
        
        dialogueCtx.OnConditionalsChanged();
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
        return maxPrioIndex;
    }

    private void SetCondPrioritiesToArrayPos()
    {
        for (int i = 0; i < conds.Count; i++)
        {
            conds[i].priority = i + 1;
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
        return dialogueCtx.DialogueEnabled && !dialogueCtx.NPCGivingDontInterruptDialogue();
    }
}