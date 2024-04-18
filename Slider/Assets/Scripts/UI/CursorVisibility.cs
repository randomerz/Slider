using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorVisibility : MonoBehaviour
{
    private void OnGUI() {
        bool cursorActive = PauseManager.IsPaused || UIArtifactMenus.IsArtifactOpen() || GameUI.instance.isMenuScene || DebugUIManager.isDebugOpen;
        bool controller = Controls.CurrentControlScheme == Controls.CONTROL_SCHEME_CONTROLLER;
        cursorActive = !controller && (!SettingsManager.Setting<bool>(Settings.HideCursor).CurrentValue || cursorActive);
        Cursor.visible = cursorActive;
        Cursor.lockState = cursorActive ? CursorLockMode.None : CursorLockMode.Confined;
    }
}
