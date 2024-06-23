using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MilitaryArtifact : UIArtifact
{
    private Coroutine waitForFightsCoroutine;

    public override void ProcessQueue()
    {
        if (waitForFightsCoroutine == null && MGFight.numberOfActiveFights > 0)
        {
            waitForFightsCoroutine = StartCoroutine(WaitForFights());
            return;
        }

        base.ProcessQueue();
    }

    private IEnumerator WaitForFights()
    {
        if (moveQueue.Count > 0)
        {
            MilitaryTurnAnimator.SpeedUpAnimations();
        }

        yield return new WaitUntil(() => MGFight.numberOfActiveFights == 0);

        waitForFightsCoroutine = null;
        ProcessQueue();
    }
}
