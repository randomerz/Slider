using UnityEngine;

public class DesertVIPSoftlockChecker : MonoBehaviour
{
    public Transform softlockTeleportTransform;

    public void CheckSoftlock()
    {
        if (!PlayerInventory.Contains("Sunglasses", Area.Desert))
        {
            Player.SetPosition(softlockTeleportTransform.position);
            ParticleManager.SpawnParticle(ParticleType.SmokePoof, softlockTeleportTransform.position);
        }
    }
}