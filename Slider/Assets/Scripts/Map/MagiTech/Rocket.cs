using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Rocket : MonoBehaviour
{
    public CameraDolly rocketCameraDolly;
    public GameObject rocket;
    public SpriteRenderer rocketSpriteRenderer;
    public Transform rocketStart;
    public Transform rocketEnd;
    public AnimationCurve rocketCurve;
    private bool isPlaying;
    private float rocketDuration = 8.0f;

    public void StartRocketCutscene()
    {
        if(isPlaying) return;
        StartCoroutine(RocketCutscene());
    }

    private IEnumerator RocketCutscene()
    {
        rocketSpriteRenderer.sortingOrder = 25; // set to entity 25
        UIEffects.FadeToBlack(disableAtEnd: false);
        yield return new WaitForSeconds(1.1f);
        Player.GetSpriteRenderer().enabled = false;
        List<Vector2> shakeData = new()
        {
            Vector2.zero,
            new(2.5f, 1f),
            new(5f, 1f),
            new(9f, 0f)
        };
        CameraShake.ShakeCustom(shakeData);
        // CameraShake.Shake(rocketDuration, 1);
        rocketCameraDolly.StartTrack(true);

        float t = 0f;
        while (t < rocketDuration)
        {
            float y = rocketCurve.Evaluate(t / rocketDuration);
            Vector3 pos = new Vector3(rocketStart.position.x,
                                      Mathf.Lerp(rocketStart.position.y, rocketEnd.position.y, y));
            rocket.transform.position = pos;
            t += Time.deltaTime;
            
            yield return null;
        }

        UIEffects.FadeToBlack(LoadEndOfGameScene, disableAtEnd:false);
    }

    private void LoadEndOfGameScene()
    {
        SaveSystem.SaveGame("Ending Game");
        SaveSystem.SetCurrentProfile(-1);

        // Undo lazy singletons
        if(Player.GetInstance() != null)
            Player.GetInstance().ResetInventory();

        SceneManager.LoadScene("EndOfGame");
    }
}
