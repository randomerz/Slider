using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSaver : MonoBehaviour
{
    private void OnEnable()
    {
        // Saving Game on Quit to Main Menu is handled in UIManager.cs
        PlayerInventory.OnPlayerGetCollectible += SaveGame;
        GameManager.OnSceneChange += SaveGame;
    }

    private void OnDisable()
    {
        PlayerInventory.OnPlayerGetCollectible -= SaveGame;
        GameManager.OnSceneChange -= SaveGame;
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
