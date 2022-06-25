using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }

    private static SaveSystem saveSystem;
    public GameUI gameUI;
    public SceneInitializer sceneInitializer; // this script makes me sad

    private void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);

            // GameObject saveSystemGO = new GameObject("Save System");
            // saveSystemGO.AddComponent<SaveSystem>();
            // saveSystem = saveSystemGO.GetComponent<SaveSystem>();
            saveSystem = new SaveSystem();
        }
        else
        {
            Destroy(gameObject);
        }

        gameUI.Init();
        sceneInitializer?.Init();
    }

    private void OnApplicationQuit()
    {
        SaveSystem.SaveGame();
    }

    public static SaveSystem GetSaveSystem() 
    {
        if (saveSystem == null)
        {
            saveSystem = new SaveSystem();
        }

        return saveSystem;
    }


    // temporary -- only to expose to Unity/Slider debugger
    public void SaveGame()
    {
        Debug.LogWarning("Called GameManager.SaveGame(), you should probably call SaveSystem.SaveGame() instead.");

        SaveSystem.SaveGame();
    }

    public void LoadGame()
    {
        Debug.LogWarning("Called GameManager.LoadGame(), you should probably call SaveSystem.LoadGame() instead.");

        SaveSystem.LoadSaveProfile(0);
    }
}
