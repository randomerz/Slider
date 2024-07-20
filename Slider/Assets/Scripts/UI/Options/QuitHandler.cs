
using UnityEngine;
using UnityEngine.SceneManagement;

public class QuitHandler : MonoBehaviour
{
    public static System.EventHandler<System.EventArgs> OnQuit;

    public void LoadMainMenu()
    {
        SaveSystem.SaveGame("Quitting to Main Menu");
        SaveSystem.SetCurrentProfile(-1);
        PauseManager.SetPauseState(false);

        // Undo lazy singletons
        if (Player.GetInstance() != null)
            Player.GetInstance().ResetInventory();
        
        OnQuit?.Invoke(this, new System.EventArgs());

        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quitting game!");
    }
}
