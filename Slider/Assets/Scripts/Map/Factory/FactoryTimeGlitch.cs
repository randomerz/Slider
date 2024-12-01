using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using UnityEngine.InputSystem;
public class FactoryTimeGlitch : MonoBehaviour
{
    public const string TIME_GLITCH_SAVE_STRING = "factoryTimeGlitchOccured";
    public const int TIME_GLITCH_ISLAND_ID = 7;

    [SerializeField] private GameObject objectsFromPastRoot;
    [SerializeField] private GameObject objectsFromFutureRoot;

    [SerializeField] private STileTilemap stileTilemap;
    [SerializeField] private Tilemap newMaterialsTilemap;

    [SerializeField] private UIHousingTracker housingTracker;

    [SerializeField] private PastLargeCrystal largeCrystal;

    [SerializeField] private GameObject errorCanvas;
    [SerializeField] private CanvasGroup transparentBackgroundGroup;
    [SerializeField] private GameObject windowsError;
    [SerializeField] private GameObject macError;
    [SerializeField] private GameObject linuxError;


    public static Action<bool> TimeGlitchPauseStateChanged;
    private bool startedTimeGlitch;
    private bool finishedTimeGlitch;

    private void Start()
    {
        errorCanvas.SetActive(false);
        if (SaveSystem.Current.GetBool(TIME_GLITCH_SAVE_STRING))
        {
            UpdateMap();
        }
    }

    private void OnDestroy()
    {
        if(startedTimeGlitch && !finishedTimeGlitch)
        {
            TimeGlitchCleanup();
        }
    }

    public void DoTimeGlitch()
    {
        if (SaveSystem.Current.GetBool(TIME_GLITCH_SAVE_STRING))
        {
            return;
        }
        StartCoroutine(TimeGlitch());
    }
    
    private IEnumerator TimeGlitch()
    {
        StartCoroutine(largeCrystal.BTTFParticleAnimation(2));

        yield return new WaitForSeconds(1f);

        // Do screen tearing here

        AudioManager.Play("Glass Crash Before");

        yield return new WaitForSeconds(0.75f);

        TimeGlitchInit();

        yield return new WaitForSecondsRealtime(2f);

        bool playerClicked = false;
        BindingBehavior clickBehavior = Controls.RegisterBindingBehavior(this, Controls.Bindings.UI.Click, context => {
            if(context.control.IsPressed())
            {
                playerClicked = true;
            }
        });

        float t = 0;
        while (t < 3 && !playerClicked)
        {
            t += Time.unscaledDeltaTime;
        }
        Controls.UnregisterBindingBehavior(clickBehavior);      

        UIEffects.FadeToWhite(callback: () => ShowErrorMessage(), disableAtEnd: false, speed: 0.5f, alpha: 0.5f, useUnscaledTime:true);
        
        yield return new WaitForSecondsRealtime(5);

        errorCanvas.SetActive(false);
        UIEffects.FadeFromScreenshot(callbackEnd: () =>  UpdateMap(), type: UIEffects.ScreenshotEffectType.PORTAL);
        UIEffects.FlashWhite(callbackEnd: () => SpawnParticles(), useUnscaledTime:true);

        largeCrystal.StopParticles();

        AudioManager.Play("Glass Crash After");

        SGrid.Current.gridTilesExplored.SetTileExplored(TIME_GLITCH_ISLAND_ID, false);

        TimeGlitchCleanup();
    }

    private void TimeGlitchInit()
    {
        CoroutineUtils.ExecuteAfterEndOfFrame(() =>{
            TimeGlitchPauseStateChanged?.Invoke(true);
            PauseManager.AddPauseRestriction(gameObject);
            Player.SetCanMove(false);
            Time.timeScale = 0;
            startedTimeGlitch = true;
            SaveSystem.Current.SetBool(TIME_GLITCH_SAVE_STRING, true);
        }, this);
    }

    private void ShowErrorMessage()
    {
        string platform = Application.platform.ToString();
        if(platform.Contains("OSX"))
        {
            macError.SetActive(true);
        }
        else if(platform.Contains("Linux"))
        {
            linuxError.SetActive(true);
        }
        else
        {
            StartCoroutine(WindowsErrorCoroutine());
        }
        errorCanvas.SetActive(true);
    }

    private IEnumerator WindowsErrorCoroutine()
    {
        float t = 0;
        while (t < 1)
        {
            float x = t;
            
            transparentBackgroundGroup.alpha = x;

            yield return new WaitForSecondsRealtime(0.05f);
            t += 0.05f;
        }
        windowsError.SetActive(true);
    }

    private void TimeGlitchCleanup()
    {
        CoroutineUtils.ExecuteAfterEndOfFrame(() =>{
            TimeGlitchPauseStateChanged?.Invoke(false);
            PauseManager.RemovePauseRestriction(gameObject);
            Player.SetCanMove(true);
            Time.timeScale = 1;
            errorCanvas.SetActive(false);
            finishedTimeGlitch = true;
        }, this);
    }
    
    private void UpdateMap()
    {
        Debug.Log($"update map");
        objectsFromPastRoot.SetActive(false);
        objectsFromFutureRoot.SetActive(true);

        stileTilemap.materials = newMaterialsTilemap;

        housingTracker.enabled = true;
    }

    private void SpawnParticles()
    {
        for(int i = 0; i < 50; i++)
        {
            ParticleManager.SpawnParticle(ParticleType.MiniSparkle, GetRandomPosition() + Player.GetPosition());
        }
    }

    private Vector3 GetRandomPosition()
    {
        float r = UnityEngine.Random.Range(0f, 10f);
        float t = UnityEngine.Random.Range(0f, 360f);

        return new Vector2(r * Mathf.Cos(t), r * Mathf.Sin(t));
    }
}