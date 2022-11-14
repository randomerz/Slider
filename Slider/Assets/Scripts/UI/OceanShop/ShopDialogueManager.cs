using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class ShopDialogueManager : MonoBehaviour
{
    private bool canOverrideDialogue = true;
    
    private TMPTextTyper currentTyperText;
    private TMPSpecialText currentSpecialText;
    private ShopDialogue currentDialogue;
    // private Coroutine typingCoroutine;

    public enum TKSprite { // tavernkeep sprite
        Normal,
        Happy,
        Question,
        Angry,
        None,
    }

    [Header("References")]
    public ShopManager shopManager;
    public Image tkImage;

    // for typing
    public TMPTextTyper mainTyperText;
    public TMPSpecialText mainSpecialText;
    public TMPTextTyper buyTyperText;
    public TMPSpecialText buySpecialText;
    public TMPTextTyper talkTyperText;
    public TMPSpecialText talkSpecialText;
    public TMPTextTyper dialogueTyperText;
    public TMPSpecialText dialogueSpecialText;

    // for updating font styles
    public TextMeshProUGUI mainText;
    public TextMeshProUGUI buyText;
    public TextMeshProUGUI talkText;
    public TextMeshProUGUI dialogueText;

    public Sprite tkNormal;
    public Sprite tkHappy;
    public Sprite tkQuestion;
    public Sprite tkAngry;
    public Sprite tkNone;
    
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
            if (currentTyperText.TrySkipText())
            {
                // skip typing!
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

        TMPTextTyper typerText = null;
        TMPSpecialText specialText = null;
        switch (shopManager.UIState)
        {
            case ShopManager.States.Main:
                typerText = mainTyperText;
                specialText = mainSpecialText;
                break;
            case ShopManager.States.Buy:
                typerText = buyTyperText;
                specialText = buySpecialText;
                break;
            case ShopManager.States.Talk:
                typerText = talkTyperText;
                specialText = talkSpecialText;
                break;
            case ShopManager.States.Dialogue:
                typerText = dialogueTyperText;
                specialText = dialogueSpecialText;
                break;
        }
        if (typerText == null)
        {
            Debug.LogError("Tried SetDialogue when UIState was None!");
            return;
        }

        currentTyperText = typerText;
        currentSpecialText = specialText;
        currentDialogue = dialogue;
        tkImage.sprite = GetSprite(dialogue.tkSprite);

        currentSpecialText.StopEffects();
        currentTyperText.StartTyping(dialogue.text);
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
            case TKSprite.None:
                return tkNone;
        }
        return null;
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

        if (!SaveSystem.Current.GetBool("oceanHasTalkedToBob"))
        {
            UpdateDialogue("First Time");
            return;
        }

        if (!PlayerInventory.Instance.GetHasCollectedAnchor())
        {
            UpdateDialogue("No Anchor");
            return;
        }

        if (SaveSystem.Current.GetBool("oceanIsVisiting")) 
        {
            UpdateDialogue("Visiting");
            return;
        }

        if ((SGrid.Current as OceanGrid).GetCheckCompletion()) 
        {

            if ((SGrid.Current as OceanGrid).GetIsCompleted())
            {
                UpdateDialogue("Ocean Complete");
                return;
            }
            else if (shopManager.UIState == ShopManager.States.Main)
            {
                UpdateDialogue("All Items Returned");
                return;
            }
        }

        if (shopManager.UIState == ShopManager.States.Main)
        {
            UpdateDialogue("Default Main");
            return;
        }

        if (shopManager.UIState == ShopManager.States.Buy)
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
                        SaveSystem.Current.SetBool("oceanHasTalkedToBob", true);
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
                    () => { 
                        canOverrideDialogue = false;
                        SaveSystem.Current.SetBool("oceanBobNormal", true);
                        shopManager.OpenDialoguePanel();
                    },
                    "Aye, that's a solid tool there. Here, have some coins on the house. Ask me about jobs and I can help you get started.",
                    TKSprite.Happy,
                    () => {
                        shopManager.OpenMainPanel();

                        SetDialogue(new ShopDialogue(
                    null,
                    "There's a whole world out there to explore.",
                    TKSprite.Happy,
                    () => {
                        canOverrideDialogue = true;
                    }
                ));
                    }
                ));
                break;
            
            case "Turn in Treasure Chest":
                SetDialogue(new ShopDialogue(
                    null,
                    "A treasure chest? The One Piece is Real!",
                    TKSprite.Happy,
                    null
                ));
                break;
            
            case "Turn in Magical Gem":
                SetDialogue(new ShopDialogue(
                    null,
                    "A magical gem! I could enhance my drinks with some Sorcery...",
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
            
            case "Turn in Rose":
                SetDialogue(new ShopDialogue(
                    null,
                    "Sorry but I have a boyfriend. I'll pay so I don't owe you.",
                    TKSprite.Angry,
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
                        canOverrideDialogue = false;
                        shopManager.OpenDialoguePanel();
                        (SGrid.Current as OceanGrid).StartFinalChallenge();
                    },
                    "Never expected it, pal, but...",
                    TKSprite.Normal,
                    () => {
                        SetDialogue(new ShopDialogue(
                    null,
                    "Looks like you've about explored the whole darned ocean. Time to put things in their rightful places.",
                    TKSprite.Question,
                    () => {
                        SetDialogue(new ShopDialogue(
                    null,
                    "I've seen what you can do with that tablet of yours. If you can put our island back together, I can help you get to Canopy Town.",
                    TKSprite.Normal,
                    () => {
                        shopManager.OpenMainPanel();

                        SetDialogue(new ShopDialogue(
                    null,
                    "I'll bust out the axe and cut the trees above the tavern.",
                    TKSprite.Happy,
                    () => {
                        canOverrideDialogue = true;
                    }
                ));
                    }
                ));
                    }
                ));
                    }
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
                        // He disappear
                        SetDialogue(new ShopDialogue(
                    null,
                    "...",
                    TKSprite.None,
                    () => {
                        dialogueText.fontStyle = FontStyles.Italic;

                        SetDialogue(new ShopDialogue(
                    null,
                    "> You watch as he leaves, with haste and his dull axe.",
                    TKSprite.None,
                    () => {
                        SetDialogue(new ShopDialogue(
                    null,
                    "> He takes large strides, his massive figure easily pushing the tavern doors open.",
                    TKSprite.None,
                    () => {
                        SetDialogue(new ShopDialogue(
                    null,
                    "> A moment passes as you think about him cutting down the trees blocking the path. You sneak a peak through the window.",
                    TKSprite.None,
                    () => {
                        (SGrid.Current as OceanGrid).ClearTreesToJungle();

                        SetDialogue(new ShopDialogue(
                    null,
                    "*CHOP, CHOP, CHOP*",
                    TKSprite.None,
                    () => {
                        SetDialogue(new ShopDialogue(
                    null,
                    "> He cuts them down... so easily.",
                    TKSprite.None,
                    () => {
                        SetDialogue(new ShopDialogue(
                    null,
                    "> Of course, he'd wipe some sweat from his brow. But this was of no effort to a man like Bob.",
                    TKSprite.None,
                    () => {
                        SetDialogue(new ShopDialogue(
                    null,
                    "> He doesn't notice you watching through the window as he makes his way back in.",
                    TKSprite.None,
                    () => {
                        SetDialogue(new ShopDialogue(
                    null,
                    "> Axe on shoulder, he pushes the doors open and hops over the bar counter.",
                    TKSprite.None,
                    () => {
                        dialogueText.fontStyle = FontStyles.Normal;

                        SetDialogue(new ShopDialogue(
                    null,
                    "Safe travels! And welcome to the Jungle.",
                    TKSprite.Happy,
                    () => {
                        shopManager.OpenMainPanel();
                        SaveSystem.Current.SetBool("oceanIsVisiting", true);

                        SetDialogue(new ShopDialogue(
                    null,
                    "Bring me back a souvenier some time.",
                    TKSprite.Question,
                    () => {
                        canOverrideDialogue = true;
                    }
                ));
                    }
                ));
                    }
                ));
                    }
                ));
                    }
                ));
                    }
                ));
                    }
                ));
                    }
                ));
                    }
                ));
                    }
                ));
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
                    "I run a tavern and pawn shop. Bring me anything interesting and you'll make a pretty penny.",
                    TKSprite.Normal,
                    () => {
                        canOverrideDialogue = true;
                        shopManager.OpenTalkPanel();
                    }
                ));
                break;
            
            case "A Budding Romance":
                SetDialogue(new ShopDialogue(
                    () => { 
                        canOverrideDialogue = false;
                        shopManager.OpenDialoguePanel();
                    },
                    "I don't like to gossip, but... that Romeo REALLY wants to get with Juliet. Maybe you can wingman and get his message across.",
                    TKSprite.Question,
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
                    "Heard the Black Trident broke upon some rocks earlier today. Poor fellow. Might be worth searching the area.",
                    TKSprite.Normal,
                    () => {
                        canOverrideDialogue = true;
                        shopManager.OpenTalkPanel();
                    }
                ));
                break;
            
            case "Magical Spells":
                SetDialogue(new ShopDialogue(
                    () => { 
                        canOverrideDialogue = false;
                        shopManager.OpenDialoguePanel();
                    },
                    "Some fellow, Fezziwig, been messing around with some magics. This whole world's gone a bit crazy recently, hasn't it?",
                    TKSprite.Question,
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
                    TKSprite.Question,
                    () => SetDialogue(

                new ShopDialogue(
                    null,
                    "Tales tell of a song of three whole verses needed to navigate it. Think it started with \"West, South,\" but I forgot the rest. Maybe others remember.",
                    TKSprite.Normal,
                    () => {
                        canOverrideDialogue = true;
                        shopManager.OpenTalkPanel();
                    }
                ))
                ));
                break;
            
            case "Tangled Up":
                SetDialogue(new ShopDialogue(
                    () => { 
                        canOverrideDialogue = false;
                        shopManager.OpenDialoguePanel();
                    },
                    "Pierre the fisherman has his buoys in a mess. Can't say this is the first time either.",
                    TKSprite.Angry,
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

                        UIArtifactWorldMap.SetAreaStatus(Area.Jungle, ArtifactWorldMapArea.AreaStatus.silhouette);
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

                        UIArtifactWorldMap.SetAreaStatus(Area.Desert, ArtifactWorldMapArea.AreaStatus.silhouette);
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
                    "Sometimes I wonder if there's anything for us in the stars.",
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
