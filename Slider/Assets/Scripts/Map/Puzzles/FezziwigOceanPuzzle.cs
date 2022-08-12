using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FezziwigOceanPuzzle : MonoBehaviour
{
    [SerializeField] private OceanArtifact oceanArtifact;
    [SerializeField] private ChadJump jumpScript;

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
        oceanArtifact.AllowRotations(false);
        while (!oceanArtifact.MoveQueueEmpty())
        {
            yield return null;
        }
        var jumpRoutine = StartCoroutine(jumpScript.Jump());
        while (!jumpScript.ChadJumped())
        {
            yield return null;
        }
        oceanArtifact.RotateAllTiles();
        while (!oceanArtifact.MoveQueueEmpty())
        {
            yield return null;
        }
        oceanArtifact.AllowRotations(true);
        Finished = !jumpScript.ChadFell() && !jumpScript.ChadFalling();
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
