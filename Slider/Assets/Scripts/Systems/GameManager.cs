using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }

    private static SaveSystem saveSystem;

    private void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);

            // GameObject saveSystemGO = new GameObject("Save System");
            // saveSystemGO.AddComponent<SaveSystem>();
            // saveSystem = saveSystemGO.GetComponent<SaveSystem>();
            saveSystem = new SaveSystem();

        }
    }

    public static SaveSystem GetSaveSystem() 
    {
        if (saveSystem == null)
        {
            saveSystem = new SaveSystem();
        }

        return saveSystem;
    }


    // temporary -- only to expose to Unity
    public void SaveGame()
    {
        Debug.LogWarning("Called GameManager.SaveGame(), you should probably call SaveSystem.SaveGame() instead.");

        SaveSystem.SaveGame();
    }

    //This is so they can be used in the inspector.
    public bool GetBool(string name)
    {
        return SaveSystem.Current.GetBool(name);
    }

    //This is the only way to do it plz.
    public void SetBoolOn(string name)
    {
        SaveSystem.Current.SetBool(name, true);
    }

    public void SetBoolOff(string name)
    {
        SaveSystem.Current.SetBool(name, false);
    }
}
