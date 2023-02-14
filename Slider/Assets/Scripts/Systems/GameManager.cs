using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//L: MANAGES THE GAME
public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }

    private static SaveSystem saveSystem;
    public GameUI gameUI;
    public SceneInitializer sceneInitializer; // this script makes me sad
    public GameSaver gameAutoSaver;

    // constant for other scripts to reference
    public bool debugModeActive;

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
