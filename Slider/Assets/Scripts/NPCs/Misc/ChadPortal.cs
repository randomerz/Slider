using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChadPortal : MonoBehaviour
{
    public Animator animator;
    public PortalTease portalTease;

    private void OnEnable() 
    {
        SGrid.OnSTileEnabled += DebugOnStileEnable;
    }

    private void OnDisable() 
    {
        SGrid.OnSTileEnabled -= DebugOnStileEnable;
    }

    private void Start() 
    {
        if (SaveSystem.Current.GetBool("magitechDesertPortal"))
        {
            // Portal has been enabled
        }
        else
        {
            animator.SetBool("hasPortalGun", true);
        }
    }

    public void DebugOnStileEnable(object sender, SGrid.OnSTileEnabledArgs e)
    {
        if (e.stile.islandId == 2 && !SaveSystem.Current.GetBool("magitechDesertPortal"))
        {
            portalTease.EnableRealPortal(fromSave: true);
        }
    }

    public void FlashScreen()
    {
        // This should last 0.5 sec
        UIEffects.FlashWhite(speed: 2);
    }

    public void OpenPortal()
    {
        portalTease.EnableRealPortal();
        SetIsShootingPortalGun(false);
    }

    public void SetIsShootingPortalGun(bool value)
    {
        animator.SetBool("isShootingGun", value);
    }

    public void SetHasPortalGun(bool value)
    {
        animator.SetBool("hasPortalGun", value);
    }
}
