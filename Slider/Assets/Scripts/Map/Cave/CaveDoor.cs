using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaveDoor : MonoBehaviour
{
    public PoweredDoor poweredDoor;

    public void StartDoorOpeningEffects()
    {
        if (!SaveSystem.Current.GetBool(poweredDoor.saveDoorString))
            StartCoroutine(DoorOpeningEffects());
    }

    private IEnumerator DoorOpeningEffects()
    {
        float animationDuration = 8;

        CameraShake.ShakeConstant(animationDuration, 0.15f);
        AudioManager.Play("Rumble Constant 9s");

        yield return new WaitForSeconds(animationDuration);

        CameraShake.Shake(0.5f, 1);
        AudioManager.Play("Slide Explosion");
    }
}
