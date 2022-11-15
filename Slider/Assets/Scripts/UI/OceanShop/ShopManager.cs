using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

// ** THIS CLASS HAS BEEN UPDATED TO USE THE NEW SINGLETON BASE CLASS. PLEASE REPORT NEW ISSUES YOU SUSPECT ARE RELATED TO THIS CHANGE TO TRAVIS AND/OR DANIEL! **
public class ShopManager : Singleton<ShopManager>, ISavable
{
    //private static ShopManager _instance;
    public class OnTurnedItemInArgs : System.EventArgs
    {
        public string item;
    }

    public static event System.EventHandler<OnTurnedItemInArgs> OnTurnedItemIn;
    
    private int totalCreditCount;
    private int credits;
    private bool turnedInAnchor;
    private bool turnedInTreasureChest;
    private bool turnedInTreasureMap;
    private bool turnedInMushroom;
    private bool turnedInGoldenFish;
    private bool turnedInRock;
    private bool turnedInRose;
    private bool startedFinalQuest;
    private bool[] wasSliderOrDrinkCollectibleBought = new bool[9]; // from 4 to 9 and then three drinks


    private Collectible collectibleToActivateOnClose; // for slider 3 cutscene when you quit shop

    public States UIState { get => _uiState;  private set { _uiState = value; UpdateNavManagerCurrentMenu(); } }
    private States _uiState;

    public TalkStates TalkState { get => _talkState; private set { _talkState = value; UpdateNavManagerCurrentMenu(); } }
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

    public Button sellButton;
    public TextMeshProUGUI sellButtonText;
    public GameObject[] buyItemButtons;
    public GameObject[] talkSubPanels;

    public GameObject bottle1;
    public GameObject bottle2;
    public GameObject bottle3;

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
        InitializeSingleton();
        SetupBindingBehaviors();
    }

    #region SPECIAL BINDING BEHAVIOR SETUP
    // We need to special case this and use the unmanaged binding behaviors because pressing Action to open the shop also removes the binding behavior
    private List<BindingBehavior> bindingBehaviors = new List<BindingBehavior>();

    private void SetupBindingBehaviors()
    {
        bindingBehaviors = new List<BindingBehavior>();
        bindingBehaviors.Add(Controls.RegisterBindingBehavior(this, Controls.Bindings.UI.Pause, context => _instance.ExitCurrentPanel()));
        bindingBehaviors.Add(
            Controls.RegisterBindingBehavior(this, Controls.Bindings.UI.Navigate, context =>
            {
                if (!UINavigationManager.ButtonInCurrentMenuIsSelected()) { UINavigationManager.SelectBestButtonInCurrentMenu(); }
            })
        );
        // Using PlayerAction, UIClick, or OpenArtifact skips text typewriter effect or advances to the next dialogue
        bindingBehaviors.Add(Controls.RegisterBindingBehavior(this, Controls.Bindings.UI.OpenArtifact, context => _instance.shopDialogueManager.OnActionPressed(context)));
        bindingBehaviors.Add(Controls.RegisterBindingBehavior(this, Controls.Bindings.UI.Click, context => _instance.shopDialogueManager.OnActionPressed(context)));
        bindingBehaviors.Add(Controls.RegisterBindingBehavior(this, Controls.Bindings.Player.Action, context => _instance.shopDialogueManager.OnActionPressed(context)));
    }

    private void OnEnable()
    {
        bindingBehaviors.ForEach(bindingBehavior => bindingBehavior.isEnabled = true);
    }

    private void OnDisable()
    {
        bindingBehaviors.ForEach(bindingBehavior => bindingBehavior.isEnabled = false);
    }

    private void OnDestroy()
    {
        bindingBehaviors.ForEach(bindingBehavior => Controls.UnregisterBindingBehavior(bindingBehavior));
    }

    #endregion

    # region Save Load

    public void Save()
    {
        SaveSystem.Current.SetInt("oceanCredits", credits);
        SaveSystem.Current.SetInt("oceanTotalCreditCount", totalCreditCount);

        SaveSystem.Current.SetBool("oceanTurnedInAnchor", turnedInAnchor);
        SaveSystem.Current.SetBool("oceanTurnedInTreasureChest", turnedInTreasureChest);
        SaveSystem.Current.SetBool("oceanTurnedInTreasureMap", turnedInTreasureMap);
        SaveSystem.Current.SetBool("oceanTurnedInMushroom", turnedInMushroom);
        SaveSystem.Current.SetBool("oceanTurnedInGoldenFish", turnedInGoldenFish);
        SaveSystem.Current.SetBool("oceanTurnedInRock", turnedInRock);
        SaveSystem.Current.SetBool("oceanTurnedInRose", turnedInRose);
        SaveSystem.Current.SetBool("oceanStartedFinalQuest", startedFinalQuest);

        for (int i = 0; i < wasSliderOrDrinkCollectibleBought.Length; i++)
        {
            SaveSystem.Current.SetBool($"oceanWasSliderBought{i}", wasSliderOrDrinkCollectibleBought[i]);
            // Debug.Log($"Was collectible {i} bought: " + wasSliderOrDrinkCollectibleBought[i]);
        }
    }

    public void Load(SaveProfile profile)
    {
        credits = profile.GetInt("oceanCredits");
        totalCreditCount = profile.GetInt("oceanTotalCreditCount");

        turnedInAnchor = profile.GetBool("oceanTurnedInAnchor");
        turnedInTreasureChest = profile.GetBool("oceanTurnedInTreasureChest");
        turnedInTreasureMap = profile.GetBool("oceanTurnedInTreasureMap");
        turnedInMushroom = profile.GetBool("oceanTurnedInMushroom");
        turnedInGoldenFish = profile.GetBool("oceanTurnedInGoldenFish");
        turnedInRock = profile.GetBool("oceanTurnedInRock");
        turnedInRose = profile.GetBool("oceanTurnedInRose");
        startedFinalQuest = profile.GetBool("oceanStartedFinalQuest");

        for (int i = 0; i < wasSliderOrDrinkCollectibleBought.Length; i++)
        {
            wasSliderOrDrinkCollectibleBought[i] = profile.GetBool($"oceanWasSliderBought{i}");
            // Debug.Log($"Was collectible {i} bought: " + wasSliderOrDrinkCollectibleBought[i]);
        }

        UpdateBuyButtons();
    }
    
    #endregion

    public void CheckTavernKeep()
    {
        // rest of rewards

        int origCreditCount = totalCreditCount;
        
        // Anchor turn in is checked in OpenShop()

        if(PlayerInventory.Contains("Rose") && !turnedInRose)
        {
            turnedInRose = true;
            EarnCredits(1);
            shopDialogueManager.UpdateDialogue("Turn in Rose");
            OnTurnedItemIn?.Invoke(this, new OnTurnedItemInArgs {item = "A Delicate Rose" });
        }
        if (PlayerInventory.Contains("Treasure Chest") && !turnedInTreasureChest)
        {
            turnedInTreasureChest = true;
            EarnCredits(1);
            shopDialogueManager.UpdateDialogue("Turn in Treasure Chest");
            OnTurnedItemIn?.Invoke(this, new OnTurnedItemInArgs {item = "Cat Beard's Treasure" });
        }
        if (PlayerInventory.Contains("Magical Gem") && !turnedInTreasureMap)
        {
            turnedInTreasureMap = true;
            EarnCredits(1);
            shopDialogueManager.UpdateDialogue("Turn in Magical Gem");
            OnTurnedItemIn?.Invoke(this, new OnTurnedItemInArgs {item = "A Magical Gem" });
        }
        if (PlayerInventory.Contains("Mushroom") && !turnedInMushroom)
        {
            turnedInMushroom = true;
            EarnCredits(1);
            shopDialogueManager.UpdateDialogue("Turn in Mushroom");
            OnTurnedItemIn?.Invoke(this, new OnTurnedItemInArgs {item = "A Funky Mushroom" });
        }
        if (PlayerInventory.Contains("Golden Fish") && !turnedInGoldenFish)
        {
            turnedInGoldenFish = true;
            EarnCredits(1);
            shopDialogueManager.UpdateDialogue("Turn in Golden Fish");
            OnTurnedItemIn?.Invoke(this, new OnTurnedItemInArgs {item = "A Golden Fish" });
        }
        if (PlayerInventory.Contains("Rock") && !turnedInRock)
        {
            turnedInRock = true;
            EarnCredits(1);
            shopDialogueManager.UpdateDialogue("Turn in Rock");
            OnTurnedItemIn?.Invoke(this, new OnTurnedItemInArgs {item = "A Peculiar Rock" });
        }

        if (totalCreditCount - origCreditCount >= 1)
        {
            AudioManager.Play("Puzzle Complete");
        }
        else
        {
            AudioManager.Play("Artifact Error");
        }
        if (totalCreditCount - origCreditCount >= 2)
        {
            shopDialogueManager.UpdateDialogue("Turn in Multiple Items");
        }

        if (totalCreditCount == 8 && !startedFinalQuest)
        {
            startedFinalQuest = true;
            shopDialogueManager.UpdateDialogue("All Items Returned");
        }

        UpdateSellButton();
    }

    private void UpdateSellButton()
    {
        if (HasSellableItems())
        {
            sellButtonText.text = sellButtonText.text.Replace("*", "") + "*";
            sellButtonText.color = GameSettings.white;
            sellButton.enabled = false;
        }
        else
        {
            sellButtonText.text = sellButtonText.text.Replace("*", "");
            sellButtonText.color = GameSettings.darkGray;
            sellButton.enabled = true;
        }
    }

    private bool HasSellableItems()
    {
        return (
            (PlayerInventory.Contains("Rose") && !turnedInRose) ||
            (PlayerInventory.Contains("Treasure Chest") && !turnedInTreasureChest) ||
            (PlayerInventory.Contains("Magical Gem") && !turnedInTreasureMap) ||
            (PlayerInventory.Contains("Mushroom") && !turnedInMushroom) ||
            (PlayerInventory.Contains("Golden Fish") && !turnedInGoldenFish) ||
            (PlayerInventory.Contains("Rock") && !turnedInRock)
        );
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

    public void TryBuySliderOrDrink(int sliderNumber)
    {
        if (sliderNumber == 4 && !wasSliderOrDrinkCollectibleBought[0])
        {
          Collectible c = SGrid.Current.GetCollectible("Slider " + 4);
          c.DoOnCollect();
          wasSliderOrDrinkCollectibleBought[0] = true;
          AudioManager.Play("Puzzle Complete");
          UpdateBuyButtons();
        }
        else if (sliderNumber <= 9 && !wasSliderOrDrinkCollectibleBought[sliderNumber - 4] && credits > 0)
        {
          SpendCredits(1);
          Collectible c = SGrid.Current.GetCollectible("Slider " + sliderNumber);
          c.DoOnCollect();
          wasSliderOrDrinkCollectibleBought[sliderNumber - 4] = true;
          AudioManager.Play("Puzzle Complete");
          UpdateBuyButtons();
        }
        else if (sliderNumber > 9 && credits > 0)
        {
          SpendCredits(1);
          wasSliderOrDrinkCollectibleBought[sliderNumber - 4] = true;
          AudioManager.Play("Glass Clink");
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
        UpdateSellButton();
        OpenMainPanel();

        // TODO: varied intro lines here
        // CheckTavernKeep();

        if (PlayerInventory.Instance.GetHasCollectedAnchor() && !turnedInAnchor)
        {
            turnedInAnchor = true;
            EarnCredits(2); //change back to 2
            shopDialogueManager.UpdateDialogue("Turn in Anchor");
            OnTurnedItemIn?.Invoke(this, new OnTurnedItemInArgs {item = "A Trusty Anchor" });
        }

        if (startedFinalQuest)
        {
            shopDialogueManager.UpdateDialogue("All Items Returned");
        }
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
                if (!SaveSystem.Current.GetBool("oceanHasTalkedToBob") || !PlayerInventory.Instance.GetHasCollectedAnchor())
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
        //bottlesShowing.SetActive(true);
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
            if (!wasSliderOrDrinkCollectibleBought[i] && numTurnedOn < 4 && i < 6) //only display if not bought and less than 4 currently being displayed
            {
                buyItemButtons[i].SetActive(true);
                numTurnedOn += 1;
            }
            else
            {
                buyItemButtons[i].SetActive(false);
            }
        }
        if (numTurnedOn == 0)
        {
          int drinksBought = 0;
          for (int i = 6; i < buyItemButtons.Length; i++)
          {
            if (!wasSliderOrDrinkCollectibleBought[i])
            {
              buyItemButtons[i].SetActive(true);
            }
            else
            {
              drinksBought++;
            }
          }
          if (drinksBought == 1)
          {
            bottle1.SetActive(false);
          }
          else if (drinksBought == 2)
          {
            bottle2.SetActive(false);
          }
          else if (drinksBought == 3)
          {
            bottle3.SetActive(false);
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
