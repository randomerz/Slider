using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FezziwigOceanPuzzle : MonoBehaviour
{
    [SerializeField] private OceanArtifact oceanArtifact;
    [SerializeField] private ChadJump jumpScript;
    [SerializeField] private Animator npcAnimator;
    [SerializeField] private FogAnimationController fogRingController;
    [SerializeField] private FogAnimationController fogWorldController;
    [SerializeField] private RainDropController rainController;


    private bool canStartCast;
    private bool isFirstCast = true;

    private bool finished;
    private Coroutine rotateTilesRoutine;

    void Start() 
    { 
        finished = false;
        canStartCast = true;

        if (PlayerInventory.Contains("Magical Gem"))
        {
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Starts the spell casting process
    /// </summary>
    public void CastSpell() 
    {
        if (rotateTilesRoutine == null)
        {
            rotateTilesRoutine = StartCoroutine(CastRoutine());
        }
    }

    private IEnumerator CastRoutine() 
    {
        canStartCast = false;
        oceanArtifact.AllowRotate(false);

        yield return new WaitUntil(() => oceanArtifact.MoveQueueEmpty());

        jumpScript.JumpStarter();
        SetFogStatus(true);
        rainController.SetRainActive(true);

        yield return new WaitUntil(() => jumpScript.ChadJumped());

        yield return new WaitForSeconds(1.25f);

        npcAnimator.SetBool("isCasting", true);
        
        if (isFirstCast)
        {
            isFirstCast = false;
            yield return new WaitForSeconds(2f);
        }

        StartCoroutine(oceanArtifact.RotateAllTiles(CastFinish));
    }

    private void CastFinish()
    {
        oceanArtifact.AllowRotate(true);
        finished = !jumpScript.ChadFell() && !jumpScript.ChadFalling();
        canStartCast = !finished;
        if (!finished) jumpScript.FinishFall();
        else npcAnimator.SetBool("isCasting", false);

        SetFogStatus(false);
        rainController.SetRainActive(false);

        rotateTilesRoutine = null;
    }

    public void CanStartSpell(Condition cond) {
        cond.SetSpec(canStartCast);
    }

    public void CannotStartSpell(Condition cond) {
        cond.SetSpec(!canStartCast);
    }

    public void SpellFinished(Condition cond) {
        cond.SetSpec(finished);
    }


    private void SetFogStatus(bool enabled)
    {
        if (enabled)
        {
            fogRingController.transform.position = SGrid.Current.GetStile(5).transform.position;
        }

        fogRingController.SetIsVisible(enabled);
        fogWorldController.SetIsVisible(enabled);
    }
}
