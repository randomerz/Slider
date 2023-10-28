
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UIManager : Singleton<UIManager>
{
    private void Awake()
    {
        InitializeSingleton();

        ManagedBindingBehavior bindingBehavior = new(this, Controls.Bindings.UI.Pause, context => PauseManager.SetPauseState(true));
        Controls.RegisterBindingBehavior(bindingBehavior);

        // Disable the pause binding behavior when the pause menu is already open. If we don't do this, the pause menu will be closed and
        // then immediately re-opened.
        PauseManager.PauseStateChanged += (isPaused) =>
        {
            _instance.StartCoroutine(SetPauseBindingBehaviorEnabledAfterEndOfFrame(bindingBehavior, isPaused));
        };
    }

    private IEnumerator SetPauseBindingBehaviorEnabledAfterEndOfFrame(BindingBehavior bindingBehavior, bool isPaused)
    {
        yield return new WaitForEndOfFrame();
        bindingBehavior.isEnabled = !isPaused;
    }

    public void LoadMainMenu()
    {
        SaveSystem.SaveGame("Quitting to Main Menu");
        SaveSystem.SetCurrentProfile(-1);
        PauseManager.SetPauseState(false);

        // Undo lazy singletons
        if (Player.GetInstance() != null)
            Player.GetInstance().ResetInventory();

        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quitting game!");
    }

}
