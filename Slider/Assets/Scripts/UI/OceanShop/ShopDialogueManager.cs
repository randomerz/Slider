using System;
using System.Collections;
using System.Collections.Generic;
using SliderVocalization;
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

    public Dictionary<ShopDialogueCode, (string, string)[]> dialogueTable = new()
    {
        {
            ShopDialogueCode.DefaultMain, new[]
            {
                ("Welcome back sailor!", null as string),
                ("Find anything interesting today?", null as string),
                ("Looks like a good day to set sail!", null as string),
                ("Buccaneer Bob, at your service.", null as string),
                ("What can I do for you today?", null as string),
                ("Ahoy there!", null as string),
            }
        },

        {
            ShopDialogueCode.FirstTime, new[]
            {
                ("Ahoy matey! Welcome to Buccaneer Bob's. Here in the Shifting Seas, freedom is our creed. Just don't get lost -- the oceans are filled with treasure.",
                    null as string),
                ("Now what are we looking for? Drinks, perhaps? Or are you interested in my all-new Tavern Pass?",
                    null as string),
            }
        },

        {
            ShopDialogueCode.NoAnchor, new[]
            {
                ("Hold on now, it seems you've got no coin in those pockets!", null as string),
                ("And you don't have an <#2e44f0>anchor</color>? Every self-respecting pirate has an anchor!",
                    null as string),
                ("Quickly now, go find one. I don't want people seein' an anchorless schmuck in my tavern. Bring one back and I'll see what I can do for you.",
                    null as string),
            }
        },
        {
            ShopDialogueCode.FirstTimeWithAnchor, new[]
            {
                ("What's that? You've already got an <#2e44f0>anchor</color>?! We've got a legend in the making, no doubt!",
                    null as string),
            }
        },
        {
            ShopDialogueCode.TurnInAnchor, new[]
            {
                ("Aye, that's a solid tool there. In fact, that was awfully quick. Keep it up, and you might just make a name for yourself around here.",
                    null as string),
            }
        },
        {
            ShopDialogueCode.ExplainTavernPass, new[]
            {
                ("Well, we've got a new business model- Oh, what's that? Your cat, <var>Cat</var>? I wager that's what the boys saw floating eastward!",
                    null as string),
                ("You'd better keep yourself safe - military was deployed there. Even blocked off access from the Shifting Seas. If you really want to go, I reckon you ought to go around the long way, through Canopy Town and the Factory.",
                    null as string),
                ("But don't worry, I've got something perfect for you! Look pal, we're trying a new, \"Business\" model. Remember that tavern pass I mentioned?",
                    null as string),
                ("How it works is, YOU bring me treasures from around the sea, and I'LL give you some points. Get enough points and you'll get a reward! For the last reward, I can even help you reach your cat.",
                    null as string),
                ("I'm calling it, \"Bob's Tavern Pass\"! As my first subscriber, you get the first tier for free.",
                    null as string),
            }
        },
        {
            ShopDialogueCode.TurnInTreasureChest, new[]
            {
                ("A treasure chest? The One Piece is Real!", null as string),
            }
        },
        {
            ShopDialogueCode.TurnInMagicalGem, new[]
            {
                ("A magical gem! Heard them hard-hat types are mining for these babies in the mountains.",
                    null as string),
            }
        },
        {
            ShopDialogueCode.TurnInMushroom, new[]
            {
                ("Never seen anything like this before. Maybe I should try it in a new cocktail...", null as string),
            }
        },
        {
            ShopDialogueCode.TurnInGoldenFish, new[]
            {
                ("Catch of the day, eh? They say a golden fish brings good tidings.", null as string),
            }
        },
        {
            ShopDialogueCode.TurnInRock, new[]
            {
                ("Is this what I think it is? How'd you even get this? You, my friend, are something special.",
                    null as string),
            }
        },
        {
            ShopDialogueCode.TurnInRose, new[]
            {
                ("You one of those hopeless romantic types? I appreciate the sentiment.", null as string),
            }
        },
        {
            ShopDialogueCode.TurnInMultipleItems, new[]
            {
                ("A fine haul! You'd make a smashing pirate.", null as string),
            }
        },
        {
            ShopDialogueCode.StartFinalChallenge, new[]
            {
                ("Well... to be honest, never expected you to finish my battle pass. You really wanna find <var>Cat</var>?, eh?",
                    null as string),
                ("Now that you've about explored the whole damn ocean, I reckon it's about time to put things in their rightful places.",
                    null as string),
                ("If you fix my island with that tablet of yours, I can help you cross over to Canopy Town. Only need the tavern part and the two beaches.",
                    null as string),
                ("I'll bust out the old axe and cut the trees north of the tavern.", null as string),
            }
        },
        {
            ShopDialogueCode.FinalChallengeReminder, new[]
            {
                ("If you fix my island with that tablet of yours, I can help you cross over to Canopy Town.",
                    null as string),
                ("I'll bust out the old axe and cut the trees above the tavern.", null as string),
            }
        },
        {
            ShopDialogueCode.OceanComplete, new[]
            {
                ("...", null as string),
                ("Wish you could stay, but something tells me you have places to be.", null as string),
                ("> You watch as he leaves, with haste and his sharp axe.", null as string),
                ("> He takes large strides, his massive figure easily pushing the tavern doors open.", null as string),
                ("> You sneak a peak through the window.", null as string),
                ("*CHOP, CHOP, CHOP*", null as string),
                ("> He cuts them down... so easily.", null as string),
                ("> Of course, he'd wipe some sweat from his brow. But this was of no effort to a man like Bob.",
                    null as string),
                ("> He doesn't notice you watching through the window as he makes his way back in.", null as string),
                ("> Axe on shoulder, he pushes the doors open and hops over the bar counter.", null as string),
                ("Safe travels! And welcome to the Jungle.", null as string),
                ("Bring me back a souvenir.", null as string),
            }
        },
        {
            ShopDialogueCode.Visiting, new[]
            {
                ("Visiting are you? Enjoy your stay!", null as string),
            }
        },
        {
            ShopDialogueCode.WhoAreYou, new[]
            {
                ("Grew up in Stonybrook Village, lived the quiet life but I always wanted more. Joined a pirate crew young, eventually retired and took up business here.",
                    null as string),
                ("I wanted to stay close to the sea and my people. Can't say I'm not lucky to have lived the life I have.",
                    null as string),
            }
        },
        {
            ShopDialogueCode.Business, new[]
            {
                ("The Tavern Pass we're trying out is pretty neat, huh?", null as string),
                ("Don't ask too much about the rewards. Oh, but for the final reward, I'll do anything you want!",
                    null as string),
                ("Looks like under your name we have registered that you want to go to the Jungle up north.",
                    null as string),
                ("Anyways, I can definitely help you on your way to Canopy Town if you finish our Tavern Pass.",
                    null as string),
                ("Remember: If you ever run into trouble, Bob's is the one-stop-shop for all the seafaring advice you'll ever need!",
                    null as string),
                ("Remember: If you ever run into trouble, Bob's is the one-stop-shop for all the seafaring advice you'll ever need!",
                    null as string),
            }
        },
        {
            ShopDialogueCode.ABuddingRomance, new[]
            {
                ("I don't like to gossip, but... that Romeo REALLY wants to get with Juliet. It's really been weighing him down. If you can be a solid wingman, maybe he can get his message across.",
                    null as string),
                ("Beware of disturbing the seas though. Too much water can sink even the most heartfelt messages.",
                    null as string),
            }
        },
        {
            ShopDialogueCode.Shipwreck, new[]
            {
                ("Heard the Black Trident broke upon some rocks earlier today. Poor Catbeard. Might be worth searching the area.",
                    null as string),
            }
        },
        {
            ShopDialogueCode.MagicalSpells, new[]
            {
                ("Some funky fellow, Fezziwig, been messing around with some magicks. This whole world's gone a bit crazy recently, hasn't it?",
                    null as string),
            }
        },
        {
            ShopDialogueCode.TheVeil, new[]
            {
                ("There's a treacherous patch of foggy sea down south we call \"The Veil\". Who knows what the mist may be hiding.",
                    null as string),
                ("Tales tell of a song of THREE pairs of verses needed to navigate it. Think it started with \"West, South,\", but I forgot the rest.",
                    null as string),
                ("Maybe others know the rest of the song? Or it might be written down, eroded away in sand.",
                    null as string),
            }
        },
        {
            ShopDialogueCode.TangledUp, new[]
            {
                ("Pierre the fisherman has his buoys in a mess. Can't say this is the first time either.",
                    null as string),
            }
        },
        {
            ShopDialogueCode.Eruption, new[]
            {
                ("Legends say the volcano only erupts when the rocks align, at least according to the old heads.",
                    null as string),
            }
        },
        {
            ShopDialogueCode.TheTavern, new[]
            {
                ("Business has taken a hit recently, with The Cataclysm and all that, but we're still afloat. Talk to some of the customers here, you might learn something!",
                    null as string),
            }
        },
        {
            ShopDialogueCode.Aliens, new[]
            {
                ("Some shifty-looking fellows walked in a while ago. They pay well so I can't complain, but something seems different about them.",
                    null as string),
                ("They looked in an awful hurry, carrying something with them too.", null as string),
            }
        },
        {
            ShopDialogueCode.Mushrooms, new[]
            {
                ("Don't tell anyone, but I love slipping mushrooms into some of my drinks. Think of it as a secret ingredient of sorts.",
                    null as string),
            }
        },
        {
            ShopDialogueCode.SwiftVictory, new[]
            {
                ("An old ship helmed by the good Captain Mako, anchored to the South. Used to run with his crew before I opened the tavern.",
                    null as string),
                ("I don't mean to be rude but... he's a bit washed up now.", null as string),
            }
        },
        {
            ShopDialogueCode.StonybrookVillage, new[]
            {
                ("My hometown! A quaint little village to the West.", null as string),
                ("Always used to play in this hidden cave behind the waterfall, made a lot of memories there. I should go back and visit sometime soon.",
                    null as string),
            }
        },
        {
            ShopDialogueCode.CanopyTown, new[]
            {
                ("A treetop town in the jungle to the North. Used to be nice and quiet, perfect for a picnic.",
                    null as string),
                ("Some blowhard named Barron hurried into the place and started up all sorts of business and noise.",
                    null as string),
            }
        },
        {
            ShopDialogueCode.TheImpactZone, new[]
            {
                ("Big, ancient crater that formed a desert to the north.", null as string),
                ("Some tell tall tales of strange happenings, things that can't be explained. I don't believe a word of it.",
                    null as string),
            }
        },
        {
            ShopDialogueCode.TheFlats, new[]
            {
                ("The plains to the east, now a hot-zone full of our nation's military.", null as string),
                ("I'm not sure what's happening, but word is it has to do with The Cataclysm.", null as string),
                ("If your cat really is there, then you'll have to go the long way around. Head north to Canopy Town, then east to the Factory. Then you should have access the military zone.",
                    null as string),
            }
        },
        {
            ShopDialogueCode.Space, new[]
            {
                ("You want to know how to get up amongst the stars?", null as string),
                ("Well, the folks at the Magic Research Institute have been working on a rocket, so it might be your lucky day. Here, I'll show you on your map.",
                    null as string),
            }
        },
    };
    
    // public List<string> MainShopDialogue => mainShopDialgoue;
    // private List<string> mainShopDialgoue = new List<string>{
    //     "Welcome back sailor!", 
    //     "Find anything interesting today?",
    //     "Looks like a good day to set sail!",
    //     "Buccaneer Bob, at your service.",
    //     "What can I do for you today?",
    //     "Ahoy there!"
    //     };

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
    
    [SerializeField]
    private VocalizableParagraph vocalizer;
    
    public class ShopDialogue
    {
        public Action onStart; // actions immediately called
        public string text; // text to set + animate depending on panel via ShopManager.uiState
        public string textTranslated;
        public TKSprite tkSprite;
        public Action onFinishAndAction; // this is for functions to call when done typing + press e, mostly for dialogue panel

        public ShopDialogue(Action onStart, (string, string) text, TKSprite tkSprite, Action onFinishAndAction)
        {
            this.onStart = onStart;
            this.text = text.Item1;
            this.textTranslated = text.Item2;
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
            toVocalize = currentTyperText.ReplaceAndStripRichText(dialogue.text);
            
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

            int maxPhonemes = 10;
            
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
                int index = r.Next(0, dialogueTable[ShopDialogueCode.DefaultMain].Length);
            
                SetDialogue(new ShopDialogue(
                    null,
                    dialogueTable[ShopDialogueCode.DefaultMain][index],
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
                    dialogueTable[ShopDialogueCode.FirstTime][0],
                    TKSprite.Normal,
                    () => SetDialogue(
                       
                new ShopDialogue(
                    null,
                    dialogueTable[ShopDialogueCode.FirstTime][1],
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
                    dialogueTable[ShopDialogueCode.NoAnchor][0],
                    TKSprite.Normal,
                    () => SetDialogue(
            
                new ShopDialogue(
                    null,
                    dialogueTable[ShopDialogueCode.NoAnchor][1],
                    TKSprite.Question,
                    () => SetDialogue(
            
                new ShopDialogue(
                    null,
                    dialogueTable[ShopDialogueCode.NoAnchor][2],
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
                    dialogueTable[ShopDialogueCode.FirstTimeWithAnchor][0],
                    TKSprite.Question,
                    () => UpdateDialogueEnum(ShopDialogueCode.ExplainTavernPass)
                ));
                break;
            
            case ShopDialogueCode.TurnInAnchor:
                SetDialogue(new ShopDialogue(
                    () => {
                        shopManager.OpenDialoguePanel();
                    },
                    dialogueTable[ShopDialogueCode.TurnInAnchor][0],
                    TKSprite.Question,
                    () => UpdateDialogueEnum(ShopDialogueCode.ExplainTavernPass)
                ));
                break;
            
            case ShopDialogueCode.ExplainTavernPass:
                        SetDialogue(new ShopDialogue(
                    () => {
                        canOverrideDialogue = false;
                    },
                    dialogueTable[ShopDialogueCode.ExplainTavernPass][0],
                    TKSprite.Happy,
                    () => {
                        SetDialogue(new ShopDialogue(
                    null,
                    dialogueTable[ShopDialogueCode.ExplainTavernPass][1],
                    TKSprite.Angry,
                    () => {
                        UIArtifactWorldMap.SetAreaStatus(Area.Jungle, ArtifactWorldMapArea.AreaStatus.silhouette);
                        UIArtifactWorldMap.SetAreaStatus(Area.Factory, ArtifactWorldMapArea.AreaStatus.silhouette);
            
                        SetDialogue(new ShopDialogue(
                    null,
                    dialogueTable[ShopDialogueCode.ExplainTavernPass][2],
                    TKSprite.Question,
                    () => {
                        SetDialogue(new ShopDialogue(
                    null,
                    dialogueTable[ShopDialogueCode.ExplainTavernPass][3],
                    TKSprite.Normal,
                    () => {
                        shopManager.OpenMainPanel();
                        SaveSystem.Current.SetBool("oceanBobNormal", true);
            
                        SetDialogue(new ShopDialogue(
                    null,
                    dialogueTable[ShopDialogueCode.ExplainTavernPass][4],
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
                    dialogueTable[ShopDialogueCode.TurnInTreasureChest][0],
                    TKSprite.Happy,
                    null
                ));
                break;
            
            case ShopDialogueCode.TurnInMagicalGem:
                SetDialogue(new ShopDialogue(
                    null,
                    dialogueTable[ShopDialogueCode.TurnInMagicalGem][0],
                    TKSprite.Normal,
                    null
                ));
                break;
            
            case ShopDialogueCode.TurnInMushroom:
                SetDialogue(new ShopDialogue(
                    null,
                    dialogueTable[ShopDialogueCode.TurnInMushroom][0],
                    TKSprite.Question,
                    null
                ));
                break;
            
            case ShopDialogueCode.TurnInGoldenFish:
                SetDialogue(new ShopDialogue(
                    null,
                    dialogueTable[ShopDialogueCode.TurnInGoldenFish][0],
                    TKSprite.Question,
                    null
                ));
                break;
            
            case ShopDialogueCode.TurnInRock:
                SetDialogue(new ShopDialogue(
                    null,
                    dialogueTable[ShopDialogueCode.TurnInRock][0],
                    TKSprite.Happy,
                    null
                ));
                break;
            
            case ShopDialogueCode.TurnInRose:
                SetDialogue(new ShopDialogue(
                    null,
                    dialogueTable[ShopDialogueCode.TurnInRose][0],
                    TKSprite.Angry,
                    null
                ));
                break;
            
            case ShopDialogueCode.TurnInMultipleItems:
                SetDialogue(new ShopDialogue(
                    null,
                    dialogueTable[ShopDialogueCode.TurnInMultipleItems][0],
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
                    dialogueTable[ShopDialogueCode.StartFinalChallenge][0],
                    TKSprite.Normal,
                    () => {
                        SetDialogue(new ShopDialogue(
                    null,
                    dialogueTable[ShopDialogueCode.StartFinalChallenge][1],
                    TKSprite.Question,
                    () => {
                        SetDialogue(new ShopDialogue(
                    null,
                    dialogueTable[ShopDialogueCode.StartFinalChallenge][2],
                    TKSprite.Normal,
                    () => {
                        shopManager.OpenMainPanel();
            
                        SetDialogue(new ShopDialogue(
                    null,
                    dialogueTable[ShopDialogueCode.StartFinalChallenge][3],
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
                    dialogueTable[ShopDialogueCode.FinalChallengeReminder][0],
                    TKSprite.Normal,
                    () => {
                        shopManager.OpenMainPanel();
            
                        SetDialogue(new ShopDialogue(
                    null,
                    dialogueTable[ShopDialogueCode.FinalChallengeReminder][1],
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
                    dialogueTable[ShopDialogueCode.OceanComplete][0],
                    TKSprite.Normal,
                    () => {
                        // He disappear
                        SetDialogue(new ShopDialogue(
                    null,
                    dialogueTable[ShopDialogueCode.OceanComplete][1],
                    TKSprite.None,
                    () => {
                        dialogueText.fontStyle = FontStyles.Italic;
            
                        SetDialogue(new ShopDialogue(
                    null,
                    dialogueTable[ShopDialogueCode.OceanComplete][2],
                    TKSprite.None,
                    () => {
                        SetDialogue(new ShopDialogue(
                    null,
                    dialogueTable[ShopDialogueCode.OceanComplete][3],
                    TKSprite.None,
                    () => {
                        SetDialogue(new ShopDialogue(
                    null,
                    dialogueTable[ShopDialogueCode.OceanComplete][4],
                    TKSprite.None,
                    () => {
                        (SGrid.Current as OceanGrid).ClearTreesToJungle();
            
                        SetDialogue(new ShopDialogue(
                    null,
                    dialogueTable[ShopDialogueCode.OceanComplete][5],
                    TKSprite.None,
                    () => {
                        SetDialogue(new ShopDialogue(
                    null,
                    dialogueTable[ShopDialogueCode.OceanComplete][6],
                    TKSprite.None,
                    () => {
                        SetDialogue(new ShopDialogue(
                    null,
                    dialogueTable[ShopDialogueCode.OceanComplete][7],
                    TKSprite.None,
                    () => {
                        SetDialogue(new ShopDialogue(
                    null,
                    dialogueTable[ShopDialogueCode.OceanComplete][8],
                    TKSprite.None,
                    () => {
                        SetDialogue(new ShopDialogue(
                    null,
                    dialogueTable[ShopDialogueCode.OceanComplete][9],
                    TKSprite.None,
                    () => {
                        dialogueText.fontStyle = FontStyles.Normal;
            
                        SetDialogue(new ShopDialogue(
                    null,
                    dialogueTable[ShopDialogueCode.OceanComplete][10],
                    TKSprite.Happy,
                    () => {
                        shopManager.OpenMainPanel();
                        SaveSystem.Current.SetBool("oceanIsVisiting", true);
            
                        SetDialogue(new ShopDialogue(
                    null,
                    dialogueTable[ShopDialogueCode.OceanComplete][11],
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
                    dialogueTable[ShopDialogueCode.Visiting][0],
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
                    dialogueTable[ShopDialogueCode.WhoAreYou][0],
                    TKSprite.Normal,
                    () => SetDialogue(
            
                new ShopDialogue(
                    null,
                    dialogueTable[ShopDialogueCode.WhoAreYou][1],
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
                    dialogueTable[ShopDialogueCode.Business][0],
                    TKSprite.Normal,
                    () => SetDialogue(
            
                new ShopDialogue(
                    null,
                    dialogueTable[ShopDialogueCode.Business][1],
                    TKSprite.Happy,
                    () => SetDialogue(
            
                new ShopDialogue(
                    null,
                    dialogueTable[ShopDialogueCode.Business][2],
                    TKSprite.Question,
                    () => SetDialogue(
            
                new ShopDialogue(
                    null,
                    dialogueTable[ShopDialogueCode.Business][3],
                    TKSprite.Normal,
                    () => SetDialogue(
            
                new ShopDialogue(
                    null,
                    dialogueTable[ShopDialogueCode.Business][4],
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
                    dialogueTable[ShopDialogueCode.ABuddingRomance][0],
                    TKSprite.Question,
                    () => SetDialogue(
            
                new ShopDialogue(
                    null,
                    dialogueTable[ShopDialogueCode.ABuddingRomance][1],
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
                    dialogueTable[ShopDialogueCode.Shipwreck][0],
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
                    dialogueTable[ShopDialogueCode.MagicalSpells][0],
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
                    dialogueTable[ShopDialogueCode.TheVeil][0],
                    TKSprite.Question,
                    () => SetDialogue(
            
                new ShopDialogue(
                    null,
                    dialogueTable[ShopDialogueCode.TheVeil][1],
                    TKSprite.Normal,
                    () => SetDialogue(
            
                new ShopDialogue(
                    null,
                    dialogueTable[ShopDialogueCode.TheVeil][1],
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
                    dialogueTable[ShopDialogueCode.TangledUp][0],
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
                    dialogueTable[ShopDialogueCode.Eruption][0],
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
                    dialogueTable[ShopDialogueCode.TheTavern][0],
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
                    dialogueTable[ShopDialogueCode.Aliens][0],
                    TKSprite.Question,
                    () => SetDialogue(
            
                new ShopDialogue(
                    null,
                    dialogueTable[ShopDialogueCode.Aliens][1],
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
                    dialogueTable[ShopDialogueCode.Mushrooms][0],
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
                    dialogueTable[ShopDialogueCode.SwiftVictory][0],
                    TKSprite.Happy,
                    () => SetDialogue(new ShopDialogue(
                    null,
                    dialogueTable[ShopDialogueCode.SwiftVictory][1],
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
                    dialogueTable[ShopDialogueCode.StonybrookVillage][0],
                    TKSprite.Happy,
                    () => SetDialogue(new ShopDialogue(
                    null,
                    dialogueTable[ShopDialogueCode.StonybrookVillage][1],
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
                    dialogueTable[ShopDialogueCode.CanopyTown][0],
                    TKSprite.Normal,
                    () => SetDialogue(new ShopDialogue(
                    null,
                    dialogueTable[ShopDialogueCode.CanopyTown][1],
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
                    dialogueTable[ShopDialogueCode.TheImpactZone][0],
                    TKSprite.Normal,
                    () => SetDialogue(new ShopDialogue(
                    null,
                    dialogueTable[ShopDialogueCode.TheImpactZone][1],
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
                    dialogueTable[ShopDialogueCode.TheFlats][0],
                    TKSprite.Normal,
                    () => SetDialogue(new ShopDialogue(
                    null,
                    dialogueTable[ShopDialogueCode.TheFlats][1],
                    TKSprite.Question,
                    () => SetDialogue(new ShopDialogue(
                    null,
                    
                    dialogueTable[ShopDialogueCode.TheFlats][2],
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
                    dialogueTable[ShopDialogueCode.Space][0],
                    TKSprite.Question,
                    () => SetDialogue(new ShopDialogue(
                    null,
                    dialogueTable[ShopDialogueCode.Space][1],
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
