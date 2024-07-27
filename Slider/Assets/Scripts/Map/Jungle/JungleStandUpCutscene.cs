using System.Collections;
using UnityEngine;

public class JungleStandUpCutscene : SimpleInteractableCutscene
{
    private NPC barron;
    private NPC justin;
    private NPC joe;

    protected override void Start()
    {
        base.Start();

        barron = cutsceneCharacters[0];
        justin = cutsceneCharacters[1];
        joe    = cutsceneCharacters[2];
    }

    protected override bool ShouldCutsceneBeSkipped()
    {
        return PlayerInventory.Contains("Slider 5", Area.Jungle);
    }

    protected override IEnumerator CutScene()
    {
        // - Alright, let's give it 5 minutes for everyone to get here
        yield return SayNextDialogue(barron);
        // - .........
        yield return SayNextDialogue(barron);
        // - Okay well let's just get started then. Justin?
        yield return SayNextDialogue(barron);
        
        // > I finished finalizing R&D, ready for you to take a look boss!
        yield return SayNextDialogue(justin);

        // - Great, I'll definetely come by when I have time.
        yield return SayNextDialogue(barron);
        // - Joe, how about you?
        yield return SayNextDialogue(barron);

        // > Oh uh... I've got my sticks sending up.
        yield return SayNextDialogue(joe);
        // > I've also been having some trouble with my J*ra cards so I had to sort that out.
        yield return SayNextDialogue(joe);

        // - Sounds good, don't hesitate to ask around for help!
        yield return SayNextDialogue(barron);
        // - Well it looks like everyone else is busy. Before we wrap up though,
        yield return SayNextDialogue(barron);
        // - Would everyone please welcome the new intern!
        yield return SayNextDialogue(barron);

        OnCutSceneFinish();
    }
}