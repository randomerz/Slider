using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }

    private static SaveSystem saveSystem;
    public GameUI gameUI;
    public SceneInitializer sceneInitializer;
    public GameSaver gameAutoSaver;

    private void Awake() {
        if (instance == null) {
            Debug.Log($"[Start] Starting game on version {Application.version}. Running on {SystemInfo.operatingSystem}. Time is {System.DateTime.Now}.");

            instance = this;
            DontDestroyOnLoad(gameObject);
            saveSystem = new SaveSystem();
        }
        else
        {
            gameAutoSaver.SaveAndDestroyAfterStart();
            Destroy(gameObject);
        }

        gameUI.Init();
        sceneInitializer?.Init();
    }

    public static SaveSystem GetSaveSystem() 
    {
        if (saveSystem == null)
        {
            saveSystem = new SaveSystem();
        }

        return saveSystem;
    }

    private void OnApplicationQuit()
    {
        SaveSystem.SaveBackups();
    }


    // temporary -- only to expose to Unity/Slider debugger
    public void SaveGame()
    {
        Debug.LogWarning("Called GameManager.SaveGame(), you should probably call SaveSystem.SaveGame() instead.");

        SaveSystem.SaveGame("Called via console");
    }

    public void LoadGame()
    {
        Debug.LogWarning("Called GameManager.LoadGame(), you should probably call SaveSystem.LoadGame() instead.");

        SaveSystem.LoadSaveProfile(0);
    }
}
