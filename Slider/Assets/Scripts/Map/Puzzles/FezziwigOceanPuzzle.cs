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
        //oceanArtifact.RotateAllTiles();
        RotateTilesRoutine = StartCoroutine(RotateTiles());
    }

    private IEnumerator RotateTiles() {
        CanStartCast = false;
        oceanArtifact.AllowRotations(false);
        while (!oceanArtifact.MoveQueueEmpty())
        {
            yield return null;
        }
        StartCoroutine(jumpScript.Jump());
        while (!jumpScript.ChadJumped())
        {
            yield return null;
        }
        npcAnimator.SetBool("isCasting", true);
        var rotateCoroutine = StartCoroutine(oceanArtifact.RotateAllTiles(RotateCallback));
        //while (!oceanArtifact.MoveQueueEmpty())
        //{
        //    yield return null;
        //}
        //while (rotateCoroutine != null)
        //{
        //    yield return null;
        //}
        
    }

    private void RotateCallback()
    {
        oceanArtifact.AllowRotations(true);
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
