using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JungleDialogueCutscenes : MonoBehaviour
{
    // Warning: if you make this value lower, make sure to also decrease
    // the time between each dcond. It should be less than this value.
    private const float DEFAULT_TIME_BETWEEN_DIALOGUES = 2.1f;
    [SerializeField] private bool debugMode = false;

    public NPC barron;
    public NPC justin;
    public NPC joe;

    private Coroutine currentCoroutine;


    private void OnEnable()
    {
        Player.OnHousingChanged += CheckForBarronUpdates;
    }

    private void OnDisable()
    {
        Player.OnHousingChanged -= CheckForBarronUpdates;
    }

    private void CheckForBarronUpdates(object sender, Player.HousingChangeArgs e)
    {
        // We don't care when the player goes into the house
        if (e.newIsInHouse)
        {
            return;
        }

        if (SaveSystem.Current.GetBool("JungleBarronIntro3"))
        {
            SaveSystem.Current.SetBool("JungleBarronIntro4", true);
        }

        if (SaveSystem.Current.GetBool("JungleStandUp2"))
        {
            SaveSystem.Current.SetBool("JungleStandUp3", true);
        }

        if (PlayerInventory.Contains("Slider 5", Area.Jungle))
        {
            SaveSystem.Current.SetBool("JungleRaceIsHappening", true);
        }
    }

    [System.Obsolete]
    public void DoBarronIntroCutscene()
    {
        if (currentCoroutine == null)
        {
            currentCoroutine = StartCoroutine(BarronIntroCutscene());
        }
    }

    [System.Obsolete]
    private IEnumerator BarronIntroCutscene()
    {
        Log("- Justin! How's the R&D on production?");
        SaveSystem.Current.SetBool("JungleBarronIntro1", true);

        if (barron.IsDialogueBoxActive())
        {
            barron.DeactivateDialogueBox();
        }
        if (justin.IsDialogueBoxActive())
        {
            justin.DeactivateDialogueBox();
        }

        yield return null;

        barron.TypeCurrentDialogue();

        yield return new WaitWhile(() => barron.IsTypingDialogue());
        
        // awkward timing because of mysterious bug
        justin.TypeCurrentDialogue();

        yield return new WaitForSeconds(DEFAULT_TIME_BETWEEN_DIALOGUES);

        Log("> Great!!! It's ready for you to take a look, boss!");
        justin.AdvanceDialogueChain();

        yield return new WaitWhile(() => justin.IsTypingDialogue());
        yield return new WaitForSeconds(DEFAULT_TIME_BETWEEN_DIALOGUES);

        Log("- Astounding, as always. I can always count on you.");
        barron.AdvanceDialogueChain();

        yield return new WaitWhile(() => barron.IsTypingDialogue());
        yield return new WaitForSeconds(DEFAULT_TIME_BETWEEN_DIALOGUES);

        Log("> No problem boss! I'll have it ready for you whenever!");
        justin.AdvanceDialogueChain();

        yield return new WaitWhile(() => justin.IsTypingDialogue());
        yield return new WaitForSeconds(DEFAULT_TIME_BETWEEN_DIALOGUES);

        Log("He walks away to the research screen and Barron turns to player");
        SaveSystem.Current.SetBool("JungleBarronIntro2", true);
        barron.AdvanceDialogueChain();

        currentCoroutine = null;
    }

    [System.Obsolete]
    public void DoStandUpCutscene()
    {
        if (currentCoroutine == null)
        {
            currentCoroutine = StartCoroutine(StandUpCutscene());
        }
    }

    [System.Obsolete]
    private IEnumerator StandUpCutscene()
    {
        SaveSystem.Current.SetBool("JungleStandUp1", true);

        if (barron.IsDialogueBoxActive())
        {
            barron.DeactivateDialogueBox();
        }
        if (justin.IsDialogueBoxActive())
        {
            justin.DeactivateDialogueBox();
        }
        if (joe.IsDialogueBoxActive())
        {
            joe.DeactivateDialogueBox();
        }

        yield return null;
        yield return null;

        Log("- Alright, let's give it 5 minutes for everyone to get here");
        barron.TypeCurrentDialogue();
        justin.TypeCurrentDialogue();
        joe.TypeCurrentDialogue();

        yield return new WaitWhile(() => barron.IsTypingDialogue());
        yield return new WaitForSeconds(DEFAULT_TIME_BETWEEN_DIALOGUES);
        
        Log("- .........");
        
        yield return new WaitWhile(() => barron.IsTypingDialogue());
        yield return new WaitForSeconds(DEFAULT_TIME_BETWEEN_DIALOGUES);
        
        Log("- Okay well let's just get started then. Justin?");
        
        yield return new WaitWhile(() => barron.IsTypingDialogue());
        yield return new WaitForSeconds(DEFAULT_TIME_BETWEEN_DIALOGUES);
        
        Log("> I finished finalizing R&D, ready for you to take a look boss!");
        justin.AdvanceDialogueChain();
        
        yield return new WaitWhile(() => justin.IsTypingDialogue());
        yield return new WaitForSeconds(DEFAULT_TIME_BETWEEN_DIALOGUES);
        barron.AdvanceDialogueChain();
        
        Log("- Great, I'll definetely come by when I have time.");
        
        yield return new WaitWhile(() => barron.IsTypingDialogue());
        yield return new WaitForSeconds(DEFAULT_TIME_BETWEEN_DIALOGUES);
        
        Log("- Joe, how about you?");
        
        yield return new WaitWhile(() => barron.IsTypingDialogue());
        yield return new WaitForSeconds(DEFAULT_TIME_BETWEEN_DIALOGUES);
        
        Log("> Oh uh... I've got my sticks sending up.");
        joe.AdvanceDialogueChain();
        
        yield return new WaitWhile(() => joe.IsTypingDialogue());
        yield return new WaitForSeconds(DEFAULT_TIME_BETWEEN_DIALOGUES);
        
        Log("> I've also been having some trouble with my J*ra cards so I had to sort that out.");
        
        yield return new WaitWhile(() => joe.IsTypingDialogue());
        yield return new WaitForSeconds(DEFAULT_TIME_BETWEEN_DIALOGUES);
        
        Log("- Sounds good, don't hesitate to ask around for help!");
        barron.AdvanceDialogueChain();
        
        yield return new WaitWhile(() => barron.IsTypingDialogue());
        yield return new WaitForSeconds(DEFAULT_TIME_BETWEEN_DIALOGUES);
        
        Log("- Well it looks like everyone else is busy. Before we wrap up though,");
        
        yield return new WaitWhile(() => barron.IsTypingDialogue());
        yield return new WaitForSeconds(DEFAULT_TIME_BETWEEN_DIALOGUES);
        
        Log("- Would everyone please welcome the new intern!");
        
        yield return new WaitWhile(() => barron.IsTypingDialogue());

        yield return new WaitForSeconds(DEFAULT_TIME_BETWEEN_DIALOGUES / 2);
        SaveSystem.Current.SetBool("JungleStandUp2", true);
        justin.AdvanceDialogueChain();
        joe.AdvanceDialogueChain();

        yield return null;

        Log("> Welcome");
        justin.TypeCurrentDialogue();

        yield return new WaitForSeconds(0.25f);

        Log("> Welcome");
        joe.TypeCurrentDialogue();

        currentCoroutine = null;
    }

    public void Log(string message)
    {
        if (debugMode)
        {
            Debug.Log(message);
        }
    }
}
