using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorVisibility : MonoBehaviour
{
    void Update()
    {
        bool cursorActive = (UIManager.IsUIOpen() || UIArtifactMenus.IsArtifactOpen() || GameUI.instance.isMenuScene);
        if(cursorActive)
        {
            print("show");
        }
        else
        {
            print("hide");
        }
        Cursor.visible = cursorActive;
        Cursor.lockState = cursorActive ? CursorLockMode.None : CursorLockMode.Confined;
    }
}
