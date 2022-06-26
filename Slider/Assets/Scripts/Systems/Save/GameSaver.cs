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
        SaveGame();

        if (destroyAfterStart)
            Destroy(gameObject);
    }

    public void SaveAndDestroyAfterStart()
    {
        destroyAfterStart = true;
        transform.SetParent(null);
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }

    private void SaveGame()
    {
        SaveSystem.SaveGame();
    }

    private void SaveGame(object sender, System.EventArgs e)
    {
        SaveGame();
    }
}
