using System.Collections;
using UnityEngine;

public class InitialPortalOpenCutScene : SimpleInteractableCutscene
{
    private NPC portalOperator;
    private NPC fezziwig;

    private void Start()
    {
        portalOperator = cutsceneCharacters[0];
        fezziwig = cutsceneCharacters[1];
    }

    protected override IEnumerator CutScene()
    {
        yield return SayNextDialogue(portalOperator);
        yield return SayNextDialogue(fezziwig);
        yield return SayNextDialogue(portalOperator);
        yield return SayNextDialogue(portalOperator);
        yield return SayNextDialogue(fezziwig);
        yield return SayNextDialogue(fezziwig, false);
        yield return SayNextDialogue(portalOperator);
        yield return SayNextDialogue(fezziwig);
        yield return SayNextDialogue(fezziwig);
        yield return SayNextDialogue(portalOperator);
    }
}