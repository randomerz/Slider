using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JungleDialogueCutscenes : MonoBehaviour
{
    private const float DEFAULT_TIME_BETWEEN_DIALOGUES = 2.25f;
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
    }

    public void DoBarronIntroCutscene()
    {
        if (currentCoroutine == null)
        {
            currentCoroutine = StartCoroutine(BarronIntroCutscene());
        }
    }

    private IEnumerator BarronIntroCutscene()
    {
        Log("- Justin! How's the R&D on production?");
        SaveSystem.Current.SetBool("JungleBarronIntro1", true);

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

    public void Log(string message)
    {
        if (debugMode)
        {
            Debug.Log(message);
        }
    }
}
