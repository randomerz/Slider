using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FactoryTabManager : ArtifactTabManager
{
    [SerializeField] private List<FactoryTab> timedGateTabs = new List<FactoryTab>();

    protected override void Awake() 
    {
        base.Awake();

        Transform timedGateTabsHolder = timedGateTabs[0].transform.parent;
        int numChildren = timedGateTabsHolder.childCount;
        if (numChildren != timedGateTabs.Count)
        {
            Debug.LogWarning($"Number of children in {timedGateTabsHolder.gameObject.name} does not match number of Tabs in list! You may have forgotten to set a reference.");
        }
    }

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
