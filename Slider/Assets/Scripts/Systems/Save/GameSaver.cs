using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSaver : MonoBehaviour
{
    private bool destroyAfterStart;

    private void OnEnable()
    {
        // Saving Game on Quit to Main Menu is handled in UIManager.cs
        PlayerInventory.OnPlayerGetCollectible += SaveGame;
    }

    private void OnDisable()
    {
        PlayerInventory.OnPlayerGetCollectible -= SaveGame;
    }

    private void Start()
    {
        // for auto save on new scene loads
        Save();

        if (destroyAfterStart)
            Destroy(gameObject);
    }

    private void Save()
    {
        SaveSystem.SaveGame("Autosaving");
    }

    public void SaveAndDestroyAfterStart()
    {
        destroyAfterStart = true;
        transform.SetParent(null);
    }

    private void OnApplicationQuit()
    {
        Save();
    }

    private void SaveGame(object sender, System.EventArgs e)
    {
        Save();
    }
}
