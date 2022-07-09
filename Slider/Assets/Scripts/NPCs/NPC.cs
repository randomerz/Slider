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

    private int currCondIndex;
    private NPCDialogueContext dialogueCtx;
    private NPCWalkingContext walkingCtx;
    private STile currentStileUnderneath;
    private GameObject poofParticles;

    public List<NPCConditionals> Conds => conds;
    public int CurrCondIndex => currCondIndex;
    public NPCConditionals CurrCond => conds[currCondIndex];
    public STile CurrentSTileUnderneath => currentStileUnderneath;

    private void Awake()
    {
        SetCondPrioritiesToArrayPos();
        InitializeContexts();

        dialogueCtx.Awake();
        walkingCtx.Awake();
    }

    private void OnEnable()
    {
        poofParticles = Resources.Load<GameObject>(poofParticleName);

        dialogueCtx.OnEnable();
        walkingCtx.OnEnable();
    }

    private void OnDisable()
    {
        dialogueCtx.OnDisable();
        walkingCtx.OnDisable();
    }

    private void Start()
    {
        dialogueCtx.Start();
        walkingCtx.Start();
    }

    private void Update()
    {
        CheckAllConditionals();

        if (CanUpdateConditionals())
        {
            PollForNewConditional();
        }

        UpdateCondControllers();

        dialogueCtx.Update();
        walkingCtx.Update();
    }

    private void FixedUpdate()
    {
        currentStileUnderneath = STile.GetSTileUnderneath(transform, currentStileUnderneath);
    }

    public void AddNewConditionals(NPCConditionals cond)
    {
        conds.Add(cond);
    }

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
    public void Teleport(Transform trans)
    {
        if (transform.position != trans.position)
        {
            Instantiate(poofParticles, transform.position, Quaternion.identity);
            transform.position = trans.position;
            transform.parent = trans.parent;
        }
    }
    #endregion

    private void InitializeContexts()
    {
        dialogueCtx = new NPCDialogueContext(this, dialogueDisplay);
        walkingCtx = new NPCWalkingContext(this, animator, sr, spriteDefaultFacingLeft);
    }

    private void UpdateCondControllers()
    {
        walkingCtx.Update();
    }

    private void PollForNewConditional()
    {
        int maxPrioCond = GetCondIndexWithMaxPriority();
        bool condIsNew = currCondIndex != maxPrioCond;
        if (condIsNew)
        {
            ChangeCurrentConditional(maxPrioCond);

            dialogueCtx.OnConditionalsChanged();
        }
    }

    private void ChangeCurrentConditional(int newCond)
    {
        currCondIndex = newCond;
        CurrCond.onConditionalEnter?.Invoke();
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