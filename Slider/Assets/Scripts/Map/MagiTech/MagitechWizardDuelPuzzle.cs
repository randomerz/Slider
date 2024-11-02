using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Note: most of the logic is still being handled by MagiTechGrid.cs
public class MagitechWizardDuelPuzzle : MonoBehaviour 
{
    [SerializeField] private DesyncItem pastStoolItem;
    [SerializeField] private DesyncItem presentStoolItem;
    private bool areTrackersOn;

    [SerializeField] private ItemResetter fireItemResetter;
    [SerializeField] private ItemResetter lightningItemResetter;
    [SerializeField] private List<ParticleSystem> fireWinningParticles;
    [SerializeField] private List<ParticleSystem> lightningWinningParticles;
    [SerializeField] private ParticleSystem fireCastingParticles;
    [SerializeField] private ParticleSystem lightningCastingParticles;

    private void Start() 
    {
        if (SGrid.Current.GetNumTilesCollected() == 3)
        {
            SetTrackers(true);
        }
    }

    private void OnEnable() 
    {
        SGrid.OnSTileEnabled += OnSTileEnabled;
    }

    private void OnDisable() 
    {
        SGrid.OnSTileEnabled -= OnSTileEnabled;
    }

    private void OnSTileEnabled(object sender, SGrid.OnSTileEnabledArgs e)
    {
        if (e.stile.islandId == 3)
        {
            SetTrackers(true);
        }
        else
        {
            SetTrackers(false);
        }
    }

    private void SetTrackers(bool value)
    {
        if (value != areTrackersOn)
        {
            areTrackersOn = value;
            pastStoolItem.SetIsTracked(areTrackersOn);
            presentStoolItem.SetIsTracked(areTrackersOn);
        }
    }

    public void PlayFireWinningParticles()
    {
        fireCastingParticles.Play();
        StartCoroutine(DoWinningParticles(fireWinningParticles));
    }

    public void StopFireWinningParticles()
    {
        fireCastingParticles.Stop();
    }

    public void PlayLightningWinningParticles()
    {
        lightningCastingParticles.Play();
        StartCoroutine(DoWinningParticles(lightningWinningParticles));
    }

    public void StopLightningWinningParticles()
    {
        lightningCastingParticles.Stop();
    }

    public void StopBothWinningParticles()
    {
        StopFireWinningParticles();
        StopLightningWinningParticles();
    }
    
    private IEnumerator DoWinningParticles(List<ParticleSystem> particles)
    {
        foreach (ParticleSystem ps in particles)
        {
            ps.Play();
            AudioManager.PlayWithVolume("Hat Click", 0.4f);

            yield return new WaitForSeconds(0.1f);
        }
    }

    public void CheckTurnInShieldFire()
    {
        if (PlayerInventory.Contains("Slider 8", Area.MagiTech))
        {
            return;
        }
        if (PlayerInventory.GetCurrentItem() == pastStoolItem)
        {
            fireItemResetter.ResetItem(pastStoolItem);
        }
        else if (PlayerInventory.GetCurrentItem() == presentStoolItem)
        {
            fireItemResetter.ResetItem(presentStoolItem);
        }
    }

    public void CheckTurnInShieldLightning()
    {
        if (PlayerInventory.Contains("Slider 8", Area.MagiTech))
        {
            return;
        }
        if (PlayerInventory.GetCurrentItem() == pastStoolItem)
        {
            lightningItemResetter.ResetItem(pastStoolItem);
        }
        else if (PlayerInventory.GetCurrentItem() == presentStoolItem)
        {
            lightningItemResetter.ResetItem(presentStoolItem);
        }
    }
}