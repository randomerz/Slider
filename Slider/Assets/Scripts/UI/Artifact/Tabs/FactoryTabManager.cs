using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FactoryTabManager : ArtifactTabManager
{
    [SerializeField] private List<FactoryTab> timedGateTabs = new List<FactoryTab>();

    public override void SetCurrentScreen(int screenIndex)
    {
        base.SetCurrentScreen(screenIndex);

        if (SGrid.Current.MyArea == Area.Factory)
        {
            for (int i = 0; i < timedGateTabs.Count; i++)
            {
                timedGateTabs[i].SetIsVisible(screenIndex == timedGateTabs[i].homeScreen);
            }
        }
        //Debug.Log(timedGateTabs[2].tabAnimator.GetBool("isVisible"));
    }
}
