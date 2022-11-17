using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempEndOfDemoPositionChanger : MonoBehaviour
{
    // This script is so when the player finishes the demo and relaunches the save, they don't spawn 
    // in the cave transition hitbox.

    public SceneChanger sceneChanger;
    public Transform nearbySpawn;
    private bool tpToNearbySpawn = true;

    private void OnEnable() 
    {
        
        // if (Player.GetPosition().y > transform.position.y)
        // {
        //     Player.SetPosition(Player.GetPosition() - 2 * Vector3.up);
        // }
        StartCoroutine(TurnOffSpawnProtection());
    }

    public void DemoChangeScene()
    {
        if (tpToNearbySpawn)
        {
            Player.SetPosition(nearbySpawn.position);
        }
        else
        {
            sceneChanger.ChangeScenes();
        }
    }
    
    private IEnumerator TurnOffSpawnProtection()
    {
        yield return new WaitForSeconds(0.05f);

        tpToNearbySpawn = false;
    }
}
