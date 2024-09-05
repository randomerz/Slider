using System;
using System.Collections;
using System.Collections.Generic;
using Localization;
using SliderVocalization;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class ShopDialogueManager : MonoBehaviour, IDialogueTableProvider
{
    private bool canOverrideDialogue = true;
    
    private TMPTextTyper currentTyperText;
    private TMPSpecialText currentSpecialText;
    private ShopDialogue currentDialogue;
    // private Coroutine typingCoroutine;

    private float actionPressedBuffer = 0;
    private const float ACTION_BUFFER_DURATION = 0.1f; // After using action, can't use it for this long

    public enum ShopDialogueCode
    {
        DefaultMain,
        FirstTime,
        NoAnchor,
        FirstTimeWithAnchor,
        TurnInAnchor,
        ExplainTavernPass,
        TurnInTreasureChest,
        TurnInMagicalGem,
        TurnInMushroom,
        TurnInGoldenFish,
        TurnInRock,
        TurnInRose,
        TurnInMultipleItems,
        StartFinalChallenge,
        FinalChallengeReminder,
        OceanComplete,
        Visiting,
        WhoAreYou,
        Business,
        ABuddingRomance,
        Shipwreck,
        MagicalSpells,
        TheVeil,
        TangledUp,
        Eruption,
        TheTavern,
        Aliens,
        Mushrooms,
        SwiftVictory,
        StonybrookVillage,
        CanopyTown,
        TheImpactZone,
        TheFlats,
        Space,
    }

    private static string[] mainShopDialgoue = new[]
    {
        "Welcome back sailor!",
        "Welcome to the Wooden Wheel!",
        "Find anything interesting today?",
        "Looks like a good day to set sail!",
        "Buccaneer Bob, at your service.",
        "What can I do for you today?",
        "Ahoy there!",
    };

    public Dictionary<string, LocalizationPair> TranslationTable { get; } = IDialogueTableProvider.InitializeTable(
        new Dictionary<ShopDialogueCode, string[]>
        {
            { ShopDialogueCode.DefaultMain, mainShopDialgoue } ,

            {
                ShopDialogueCode.FirstTime, new[] {
                    "Ahoy matey! Welcome to Buccaneer Bob's. Here in the Shifting Seas, freedom is our creed. Just don't get lost -- the oceans are filled with treasure.",
                    "Now what are we looking for? Drinks, perhaps? Or are you interested in my all-new Tavern Pass?",
                }
            },

            {
                ShopDialogueCode.NoAnchor, new[]
                {
                    "Hold on now, it seems you've got no coin in those pockets!",
                    "And you don't have an <#2e44f0>anchor</color>? Every self-respecting pirate has an anchor!",
                    "Quickly now, go find one. I don't want people seein' an anchorless schmuck in my tavern. Bring one back and I'll see what I can do for you.",
                }
            },
            {
                ShopDialogueCode.FirstTimeWithAnchor, new[]
                {
                    "What's that? You've already got an <#2e44f0>anchor</color>?! We've got a legend in the making, no doubt!",
                }
            },
            {
                ShopDialogueCode.TurnInAnchor, new[]
                {
                    "Aye, that's a solid tool there. In fact, that was awfully quick. Keep it up, and you might just make a name for yourself around here.",
                }
            },
            {
                ShopDialogueCode.ExplainTavernPass, new[]
                {
                    "Well, we've got a new business model- Oh, what's that? Your cat, <var>Cat</var>? I wager that's what the boys saw floating eastward!",
                    "You'd better keep yourself safe - military was deployed there. Even blocked off access from the Shifting Seas. If you really want to go, I reckon you ought to go around the long way, through Canopy Town and the Factory.",
                    "But don't worry, I've got something perfect for you! Look pal, we're trying a new, \"Business\" model. Remember that tavern pass I mentioned?",
                    "How it works is, YOU bring me treasures from around the sea, and I'LL give you some points. Get enough points and you'll get a reward! For the last reward, I can even help you reach your cat.",
                    "I'm calling it, \"Bob's Tavern Pass\"! As my first subscriber, you get the first tier for free.",
                }
            },
            {
                ShopDialogueCode.TurnInTreasureChest, new[]
                {
                    "A treasure chest? The One Piece is Real!",
                }
            },
            {
                ShopDialogueCode.TurnInMagicalGem, new[]
                {
                    "A magical gem! Heard them hard-hat types are mining for these babies in the mountains.",
                }
            },
            {
                ShopDialogueCode.TurnInMushroom, new[]
                {
                    "Never seen anything like this before. Maybe I should try it in a new cocktail...",
                }
            },
            {
                ShopDialogueCode.TurnInGoldenFish, new[]
                {
                    "Catch of the day, eh? They say a golden fish brings good tidings.",
                }
            },
            {
                ShopDialogueCode.TurnInRock, new[]
                {
                    "Is this what I think it is? How'd you even get this? You, my friend, are something special.",
                }
            },
            {
                ShopDialogueCode.TurnInRose, new[]
                {
                    "You one of those hopeless romantic types? I appreciate the sentiment."
                }
            },
            {
                ShopDialogueCode.TurnInMultipleItems, new[]
                {
                    "A fine haul! You'd make a smashing pirate.",
                }
            },
            {
                ShopDialogueCode.StartFinalChallenge, new[]
                {
                    "Well... to be honest, never expected you to finish my battle pass. You really wanna find <var>Cat</var>?, eh?",
                    "Now that you've about explored the whole damn ocean, I reckon it's about time to put things in their rightful places.",
                    "If you fix my island with that tablet of yours, I can help you cross over to Canopy Town. Only need the tavern part and the two beaches.",
                    "I'll bust out the old axe and cut the trees north of the tavern.",
                }
            },
            {
                ShopDialogueCode.FinalChallengeReminder, new[]
                {
                    "If you fix my island with that tablet of yours, I can help you cross over to Canopy Town.",
                    "I'll bust out the old axe and cut the trees above the tavern.",
                }
            },
            {
                ShopDialogueCode.OceanComplete, new[]
                {
                    "...",
                    "Wish you could stay, but something tells me you have places to be.",
                    "> You watch as he leaves, with haste and his sharp axe.",
                    "> He takes large strides, his massive figure easily pushing the tavern doors open.",
                    "> You sneak a peak through the window.",
                    "*CHOP, CHOP, CHOP*",
                    "> He cuts them down... so easily.",
                    "> Of course, he'd wipe some sweat from his brow. But this was of no effort to a man like Bob.",
                    "> He doesn't notice you watching through the window as he makes his way back in.",
                    "> Axe on shoulder, he pushes the doors open and hops over the bar counter.",
                    "Safe travels! And welcome to the Jungle.",
                    "Bring me back a souvenir.",
                }
            },
            {
                ShopDialogueCode.Visiting, new[]
                {
                    "Visiting are you? Enjoy your stay!",
                }
            },
            {
                ShopDialogueCode.WhoAreYou, new[]
                {
                    "Grew up in Stonybrook Village, lived the quiet life but I always wanted more. Joined a pirate crew young, eventually retired and took up business here.",
                    "I wanted to stay close to the sea and my people. Can't say I'm not lucky to have lived the life I have.",
                }
            },
            {
                ShopDialogueCode.Business, new[]
                {
                    "The Tavern Pass we're trying out is pretty neat, huh?",
                    "Don't ask too much about the rewards. Oh, but for the final reward, I'll do anything you want!",
                    "Looks like under your name we have registered that you want to go to the Jungle up north.",
                    "Anyways, I can definitely help you on your way to Canopy Town if you finish our Tavern Pass.",
                    "Remember: If you ever run into trouble, Bob's is the one-stop-shop for all the seafaring advice you'll ever need!",
                    "Remember: If you ever run into trouble, Bob's is the one-stop-shop for all the seafaring advice you'll ever need!",
                }
            },
            {
                ShopDialogueCode.ABuddingRomance, new[]
                {
                    "I don't like to gossip, but... that Romeo REALLY wants to get with Juliet. It's really been weighing him down. If you can be a solid wingman, maybe he can get his message across.",
                    "Beware of disturbing the seas though. Too much water can sink even the most heartfelt messages.",
                }
            },
            {
                ShopDialogueCode.Shipwreck, new[]
                {
                    "Heard the Black Trident broke upon some rocks earlier today. Poor Catbeard. Might be worth searching the area.",
                }
            },
            {
                ShopDialogueCode.MagicalSpells, new[]
                {
                    "Some funky fellow, Fezziwig, been messing around with some magicks. This whole world's gone a bit crazy recently, hasn't it?",
                }
            },
            {
                ShopDialogueCode.TheVeil, new[]
                {
                    "There's a treacherous patch of foggy sea down south we call \"The Veil\". Who knows what the mist may be hiding.",
                    "Tales tell of a song of THREE pairs of verses needed to navigate it. Think it started with \"West, South,\", but I forgot the rest.",
                    "Maybe others know the rest of the song? Or it might be written down, eroded away in sand.",
                }
            },
            {
                ShopDialogueCode.TangledUp, new[]
                {
                    "Pierre the fisherman has his buoys in a mess. Can't say this is the first time either.",
                }
            },
            {
                ShopDialogueCode.Eruption, new[]
                {
                    "Legends say the volcano only erupts when the rocks align, at least according to the old heads.",
                }
            },
            {
                ShopDialogueCode.TheTavern, new[]
                {
                    "Business has taken a hit recently, with The Cataclysm and all that, but we're still afloat. Talk to some of the customers here, you might learn something!",
                }
            },
            {
                ShopDialogueCode.Aliens, new[]
                {
                    "Some shifty-looking fellows walked in a while ago. They pay well so I can't complain, but something seems different about them.",
                    "They looked in an awful hurry, carrying something with them too.",
                }
            },
            {
                ShopDialogueCode.Mushrooms, new[]
                {
                    "Don't tell anyone, but I love slipping mushrooms into some of my drinks. Think of it as a secret ingredient of sorts.",
                }
            },
            {
                ShopDialogueCode.SwiftVictory, new[]
                {
                    "An old ship helmed by the good Captain Mako, anchored to the South. Used to run with his crew before I opened the tavern.",
                    "I don't mean to be rude but... he's a bit washed up now.",
                }
            },
            {
                ShopDialogueCode.StonybrookVillage, new[]
                {
                    "My hometown! A quaint little village to the West.",
                    "Always used to play in this hidden cave behind the waterfall, made a lot of memories there. I should go back and visit sometime soon.",
                }
            },
            {
                ShopDialogueCode.CanopyTown, new[]
                {
                    "A treetop town in the jungle to the North. Used to be nice and quiet, perfect for a picnic.",
                    "Some blowhard named Barron hurried into the place and started up all sorts of business and noise.",
                }
            },
            {
                ShopDialogueCode.TheImpactZone, new[]
                {
                    "Big, ancient crater that formed a desert to the north.",
                    "Some tell tall tales of strange happenings, things that can't be explained. I don't believe a word of it.",
                }
            },
            {
                ShopDialogueCode.TheFlats, new[]
                {
                    "The plains to the east, now a hot-zone full of our nation's military.",
                    "I'm not sure what's happening, but word is it has to do with The Cataclysm.",
                    "If your cat really is there, then you'll have to go the long way around. Head north to Canopy Town, then east to the Factory. Then you should have access the military zone.",
                }
            },
            {
                ShopDialogueCode.Space, new[]
                {
                    "You want to know how to get up amongst the stars?",
                    "Well, the folks at the Magic Research Institute have been working on a rocket, so it might be your lucky day. Here, I'll show you on your map.",
                }
            },
        });

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

    public VocalizableParagraph Vocalizer => vocalizer;
    [SerializeField]
    private VocalizableParagraph vocalizer;
    
    public class ShopDialogue
    {
        public Action onStart; // actions immediately called
        public string text; // text to set + animate depending on panel via ShopManager.uiState
        public string textTranslated;
        public TKSprite tkSprite;
        public Action onFinishAndAction; // this is for functions to call when done typing + press e, mostly for dialogue panel

        public ShopDialogue(Action onStart, LocalizationPair text, TKSprite tkSprite, Action onFinishAndAction)
        {
            this.onStart = onStart;
            this.text = text.original;
            this.textTranslated = text.TranslatedFallbackToOriginal;
            this.tkSprite = tkSprite;
            this.onFinishAndAction = onFinishAndAction;
        }
    }

    private void OnDisable() 
    {
        // reset this whenever the player leaves
        canOverrideDialogue = true;
    }

    private void Update()
    {
        actionPressedBuffer -= Time.deltaTime;
    }

    // when the player presses the 'E' key
    public void OnActionPressed(InputAction.CallbackContext context)
    {
        if (actionPressedBuffer > 0)
        {
            return;
        }
        actionPressedBuffer = ACTION_BUFFER_DURATION;

        // Tr: Ignore event when the key/mouse is released.
        // I thought this was safer than changing the controls to only trigger on press.
        if (context.control.IsPressed())
        {
            if (currentTyperText != null && currentTyperText.TrySkipText())
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
        vocalizer.Stop();
        
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
        
        ShopManager.CanClosePanel = shopManager.UIState != ShopManager.States.Dialogue;

        currentTyperText = typerText;
        currentSpecialText = specialText;
        currentDialogue = dialogue;
        tkImage.sprite = GetSprite(dialogue.tkSprite);
        
        string toVocalize;
        string typed;
        
        // if translation exists, vocalize the original english version and type out the localized version
        // shrink the font to 1/3 of original size due to a "Tiny" font being used for English
        if (dialogue.textTranslated != null)
        {
            toVocalize = currentTyperText.ParseTextPure(dialogue.text, true);
            
            // AT: not sure if do the same replacement here...
            // localizedMessage = localizedMessage.Replace('‘', '\'').Replace('’', '\'').Replace("…", "...");
            
            typed = currentTyperText.StartTyping(dialogue.textTranslated);
        }
        else
        {
            typed = currentTyperText.StartTyping(dialogue.text);
            toVocalize = typed;
        }
        
        if (AudioManager.useVocalizer)
        {
            // The following doesn't actually fix Bob's voice being completely silenced due to shop camera positioning
            // Instead in FMOD Bob's voice has the spatializer turned off
            // vocalizer.transform.position = currentTyperText.transform.position;

            int maxPhonemes = 5;
            
            float totalDuration = vocalizer.SetText(toVocalize, NPCEmotes.Emotes.None, maxPhonemes);
        
            currentTyperText.SetTextSpeed(totalDuration / typed.Length);
        
            AudioManager.DampenMusic(this, 0.6f, totalDuration + 0.2f);
        
            if (vocalizer.GetVocalizationState() == VocalizerCompositeState.CanPlay)
            {
                vocalizer.StartReadAll(NPCEmotes.Emotes.None);
            }
        }
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
        vocalizer.Stop();
        if (currentDialogue != null)
        {
            ShopDialogue temp = currentDialogue;
            currentDialogue.onFinishAndAction?.Invoke();
            
            // if currentDialogue changed then we want to leave it changed, otherwise null it so it doesnt get called again
            if (currentDialogue == temp)
            {   
                ShopManager.CanClosePanel = true;
                currentDialogue = null;
            }
        }
    }



    #region Dialogues

    // for public access
    public void SetSprite(TKSprite sprite)
    {
        tkImage.sprite = GetSprite(sprite);
    }
    
    public void UpdateDialogue()
    {
        if (!canOverrideDialogue)
            return;

        if (!SaveSystem.Current.GetBool("oceanHasTalkedToBob"))
        {
            UpdateDialogueEnum(ShopDialogueCode.FirstTime);
            return;
        }

        if (!PlayerInventory.Instance.GetHasCollectedAnchor())
        {
            UpdateDialogueEnum(ShopDialogueCode.NoAnchor);
            return;
        }

        if (SaveSystem.Current.GetBool("oceanIsVisiting")) 
        {
            UpdateDialogueEnum(ShopDialogueCode.Visiting);
            return;
        }

        if ((SGrid.Current as OceanGrid).GetCheckCompletion()) 
        {

            if ((SGrid.Current as OceanGrid).GetIsCompleted())
            {
                UpdateDialogueEnum(ShopDialogueCode.OceanComplete);
                return;
            }
            else if (shopManager.UIState == ShopManager.States.Main)
            {
                UpdateDialogueEnum(ShopDialogueCode.FinalChallengeReminder);
                return;
            }
        }

        if (shopManager.UIState == ShopManager.States.Main)
        {
            UpdateDialogueEnum(ShopDialogueCode.DefaultMain);
            return;
        }

        // if (shopManager.UIState == ShopManager.States.Buy)
        // {
        //     UpdateDialogue("Default Buy");
        //     return;
        // }
    }

    public void UpdateDialogue(string code) =>

    UpdateDialogueEnum(code switch {
        "Default Main"=>ShopDialogueCode.DefaultMain,
        "First Time"=>ShopDialogueCode.FirstTime,
        "No Anchor"=>ShopDialogueCode.NoAnchor,
        "First Time with Anchor"=>ShopDialogueCode.FirstTimeWithAnchor,
        "Turn in Anchor"=>ShopDialogueCode.TurnInAnchor,
        "Explain Tavern Pass"=>ShopDialogueCode.ExplainTavernPass,
        "Turn in Treasure Chest"=>ShopDialogueCode.TurnInTreasureChest,
        "Turn in Magical Gem"=>ShopDialogueCode.TurnInMagicalGem,
        "Turn in Mushroom"=>ShopDialogueCode.TurnInMushroom,
        "Turn in Golden Fish"=>ShopDialogueCode.TurnInGoldenFish,
        "Turn in Rock"=>ShopDialogueCode.TurnInRock,
        "Turn in Rose"=>ShopDialogueCode.TurnInRose,
        "Turn in Multiple Items"=>ShopDialogueCode.TurnInMultipleItems,
        "Start Final Challenge"=>ShopDialogueCode.StartFinalChallenge,
        "Final Challenge Reminder"=>ShopDialogueCode.FinalChallengeReminder,
        "Ocean Complete"=>ShopDialogueCode.OceanComplete,
        "Visiting"=>ShopDialogueCode.Visiting,
        "Who are You"=>ShopDialogueCode.WhoAreYou,
        "Business"=>ShopDialogueCode.Business,
        "A Budding Romance"=>ShopDialogueCode.ABuddingRomance,
        "Shipwreck"=>ShopDialogueCode.Shipwreck,
        "Magical Spells"=>ShopDialogueCode.MagicalSpells,
        "The Veil"=>ShopDialogueCode.TheVeil,
        "Tangled Up"=>ShopDialogueCode.TangledUp,
        "Eruption"=>ShopDialogueCode.Eruption,
        "The Tavern"=>ShopDialogueCode.TheTavern,
        "Aliens"=>ShopDialogueCode.Aliens,
        "Mushrooms"=>ShopDialogueCode.Mushrooms,
        "Swift Victory"=>ShopDialogueCode.SwiftVictory,
        "Stonybrook Village"=>ShopDialogueCode.StonybrookVillage,
        "Canopy Town"=>ShopDialogueCode.CanopyTown,
        "The Impact Zone"=>ShopDialogueCode.TheImpactZone,
        "The Flats"=>ShopDialogueCode.TheFlats,
        "Space"=>ShopDialogueCode.Space,
        _ => throw new ArgumentOutOfRangeException()
    });

    private void UpdateDialogueEnum(ShopDialogueCode code)
    {
        if (!canOverrideDialogue)
            return;

        switch (code)
        {
            case ShopDialogueCode.DefaultMain:
                System.Random r = new System.Random();
                int index = r.Next(0, mainShopDialgoue.Length);
            
                SetDialogue(new ShopDialogue(
                    null,
                    this.GetLocalized(ShopDialogueCode.DefaultMain, index),
                    TKSprite.Normal,
                    null
                ));
                break;
                
            // // case "Default Buy":
            // //     SetDialogue(new ShopDialogue(
            // //         null,
            // //         "Whaddya' want, landlubber",
            // //         TKSprite.Normal,
            // //         null
            // //     ));
            // //     break;
            //     
            // // case "Default Purchase":
            // //     SetDialogue(new ShopDialogue(
            // //         null,
            // //         "Hmm, a wise choice!",
            // //         TKSprite.Happy,
            // //         null
            // //     ));
            // //     break;
            
            case ShopDialogueCode.FirstTime:
                SetDialogue(new ShopDialogue(
                    () => { 
                        canOverrideDialogue = false;
                        shopManager.OpenDialoguePanel();
                    },
                    this.GetLocalized(ShopDialogueCode.FirstTime, 0),
                    TKSprite.Normal,
                    () => SetDialogue(
                       
                new ShopDialogue(
                    null,
                    this.GetLocalized(ShopDialogueCode.FirstTime, 1),
                    TKSprite.Happy,
                    () => {
                        canOverrideDialogue = true;
                        SaveSystem.Current.SetBool("oceanHasTalkedToBob", true);
                        if (!PlayerInventory.Instance.GetHasCollectedAnchor())
                        {
                            UpdateDialogueEnum(ShopDialogueCode.NoAnchor);
                        }
                        else
                        {
                          UpdateDialogueEnum(ShopDialogueCode.FirstTimeWithAnchor);
                        }
                    }
                ))
                ));
                break;
            
            case ShopDialogueCode.NoAnchor:
                SetDialogue(new ShopDialogue(
                    () => { 
                        canOverrideDialogue = false;
                        shopManager.OpenDialoguePanel();
                    },
                    this.GetLocalized(ShopDialogueCode.NoAnchor, 0),
                    TKSprite.Normal,
                    () => SetDialogue(
            
                new ShopDialogue(
                    null,
                    this.GetLocalized(ShopDialogueCode.NoAnchor, 1),
                    TKSprite.Question,
                    () => SetDialogue(
            
                new ShopDialogue(
                    null,
                    this.GetLocalized(ShopDialogueCode.NoAnchor, 2),
                    TKSprite.Angry,
                    () => shopManager.CloseShop()
                ))
                ))
                ));
                break;
            
            case ShopDialogueCode.FirstTimeWithAnchor:
                SetDialogue(new ShopDialogue (
                    () => {
                        shopManager.OpenDialoguePanel();
                    },
                    this.GetLocalized(ShopDialogueCode.FirstTimeWithAnchor, 0),
                    TKSprite.Question,
                    () => UpdateDialogueEnum(ShopDialogueCode.ExplainTavernPass)
                ));
                break;
            
            case ShopDialogueCode.TurnInAnchor:
                SetDialogue(new ShopDialogue(
                    () => {
                        shopManager.OpenDialoguePanel();
                    },
                    this.GetLocalized(ShopDialogueCode.TurnInAnchor, 0),
                    TKSprite.Question,
                    () => UpdateDialogueEnum(ShopDialogueCode.ExplainTavernPass)
                ));
                break;
            
            case ShopDialogueCode.ExplainTavernPass:
                        SetDialogue(new ShopDialogue(
                    () => {
                        canOverrideDialogue = false;
                    },
                    this.GetLocalized(ShopDialogueCode.ExplainTavernPass, 0),
                    TKSprite.Happy,
                    () => {
                        SetDialogue(new ShopDialogue(
                    null,
                    this.GetLocalized(ShopDialogueCode.ExplainTavernPass, 1),
                    TKSprite.Angry,
                    () => {
                        UIArtifactWorldMap.SetAreaStatus(Area.Jungle, ArtifactWorldMapArea.AreaStatus.silhouette);
                        UIArtifactWorldMap.SetAreaStatus(Area.Factory, ArtifactWorldMapArea.AreaStatus.silhouette);
            
                        SetDialogue(new ShopDialogue(
                    null,
                    this.GetLocalized(ShopDialogueCode.ExplainTavernPass, 2),
                    TKSprite.Question,
                    () => {
                        SetDialogue(new ShopDialogue(
                    null,
                    this.GetLocalized(ShopDialogueCode.ExplainTavernPass, 3),
                    TKSprite.Normal,
                    () => {
                        shopManager.OpenMainPanel();
                        SaveSystem.Current.SetBool("oceanBobNormal", true);
            
                        SetDialogue(new ShopDialogue(
                    null,
                    this.GetLocalized(ShopDialogueCode.ExplainTavernPass, 4),
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
                    }
                ));
                break;
            
            case ShopDialogueCode.TurnInTreasureChest:
                SetDialogue(new ShopDialogue(
                    null,
                    this.GetLocalized(ShopDialogueCode.TurnInTreasureChest, 0),
                    TKSprite.Happy,
                    null
                ));
                break;
            
            case ShopDialogueCode.TurnInMagicalGem:
                SetDialogue(new ShopDialogue(
                    null,
                    this.GetLocalized(ShopDialogueCode.TurnInMagicalGem, 0),
                    TKSprite.Normal,
                    null
                ));
                break;
            
            case ShopDialogueCode.TurnInMushroom:
                SetDialogue(new ShopDialogue(
                    null,
                    this.GetLocalized(ShopDialogueCode.TurnInMushroom, 0),
                    TKSprite.Question,
                    null
                ));
                break;
            
            case ShopDialogueCode.TurnInGoldenFish:
                SetDialogue(new ShopDialogue(
                    null,
                    this.GetLocalized(ShopDialogueCode.TurnInGoldenFish, 0),
                    TKSprite.Question,
                    null
                ));
                break;
            
            case ShopDialogueCode.TurnInRock:
                SetDialogue(new ShopDialogue(
                    null,
                    this.GetLocalized(ShopDialogueCode.TurnInRock, 0),
                    TKSprite.Happy,
                    null
                ));
                break;
            
            case ShopDialogueCode.TurnInRose:
                SetDialogue(new ShopDialogue(
                    null,
                    this.GetLocalized(ShopDialogueCode.TurnInRose, 0),
                    TKSprite.Angry,
                    null
                ));
                break;
            
            case ShopDialogueCode.TurnInMultipleItems:
                SetDialogue(new ShopDialogue(
                    null,
                    this.GetLocalized(ShopDialogueCode.TurnInMultipleItems, 0),
                    TKSprite.Happy,
                    null
                ));
                break;
            
            case ShopDialogueCode.StartFinalChallenge:
                SetDialogue(new ShopDialogue(
                    () => {
                        canOverrideDialogue = false;
                        shopManager.OpenDialoguePanel();
                        (SGrid.Current as OceanGrid).StartFinalChallenge();
                    },
                    this.GetLocalized(ShopDialogueCode.StartFinalChallenge, 0),
                    TKSprite.Normal,
                    () => {
                        SetDialogue(new ShopDialogue(
                    null,
                    this.GetLocalized(ShopDialogueCode.StartFinalChallenge, 1),
                    TKSprite.Question,
                    () => {
                        SetDialogue(new ShopDialogue(
                    null,
                    this.GetLocalized(ShopDialogueCode.StartFinalChallenge, 2),
                    TKSprite.Normal,
                    () => {
                        shopManager.OpenMainPanel();
            
                        SetDialogue(new ShopDialogue(
                    null,
                    this.GetLocalized(ShopDialogueCode.StartFinalChallenge, 3),
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
            
            case ShopDialogueCode.FinalChallengeReminder:
                SetDialogue(new ShopDialogue(
                    () => {
                        canOverrideDialogue = false;
                        shopManager.OpenDialoguePanel();
                    },
                    this.GetLocalized(ShopDialogueCode.FinalChallengeReminder, 0),
                    TKSprite.Normal,
                    () => {
                        shopManager.OpenMainPanel();
            
                        SetDialogue(new ShopDialogue(
                    null,
                    this.GetLocalized(ShopDialogueCode.FinalChallengeReminder, 1),
                    TKSprite.Happy,
                    () => {
                        canOverrideDialogue = true;
                    }
                ));
                    }
                ));
                break;
            
            
            
            case ShopDialogueCode.OceanComplete:
                SetDialogue(new ShopDialogue(
                    () => { 
                        canOverrideDialogue = false;
                        shopManager.OpenDialoguePanel();
                    },
                    this.GetLocalized(ShopDialogueCode.OceanComplete, 0),
                    TKSprite.Normal,
                    () => {
                        SetDialogue(new ShopDialogue(
                    null,
                    this.GetLocalized(ShopDialogueCode.OceanComplete, 1),
                    TKSprite.Normal,
                    () => {
                        // He disappear
                        dialogueText.fontStyle = FontStyles.Italic;
            
                        SetDialogue(new ShopDialogue(
                    null,
                    this.GetLocalized(ShopDialogueCode.OceanComplete, 2),
                    TKSprite.None,
                    () => {
                        SetDialogue(new ShopDialogue(
                    null,
                    this.GetLocalized(ShopDialogueCode.OceanComplete, 3),
                    TKSprite.None,
                    () => {
                        SetDialogue(new ShopDialogue(
                    null,
                    this.GetLocalized(ShopDialogueCode.OceanComplete, 4),
                    TKSprite.None,
                    () => {
                        (SGrid.Current as OceanGrid).ClearTreesToJungle();
            
                        SetDialogue(new ShopDialogue(
                    null,
                    this.GetLocalized(ShopDialogueCode.OceanComplete, 5),
                    TKSprite.None,
                    () => {
                        SetDialogue(new ShopDialogue(
                    null,
                    this.GetLocalized(ShopDialogueCode.OceanComplete, 6),
                    TKSprite.None,
                    () => {
                        SetDialogue(new ShopDialogue(
                    null,
                    this.GetLocalized(ShopDialogueCode.OceanComplete, 7),
                    TKSprite.None,
                    () => {
                        SetDialogue(new ShopDialogue(
                    null,
                    this.GetLocalized(ShopDialogueCode.OceanComplete, 8),
                    TKSprite.None,
                    () => {
                        SetDialogue(new ShopDialogue(
                    null,
                    this.GetLocalized(ShopDialogueCode.OceanComplete, 9),
                    TKSprite.None,
                    () => {
                        dialogueText.fontStyle = FontStyles.Normal;
            
                        SetDialogue(new ShopDialogue(
                    null,
                    this.GetLocalized(ShopDialogueCode.OceanComplete, 10),
                    TKSprite.Happy,
                    () => {
                        shopManager.OpenMainPanel();
                        SaveSystem.Current.SetBool("oceanIsVisiting", true);
            
                        SetDialogue(new ShopDialogue(
                    null,
                    this.GetLocalized(ShopDialogueCode.OceanComplete, 11),
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
            
            case ShopDialogueCode.Visiting:
                SetDialogue(new ShopDialogue(
                    null,
                    this.GetLocalized(ShopDialogueCode.Visiting, 0),
                    TKSprite.Happy,
                    null
                ));
                break;
            
            
            
            case ShopDialogueCode.WhoAreYou:
                SetDialogue(new ShopDialogue(
                    () => { 
                        canOverrideDialogue = false;
                        shopManager.OpenDialoguePanel();
                    },
                    this.GetLocalized(ShopDialogueCode.WhoAreYou, 0),
                    TKSprite.Normal,
                    () => SetDialogue(
            
                new ShopDialogue(
                    null,
                    this.GetLocalized(ShopDialogueCode.WhoAreYou, 1),
                    TKSprite.Normal,
                    () => {
                        canOverrideDialogue = true;
                        shopManager.OpenTalkPanel();
                    }
                ))
                ));
                break;
            
            case ShopDialogueCode.Business:
                SetDialogue(new ShopDialogue(
                    () => { 
                        canOverrideDialogue = false;
                        shopManager.OpenDialoguePanel();
                    },
                    this.GetLocalized(ShopDialogueCode.Business, 0),
                    TKSprite.Normal,
                    () => SetDialogue(
            
                new ShopDialogue(
                    null,
                    this.GetLocalized(ShopDialogueCode.Business, 1),
                    TKSprite.Happy,
                    () => SetDialogue(
            
                new ShopDialogue(
                    null,
                    this.GetLocalized(ShopDialogueCode.Business, 2),
                    TKSprite.Question,
                    () => SetDialogue(
            
                new ShopDialogue(
                    null,
                    this.GetLocalized(ShopDialogueCode.Business, 3),
                    TKSprite.Normal,
                    () => SetDialogue(
            
                new ShopDialogue(
                    null,
                    this.GetLocalized(ShopDialogueCode.Business, 4),
                    TKSprite.Normal,
                    () => {
                        canOverrideDialogue = true;
                        shopManager.OpenTalkPanel();
                    }
                ))
            
                ))
            
                ))
            
                ))
            
                ));
                break;
            
            case ShopDialogueCode.ABuddingRomance:
                SetDialogue(new ShopDialogue(
                    () => { 
                        canOverrideDialogue = false;
                        shopManager.OpenDialoguePanel();
                    },
                    this.GetLocalized(ShopDialogueCode.ABuddingRomance, 0),
                    TKSprite.Question,
                    () => SetDialogue(
            
                new ShopDialogue(
                    null,
                    this.GetLocalized(ShopDialogueCode.ABuddingRomance, 1),
                    TKSprite.Normal,
                    () => {
                        canOverrideDialogue = true;
                        shopManager.OpenTalkPanel();
                    }
                ))
                ));
                break;
            
            case ShopDialogueCode.Shipwreck:
                SetDialogue(new ShopDialogue(
                    () => { 
                        canOverrideDialogue = false;
                        shopManager.OpenDialoguePanel();
                    },
                    this.GetLocalized(ShopDialogueCode.Shipwreck, 0),
                    TKSprite.Normal,
                    () => {
                        canOverrideDialogue = true;
                        shopManager.OpenTalkPanel();
                    }
                ));
                break;
            
            case ShopDialogueCode.MagicalSpells:
                SetDialogue(new ShopDialogue(
                    () => { 
                        canOverrideDialogue = false;
                        shopManager.OpenDialoguePanel();
                    },
                    this.GetLocalized(ShopDialogueCode.MagicalSpells, 0),
                    TKSprite.Question,
                    () => {
                        canOverrideDialogue = true;
                        shopManager.OpenTalkPanel();
                    }
                ));
                break;
            
            case ShopDialogueCode.TheVeil:
                SetDialogue(new ShopDialogue(
                    () => { 
                        canOverrideDialogue = false;
                        shopManager.OpenDialoguePanel();
                    },
                    this.GetLocalized(ShopDialogueCode.TheVeil, 0),
                    TKSprite.Question,
                    () => SetDialogue(
            
                new ShopDialogue(
                    null,
                    this.GetLocalized(ShopDialogueCode.TheVeil, 1),
                    TKSprite.Normal,
                    () => SetDialogue(
            
                new ShopDialogue(
                    null,
                    this.GetLocalized(ShopDialogueCode.TheVeil, 2),
                    TKSprite.Question,
                    () => {
                        FoggyMusicHintManager.Instance.SetBobHint(false);
                        canOverrideDialogue = true;
                        shopManager.OpenTalkPanel();
                    }
                ))
                ))
                ));
                break;
            
            case ShopDialogueCode.TangledUp:
                SetDialogue(new ShopDialogue(
                    () => { 
                        canOverrideDialogue = false;
                        shopManager.OpenDialoguePanel();
                    },
                    this.GetLocalized(ShopDialogueCode.TangledUp, 0),
                    TKSprite.Angry,
                    () => {
                        canOverrideDialogue = true;
                        shopManager.OpenTalkPanel();
                    }
                ));
                break;
            
            case ShopDialogueCode.Eruption:
                SetDialogue(new ShopDialogue(
                    () => { 
                        canOverrideDialogue = false;
                        shopManager.OpenDialoguePanel();
                    },
                    this.GetLocalized(ShopDialogueCode.Eruption, 0),
                    TKSprite.Normal,
                    () => {
                        canOverrideDialogue = true;
                        shopManager.OpenTalkPanel();
                    }
                ));
                break;
            
            case ShopDialogueCode.TheTavern:
                SetDialogue(new ShopDialogue(
                    () => { 
                        canOverrideDialogue = false;
                        shopManager.OpenDialoguePanel();
                    },
                    this.GetLocalized(ShopDialogueCode.TheTavern, 0),
                    TKSprite.Normal,
                    () => {
                        canOverrideDialogue = true;
                        shopManager.OpenTalkPanel();
                    }
                ));
                break;
            
            case ShopDialogueCode.Aliens:
                SetDialogue(new ShopDialogue(
                    () => { 
                        canOverrideDialogue = false;
                        shopManager.OpenDialoguePanel();
                    },
                    this.GetLocalized(ShopDialogueCode.Aliens, 0),
                    TKSprite.Question,
                    () => SetDialogue(
            
                new ShopDialogue(
                    null,
                    this.GetLocalized(ShopDialogueCode.Aliens, 1),
                    TKSprite.Normal,
                    () => {
                        canOverrideDialogue = true;
                        shopManager.OpenTalkPanel();
                    }
                ))
                ));
                break;
            
            case ShopDialogueCode.Mushrooms:
                SetDialogue(new ShopDialogue(
                    () => { 
                        canOverrideDialogue = false;
                        shopManager.OpenDialoguePanel();
                    },
                    this.GetLocalized(ShopDialogueCode.Mushrooms, 0),
                    TKSprite.Question,
                    () => {
                        canOverrideDialogue = true;
                        shopManager.OpenTalkPanel();
                    }
                ));
                break;
            
            case ShopDialogueCode.SwiftVictory:
                SetDialogue(new ShopDialogue(
                    () => { 
                        canOverrideDialogue = false;
                        shopManager.OpenDialoguePanel();
                    },
                    this.GetLocalized(ShopDialogueCode.SwiftVictory, 0),
                    TKSprite.Happy,
                    () => SetDialogue(new ShopDialogue(
                    null,
                    this.GetLocalized(ShopDialogueCode.SwiftVictory, 1),
                    TKSprite.Question,
                    () => {
                        canOverrideDialogue = true;
                        shopManager.OpenTalkPanel();
                    }
                ))
                ));
                break;
            
            case ShopDialogueCode.StonybrookVillage:
                SetDialogue(new ShopDialogue(
                    () => { 
                        canOverrideDialogue = false;
                        shopManager.OpenDialoguePanel();
                    },
                    this.GetLocalized(ShopDialogueCode.StonybrookVillage, 0),
                    TKSprite.Happy,
                    () => SetDialogue(new ShopDialogue(
                    null,
                    this.GetLocalized(ShopDialogueCode.StonybrookVillage, 1),
                    TKSprite.Normal,
                    () => {
                        canOverrideDialogue = true;
                        shopManager.OpenTalkPanel();
                    }
                ))
                ));
                break;
            
            case ShopDialogueCode.CanopyTown:
                SetDialogue(new ShopDialogue(
                    () => { 
                        canOverrideDialogue = false;
                        shopManager.OpenDialoguePanel();
            
                        UIArtifactWorldMap.SetAreaStatus(Area.Jungle, ArtifactWorldMapArea.AreaStatus.silhouette);
                    },
                    this.GetLocalized(ShopDialogueCode.CanopyTown, 0),
                    TKSprite.Normal,
                    () => SetDialogue(new ShopDialogue(
                    null,
                    this.GetLocalized(ShopDialogueCode.CanopyTown, 1),
                    TKSprite.Angry,
                    () => {
                        canOverrideDialogue = true;
                        shopManager.OpenTalkPanel();
                    }
                ))
                ));
                break;
            
            // No longer being used
            case ShopDialogueCode.TheImpactZone:
                SetDialogue(new ShopDialogue(
                    () => { 
                        canOverrideDialogue = false;
                        shopManager.OpenDialoguePanel();
            
                        UIArtifactWorldMap.SetAreaStatus(Area.Desert, ArtifactWorldMapArea.AreaStatus.silhouette);
                    },
                    this.GetLocalized(ShopDialogueCode.TheImpactZone, 0),
                    TKSprite.Normal,
                    () => SetDialogue(new ShopDialogue(
                    null,
                    this.GetLocalized(ShopDialogueCode.TheImpactZone, 1),
                    TKSprite.Question,
                    () => {
                        canOverrideDialogue = true;
                        shopManager.OpenTalkPanel();
                    }
                ))
                ));
                break;
            
            case ShopDialogueCode.TheFlats:
                SetDialogue(new ShopDialogue(
                    () => { 
                        canOverrideDialogue = false;
                        shopManager.OpenDialoguePanel();
            
                        UIArtifactWorldMap.SetAreaStatus(Area.Desert, ArtifactWorldMapArea.AreaStatus.silhouette);
                    },
                    this.GetLocalized(ShopDialogueCode.TheFlats, 0),
                    TKSprite.Normal,
                    () => SetDialogue(new ShopDialogue(
                    null,
                    this.GetLocalized(ShopDialogueCode.TheFlats, 1),
                    TKSprite.Question,
                    () => SetDialogue(new ShopDialogue(
                    null,
                    
                    this.GetLocalized(ShopDialogueCode.TheFlats, 2),
                    TKSprite.Normal,
                    () => {
                        canOverrideDialogue = true;
                        shopManager.OpenTalkPanel();
                    }
                ))
                ))
                ));
                break;
            
            case ShopDialogueCode.Space:
                SetDialogue(new ShopDialogue(
                    () => { 
                        canOverrideDialogue = false;
                        shopManager.OpenDialoguePanel();
                    },
                    this.GetLocalized(ShopDialogueCode.Space, 0),
                    TKSprite.Question,
                    () => SetDialogue(new ShopDialogue(
                    null,
                    this.GetLocalized(ShopDialogueCode.Space, 1),
                    TKSprite.Normal,
                    () => {
                        UIArtifactWorldMap.SetAreaStatus(Area.MagiTech, ArtifactWorldMapArea.AreaStatus.silhouette);
            
                        canOverrideDialogue = true;
                        shopManager.OpenTalkPanel();
                    }
                ))
                ));
                break;
            
            // default:
            //     Debug.LogError("Couldn't find dialogue with code: " + codeName);
            //     break;
        }
    }



    #endregion
}
