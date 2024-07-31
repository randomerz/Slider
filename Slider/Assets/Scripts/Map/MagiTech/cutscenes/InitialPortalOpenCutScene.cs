using System.Collections;
using UnityEngine;

public class InitialPortalOpenCutScene : SimpleInteractableCutscene
{
    private NPC portalOperator;
    private NPC fezziwig;

    public GameObject portalGunGO;
    public MagiTechInitialPortalTease initialPortalTease;

    protected override void Start()
    {
        base.Start();

        portalOperator = cutsceneCharacters[0];
        fezziwig = cutsceneCharacters[1];
    }

    protected override bool ShouldCutsceneBeSkipped()
    {
        return PlayerInventory.Contains("Slider 2", Area.MagiTech);
    }

    protected override void OnCutsceneNotFinished()
    {
        base.OnCutsceneNotFinished();

        SaveSystem.Current.SetBool("chadFinishedRunningIntoPortal", true);
    }

    protected override IEnumerator CutScene()
    {
        Debug.Log($"Starting Portal Cutscene");
        yield return SayNextDialogue(portalOperator);
        yield return SayNextDialogue(fezziwig);
        yield return SayNextDialogue(portalOperator);
        yield return SayNextDialogue(portalOperator);
        yield return SayNextDialogue(fezziwig);
        //Fezziwig casts the spell here
        yield return SayNextDialogue(fezziwig, false);
        yield return SayNextDialogue(portalOperator, true, 0.3f);
        //fezziwig gets cut off mid sentence here
        yield return SayNextDialogue(fezziwig, true, 0.01f);
        yield return SayNextDialogue(portalOperator);
        yield return SayNextDialogue(portalOperator);

        OnCutSceneFinish();
    }

    protected override void OnCutSceneFinish()
    {
        base.OnCutSceneFinish();

        portalGunGO.SetActive(false);
        // In case someone quits before fezziwig opens the portal
        if (!SaveSystem.Current.GetBool("magitechInitialPortalOpened"))
        {
            initialPortalTease.EnableRealPortal(true);
        }
    }
}