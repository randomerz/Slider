using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaveDoor : MonoBehaviour
{
    public PoweredDoor poweredDoor;

    private void Awake() {
        Debug.Log("Cave door awake");
    }

    public void StartDoorOpeningEffects()
    {
        if (!SaveSystem.Current.GetBool(poweredDoor.saveDoorString))
            StartCoroutine(DoorOpeningEffects());
    }

    private IEnumerator DoorOpeningEffects()
    {
        float animationDuration = 8;

        CameraShake.ShakeIncrease(animationDuration, 0.5f);
        AudioManager.Play("Slide Rumble");

        yield return new WaitForSeconds(animationDuration);

        CameraShake.Shake(0.5f, 1);
        AudioManager.Play("Slide Explosion");
    }
}
