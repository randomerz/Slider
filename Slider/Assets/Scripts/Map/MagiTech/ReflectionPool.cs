using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReflectionPool : MonoBehaviour
{
    public List<Animator> obeliskAnimators;
    public List<GameObject> toggleOn;
    public ConditionChecker conditionChecker;

    private void OnEnable()
    {
        SGrid.OnSTileEnabled += Check; 
        ArtifactTabManager.AfterScrollRearrage += Check;

    }

    private void OnDisable()
    {
        SGrid.OnSTileEnabled -= Check;
        ArtifactTabManager.AfterScrollRearrage -= Check;
    }

    #region  check
    private void Check(object sender, EventArgs e)
    {
        Check();
    }

    private void Check(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        Check();
    }

    private void Check(object sender, SGrid.OnGridMoveArgs e)
    {
        Check();
    }

    private void Check(object sender, SGrid.OnSTileEnabledArgs e)
    {
        Check();
    }


    private void Check()
    {
        conditionChecker.CheckConditions();
    }
#endregion

    public void TurnOnReflectionPool()
    {
        foreach(Animator a in obeliskAnimators)
        {
            a.Play("MagiTechObeliskOn");
        }
        foreach(GameObject g in toggleOn)
        {
            g.SetActive(true);
        }
    }

    public void TurnOffReflectionPool()
    {
        foreach(Animator a in obeliskAnimators)
        {
            a.Play("MagiTechObeliskOff");
        }
        foreach(GameObject g in toggleOn)
        {
            g.SetActive(false);
        }
    }
}
