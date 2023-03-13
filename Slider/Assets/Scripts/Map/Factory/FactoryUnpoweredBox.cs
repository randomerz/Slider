using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FactoryUnpoweredBox : MonoBehaviour
{
    public PowerSource poweredBox;
    public string saveString;

    void Start()
    {
        if (SaveSystem.Current.GetBool(saveString))
        {
            poweredBox.enabled = true;
        }
    }
    
    public void FixBox()
    {
        if (!SaveSystem.Current.GetBool(saveString))
        {
            SaveSystem.Current.SetBool(saveString, true);
            AudioManager.Play("Power On");
            poweredBox.enabled = true;

            ParticleManager.SpawnParticle(ParticleType.SmokePoof, transform.position);
        }
    }
}
