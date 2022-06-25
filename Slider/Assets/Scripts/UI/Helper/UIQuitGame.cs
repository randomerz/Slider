using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIQuitGame : MonoBehaviour
{
    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit(0);
    }
}
