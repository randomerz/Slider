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

    [SerializeField] private GameObject errorCanvas;
    [SerializeField] private GameObject windowsError;
    [SerializeField] private GameObject macError;
    [SerializeField] private GameObject linuxError;


    public static Action<bool> TimeGlitchPauseStateChanged;
    private bool startedTimeGlitch;
    private bool finishedTimeGlitch;

    private void Start()
    {
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
        TimeGlitchInit();
        yield return new WaitForSecondsRealtime(2f);
        print("0");
        bool playerClicked = false;
        BindingBehavior clickBehavior = Controls.RegisterBindingBehavior(this, Controls.Bindings.UI.Click, context => {
            if(context.control.IsPressed())
            {
                playerClicked = true;
            }
        });

        float t = 0;
        while (t < 5 && !playerClicked)
        {
            t += Time.unscaledDeltaTime;
        }
        Controls.UnregisterBindingBehavior(clickBehavior);      

        UIEffects.FadeToWhite(disableAtEnd: false, speed: 0.5f, alpha: 0.5f, useUnscaledTime:true);

        //show "slider.exe has stoped working"
        
        yield return new WaitForSecondsRealtime(5);
        //flash white
        //screenshot?
        //cleanup
        UIEffects.FlashWhite(useUnscaledTime:true);
        AudioManager.Play("Slide Explosion");
        AudioManager.Play("Hurt");

        UpdateMap();

        // TODO: Update UI icons

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
        
    }

    private void TimeGlitchCleanup()
    {
        CoroutineUtils.ExecuteAfterEndOfFrame(() =>{
            TimeGlitchPauseStateChanged?.Invoke(false);
            PauseManager.RemovePauseRestriction(gameObject);
            Player.SetCanMove(true);
            Time.timeScale = 1;
            finishedTimeGlitch = true;
        }, this);
    }
    
    private void UpdateMap()
    {
        objectsFromPastRoot.SetActive(false);
        objectsFromFutureRoot.SetActive(true);

        stileTilemap.materials = newMaterialsTilemap;

        housingTracker.enabled = true;
    }
}