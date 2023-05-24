using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorVisibility : MonoBehaviour
{
     private void OnGUI() {
        bool cursorActive = (UIManager.IsUIOpen() || UIArtifactMenus.IsArtifactOpen() || GameUI.instance.isMenuScene);
        Cursor.visible = cursorActive;
        Cursor.lockState = cursorActive ? CursorLockMode.None : CursorLockMode.Confined;
    }
}
