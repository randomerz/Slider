using System.Collections;
using UnityEngine;

public class MagitechComputerExplodeProp : MonoBehaviour, ISavable
{
    private const string SAVE_STRING = "magitechDidComputerPropExplode";
    private bool didExplode;

    public PowerSource powerSource;
    public Transform computerLocation;

    public void Save()
    {
        SaveSystem.Current.SetBool(SAVE_STRING, didExplode);
    }

    public void Load(SaveProfile profile)
    {
        if (SaveSystem.Current.GetBool(SAVE_STRING))
        {
            ExplodeComputer(true);
        }
    }

    public void ExplodeComputer(bool fromSave=false)
    {
        if (didExplode)
        {
            return;
        }
        didExplode = true;

        if (fromSave)
        {
            powerSource.StartSignal(false);
        }
        else
        {
            StartCoroutine(DoExplode());
        }
    }

    private IEnumerator DoExplode()
    {
        ParticleManager.SpawnParticle(ParticleType.MiniSparkle, computerLocation.position, computerLocation);

        yield return new WaitForSeconds(1);
        
        powerSource.StartSignal(false);
        
        ParticleManager.SpawnParticle(ParticleType.SmokePoof, computerLocation.position, computerLocation);
        AudioManager.PlayWithVolume("Slide Explosion", 0.5f);
        AudioManager.Play("Power Off");
    }
}