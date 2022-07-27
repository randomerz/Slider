using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FezziwigOceanMagicPuzzle : MonoBehaviour
{
    [SerializeField] private OceanArtifact oceanArtifact;
    [SerializeField] private ChadJump jumpScript;
    [SerializeField] private Collectible reward;

    private bool CanStartCast;
    private Coroutine RotateTilesRoutine;

    void Start() { CanStartCast = true; }


    /// <summary>
    /// Starts the spell casting process
    /// </summary>
    public void CastSpell() {
        RotateTilesRoutine = StartCoroutine(RotateTiles());
    }

    private IEnumerator RotateTiles() {
        CanStartCast = false;
        Vector2Int[] rotateButtons = {
            new Vector2Int(0,0),
            new Vector2Int(1,0),
            new Vector2Int(0,1),
            new Vector2Int(1,1)
        };
        foreach(Vector2Int currButton in rotateButtons) {
            oceanArtifact.RotateTiles(currButton.x, currButton.y, false);
            yield return new WaitForSeconds(1f);
        }

        if (!jumpScript.ChadFell()) Debug.Log("Spell Complete");
        else CanStartCast = true;
    }

    public void CanStartSpell(Condition cond) {
        cond.SetSpec(CanStartCast);
    }
}
