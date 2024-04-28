using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PausePanelHandler : MonoBehaviour
{
    [SerializeField] private UIMenu pausePanel;

    private void Awake()
    {
        PauseManager.PauseStateChanged += OpenPauseMenuWhenPaused;

        Controls.RegisterBindingBehavior(this, Controls.Bindings.UI.Pause, context => { if (!PauseManager.IsPaused) PauseManager.SetPauseState(true); });
    }

    private void OpenPauseMenuWhenPaused(bool isPaused)
    {
        if (isPaused)
        {
            pausePanel.Open();
        }
    }

    private void OnDestroy()
    {
        PauseManager.PauseStateChanged -= OpenPauseMenuWhenPaused;
    }

    // Called by OnClose on the Pause Panel UIMenu
    public void OnPausePanelClosed()
    {
        PauseManager.SetPauseState(false);
    }
}
