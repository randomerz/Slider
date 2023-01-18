using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TavernPassManager : MonoBehaviour, ISavable
{
    public ShopManager shopManager;
    public TavernPassButton[] tavernPassButtons;
    public TextMeshProUGUI rewardDescriptionText;
    public Slider progressBar;
    public float progressAnimationDuration;
    public AnimationCurve progressAnimationCurve;

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

    private void Start() 
    {
        InitializeProgressBar();
    }

    public void Save()
    {
        SaveSystem.Current.SetInt("oceanTavernPassDisplayedCredits", displayedCredits);
    }

    public void Load(SaveProfile profile)
    {
        displayedCredits = profile.GetInt("oceanTavernPassDisplayedCredits");
    }

    public void InitializeProgressBar()
    {
        int currentNumCredits = shopManager.GetCredits();
        float progress = CalculateProgressPercent(displayedCredits);
        progressBar.value = progress;

        // displayedCredits = currentNumCredits;
    }

    public void OnOpenTavernPass()
    {
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

        int from = displayedCredits;
        int to = Mathf.Min(currentNumCredits, nextTarget);

        yield return StartCoroutine(AnimateProgressBar(
            CalculateProgressPercent(from), 
            CalculateProgressPercent(to)
        ));

        if (to == nextTarget)
        {
            yield return GiveRewards(creditsToNextRewardIndex[nextTarget - 1]);

            IncrementButton();
        }


        displayedCredits = to;

        if (displayedCredits < currentNumCredits)
            StartCoroutine(UpdateProgressBar());
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

    private IEnumerator GiveRewards(int tier)
    {
        string rewardName = tavernPassButtons[tier].rewardName;
        switch (tier)
        {
            case 0:
                // Free Slider
                break;
            case 1:
                // All Sliders
                break;
            case 2:
                // Tavern Bell
                break;
            case 3:
                // Tavern Cat
                break;
            case 4:
                // Bob's Favor
                break;
        }

        Debug.Log($"Give reward for tier {tier}: {rewardName}");
        yield return new WaitForSeconds(0.5f);
    }

    public bool TavernPassHasReward()
    {
        int currentNumCredits = shopManager.GetCredits();
        return creditsToNextRewardIndex[currentNumCredits] != creditsToNextRewardIndex[displayedCredits] ||
               (currentNumCredits == 6 && displayedCredits != 6);
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
