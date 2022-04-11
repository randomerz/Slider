using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// All this does is give us an easy way to cover the screen while transitioning scenes.
/// We need this separate so we can make it DontDestroyOnLoad and not break anything.
/// </summary>
public class SceneTransitionOverlayManager : MonoBehaviour
{
    private static SceneTransitionOverlayManager instance;

    [SerializeField] private GameObject black;

    // Start is called before the first frame update
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
            SceneManager.sceneUnloaded += (Scene scene) => { HideOverlay(); };
        } else
        {
            Destroy(this);
        }
    }

    public static void ShowOverlay()
    {
        instance.black.SetActive(true);
    }
    public static void HideOverlay()
    {
        instance.black.SetActive(false);
    }
}
