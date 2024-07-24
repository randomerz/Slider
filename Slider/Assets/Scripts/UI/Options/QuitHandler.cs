
using UnityEngine;
using UnityEngine.SceneManagement;

public class QuitHandler : MonoBehaviour
{
    public static System.EventHandler<System.EventArgs> OnQuit;

    public void LoadMainMenu()
    {
        SaveSystem.SaveGame("Quitting to Main Menu");
        PauseManager.SetPauseState(false);

        // Undo lazy singletons
        if (Player.GetInstance() != null)
            Player.GetInstance().ResetInventory();
        
        OnQuit?.Invoke(this, new System.EventArgs());

        SceneManager.LoadScene("MainMenu");
        SaveSystem.SetCurrentProfile(-1);
        // Sometimes NPC's Update() will poll the current save profile
        CoroutineUtils.ExecuteAfterEndOfFrame(() => SaveSystem.SetCurrentProfile(-1), this);
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quitting game!");
    }
}
