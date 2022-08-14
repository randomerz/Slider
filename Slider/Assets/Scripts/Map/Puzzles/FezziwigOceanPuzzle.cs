using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FezziwigOceanPuzzle : MonoBehaviour
{
    [SerializeField] private OceanArtifact oceanArtifact;
    [SerializeField] private ChadJump jumpScript;
    [SerializeField] private Animator npcAnimator;

    private bool CanStartCast;

    private bool Finished;
    private Coroutine RotateTilesRoutine;

    void Start() { 
        Finished = false;
        CanStartCast = true;
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

        StartCoroutine(jumpScript.Jump());

        yield return new WaitUntil(() => jumpScript.ChadJumped());

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
}
