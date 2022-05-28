using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : MonoBehaviour
{
    public string characterName;
    public List<DialogueConditionals> dconds;

    [SerializeField] private DialogueDisplay dialogueDisplay;

    //indices to get the right dialogue
    private int currDconds;
    private int currDialogueInChain;

    private bool dialogueEnabled;
    private bool startedTyping;
    private bool waitingForPlayerContinue;


    private STile currentStileUnderneath;
    private WorldNavAgent nav;

    private Coroutine waitNextDialogueCoroutine;

    private void Awake()
    {
        nav = GetComponent<WorldNavAgent>();
        dialogueEnabled = true;
        startedTyping = false;
        waitNextDialogueCoroutine = null;
    }

    private void OnEnable()
    {
        PlayerAction.OnAction += OnPlayerAction;
    }

    private void OnDisable()
    {
        PlayerAction.OnAction -= OnPlayerAction;
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
            currDconds = newDialogue;
            currDialogueInChain = 0;
            dialogueDisplay.NewMessagePing();
            dconds[currDconds].onDialogueChanged?.Invoke();
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

        if (currentStileUnderneath != null)
        {
            transform.SetParent(currentStileUnderneath.transform);
        }
        else
        {
            transform.SetParent(null);
        }
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

    public void TriggerDialogue()
    {
       if (dialogueEnabled)
       {
            if (dconds[currDconds].dialogueChain.Count == 0)
            {
                dconds[currDconds].OnDialogueStart();
                dialogueDisplay.DisplaySentence(dconds[currDconds].GetDialogue());
            } else
            {
                dconds[currDconds].OnDialogueChainStart(currDialogueInChain);
                dialogueDisplay.DisplaySentence(dconds[currDconds].GetDialogueChain(currDialogueInChain));
            }

            startedTyping = true;
       }
    }

    public void FinishDialogue()
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

    public void FadeDialogue()
    {
        var dChain = dconds[currDconds].dialogueChain;
        if (dChain.Count > 0 && dChain[currDialogueInChain].dontInterrupt)
        {
            //Dialogue keeps playing even if the player exits
            return;
        }

        dialogueDisplay.FadeAwayDialogue();

        //Don't allow player to continue conversation.
        if (waitNextDialogueCoroutine != null)
        {
            waitingForPlayerContinue = false;
            StopCoroutine(waitNextDialogueCoroutine);
            waitNextDialogueCoroutine = null;
        }

        //Dialogue that doesn't repeat should be skipped now.
        if (dChain.Count > 0)
        {
            if (dChain[currDialogueInChain].doNotRepeatAfterTriggered)
            {
                SetNextDialogueInChain();
            }
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

    private void OnPlayerAction(object sender, System.EventArgs e)
    {
        /*
        if (waitingForPlayerContinue)
        {
            SetNextDialogueInChain(true);
            waitingForPlayerContinue = false;
        } else if (startedTyping && !dialogueDisplay.textTyperText.finishedTyping)
        {
            dialogueDisplay.textTyperText.TrySkipText();
            dialogueDisplay.textTyperBG.TrySkipText();
        }
        */
    }

    private void SetNextDialogueInChain(bool triggerNext = false)   //args are useless, this is just so it can be called by player action.
    {
        //The dialogue will just chill on the last line if it's already been exhausted (could maybe customize to repeat the last line or start from the beginning).
        if (currDialogueInChain < dconds[currDconds].dialogueChain.Count - 1)
        {
            currDialogueInChain++;
            if (triggerNext)
            {
                TriggerDialogue();
            }
        } else
        {
            dconds[currDconds].OnDialogueChainExhausted();
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

    public void Teleport(Transform trans)
    {
        transform.position = trans.position;
        transform.parent = trans.parent;
    }

    public void WalkTo(Transform trans)
    {
        //NPCs can't talkie while they walkie (under normal circumstances)
        dialogueEnabled = false;
        nav.SetDestination(TileUtil.WorldToTileCoords(trans.position), null, (pos) =>
        {
            dialogueEnabled = true;
        });
    }
}