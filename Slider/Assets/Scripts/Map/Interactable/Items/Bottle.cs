using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bottle : MonoBehaviour
{
    public void PlayerDrinkBottle()
    {
        ParticleManager.SpawnParticle(ParticleType.SmokePoof, transform.position, transform);
        AudioManager.Play("MagicChimes2");
        MirageSTileManager.GetInstance().EnableMirage();
    }
}
