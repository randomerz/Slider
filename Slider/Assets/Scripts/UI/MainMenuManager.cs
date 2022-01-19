using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class MainMenuManager : MonoBehaviour
{
    public Animator titleAnimator;
    public Animator textAnimator;

    public string sceneToLoad;

    private bool canPlay;

    void Start()
    {
        StartCoroutine(OpenCutscene());

        // InputSystem.onAnyButtonPress.CallOnce(ctrl => Debug.Log($"{ctrl} pressed"));
        InputSystem.onAnyButtonPress.CallOnce(ctrl => StartGame()); // this is really janky, we may want to switch to "press start"
    }

    private void OnEnable() {
        // Can't do onAnyButtonPress += StartGame;
        // :|
    }

    private void OnDisable() {
        
    }


    private void StartGame() 
    {
        if (canPlay)
        {
            SceneManager.LoadScene(sceneToLoad);
        }
        else 
        {
            InputSystem.onAnyButtonPress.CallOnce(ctrl => StartGame());
        }
    }

    private IEnumerator OpenCutscene()
    {
        yield return new WaitForSeconds(1f);
            
        CameraShake.ShakeIncrease(2.1f, 0.1f);

        yield return new WaitForSeconds(2f);

        CameraShake.Shake(0.25f, 0.2f);
        canPlay = true;

        yield return new WaitForSeconds(1f);

        textAnimator.SetBool("isVisible", true);
    }
}
