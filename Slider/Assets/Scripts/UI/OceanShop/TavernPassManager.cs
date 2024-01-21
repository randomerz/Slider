using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TavernPassManager : MonoBehaviour, ISavable
{
    public GameObject tavernJukebox;
    public GameObject tavernCat;

    public ShopManager shopManager;
    public TavernPassRewardEffect rewardEffect;
    public UICanvasZoomer canvasZoomer;
    public TavernPassButton[] tavernPassButtons;
    public TextMeshProUGUI rewardDescriptionText;
    public Slider progressBar;
    public float progressAnimationDuration;
    public AnimationCurve progressAnimationCurve;

    private Dictionary<int, int> creditsToCurrentRewardIndex = new Dictionary<int, int>() {
        {0, 0},
        {1, 1},
        {2, 2},
        {3, 2},
        {4, 3},
        {5, 3},
        {6, 4},
    };
    private Dictionary<int, int> creditsToNextRewardIndex = new Dictionary<int, int>() {
        {0, 1},
        {1, 2},
        {2, 3},
        {3, 3},
        {4, 4},
        {5, 4},
        {6, 4},
    };
    private Dictionary<int, int> creditsToNextCreditsRequired = new Dictionary<int, int>() {
        {0, 1},
        {1, 2},
        {2, 4},
        {3, 4},
        {4, 6},
        {5, 6},
        {6, 6},
    };

    // private int currentNumCredits;
    private int displayedCredits;
    private int currentSelectedIndex;
    private bool didInitialize;

    private void Start() 
    {
        InitializeProgressBar();
    }

    public void Save()
    {
        SaveSystem.Current.SetInt("oceanTavernPassDisplayedCredits", displayedCredits);

        // SaveSystem.Current.SetBool("oceanTavernPassFreeTierComplete", tavernPassButtons[0].isComplete);
    }

    public void Load(SaveProfile profile)
    {
        displayedCredits = profile.GetInt("oceanTavernPassDisplayedCredits");

        if (tavernJukebox != null) tavernJukebox.SetActive(displayedCredits >= 2);
        if (tavernCat != null) tavernCat.SetActive(displayedCredits >= 4);

        // if (SaveSystem.Current.GetBool("oceanTavernPassFreeTierComplete"))
        // {
        //     tavernPassButtons[0].SetComplete(true);
        // }
    }

    public void InitializeProgressBar()
    {
        if (didInitialize)
            return;
        didInitialize = true;

        int currentNumCredits = shopManager.GetCredits();
        float progress = CalculateProgressPercent(displayedCredits);
        progressBar.value = progress;

        tavernPassButtons[0].SetComplete(HasSlider5());
        for (int i = 1; i <= displayedCredits; i++)
        {
            tavernPassButtons[creditsToCurrentRewardIndex[i]].SetComplete(true);
        }
    }

    private bool HasSlider5()
    {
        return SGrid.Current.GetStile(5).isTileActive;
    }

    public void OnOpenTavernPass()
    {
        if (!didInitialize)
        {
            InitializeProgressBar();
        }

        if (!tavernPassButtons[0].isComplete)
        {
            ShopManager.CanClosePanel = false;
            string rewardName = tavernPassButtons[0].rewardName;
            Sprite rewardSprite = tavernPassButtons[0].rewardImage.sprite;

            UICanvasScreenShake.Shake(2, 20);
            rewardEffect.StartEffect(rewardName, rewardSprite, () => {
                GiveRewards(0);
                IncrementButton();
                ShopManager.CanClosePanel = true;
            },
            () => tavernPassButtons[0].PlayEffect());

            return;
        }

        SelectButton(creditsToNextRewardIndex[displayedCredits]);
        TryUpdateProgressBar();
    }

    public void TryUpdateProgressBar()
    {
        int currentNumCredits = shopManager.GetCredits();
        if (currentNumCredits == displayedCredits)
            return;

        StartCoroutine(UpdateProgressBar());
    }

    private float CalculateProgressPercent(int credits)
    {
        // 0 = 0
        // 1 = 0.25
        // 2 = 0.5
        // 3 = 0.625
        // 4 = 0.75
        // 5 = 0.875
        // 6 = 1

        if (credits <= 2)
            return credits / 4.0f;
        else
            return (credits + 2) / 8.0f;
    }

    private IEnumerator UpdateProgressBar()
    {
        int currentNumCredits = shopManager.GetCredits();
        int nextTarget = creditsToNextCreditsRequired[displayedCredits];
        if (displayedCredits >= currentNumCredits || displayedCredits >= nextTarget)
        {
            yield break;
        }

        // Start update
        ShopManager.CanClosePanel = false;

        int from = displayedCredits;
        int to = Mathf.Min(currentNumCredits, nextTarget);

        canvasZoomer.DoZoomIn(progressAnimationDuration);
        UICanvasScreenShake.ShakeIncrease(progressAnimationDuration - 0.25f, 30);
        yield return StartCoroutine(AnimateProgressBar(
            CalculateProgressPercent(from), 
            CalculateProgressPercent(to)
        ));

        if (to == nextTarget)
        {
            int tier = creditsToCurrentRewardIndex[nextTarget];
            string rewardName = tavernPassButtons[tier].rewardName;
            Sprite rewardSprite = tavernPassButtons[tier].rewardImage.sprite;

            if (tier == tavernPassButtons.Length - 1) // if giving final tier
            {
                // We transition into dialogue panel instead
                canvasZoomer.DoZoomReleaseBig(2);
                rewardEffect.StartEffect(rewardName, rewardSprite, () => {
                    displayedCredits = to;
                    ShopManager.CanClosePanel = true;

                    GiveRewards(tier);
                },
                () => {
                    DisableButtonPassRenderTextures();
                    tavernPassButtons[tier].PlayEffect();
                });
                yield break;
            }

            canvasZoomer.DoZoomReleaseBig(2);
            yield return rewardEffect.StartEffectCoroutine(
                rewardName, 
                rewardSprite, 
                () => GiveRewards(tier), 
                () => {
                    DisableButtonPassRenderTextures();
                    tavernPassButtons[tier].PlayEffect();
                }
            );

            IncrementButton();
        }
        else
        {
            canvasZoomer.DoZoomReleaseSmall(1);
        }


        displayedCredits = to;

        ShopManager.CanClosePanel = true;
        // Finish update

        if (displayedCredits < currentNumCredits)
            StartCoroutine(UpdateProgressBar());
    }

    private void DisableButtonPassRenderTextures()
    {
        foreach (TavernPassButton b in tavernPassButtons)
        {
            b.DisableRenderTexture();
        }
    }

    private IEnumerator AnimateProgressBar(float from, float to)
    {
        float t = 0;
        while (t < progressAnimationDuration)
        {
            float x = progressAnimationCurve.Evaluate(t);
            progressBar.value = Mathf.Lerp(from, to, x);

            yield return null;
            t += Time.deltaTime;
        }

        progressBar.value = to;
    }

    private void GiveRewards(int tier)
    {
        switch (tier)
        {
            case 0:
                // Free Slider
                SGrid.Current.CollectSTile(5);
                break;

            case 1:
                // All Sliders
                for (int i = 4; i <= 9; i++)
                {
                    // Given in free tier; we don't want it to flash
                    if (i == 5)
                        continue;

                    SGrid.Current.CollectSTile(i);
                }
                break;

            case 2:
                // Tavern Jukebox
                if (tavernJukebox != null) tavernJukebox.SetActive(true);
                SaveSystem.Current.SetBool("oceanTavernJukebox", true);
                break;

            case 3:
                // Tavern Cat
                if (tavernCat != null) tavernCat.SetActive(true);
                break;

            case 4:
                // Bob's Favor
                shopManager.StartFinalChallenge();
                break;
        }

        tavernPassButtons[tier].SetComplete(true);
        SaveSystem.SaveGame("Tavern Pass Reward");

        // string rewardName = tavernPassButtons[tier].rewardName;
        // Debug.Log($"Give reward for tier {tier}: {rewardName}");
    }

    public bool TavernPassHasReward()
    {
        if (!didInitialize)
        {
            InitializeProgressBar();
        }
        
        int currentNumCredits = shopManager.GetCredits();
        return creditsToNextRewardIndex[currentNumCredits] != creditsToNextRewardIndex[displayedCredits] || // if next reward is available
               (currentNumCredits == 6 && displayedCredits != 6) || // or final reward is available
               !tavernPassButtons[0].isComplete; // or on first open
    }

    #region UI

    public void DeselectButtons()
    {
        foreach (TavernPassButton b in tavernPassButtons)
        {
            b.Deselect();
        }   
    }

    public void SelectButton(int index)
    {
        if (0 > index || index > tavernPassButtons.Length - 1)
            return;

        currentSelectedIndex = index;
        DeselectButtons();
        tavernPassButtons[index].Select();
        rewardDescriptionText.text = tavernPassButtons[index].rewardName;
    }

    public void DecrementButton()
    {
        if (currentSelectedIndex > 0)
            SelectButton(currentSelectedIndex - 1);
    }

    public void IncrementButton()
    {
        if (currentSelectedIndex < tavernPassButtons.Length - 1)
            SelectButton(currentSelectedIndex + 1);
    }

    #endregion
}
