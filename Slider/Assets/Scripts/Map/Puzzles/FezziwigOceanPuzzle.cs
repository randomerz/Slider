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

    private bool CanStartCast;

    private bool Finished;
    private Coroutine RotateTilesRoutine;

    void Start() { 
        Finished = false;
        CanStartCast = true;

        if (PlayerInventory.Contains("Magical Gem"))
        {
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Starts the spell casting process
    /// </summary>
    public void CastSpell() {
        RotateTilesRoutine = StartCoroutine(CastRoutine());
    }

    private IEnumerator CastRoutine() {
        CanStartCast = false;
        oceanArtifact.AllowRotate(false);

        yield return new WaitUntil(() => oceanArtifact.MoveQueueEmpty());

        jumpScript.JumpStarter();
        SetFogStatus(true);

        yield return new WaitUntil(() => jumpScript.ChadJumped());

        yield return new WaitForSeconds(1.25f);

        npcAnimator.SetBool("isCasting", true);
        StartCoroutine(oceanArtifact.RotateAllTiles(CastFinish));
    }

    private void CastFinish()
    {
        oceanArtifact.AllowRotate(true);
        Finished = !jumpScript.ChadFell() && !jumpScript.ChadFalling();
        CanStartCast = !Finished;
        if (!Finished) jumpScript.FinishFall();
        else npcAnimator.SetBool("isCasting", false);

        SetFogStatus(false);
    }

    public void CanStartSpell(Condition cond) {
        cond.SetSpec(CanStartCast);
    }

    public void CannotStartSpell(Condition cond) {
        cond.SetSpec(!CanStartCast);
    }

    public void SpellFinished(Condition cond) {
        cond.SetSpec(Finished);
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
