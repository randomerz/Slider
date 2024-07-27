using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class SimpleInteractableCutscene : MonoBehaviour, IInteractable
{
    [Tooltip("Optional Flag to set when cutscene starts")]
    [SerializeField] private string cutsceneStartedFlag;
    [Tooltip("Optional Flag to set when cutscene finishes")]
    [SerializeField] private string cutsceneFinishedFlag;
    [Tooltip("Array of all the characters in the cutscene")]
    [SerializeField] protected NPC[] cutsceneCharacters;
    [SerializeField] protected bool stillResolveCutsceneIfNotFinished = true;

    private const float DEFAULT_TIME_BETWEEN_DIALOGUE_LINES = 1.3f;

    private NPC currentlyTalkingCharacter;
    private bool currentlyTyping = false;
    private bool currentlyWaitingAfterTyped = false;
    private bool skipWaitingAfterTyped = false;
    private bool currentDialogueAllowedToSkip = false;
    private bool playerInTrigger = false;
    private bool hasInitialized;

    public bool cutsceneStarted { get; private set; } = false;
    public bool cutsceneFinished { get; private set; } = false;

    protected virtual void Start()
    {
        hasInitialized = true;

        if (string.IsNullOrEmpty(cutsceneStartedFlag) || string.IsNullOrEmpty(cutsceneFinishedFlag))
        {
            Debug.LogWarning($"Cutscene Started or Finished flag was not set for {name}.");
        }

        if (!string.IsNullOrEmpty(cutsceneStartedFlag) && SaveSystem.Current.GetBool(cutsceneStartedFlag))
        {
            cutsceneStarted = true;
        }
        if (!string.IsNullOrEmpty(cutsceneFinishedFlag) && SaveSystem.Current.GetBool(cutsceneFinishedFlag))
        {
            OnCutSceneFinish();
        }
        else
        {
            if (stillResolveCutsceneIfNotFinished && cutsceneStarted)
            {
                OnCutsceneNotFinished();
                OnCutSceneFinish();
            }
            if (ShouldCutsceneBeSkipped())
            {
                OnCutSceneFinish();
            }
        }
    }

    // Called the next time in Start if cutscene was not finished
    protected virtual void OnCutsceneNotFinished()
    {
        if (!string.IsNullOrEmpty(cutsceneFinishedFlag))
        {
            SaveSystem.Current.SetBool(cutsceneFinishedFlag, true);
        }
    }

    protected virtual bool ShouldCutsceneBeSkipped()
    {
        // For example, if the cutscene is on Slider 1 and you get Slider 2, skip it.
        return false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag != "Player")
        {
            return;
        }

        // In case the player spawns in the cutscene trigger
        if (!hasInitialized)
        {
            CoroutineUtils.ExecuteAfterEndOfFrame(() => CheckOnTriggerEnter(), this);
        }
        else
        {
            CheckOnTriggerEnter();
        }
    }

    private void CheckOnTriggerEnter()
    {
        if (ShouldCutsceneBeSkipped())
        {
            OnCutSceneFinish();
            return;
        }

        playerInTrigger = true;

        if (!cutsceneFinished)
        {
            AllowPlayerInteraction(true);
        }

        if (!cutsceneStarted)
        {
            StartCutScene();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag != "Player")
        {
            return;
        }

        playerInTrigger = false;

        if (!cutsceneFinished)
        {
            AllowPlayerInteraction(false);
        }
    }

    private void AllowPlayerInteraction(bool allowed)
    {
        if (allowed)
        {
            Player.GetPlayerAction().AddInteractable(this);
        }
        else
        {
            Player.GetPlayerAction().RemoveInteractable(this);
        }
    }

    public bool Interact()
    {
        TrySkipCurrentDialogue();

        return true;
    }

    private void TrySkipCurrentDialogue()
    {
        if (currentlyTalkingCharacter != null && currentDialogueAllowedToSkip)
        {
            if (currentlyTyping)
            {
                currentlyTalkingCharacter.SkipText();
            }
            else if (currentlyWaitingAfterTyped)
            {
                skipWaitingAfterTyped = true;
            }
        }
    }

    private void StartCutScene()
    {
        StartCoroutine(StartCutSceneCoroutine());
    }

    private IEnumerator StartCutSceneCoroutine()
    {
        EnableAllNormalCharacterDialogueTriggers(false);

        cutsceneStarted = true;

        if (cutsceneStartedFlag != null)
        {
            SaveSystem.Current.SetBool(cutsceneStartedFlag, true);
        }

        foreach (NPC character in cutsceneCharacters)
        {
            if (character.IsDialogueBoxActive())
            {
                character.DeactivateDialogueBox();
            }
        }

        yield return null; // Give time for conditionals to update

        foreach (NPC character in cutsceneCharacters)
        {
            character.PollForNewConditional();
        }

        yield return CutScene();
        
        yield return null; // Give time for conditionals to update

        foreach (NPC character in cutsceneCharacters)
        {
            character.PollForNewConditional();
        }

        if (playerInTrigger)
        {
            AllowPlayerInteraction(false);
        }

        cutsceneFinished = true;

        if (cutsceneFinishedFlag != null)
        {
            SaveSystem.Current.SetBool(cutsceneFinishedFlag, true);
        }
        // //we want a bit of a gap after cutscene is done before you can start talking to them like normal, otherwise it's weird
        yield return new WaitForSeconds(0.1f);

        EnableAllNormalCharacterDialogueTriggers(true);
    }

    private void EnableAllNormalCharacterDialogueTriggers(bool enable)
    {
        foreach (NPC character in cutsceneCharacters)
        {
            EnableNormalCharacterDialogueTrigger(character, enable);
        }
    }

    private void EnableNormalCharacterDialogueTrigger(NPC character, bool enable)
    {
        OnTriggerEnter triggerEnterScript = character.GetComponent<OnTriggerEnter>();
        if (triggerEnterScript != null)
        {
            triggerEnterScript.SetOnPlayerEnterActive(enable);
        }
    }

    protected virtual IEnumerator CutScene()
    {
        yield return null;
        OnCutSceneFinish();
    }

    // Enable/disable gameobjects, colliders, etc.
    // This way we can call this when you load in from a save.
    protected virtual void OnCutSceneFinish()
    {
        cutsceneFinished = true;
    }
   
    protected IEnumerator SayNextDialogue(NPC character, bool skippable = true, float timeWaitAfterFinishedTyping = DEFAULT_TIME_BETWEEN_DIALOGUE_LINES)
    {
        character.TypeCurrentDialogue();
        currentlyTalkingCharacter = character;
        currentlyTyping = true;
        currentDialogueAllowedToSkip = skippable;

        yield return new WaitWhile(() => character.IsTypingDialogue());

        currentlyTyping = false;
        currentlyWaitingAfterTyped = true;

        float startTime = Time.time;
        float currentTime = Time.time;
        while ((currentTime - startTime < timeWaitAfterFinishedTyping) && !skipWaitingAfterTyped)
        {
            yield return null;
            currentTime = Time.time;
        }

        currentlyWaitingAfterTyped = false;
        currentDialogueAllowedToSkip = false;
        //yield return new WaitForSeconds(timeWaitAfterFinishedTyping);

        character.AdvanceDialogueChain(); //moves dialogue to next part in chain, considering this line to be said
        character.DeactivateDialogueBox(); //makes the current dialogue disappear from screen

        currentlyTalkingCharacter = null;
        skipWaitingAfterTyped = false;
    }
}
