using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// For FactoryMusicProgression Parameter:
/// 0) 2 is on at the start
/// 1) 3 turns on when you get tile 2
/// 2) 4 turns on when you get tile 5
/// 3) 5 turns on/off as you power the big crystal
/// 4) stinger when you power the crystal fully(prob will need to be changed to fit the "cutscene" timing)
/// 5) 1 is on during the past sequence
/// 6) 6 turns on when you get back to the future
/// </summary>
public class FactoryMusicController : MonoBehaviour, ISavable
{
    public const float FACTORY_STINGER_DURATION = 9; // in seconds

    private int musicProgressionParameter;

    private void OnEnable()
    {
        SGrid.OnSTileEnabled += OnSTileGet;
    }

    private void OnDisable()
    {
        SGrid.OnSTileEnabled -= OnSTileGet;
    }

    public void Save()
    {
        SaveSystem.Current.SetInt("FactoryMusicProgressionParameter", musicProgressionParameter);
    }

    public void Load(SaveProfile profile)
    {
        musicProgressionParameter = profile.GetInt("FactoryMusicProgressionParameter", 0);
        SetFactoryMusicProgress(musicProgressionParameter);
        SetFactoryStingerParameter(false);
    }

    private void OnDestroy()
    {
        AudioManager.StopAmbience("Forest Ambience");
    }


    // Logic for updating track dynamically

    private void OnSTileGet(object sender, SGrid.OnSTileEnabledArgs e)
    {
        switch (e.stile.islandId)
        {
            case 2:
                if (musicProgressionParameter < 1)
                {
                    SetFactoryMusicProgress(1);
                }
                break;
            case 5:
                if (musicProgressionParameter < 2)
                {
                    SetFactoryMusicProgress(2);
                }
                break;
        }

    }

    public void SetCrystalPowerTreshold(bool meetsThreshold)
    {
        if (musicProgressionParameter == 2 && meetsThreshold)
        {
            SetFactoryMusicProgress(3);
        }
        else if (musicProgressionParameter == 3 && !meetsThreshold)
        {
            SetFactoryMusicProgress(2);
        }
    }

    public void SetIsInPast(bool isInPast)
    {
        if (isInPast)
        {
            SetFactoryMusicProgress(5);
            AudioManager.PlayMusic("Factory");
            AudioManager.PlayAmbience("Forest Ambience");
        }
        else if (PlayerInventory.Contains("Slider 8", Area.Factory))
        {
            SetFactoryMusicProgress(6);
            AudioManager.StopAmbience("Forest Ambience");
        }
    }


    // FMOD stuff

    private void SetFactoryMusicProgress(int index)
    {
        AudioManager.SetGlobalParameter("FactoryMusicProgression", index);
        musicProgressionParameter = index;
    }

    public void DoFactoryStinger()
    {
        SetFactoryMusicProgress(4);
        SetFactoryStingerParameter(true);
        StartCoroutine(ResetFactoryStinger());
    }

    private IEnumerator ResetFactoryStinger()
    {
        yield return new WaitForSeconds(1);
        SetFactoryStingerParameter(false);
    }

    private void SetFactoryStingerParameter(bool value)
    {
        AudioManager.SetGlobalParameter("FactoryMusicStinger", value ? 1 : 0);
    }
}
