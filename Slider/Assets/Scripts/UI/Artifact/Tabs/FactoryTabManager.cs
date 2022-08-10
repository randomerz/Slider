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
            timedGateTabs[0].SetIsVisible(screenIndex == timedGateTabs[0].homeScreen);
            timedGateTabs[1].SetIsVisible(screenIndex == timedGateTabs[1].homeScreen);
            timedGateTabs[2].SetIsVisible(screenIndex == timedGateTabs[2].homeScreen);
            timedGateTabs[3].SetIsVisible(screenIndex == timedGateTabs[3].homeScreen);
        }
        //Debug.Log(timedGateTabs[2].tabAnimator.GetBool("isVisible"));
    }
}
