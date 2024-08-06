using System.Collections;
using UnityEngine;

public class JungleIntroCutscene : SimpleInteractableCutscene
{
    private NPC barron;
    private NPC justin;

    protected override void Start()
    {
        base.Start();

        barron = cutsceneCharacters[0];
        justin = cutsceneCharacters[1];
    }

    protected override bool ShouldCutsceneBeSkipped()
    {
        return PlayerInventory.Contains("Slider 4", Area.Jungle);
    }

    protected override IEnumerator CutScene()
    {
        // Justin! How's the R&D on shape production?
        yield return SayNextDialogue(barron);

        // Great!!! It's ready for you to take a look, boss!
        yield return SayNextDialogue(justin);

        // Astounding, as always. I can always count on you.
        yield return SayNextDialogue(barron);
        
        // No problem boss! I'll have it ready for you whenever!
        yield return SayNextDialogue(justin);

        OnCutSceneFinish();
    }

    protected override void OnCutSceneFinish()
    {
        base.OnCutSceneFinish();
        
        SaveSystem.Current.SetBool("JungleJustinWalkingToTV", true);
    }
}