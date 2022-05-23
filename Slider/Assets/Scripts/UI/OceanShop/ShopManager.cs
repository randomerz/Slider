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

    private Collectible collectibleToActivateOnClose; // for slider 3 cutscene when you quit shop

    public States UIState { get => _uiState;  private set { _uiState = value; UpdateNavManagerCurrentMenu(); } }
    private States _uiState;

    public TalkStates TalkState { get => _talkState; private set { _talkState = value; Debug.Log(_talkState); UpdateNavManagerCurrentMenu(); } }
    private TalkStates _talkState;

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

    public enum States 
    {
        None,
        Main,
        Buy,
        Talk,
        Dialogue,
    }

    public enum TalkStates
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
        {
            return;
        }

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

        // Pressing a navigation key selects a button is one is not already selected
        _instance.controls.UI.Navigate.performed += context =>
        {
            if (!UINavigationManager.ButtonInCurrentMenuIsSelected())
            {
                UINavigationManager.SelectBestButtonInCurrentMenu();
            }
        };
    }

    public void CheckTavernKeep()
    {
        // first talk
        // SGrid.current.ActivateSliderCollectible(3);
        if (!PlayerInventory.Contains("Slider 3", Area.Ocean))
        {
            collectibleToActivateOnClose = SGrid.current.GetCollectible("Slider 3");
        }

        // rest of rewards
        if (PlayerInventory.Instance.GetHasCollectedAnchor() && !turnedInAnchor)
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
            Collectible c = SGrid.current.GetCollectible("Slider " + sliderNumber);
            c.DoOnCollect();
            // SGrid.current.ActivateSliderCollectible(sliderNumber);
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

    private void UpdateNavManagerCurrentMenu()
    {
        switch (_uiState)
        {
            case States.None:
                break;
            case States.Main:
                UINavigationManager.CurrentMenu = mainPanel;
                break;
            case States.Buy:
                UINavigationManager.CurrentMenu = buyPanel;
                break;
            case States.Talk:
                if (_talkState == 0)
                {
                    UINavigationManager.CurrentMenu = talkPanel;
                } 
                else
                {
                    UINavigationManager.CurrentMenu = talkSubPanels[(int)_talkState];
                }
                break;
            case States.Dialogue:
                break;
        }
    }

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

        // For the 3rd collectible
        if (collectibleToActivateOnClose != null)
        {
            Debug.Log("nay nay");
            collectibleToActivateOnClose.DoPickUp();
            collectibleToActivateOnClose = null;
        }
    }

    // Called whenever you press 'Esc'
    public void ExitCurrentPanel()
    {
        switch (UIState)
        {
            case States.None:
                break;
            case States.Main:
                CloseShop();
                break;
            case States.Buy:
                OpenMainPanel();
                break;
            case States.Talk:
                if (TalkState == TalkStates.Default)
                    OpenMainPanel();
                else
                    SetTalkState(0);
                break;
            case States.Dialogue:
                if (shopDialogueManager.isFirstTime || !PlayerInventory.Instance.GetHasCollectedAnchor())
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
        UIState = States.None;
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
        UIState = States.Main;
        mainPanel.SetActive(true);
        shopDialogueManager.UpdateDialogue();
    }

    public void OpenBuyPanel()
    {
        CloseAllPanels();
        UIState = States.Buy;
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
        UIState = States.Talk;
        talkPanel.SetActive(true);
        shopDialogueManager.UpdateDialogue();
    }

    // for inspector
    public void SetTalkState(int state)
    {
        TalkState = (TalkStates)state;
        for (int i = 0; i < talkSubPanels.Length; i++)
        {
            talkSubPanels[i].SetActive(i == state);
        }
    }

    public void OpenDialoguePanel()
    {
        CloseAllPanels();
        UIState = States.Dialogue;
        dialoguePanel.SetActive(true);
        shopDialogueManager.UpdateDialogue();
    }

    #endregion

}
