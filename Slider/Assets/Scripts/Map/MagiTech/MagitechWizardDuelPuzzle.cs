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

    [SerializeField] private List<ParticleSystem> fireWinningParticles;
    [SerializeField] private List<ParticleSystem> lightningWinningParticles;

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
        StartCoroutine(DoWinningParticles(fireWinningParticles));
    }

    public void PlayLightningWinningParticles()
    {
        StartCoroutine(DoWinningParticles(lightningWinningParticles));
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
}