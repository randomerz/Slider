using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public Animator titleAnimator;
    public Animator textAnimator;

    private bool canPlay;

    void Start()
    {
        StartCoroutine(OpenCutscene());
    }

    private void Update()
    {
        if (Input.anyKey && canPlay)
        {
            SceneManager.LoadScene("Game");
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
