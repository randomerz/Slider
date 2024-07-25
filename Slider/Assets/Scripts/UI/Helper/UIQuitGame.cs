using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIQuitGame : MonoBehaviour
{
    public void OpenSteamPageHTTP()
    {
        Application.OpenURL("https://store.steampowered.com/app/1916890/Slider/?utm_source=demo&campaign=CTA");
    }

    public void OpenTwitterPageHTTP()
    {
        Application.OpenURL("https://twitter.com/_boomo_");
    }

    public void LoadMainMenu()
    {
        SaveSystem.SaveGame("Quitting to Main Menu");
        SaveSystem.SetCurrentProfile(-1);
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit(0);
    }
}
