using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OceanBuoys : MonoBehaviour
{
    public List<int> buoytiles = new List<int> {1, 3, 4, 8, 9};
    public KnotBox knotBox;
    public GameObject buoyUITrackerPrefab;
    private bool knotBoxEnabledLastFrame;

    private void OnEnable()
    {
        SGridAnimator.OnSTileMoveEnd += BuoySuccessEffectsCheck;
        SGridAnimator.OnSTileMoveStart += CheckBuoyFirstTry;

    }

    private void OnDisable()
    {
        SGridAnimator.OnSTileMoveEnd -= BuoySuccessEffectsCheck;
        SGridAnimator.OnSTileMoveStart -= CheckBuoyFirstTry;

    }

    private void LateUpdate() {
        knotBoxEnabledLastFrame = knotBox.isActiveAndEnabled;
    }

    private bool BuoyConditions()
    {
        return (
            AllBuoy() && 
            knotBox.isActiveAndEnabled && 
            knotBoxEnabledLastFrame &&
            knotBox.CheckNumLines() == 0
        );
    }

    private void BuoySuccessEffectsCheck(object sender, System.EventArgs e)
    {
        if (PlayerInventory.Contains("Golden Fish"))
            return;

        if (BuoyConditions())
        {
            knotBox.CheckParticles();
            AudioManager.Play("Puzzle Complete");
        }
    }

    public bool AllBuoy()
    {
        return SGrid.AreTilesActive(SGrid.Current.GetStiles(buoytiles));
    }

    public void ToggleKnotBox()
    {
        if (AllBuoy())
        {
            knotBox.enabled = !knotBox.enabled;
            knotBox.CheckLines();
            if (knotBox.enabled)
            {
                List<UITrackerBuoyAddOn> uiTrackerAddOns = new List<UITrackerBuoyAddOn>();
                foreach (GameObject knotnode in knotBox.knotnodes)
                {
                    GameObject buoyUITrackerGO = Instantiate(buoyUITrackerPrefab);
                    UITracker buoyUITracker = buoyUITrackerGO.GetComponent<UITracker>();
                    uiTrackerAddOns.Add(buoyUITrackerGO.GetComponent<UITrackerBuoyAddOn>());

                    UITrackerManager.AddNewCustomTracker(buoyUITracker, knotnode);
                }

                for (int i = 0; i < uiTrackerAddOns.Count; i++)
                {
                    int next = (i + 1) % uiTrackerAddOns.Count;
                    int prev = (i + uiTrackerAddOns.Count - 1) % uiTrackerAddOns.Count;

                    uiTrackerAddOns[i].target1 = uiTrackerAddOns[next].myRectTransform;
                    uiTrackerAddOns[i].target2 = uiTrackerAddOns[prev].myRectTransform;
                    uiTrackerAddOns[i].index1 = i;
                    uiTrackerAddOns[i].index2 = prev;
                    uiTrackerAddOns[i].knotBox = knotBox;
                }
            }
            else
            {
                foreach (GameObject knotnode in knotBox.knotnodes)
                {
                    UITrackerManager.RemoveTracker(knotnode);
                }
            }
        }
    }

    public void DisableKnotBox()
    {
        if(knotBox.enabled) ToggleKnotBox();
    }

    public void EnableKnotBox()
    {
        if(!knotBox.enabled) ToggleKnotBox();
    }

    public void BuoyAllFound(Condition c)
    {
        c.SetSpec(AllBuoy());
    }

    public void KnotBoxEnabled(Condition c)
    {
        c.SetSpec(knotBox.isActiveAndEnabled && AllBuoy());
    }

    public void KnotBoxDisabled(Condition c)
    {
        c.SetSpec(!knotBox.isActiveAndEnabled && AllBuoy());
    }

    public void BuoyCheck(Condition c)
    {
        c.SetSpec(BuoyConditions());
    }

    public void BuoyFirstTimeCheck(Condition c)
    {
        c.SetSpec(knotBox.CheckNumLines() == 0);
    }

    private void CheckBuoyFirstTry(object sender, System.EventArgs e)
    {
        if(SaveSystem.Current.GetBool("OceanTalkedToKevin"))
            SaveSystem.Current.SetBool("OceanFailedFirstBuoy", true);
    }

    
}
