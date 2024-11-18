using System.Collections;
using System.Collections.Generic;
using Localization;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
public class CreditsManager : MonoBehaviour
{
    [System.Serializable]
    public class CreditsTileMove
    {
        public List<CreditsTile> tiles;
        public List<Direction> directions;
    }

    private const string MAIN_MENU_SCENE = "MainMenu";
    private const float MOVE_DURATION = 1f;
    private const float REST_DURATION = 5f;
    private const int TILE_WIDTH = 17;


    protected BindingHeldBehavior dollySkipBindingBehavior;

    [SerializeField] private GameObject backgroundTextDanielAndVGDev;
    [SerializeField] private GameObject backgroundTextThankYouForPlaying;
    
    [SerializeField] private Slider skipPromptSlider;
    [SerializeField] private TextMeshProUGUI skipPromptText;
    [SerializeField] private float holdDurationToSkip = 1f;
    [SerializeField] private AnimationCurve holdAnimationCurve;
    public AnimationCurve tileAnimationCurve;

    public List<CreditsTileMove> creditsTileMoves;

    private AsyncOperation sceneLoad;
    
    // Shouldn't this be handled by the text replace anyway?
    // 
    // #region Localization
    // enum SkipType
    // {
    //     Default
    // }
    // public Dictionary<string, LocalizationPair> TranslationTable { get; } = 
    //     IDialogueTableProvider.InitializeTable(
    //         new Dictionary<SkipType, string>
    //         {
    //             { SkipType.Default, "Skip" }
    //         });
    // #endregion

    private void Start()
    {
       UIEffects.FadeFromBlack();
       dollySkipBindingBehavior = new BindingHeldBehavior(
                Controls.Bindings.Player.Action,
                holdDurationToSkip,
                onHoldStarted: (ignored) => { InitializeSkipPrompt(); },
                onEachFrameWhileButtonHeld: UpdateSkipPrompt,
                onHoldCompleted: SkipToEndOfCredits,
                onButtonReleasedEarly: (ignored) => { InitializeSkipPrompt(); }
        );
        Controls.RegisterBindingBehavior(dollySkipBindingBehavior);  
        AudioManager.PlayMusic("Main Menu");
        AudioManager.SetGlobalParameter("MainMenuActivated", 1);
        StartCoroutine(Credits());
    }

    private IEnumerator Credits()
    {
        yield return new WaitForSeconds(REST_DURATION/2);
        for(int i = 0; i < creditsTileMoves.Count; i++)
        {
            CreditsTileMove ctm = creditsTileMoves[i];

            for(int j = 0; j < ctm.tiles.Count; j++)
            {
                StartCoroutine(MoveTile(ctm.tiles[j], ctm.directions[j]));
            }
            AudioManager.PlayWithVolume("Slide Rumble", 0.5f);
            CameraShake.Shake(0.5f, 0.1f);
            yield return new WaitForSeconds(MOVE_DURATION);
            AudioManager.PlayWithVolume("Slide Explosion", 0.5f);
            CameraShake.Shake(0.2f, 0.2f);
            yield return new WaitForSeconds(REST_DURATION);

            backgroundTextDanielAndVGDev.SetActive(false);
            backgroundTextThankYouForPlaying.SetActive(true);
        }
        EndCredits();
    }

    private IEnumerator MoveTile(CreditsTile stile, Direction direction)
    {
        Vector3 startPos = stile.transform.position;
        Vector3 endPos = DirectionUtil.D2V3(direction) * TILE_WIDTH + stile.transform.position;
        float t = 0;
        Vector3 pos = stile.transform.position;
        if(startPos.Equals(Vector3.zero))
        {
            stile.OnStartSlideOut?.Invoke();
        }
        if(endPos.Equals(Vector3.zero))
        {
            stile.OnStartSlideIn?.Invoke();
        }
        while (t < MOVE_DURATION)
        {
            t += Time.deltaTime;
            
            float s = tileAnimationCurve.Evaluate(Mathf.Min(t / MOVE_DURATION, 1));
            pos = Vector3.Lerp(startPos, endPos, s);
            stile.transform.position = pos;
            yield return null;
        }
        stile.transform.position = endPos;
        if(endPos.Equals(Vector3.zero))
        {
            stile.OnEndSlideIn?.Invoke();
            foreach(CreditsTile.DelayedEvent e in stile.events)
            {
                CoroutineUtils.ExecuteAfterDelay(() => e.Event?.Invoke(), stile, e.Delay);
            }
        }
    }

    private void InitializeSkipPrompt()
    {
        // skipPromptText.text = this.GetLocalizedSingle(SkipType.Default);
        skipPromptSlider.value = 0;
        skipPromptSlider.gameObject.SetActive(true);
    }

    private void UpdateSkipPrompt(float durationButtonHeldSoFar)
    {
        skipPromptSlider.value = holdAnimationCurve.Evaluate(durationButtonHeldSoFar / holdDurationToSkip);
    }

    protected void SkipToEndOfCredits(InputAction.CallbackContext ignored)
    {
        EndCredits();
    }

    private void EndCredits()
    {
        skipPromptSlider.gameObject.SetActive(false);
        Controls.UnregisterBindingBehavior(dollySkipBindingBehavior);
        GoToMainMenu();
    }

    private void GoToMainMenu()
    {
        sceneLoad = SceneManager.LoadSceneAsync(MAIN_MENU_SCENE);
        sceneLoad.allowSceneActivation = false; // "Don't initialize the new scene, just have it ready"

        UIEffects.FadeToBlack(() => {
            sceneLoad.allowSceneActivation = true;
        }, disableAtEnd: false);
    }

}
