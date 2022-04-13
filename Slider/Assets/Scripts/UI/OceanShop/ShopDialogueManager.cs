using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class ShopDialogueManager : MonoBehaviour
{
    // TODO: serialize these somehow
    private bool canOverrideDialogue = true;
    public bool isFirstTime = true;
    private bool isVisiting = false;
    
    private TextMeshProUGUI currentText;
    private ShopDialogue currentDialogue;
    private Coroutine typingCoroutine;

    public enum TKSprite { // tavernkeep sprite
        Normal,
        Happy,
        Question,
        Angry,
    }

    [Header("References")]
    public ShopManager shopManager;
    public Image tkImage;
    public TextMeshProUGUI mainText;
    public TextMeshProUGUI buyText;
    public TextMeshProUGUI talkText;
    public TextMeshProUGUI dialogueText;

    public Sprite tkNormal;
    public Sprite tkHappy;
    public Sprite tkQuestion;
    public Sprite tkAngry;
    
    public class ShopDialogue
    {
        public Action onStart; // actions immediately called
        public string text; // text to set + animate depending on panel via ShopManager.uiState
        public TKSprite tkSprite;
        public Action onFinishAndAction; // this is for functions to call when done typing + press e, mostly for dialogue panel

        public ShopDialogue(Action onStart, string text, TKSprite tkSprite, Action onFinishAndAction)
        {
            this.onStart = onStart;
            this.text = text;
            this.tkSprite = tkSprite;
            this.onFinishAndAction = onFinishAndAction;
        }
    }

    private void OnDisable() 
    {
        // reset this whenever the player leaves
        canOverrideDialogue = true;
    }

    // when the player presses the 'E' key
    public void OnActionPressed(InputAction.CallbackContext context)
    {
        // Tr: Ignore event when the key/mouse is released.
        // I thought this was safer than changing the controls to only trigger on press.
        if (context.control.IsPressed())
        {
            if (typingCoroutine != null)
            {
                FinishTyping();
            }
            else
            {
                FinishAndAction();
            }
        }
    }



    public void SetDialogue(ShopDialogue dialogue)
    {
        dialogue.onStart?.Invoke();

        TextMeshProUGUI myText = null;
        switch (shopManager.uiState)
        {
            case ShopManager.State.Main:
                myText = mainText;
                break;
            case ShopManager.State.Buy:
                myText = buyText;
                break;
            case ShopManager.State.Talk:
                myText = talkText;
                break;
            case ShopManager.State.Dialogue:
                myText = dialogueText;
                break;
        }
        if (myText == null)
        {
            Debug.LogError("Tried SetDialogue when UIState was None!");
            return;
        }

        currentText = myText;
        currentDialogue = dialogue;
        tkImage.sprite = GetSprite(dialogue.tkSprite);

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeDialogue());
    }

    private Sprite GetSprite(TKSprite tkSprite)
    {
        switch (tkSprite)
        {
            case TKSprite.Normal:
                return tkNormal;
            case TKSprite.Happy:
                return tkHappy;
            case TKSprite.Question:
                return tkQuestion;
            case TKSprite.Angry:
                return tkAngry;
        }
        return null;
    }

    private IEnumerator TypeDialogue()
    {
        if (currentText == null || currentDialogue == null)
            Debug.LogError("Current Text or Current Dialogue are null!");

        currentText.text = "";

        char[] charArr = currentDialogue.text.ToCharArray();
        for (int i = 0; i < currentDialogue.text.Length; i++)
        {
            char nextChar = charArr[i];
            currentText.text += nextChar;

            if (GameSettings.punctuation.IndexOf(nextChar) != -1)
                yield return new WaitForSeconds(GameSettings.textSpeed);

            yield return new WaitForSeconds(GameSettings.textSpeed);
        }

        FinishTyping();
    }

    // if typing and press 'E'
    private void FinishTyping()
    {
        if (currentText == null || currentDialogue == null)
            Debug.LogError("Current Text or Current Dialogue are null!");

        StopCoroutine(typingCoroutine);
        typingCoroutine = null;
        currentText.text = currentDialogue.text;
    }

    // if not typing and press 'E'
    private void FinishAndAction()
    {
        if (currentDialogue != null)
        {
            ShopDialogue temp = currentDialogue;
            currentDialogue.onFinishAndAction?.Invoke();
            
            // if currentDialogue changed then we want to leave it changed, otherwise null it so it doesnt get called again
            if (currentDialogue == temp)
                currentDialogue = null;
        }
    }



    #region Dialogues
    
    public void UpdateDialogue()
    {
        if (!canOverrideDialogue)
            return;

        if (isFirstTime)
        {
            UpdateDialogue("First Time");
            return;
        }

        if (!PlayerInventory.GetHasCollectedAnchor())
        {
            UpdateDialogue("No Anchor");
            return;
        }

        if (isVisiting) 
        {
            UpdateDialogue("Visiting");
            return;
        }

        if ((SGrid.current as OceanGrid).GetCheckCompletion()) 
        {

            if ((SGrid.current as OceanGrid).GetIsCompleted())
            {
                UpdateDialogue("Ocean Complete");
                return;
            }
            else if (shopManager.uiState == ShopManager.State.Main)
            {
                UpdateDialogue("All Items Returned");
                return;
            }
        }

        if (shopManager.uiState == ShopManager.State.Main)
        {
            UpdateDialogue("Default Main");
            return;
        }

        if (shopManager.uiState == ShopManager.State.Buy)
        {
            UpdateDialogue("Default Buy");
            return;
        }
    }

    public void UpdateDialogue(string codeName)
    {
        if (!canOverrideDialogue)
            return;

        switch (codeName)
        {
            case "Default Main":
                SetDialogue(new ShopDialogue(
                    null,
                    "Buccaneer Bob, at your service.",
                    TKSprite.Normal,
                    null
                ));
                break;
                
            case "Default Buy":
                SetDialogue(new ShopDialogue(
                    null,
                    "Whaddya' want, landlubber",
                    TKSprite.Normal,
                    null
                ));
                break;
                
            case "Default Purchase":
                SetDialogue(new ShopDialogue(
                    null,
                    "Hmm, a wise choice!",
                    TKSprite.Happy,
                    null
                ));
                break;



            case "First Time":
                SetDialogue(new ShopDialogue(
                    () => { 
                        canOverrideDialogue = false;
                        shopManager.OpenDialoguePanel();
                    },
                    "Ahoy matey! Welcome to Buccaneer Bob's, the finest tavern in the land. Let's get you started with some drinks.",
                    TKSprite.Normal,
                    () => SetDialogue(

                new ShopDialogue(
                    null,
                    "Would you like Gunmaster's Gin, Raider's Rum, or Sea Wolf's Whiskey?",
                    TKSprite.Happy,
                    () => {
                        canOverrideDialogue = true;
                        isFirstTime = false;
                        UpdateDialogue("No Anchor");
                    }
                ))
                ));
                break;

            case "No Anchor":
                SetDialogue(new ShopDialogue(
                    () => { 
                        canOverrideDialogue = false;
                        shopManager.OpenDialoguePanel();
                    },
                    "Hold on now, it seems you've got no coin in those pockets!",
                    TKSprite.Normal,
                    () => SetDialogue(

                new ShopDialogue(
                    null,
                    "And you don't have an anchor? Every self-respecting pirate has one.",
                    TKSprite.Question,
                    () => SetDialogue(

                new ShopDialogue(
                    null,
                    "Quickly, go find one. I don't want people seein' an anchorless schmuck in my tavern. Bring one back and I'll see what I can do for you.",
                    TKSprite.Angry,
                    () => shopManager.CloseShop()
                ))
                ))
                ));
                break;


            
            case "Turn in Anchor":
                SetDialogue(new ShopDialogue(
                    null,
                    "Aye, that's a solid tool there. Here, have some coins on the house. Take a look at what we have and see if you're interested.",
                    TKSprite.Happy,
                    null
                ));
                break;
            
            case "Turn in Treasure Chest":
                SetDialogue(new ShopDialogue(
                    null,
                    "Nice chest you've got there. Now let's see what's hidden inside...",
                    TKSprite.Normal,
                    null
                ));
                break;
            
            case "Turn in Treasure Map":
                SetDialogue(new ShopDialogue(
                    null,
                    "Well look at this! A good old treasure map. Seems a bit, uh, out of its prime.",
                    TKSprite.Normal,
                    null
                ));
                break;
            
            case "Turn in Mushroom":
                SetDialogue(new ShopDialogue(
                    null,
                    "Never seen anything like this before. Maybe I should try it in a new cocktail...",
                    TKSprite.Question,
                    null
                ));
                break;
            
            case "Turn in Golden Fish":
                SetDialogue(new ShopDialogue(
                    null,
                    "Don't really know what I'm supposed to do with this one, but I'll take it.",
                    TKSprite.Question,
                    null
                ));
                break;
            
            case "Turn in Rock":
                SetDialogue(new ShopDialogue(
                    null,
                    "Is this what I think it is? How'd you even get this? You, my friend, are something special.",
                    TKSprite.Happy,
                    null
                ));
                break;
            
            case "Turn in Multiple Items":
                SetDialogue(new ShopDialogue(
                    null,
                    "A fine haul! You'd make a smashing pirate.",
                    TKSprite.Happy,
                    null
                ));
                break;
            
            case "All Items Returned":
                SetDialogue(new ShopDialogue(
                    () => {
                        (SGrid.current as OceanGrid).StartFinalChallenge();
                    },
                    "Looks like you've about explored the whole darned ocean. Time to put things in their rightful places.",
                    TKSprite.Normal,
                    null
                ));
                break;
            


            case "Ocean Complete":
                SetDialogue(new ShopDialogue(
                    () => { 
                        canOverrideDialogue = false;
                        shopManager.OpenDialoguePanel();
                    },
                    "Wish you could stay, but something tells me you have places to be.",
                    TKSprite.Normal,
                    () => {
                        // Cutscene stuff???
                        (SGrid.current as OceanGrid).ClearTreesToJungle();
                        shopManager.OpenMainPanel();

                        isVisiting = true;

                        SetDialogue(new ShopDialogue(
                    null,
                    "Safe travels! And welcome to the Jungle.",
                    TKSprite.Happy,
                    () => {
                        canOverrideDialogue = true;
                    }
                ));
                    }
                ));
                break;
            
            case "Visiting":
                SetDialogue(new ShopDialogue(
                    null,
                    "Visiting are you? Enjoy your stay!",
                    TKSprite.Happy,
                    null
                ));
                break;
            


            case "Who are You":
                SetDialogue(new ShopDialogue(
                    () => { 
                        canOverrideDialogue = false;
                        shopManager.OpenDialoguePanel();
                    },
                    "Grew up in Stonybrook Village, lived the quiet life but I always wanted more. Joined a pirate crew young, eventually retired and set up business here.",
                    TKSprite.Normal,
                    () => SetDialogue(

                new ShopDialogue(
                    null,
                    "Wanted to stay close to the sea and my people. Can't say I'm not lucky to have lived the life I have.",
                    TKSprite.Normal,
                    () => {
                        canOverrideDialogue = true;
                        shopManager.OpenTalkPanel();
                    }
                ))
                ));
                break;
            
            case "Business":
                SetDialogue(new ShopDialogue(
                    () => { 
                        canOverrideDialogue = false;
                        shopManager.OpenDialoguePanel();
                    },
                    "I run a tavern and pawn shop. Bring me anything interesting and you'll make a pretty penny",
                    TKSprite.Normal,
                    () => {
                        canOverrideDialogue = true;
                        shopManager.OpenTalkPanel();
                    }
                ));
                break;
            
            case "Shipwreck":
                SetDialogue(new ShopDialogue(
                    () => { 
                        canOverrideDialogue = false;
                        shopManager.OpenDialoguePanel();
                    },
                    "Heard the Black Trident broke upon some rocks earlier this week. Might be worth searching the area.",
                    TKSprite.Normal,
                    () => {
                        canOverrideDialogue = true;
                        shopManager.OpenTalkPanel();
                    }
                ));
                break;
            
            case "Down with the Ship":
                SetDialogue(new ShopDialogue(
                    () => { 
                        canOverrideDialogue = false;
                        shopManager.OpenDialoguePanel();
                    },
                    "Privateer Pete's ship took some damage on their last voyage. Guiding it back to shore is sure to secure you some booty.",
                    TKSprite.Normal,
                    () => {
                        canOverrideDialogue = true;
                        shopManager.OpenTalkPanel();
                    }
                ));
                break;
            
            case "The Veil":
                SetDialogue(new ShopDialogue(
                    () => { 
                        canOverrideDialogue = false;
                        shopManager.OpenDialoguePanel();
                    },
                    "There's a treacherous patch of foggy sea down south we call \"The Veil\". Who knows what the mist may be hiding.",
                    TKSprite.Normal,
                    () => {
                        canOverrideDialogue = true;
                        shopManager.OpenTalkPanel();
                    }
                ));
                break;
            
            case "Tangled Up":
                SetDialogue(new ShopDialogue(
                    () => { 
                        canOverrideDialogue = false;
                        shopManager.OpenDialoguePanel();
                    },
                    "Local fisherman has his buoys in a mess. Can't say this is the first time either.",
                    TKSprite.Normal,
                    () => {
                        canOverrideDialogue = true;
                        shopManager.OpenTalkPanel();
                    }
                ));
                break;
            
            case "Eruption":
                SetDialogue(new ShopDialogue(
                    () => { 
                        canOverrideDialogue = false;
                        shopManager.OpenDialoguePanel();
                    },
                    "Legends say the volcano only erupts when the rocks align, at least according to the old heads.",
                    TKSprite.Normal,
                    () => {
                        canOverrideDialogue = true;
                        shopManager.OpenTalkPanel();
                    }
                ));
                break;
            
            case "The Tavern":
                SetDialogue(new ShopDialogue(
                    () => { 
                        canOverrideDialogue = false;
                        shopManager.OpenDialoguePanel();
                    },
                    "Business has taken a hit recently, with The Cataclysm and all that, but we're still afloat. Talk to some of the customers here, you might learn something!",
                    TKSprite.Normal,
                    () => {
                        canOverrideDialogue = true;
                        shopManager.OpenTalkPanel();
                    }
                ));
                break;
            
            case "Aliens":
                SetDialogue(new ShopDialogue(
                    () => { 
                        canOverrideDialogue = false;
                        shopManager.OpenDialoguePanel();
                    },
                    "Some shifty-looking fellows walked in a while ago. They pay well so I can't complain, but something seems different about them.",
                    TKSprite.Question,
                    () => SetDialogue(

                new ShopDialogue(
                    null,
                    "Maybe they came in that strange flying saucer, ha!",
                    TKSprite.Normal,
                    () => {
                        canOverrideDialogue = true;
                        shopManager.OpenTalkPanel();
                    }
                ))
                ));
                break;
            
            case "Mushrooms":
                SetDialogue(new ShopDialogue(
                    () => { 
                        canOverrideDialogue = false;
                        shopManager.OpenDialoguePanel();
                    },
                    "Don't tell anyone, but I love slipping mushrooms into some of my drinks. Think of it as a secret ingredient of sorts.",
                    TKSprite.Question,
                    () => {
                        canOverrideDialogue = true;
                        shopManager.OpenTalkPanel();
                    }
                ));
                break;
            
            case "Swift Victory":
                SetDialogue(new ShopDialogue(
                    () => { 
                        canOverrideDialogue = false;
                        shopManager.OpenDialoguePanel();
                    },
                    "An old ship helmed by the good Captain Mako, anchored to the South. Used to run with his crew before I opened the tavern.",
                    TKSprite.Normal,
                    () => {
                        canOverrideDialogue = true;
                        shopManager.OpenTalkPanel();
                    }
                ));
                break;
            
            case "Stonybrook Village":
                SetDialogue(new ShopDialogue(
                    () => { 
                        canOverrideDialogue = false;
                        shopManager.OpenDialoguePanel();
                    },
                    "My hometown! A quaint little village to the West.",
                    TKSprite.Happy,
                    () => SetDialogue(new ShopDialogue(
                    null,
                    "Always used to play in this hidden cave behind the waterfall, made a lot of memories there. I should go back and visit sometime soon.",
                    TKSprite.Normal,
                    () => {
                        canOverrideDialogue = true;
                        shopManager.OpenTalkPanel();
                    }
                ))
                ));
                break;
            
            case "Canopy Town":
                SetDialogue(new ShopDialogue(
                    () => { 
                        canOverrideDialogue = false;
                        shopManager.OpenDialoguePanel();
                    },
                    "Treetop town in the jungle to the North.",
                    TKSprite.Normal,
                    () => SetDialogue(new ShopDialogue(
                    null,
                    "Heard some city folk are thinking of beginning some construction in the area. Can't say I'm too big of a fan of destroying nature.",
                    TKSprite.Angry,
                    () => {
                        canOverrideDialogue = true;
                        shopManager.OpenTalkPanel();
                    }
                ))
                ));
                break;
            
            case "The Impact Zone":
                SetDialogue(new ShopDialogue(
                    () => { 
                        canOverrideDialogue = false;
                        shopManager.OpenDialoguePanel();
                    },
                    "Big crater in the desert to the north.",
                    TKSprite.Normal,
                    () => SetDialogue(new ShopDialogue(
                    null,
                    "Some say tell tales of strange happenings, things that can't be explained. I don't believe a word of it.",
                    TKSprite.Question,
                    () => {
                        canOverrideDialogue = true;
                        shopManager.OpenTalkPanel();
                    }
                ))
                ));
                break;
            
            case "Everywhere Else":
                SetDialogue(new ShopDialogue(
                    () => { 
                        canOverrideDialogue = false;
                        shopManager.OpenDialoguePanel();
                    },
                    "Our world is pretty small. Haven't seen it all, but I've seen a lot.",
                    TKSprite.Normal,
                    () => SetDialogue(new ShopDialogue(
                    null,
                    "Sometimes I wonder if there's anything for us in the stars",
                    TKSprite.Normal,
                    () => {
                        canOverrideDialogue = true;
                        shopManager.OpenTalkPanel();
                    }
                ))
                ));
                break;

            default:
                Debug.LogError("Couldn't find dialogue with code: " + codeName);
                break;
        }
    }



    #endregion
}
