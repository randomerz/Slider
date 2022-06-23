using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIQuitGame : MonoBehaviour
{
    public void OpenSteamPageHTTP()
    {
        Application.OpenURL("https://store.steampowered.com/app/1916890/Slider/");
    }

    public void LoadMainMenu()
    {
        SaveSystem.SaveGame();
        SaveSystem.SetCurrentProfile(-1);
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit(0);
    }
}
