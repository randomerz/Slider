using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class SceneChanger : MonoBehaviour
{
    public string sceneName;
    public SceneSpawns.SpawnLocation sceneSpawnName;

    void Start()
    {
        
    }

    // void Update()
    // {
    //     Keyboard kb = InputSystem.GetDevice<Keyboard>();
    //     if (kb.pKey.wasPressedThisFrame) 
    //     { // temporary
    //         ChangeScenes();
    //     }
    // }

    public void ChangeScenes() 
    {
        SGrid.current.SaveGrid();
        SceneSpawns.nextSpawn = sceneSpawnName;
        SceneManager.LoadScene(sceneName);
    }
}
