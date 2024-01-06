using System;
using System.Collections;
using UnityEngine;


public class JungleRecipeBookHints : MonoBehaviour 
{
    private Coroutine hintCoroutine;

    public NPC hintNPC;
    public Transform hintPosition;
    
    private const float HINT_DELAY = 4;
    private const string JUNGLE_UI_HINT_TEXT = "jungleUIHintText";

    private const string HINT_CONTROLS = "You can move the screen left/right to view different recipes.";
    private readonly string[] HINTS_GENERAL = {
        "The hints will fill up as you create more shapes.",
        "The business stuff? Oh don't worry about it, it's all made up anyway.",
        "Hope this helps!",
        "The triple-merges can be a bit tricky.",
    };
    private bool hasScreenChanged;
    private bool hasControlsHintBeenUsed;
    private bool[] hasGeneralHintBeenUsed;
    
    private void Awake() 
    {
        hasGeneralHintBeenUsed = new bool[HINTS_GENERAL.Length];
    }

    private void OnEnable() 
    {
        JungleRecipeBookUI.onScreenChange += UpdateHasScreenChanged;
    }

    private void OnDisable() 
    {
        JungleRecipeBookUI.onScreenChange -= UpdateHasScreenChanged;
    }

    private void UpdateHasScreenChanged(object sender, EventArgs e) => hasScreenChanged = true;

    public void StartHintRoutine()
    {
        if (hintNPC.transform.position != hintPosition.position)
        {
            return;
        }
        
        hintCoroutine = StartCoroutine(HintRoutine());
    }

    public void StopHintRoutine()
    {
        if (hintCoroutine != null)
        {
            StopCoroutine(hintCoroutine);
        }

        // maybe write something for hintNPC.dialogueCtx.DeactivateDialogueBox();
        hintCoroutine = null;
    }

    private IEnumerator HintRoutine()
    {
        // This method should be split but whatever
        if (!hasControlsHintBeenUsed)
        {
            for (int i = 0; i < HINT_DELAY; i++)
            {
                yield return new WaitForSeconds(1);

                if (hasScreenChanged)
                {
                    hasControlsHintBeenUsed = true;
                    StopHintRoutine();
                    StartHintRoutine();
                }
            }
            
            DisplayAndTriggerDialogue(HINT_CONTROLS);
            hasControlsHintBeenUsed = true;
            hintCoroutine = null;
            yield break;
        }

        // General hints
        int hintIndex = UnityEngine.Random.Range(0, HINTS_GENERAL.Length);
        if (hasGeneralHintBeenUsed[hintIndex])
        {
            hintCoroutine = null;
            yield break;
        }

        yield return new WaitForSeconds(HINT_DELAY);

        DisplayAndTriggerDialogue(HINTS_GENERAL[hintIndex]);
        hasGeneralHintBeenUsed[hintIndex] = true;
        hintCoroutine = null;
    }

    private void DisplayAndTriggerDialogue(string message) 
    {
        if (SaveSystem.Current.GetString(JUNGLE_UI_HINT_TEXT) == message)
        {
            return;
        }

        SaveSystem.Current.SetString(JUNGLE_UI_HINT_TEXT, message);
        hintNPC.TypeCurrentDialogue();
    }
    
    public void AllRecipesCompleted(Condition c) => 
        c.SetSpec(JungleRecipeBookSave.AllRecipesCompleted());
}