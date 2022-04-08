using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShopManager : MonoBehaviour
{
    private static ShopManager _instance;

    private int totalCreditCount;
    private int credits; // TODO: serialize
    private bool turnedInAnchor;
    private bool turnedInTreasureChest;
    private bool turnedInTreasureMap;
    private bool turnedInMushroom;
    private bool turnedInGoldenFish;
    private bool turnedInRock;
    private bool startedFinalQuest;
    private bool[] wasSliderCollectibleBought = new bool[6]; // from 4 to 9

    public State uiState { get; private set; }
    public TalkState talkState { get; private set; }

    //  === References ===
    public ShopDialogueManager shopDialogueManager;
    public ShopCoinToBeHadManager coinToBeHadManager;

    // this is like the entire thing
    public GameObject uiShopPanel;
    // these are sub sections (the bottom ones)
    public GameObject mainPanel;
    public GameObject buyPanel;
    public GameObject talkPanel;
    public GameObject dialoguePanel;

    public GameObject[] buyItemButtons;
    public GameObject[] talkSubPanels;

    private InputSettings controls;

    public enum State 
    {
        None,
        Main,
        Buy,
        Talk,
        Dialogue,
    }

    public enum TalkState
    {
        Default,
        CoinToBeHad,
        RumorMill,
        TheWorld
    }

    private void Awake() 
    {
        _instance = this;
        _instance.controls = new InputSettings();
        LoadBindings();
    }

    private void OnEnable() 
    {
        controls.Enable();
    }

    private void OnDisable() 
    {
        controls.Disable();    
    }

    public static void LoadBindings()
    {
        if (_instance == null)
            return;
            
        var rebinds = PlayerPrefs.GetString("rebinds");
        if (!string.IsNullOrEmpty(rebinds))
        {
            _instance.controls.LoadBindingOverridesFromJson(rebinds);
        }
        _instance.controls.UI.Pause.performed += context => _instance.ExitCurrentPanel();

        // Using PlayerAction, UIClick, or OpenArtifact skips text typewriter effect or advances to the next dialogue
        _instance.controls.UI.OpenArtifact.performed += context => _instance.shopDialogueManager.OnActionPressed(context);
        _instance.controls.UI.Click.performed += context => _instance.shopDialogueManager.OnActionPressed(context);
        _instance.controls.Player.Action.performed += context => _instance.shopDialogueManager.OnActionPressed(context);
    }

    public void CheckTavernKeep()
    {
        // first talk
        SGrid.current.ActivateSliderCollectible(3);

        // rest of rewards
        if (PlayerInventory.GetHasCollectedAnchor() && !turnedInAnchor)
        {
            turnedInAnchor = true;
            EarnCredits(2);
            shopDialogueManager.UpdateDialogue("Turn in Anchor");
        }

        int origCreditCount = totalCreditCount;
        if (PlayerInventory.Contains("Treasure Chest") && !turnedInTreasureChest)
        {
            turnedInTreasureChest = true;
            EarnCredits(1);
            shopDialogueManager.UpdateDialogue("Turn in Treasure Chest");
        }
        if (PlayerInventory.Contains("Treasure Map") && !turnedInTreasureMap)
        {
            turnedInTreasureMap = true;
            EarnCredits(1);
            shopDialogueManager.UpdateDialogue("Turn in Treasure Map");
        }
        if (PlayerInventory.Contains("Mushroom") && !turnedInMushroom)
        {
            turnedInMushroom = true;
            EarnCredits(1);
            shopDialogueManager.UpdateDialogue("Turn in Mushroom");
        }
        if (PlayerInventory.Contains("Golden Fish") && !turnedInGoldenFish)
        {
            turnedInGoldenFish = true;
            EarnCredits(1);
            shopDialogueManager.UpdateDialogue("Turn in Golden Fish");
        }
        if (PlayerInventory.Contains("Rock") && !turnedInRock)
        {
            turnedInRock = true;
            EarnCredits(1);
            shopDialogueManager.UpdateDialogue("Turn in Rock");
        }
        
        if (totalCreditCount - origCreditCount >= 1)
        {
            AudioManager.Play("Puzzle Complete");
        }
        if (totalCreditCount - origCreditCount >= 2)
        {
            shopDialogueManager.UpdateDialogue("Turn in Multiple Items");
        }

        if (totalCreditCount == 7 && !startedFinalQuest)
        {
            startedFinalQuest = true;
        }

        if (startedFinalQuest)
        {
            shopDialogueManager.UpdateDialogue("All Items Returned");
        }

        // just activate in order for now
        //for (int i = 4; i < Mathf.Min(4 + totalCreditCount, 10); i++)
        //{
          //  ActivateSliderCollectible(i);
        //}

        

        // check final quest on completing all others
        // if (totalCreditCount == 7 + 1 && !startedFinalQuest) // todo: remove +1 later
        // {
        //     startedFinalQuest = true;
        //     SGrid.current.checkCompletion = true;
        //     SGrid.OnGridMove += SGrid.CheckCompletions;

        //     AudioManager.Play("Puzzle Complete");
        // }
    }

    public int GetCredits()
    {
        return credits;
    }

    private void EarnCredits(int value)
    {
        totalCreditCount += value;
        credits += value;
    }

    public void SpendCredits(int value)
    {
        credits -= value;
    }

    public void TryBuySlider(int sliderNumber)
    {
        if ((credits > 0 || sliderNumber == 4) && !wasSliderCollectibleBought[sliderNumber - 4])
        {
            if (sliderNumber != 4) SpendCredits(1);
            SGrid.current.ActivateSliderCollectible(sliderNumber);
            wasSliderCollectibleBought[sliderNumber - 4] = true;
            AudioManager.Play("Puzzle Complete");

            UpdateBuyButtons();
        }
        else
        {
            AudioManager.Play("Artifact Error");
        }
    }

    #region UI

    // Called when you walk up to the tavernkeeper and press 'E'
    public void OpenShop()
    {
        UIManager.PauseGameGlobal();
        UIManager.canOpenMenus = false;
        
        // scuffed parts
        Player.SetCanMove(false);
        Time.timeScale = 1;


        uiShopPanel.SetActive(true);
        OpenMainPanel();

        
        CheckTavernKeep();
    }

    public void CloseShop()
    {
        SetTalkState(0);

        CloseAllPanels();
        uiShopPanel.SetActive(false);
        UIManager.canOpenMenus = true;
        UIManager.CloseUI();

        Player.SetCanMove(true);
    }

    // Called whenever you press 'Esc'
    public void ExitCurrentPanel()
    {
        switch (uiState)
        {
            case State.None:
                break;
            case State.Main:
                CloseShop();
                break;
            case State.Buy:
                OpenMainPanel();
                break;
            case State.Talk:
                if (talkState == TalkState.Default)
                    OpenMainPanel();
                else
                    SetTalkState(0);
                break;
            case State.Dialogue:
                if (shopDialogueManager.isFirstTime || !PlayerInventory.GetHasCollectedAnchor())
                {
                    CloseShop();
                    break;
                }
                OpenTalkPanel();
                break;
        }
    }


    public void CloseAllPanels()
    {
        uiState = State.None;
        mainPanel.SetActive(false);
        buyPanel.SetActive(false);
        talkPanel.SetActive(false);
        dialoguePanel.SetActive(false);

        // whenever panels are switched
        coinToBeHadManager.UpdateButtons();
        coinToBeHadManager.UpdateTexts();
    }

    public void OpenMainPanel()
    {
        CloseAllPanels();
        uiState = State.Main;
        mainPanel.SetActive(true);
        shopDialogueManager.UpdateDialogue();
    }

    public void OpenBuyPanel()
    {
        CloseAllPanels();
        uiState = State.Buy;
        buyPanel.SetActive(true);
        UpdateBuyButtons();
        shopDialogueManager.UpdateDialogue();
    }

    public void UpdateBuyButtons()
    {
        int numTurnedOn = 0;

        for (int i = 0; i < buyItemButtons.Length; i++)
        {
            if (!wasSliderCollectibleBought[i] && numTurnedOn < 4)
            {
                buyItemButtons[i].SetActive(true);
                numTurnedOn += 1;
            }
            else
            {
                buyItemButtons[i].SetActive(false);
            }
        }
    }

    public void OpenTalkPanel()
    {
        CloseAllPanels();
        uiState = State.Talk;
        talkPanel.SetActive(true);
        shopDialogueManager.UpdateDialogue();
    }

    // for inspector
    public void SetTalkState(int state)
    {
        talkState = (TalkState)state;
        for (int i = 0; i < talkSubPanels.Length; i++)
        {
            talkSubPanels[i].SetActive(i == state);
        }
    }

    public void OpenDialoguePanel()
    {
        CloseAllPanels();
        uiState = State.Dialogue;
        dialoguePanel.SetActive(true);
        shopDialogueManager.UpdateDialogue();
    }

    #endregion

}
