using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PausePanelHandler : MonoBehaviour
{
    [SerializeField] private UIMenu pausePanel;

    private void Awake()
    {
        PauseManager.PauseStateChanged += (bool isPaused) => { if (isPaused) pausePanel.Open(); };
    }

    // Called by OnClose on the Pause Panel UIMenu
    public void OnPausePanelClosed()
    {
        PauseManager.SetPauseState(false);
    }
}
