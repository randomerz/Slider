using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DesertOasisSoftLockCheck : MonoBehaviour
{
    private const string SOFTLOCK_FLAG = "DesertIsStuckInOasis";

    public Transform safePosition;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            CheckSoftLock();
        }
    }

    public void CheckSoftLock()
    {
        if (!Player.GetInstance().GetIsOnWater())
        {
            if (SaveSystem.Current.GetBool("desertPartyFinished"))
            {
                SaveSystem.Current.SetBool(SOFTLOCK_FLAG, true);
            }
        }
    }

    public void FixSoftLock()
    {
        Player.SetPosition(safePosition.position);
        Player.GetInstance().SetIsOnWater(false);
        SaveSystem.Current.SetBool(SOFTLOCK_FLAG, false);
    }
}
