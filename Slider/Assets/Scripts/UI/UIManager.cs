
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UIManager : Singleton<UIManager>
{
    private ManagedBindingBehavior pauseBindingBehavior;

    private void Awake()
    {
        InitializeSingleton();

        pauseBindingBehavior = new(this, Controls.Bindings.UI.Pause, context => { if (!PauseManager.IsPaused) PauseManager.SetPauseState(true); });
        Controls.RegisterBindingBehavior(pauseBindingBehavior);

        // Disable the pause binding behavior when the pause menu is already open. If we don't do this, the pause menu will be closed and
        // then immediately re-opened.
        PauseManager.PauseStateChanged += SetPauseBindingBehaviorEnabledAfterEndOfFrame;
    }

    private void OnDestroy()
    {
        PauseManager.PauseStateChanged -= SetPauseBindingBehaviorEnabledAfterEndOfFrame;
    }

    private void SetPauseBindingBehaviorEnabledAfterEndOfFrame(bool isPaused)
    {
        StartCoroutine(ISetPauseBindingBehaviorEnabledAfterEndOfFrame(isPaused));
    }

    private IEnumerator ISetPauseBindingBehaviorEnabledAfterEndOfFrame(bool isPaused)
    {
        yield return new WaitForEndOfFrame();
        pauseBindingBehavior.isEnabled = !isPaused;
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
