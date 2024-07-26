using System.Collections;
using UnityEngine;

public class ArcheologistChadCutscene : SimpleInteractableCutscene
{
    private NPC archeologist;
    private NPC chad;

    protected override void Start()
    {
        base.Start();

        archeologist = cutsceneCharacters[0];
        chad = cutsceneCharacters[1];
    }

    protected override IEnumerator CutScene()
    {
        yield return new WaitForSeconds(0.2f);
        
        // chad, whats wrong??
        yield return SayNextDialogue(archeologist);

        // No... I- I can't do anything right.
        yield return SayNextDialogue(chad);
        // I'm a terrible research assistant.
        yield return SayNextDialogue(chad);

        // chad-
        yield return SayNextDialogue(archeologist, true, 0.3f);

        // That explorer has helped you with your research more than me.
        yield return SayNextDialogue(chad);
        // What am I even here for? I just keep letting everyone down.
        yield return SayNextDialogue(chad);
        // I give up...
        yield return SayNextDialogue(chad);

        // chad... im sorry... i should have noticed sooner.
        yield return SayNextDialogue(archeologist);
        // but you shouldnt feel bad! youve done so much to help me!
        yield return SayNextDialogue(archeologist);
        // you helped us journey to this continent, and you even found relics in the village and jungle!
        yield return SayNextDialogue(archeologist);
        // stop comparing yourself to others. think about what <wavy>you</wavy> can do!
        yield return SayNextDialogue(archeologist);

        // What I could do, huh?
        yield return SayNextDialogue(chad);

        OnCutSceneFinish();
    }
}