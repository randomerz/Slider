using System.Collections;
using UnityEngine;

public class ForemanLaserShutoffCutScene : SimpleInteractableCutscene
{
    private NPC foreman;
    private NPC laserOperator;

    protected override void Start()
    {
        base.Start();

        foreman = cutsceneCharacters[0];
        laserOperator = cutsceneCharacters[1];
    }

    protected override IEnumerator CutScene()
    {
        yield return SayNextDialogue(foreman);
        yield return SayNextDialogue(foreman);
        yield return SayNextDialogue(foreman);
        yield return SayNextDialogue(laserOperator);
        yield return SayNextDialogue(foreman);
        yield return SayNextDialogue(foreman);
        yield return SayNextDialogue(laserOperator);
    }
}