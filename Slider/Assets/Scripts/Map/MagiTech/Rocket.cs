using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Rocket : MonoBehaviour
{
    public GameObject rocket;
    public SpriteRenderer rocketSpriteRenderer;
    public Transform rocketStart;
    public Transform rocketEnd;
    public AnimationCurve rocketCurve;
    public CameraDolly rocketCameraDolly;
    public Animator catwalkAnimator;
    public Animator capAnimator;
    public Animator supportAnimator;
    public GameObject rocketFlamesParent;
    public List<ParticleSystem> rocketParticles;
    public PlayerActionHints hints;

    public SpriteRenderer playerSprite;
    public SpriteRenderer chadSprite;

    private bool isPlaying;
    private const float CAP_ANIMATION_LENGTH = 3.0f;
    private const float RAISE_ANIMATION_UNTIL_SPARKS = 1.25f;
    private const float ROCKET_DURATION = 8.0f;

    private AsyncOperation sceneLoad;
    private const string END_OF_GAME_SCENE = "EndOfGame";

    public void StartRocketCutscene()
    {
        if (isPlaying) 
            return;

        isPlaying = true;

        UIEffects.FadeToBlack(() => {
            StartCoroutine(RocketCutscene());
        }, disableAtEnd: false);
    }

    private IEnumerator RocketCutscene()
    {
        hints.DisableAllHints();
        playerSprite.gameObject.SetActive(false);
        chadSprite.enabled = false;

        sceneLoad = SceneManager.LoadSceneAsync(END_OF_GAME_SCENE);
        sceneLoad.allowSceneActivation = false; // "Don't initialize the new scene, just have it ready"

        // Really jank way of turning down music
        AudioManager.DampenMusic(this, 0, 6);
        CoroutineUtils.ExecuteAfterDelay(() => {
            AudioManager.StopDampen(this);
            AudioManager.StopMusic("MagiTech");
            AudioManager.Play("MagiTech Blast Off");
        }, this, 1);
        
        rocketCameraDolly.OnRollercoasterEnd += OnSkipCutscene;
        rocketCameraDolly.StartTrack(true);

        yield return new WaitForSeconds(3f);

        catwalkAnimator.SetTrigger("detach");
        AudioManager.PlayWithVolume("Hat Click", 0.5f);

        yield return new WaitForSeconds(1);

        capAnimator.SetTrigger("detach");
        AudioManager.PlayWithVolume("Hat Click", 0.5f);

        yield return new WaitForSeconds(CAP_ANIMATION_LENGTH - 0.5f);

        supportAnimator.SetTrigger("raise");

        yield return new WaitForSeconds(RAISE_ANIMATION_UNTIL_SPARKS);
        
        AudioManager.PlayWithVolume("Hat Click", 0.7f);
        AudioManager.Play("Rumble Increase 8s");

        rocketFlamesParent.SetActive(true);
        foreach (ParticleSystem ps in rocketParticles)
        {
            ps.Play();
        }

        rocketSpriteRenderer.sortingOrder = 25; // set to entity 25

        yield return new WaitForSeconds(1.75f);

        Player.GetSpriteRenderer().enabled = false;
        List<Vector2> shakeData = new()
        {
            Vector2.zero,
            new(2.5f, 0.5f),
            new(5f, 0.5f),
            new(9f, 0f)
        };
        CameraShake.ShakeCustom(shakeData);

        CoroutineUtils.ExecuteEachFrame(
            (x) => {
                rocket.transform.position = Vector3.Lerp(rocketStart.position, rocketEnd.position, x);
            },
            () => {},
            this,
            ROCKET_DURATION,
            rocketCurve
        );

        CoroutineUtils.ExecuteAfterDelay(() => {
            UIEffects.FadeToBlack(() => LoadEndOfGameScene(), 0.5f);
        }, this, ROCKET_DURATION - 2);
    }

    private void LoadEndOfGameScene()
    {
        SceneTransitionOverlayManager.ShowOverlay();

        SaveSystem.SaveGame("Ending Game");
        SaveSystem.Current.SetCompletionStatus(true);

        // Undo lazy singletons
        if (Player.GetInstance() != null)
            Player.GetInstance().ResetInventory();
            
        rocketCameraDolly.OnRollercoasterEnd -= OnSkipCutscene;

        sceneLoad.allowSceneActivation = true;
    }

    private void OnSkipCutscene(object sender, System.EventArgs e)
    {
        StopAllCoroutines();
        LoadEndOfGameScene();
    }
}
