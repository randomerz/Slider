using System.Collections;
using UnityEngine;

public class ArcheologistChadCutscene : SimpleInteractableCutscene
{
    private NPC archeologist;
    private NPC chad;

    private void Start()
    {
        archeologist = cutsceneCharacters[0];
        chad = cutsceneCharacters[1];
    }

    protected override IEnumerator CutScene()
    {
        yield return new WaitForSeconds(0.2f);
        //chad, are u alright?
        yield return SayNextDialogue(archeologist);
        //No... I- I can't do anything right.
        yield return SayNextDialogue(chad);
        //I'm a terrible research assistant.
        yield return SayNextDialogue(chad);
        //chad-
        yield return SayNextDialogue(archeologist, true, 0.6f);
        //That explorer has helped you with your research more than me.
        yield return SayNextDialogue(chad);
        //I'm never gonna be an archeologist. I'm never gonna be anything. I give up.
        yield return SayNextDialogue(chad);
        //chad! stop this! youve done more for me than u know!
        yield return SayNextDialogue(archeologist);
        //i dont want to lose you as my assistant chad...
        yield return SayNextDialogue(archeologist);
        //but maybe u r destined to be something else?
        yield return SayNextDialogue(archeologist);
        //Hmmm... I could be... something else?
        yield return SayNextDialogue(chad);
    }
}