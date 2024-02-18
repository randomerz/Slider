using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoggyMusicHintManager : Singleton<FoggyMusicHintManager>
{
    public static FoggyMusicHintManager Instance => _instance;

    public List<ParticleSystem> ikeParticles;
    public List<ParticleSystem> bobParticles;
    public List<ParticleSystem> coconutParticles;
    public ShopManager shopManager;

    private bool wasOnLastFrame;

    private void Awake()
    {
        InitializeSingleton();
    }

    private void Update()
    {
        UpdateHints();
    }

    private bool UpdateHints()
    {
        // If they finished the puzzle
        if (PlayerInventory.Contains("Mushroom"))
        {
            if (wasOnLastFrame)
            {
                wasOnLastFrame = false;
                SetIkeHint(false);
                SetBobHint(false);
                SetCoconutHint(false);
            }
            return false;
        }

        // If it's the last puzzle
        if (shopManager.GetCredits() == 5 && !wasOnLastFrame)
        {
            if (!wasOnLastFrame)
            {
                wasOnLastFrame = true;
                SetIkeHint(true);
                SetBobHint(true);
                SetCoconutHint(true);
            }
            return true;
        }

        // // After you have entered foggy seas for the first time
        // if (SaveSystem.Current.GetBool("OceanEnteredFoggy") && !SaveSystem.Current.GetBool("OceanFoggyHintsOn"))
        // {
        //     SaveSystem.Current.SetBool("OceanFoggyHintsOn", true);
        //     wasOnLastFrame = true;
        //     SetIkeHint(true);
        //     SetBobHint(true);
        //     SetCoconutHint(true);
        //     return true;
        // }

        return false;
    }

    public void SetIkeHint(bool value) => StartCoroutine(SetParticles(ikeParticles, value));
    public void SetBobHint(bool value) => StartCoroutine(SetParticles(bobParticles, value));
    public void SetCoconutHint(bool value) => StartCoroutine(SetParticles(coconutParticles, value));

    private IEnumerator SetParticles(List<ParticleSystem> particles, bool value)
    {
        if (!value)
        {
            wasOnLastFrame = false;
        }

        // todo: sfx? will need to refactor
        foreach (ParticleSystem p in particles)
        {
            if (value)
                p.Play();
            else
                p.Stop();
            yield return new WaitForSeconds(0.1f);
        }
    }
}
