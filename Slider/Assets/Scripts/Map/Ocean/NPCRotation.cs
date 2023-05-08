using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCRotation : MonoBehaviour
{
    private List<string> rotationUpdates = new List<string>();
    [SerializeField] NPC diceGirl;
    [SerializeField] NPC diceGuy;
    [SerializeField] NPC fisherman;
    [SerializeField] NPC alien;
    [SerializeField] NPC porker;
    [SerializeField] NPC ike;
    [SerializeField] NPC traveling_merchant;
    [SerializeField] NPC catBeard;
    [SerializeField] NPC fezziwig;
    [SerializeField] NPC amberOak;
    [SerializeField] NPC muncher;
    [SerializeField] Transform leftDice; //vanessa -> amber oak
    [SerializeField] Transform rightDice;// bruce ->catbeard
    [SerializeField] Transform leftSign;//amberoak -> alien
    [SerializeField] Transform rightSign;//fezziwig
    [SerializeField] Transform leftEntrance; //trav merch
    [SerializeField] Transform rightEntrance; //porker
    [SerializeField] Transform left_left_table; //fisherman
    [SerializeField] Transform off_camera; 
    [SerializeField] Transform coconuts; //porker
    [SerializeField] Transform IkeSpot; //ike
    
    [SerializeField] private List<GameObject> diceGameObjects = new List<GameObject>();

    public bool gotBreadge = false; //saved in oceangrid.cs maybe need to update to Savable in the future
    public bool unlockedAllSliders = false;

    public void OnEnable()
    {
        ShopManager.OnTurnedItemIn += ChangeNPCS;
        AnchorFirstAppearance.OnAnchorAcquire += MoveAmberOak;
    }

    public void OnDisable()
    {
        ShopManager.OnTurnedItemIn -= ChangeNPCS;
        AnchorFirstAppearance.OnAnchorAcquire -= MoveAmberOak;
    }

    private void Start() 
    {
        InitTavern();
        UpdateTavern();
    }

    public void InitTavern()
    {
        List<Collectible.CollectibleData> collectibles = PlayerInventory.Instance.GetCollectiblesList();
        foreach (Collectible.CollectibleData item in collectibles)
        {
            switch (item.name)
            {
                case "Golden Fish": //fisherman joins the tavern
                    rotationUpdates.Add("fisherman");
                    break;

                case "Rock": //alien
                    rotationUpdates.Add("alien");
                    break;

                case "Treasure Chest"://dice ppl leave
                    rotationUpdates.Add("diceGirl");
                    rotationUpdates.Add("catBeard");
                    break;

                case "Magical Gem": //fezziwig joins
                    rotationUpdates.Add("fezziwig");
                    break;
                default:
                    break;
            }
            if (PlayerInventory.Instance.GetHasCollectedAnchor())
            {
                MoveAmberOak(this, null);
            }
            // if (SaveSystem.Current.GetBool("oceanPickedUpCoconut"))
            // {
            //     MovePorker();
            // }

            traveling_merchant.Teleport(off_camera, false);
            alien.Teleport(off_camera, false);
        }
    }


    //whenever player turns in an item, depending on item, bring npcs/in and out of tavern
    public void ChangeNPCS(object sender, ShopManager.OnTurnedItemInArgs args)
    {
        switch (args.item)
        {
            case "A Golden Fish": //fisherman joins the tavern
                rotationUpdates.Add("fisherman");
                break;

            case "A Peculiar Rock": //alien
                rotationUpdates.Add("alien");
                break;

            case "Cat Beard's Treasure": //catbeard joins
                rotationUpdates.Add("catBeard");
                break;

            case "A Magical Gem": //fezziwig joins, dice ppl leave
                rotationUpdates.Add("diceGirl");
                rotationUpdates.Add("fezziwig");
                unlockedAllSliders = true;
                break;
            default:
                break;
        }
    }

    public void UpdateTavern()
    {
        float rng = UnityEngine.Random.Range(0f, 1f);
        if (rng < .1f && !gotBreadge && unlockedAllSliders)
        {
            traveling_merchant.Teleport(leftEntrance, false);
        }
        else
        {
            traveling_merchant.Teleport(off_camera, false);
        }
        foreach (string person in rotationUpdates)
        {
            switch (person)
            {
                case "fisherman": //fisherman joins the tavern
                    fisherman.makeFaceRight();
                    fisherman.Teleport(left_left_table, false);
                    break;

                case "alien": //alien joins
                    alien.Teleport(leftSign, false);
                    break;

                case "diceGirl"://dice ppl leave, catberad and broke replace them
                    diceGirl.Teleport(off_camera, false);
                    diceGuy.Teleport(off_camera, false);
                    foreach (GameObject go in diceGameObjects)
                        go.SetActive(false);
                        
                    amberOak.Teleport(leftDice, false);
                    catBeard.Teleport(rightDice, false);
                    break;

                case "fezziwig": //fezziwig joins
                    fezziwig.Teleport(rightSign, false);
                    SaveSystem.Current.SetBool("oceanFezziwigInTavern", true);
                    break;
                case "porker"://move porker to the coconuts and change his dialogue
                    porker.Teleport(coconuts, false);
                    break;


                default:
                    break;
            }
        }
        rotationUpdates.Clear();
    }

    public void MovePorker()
    {
        if (!SaveSystem.Current.GetBool("oceanPorkerTraining") && PlayerInventory.Contains("Slider 7", Area.Ocean))
            porker.Teleport(coconuts, true);
    }

    public void MoveAmberOak(object sender, System.EventArgs e)
    {
        porker.Teleport(rightEntrance, false);
        ike.Teleport(IkeSpot, false);
        amberOak.Teleport(leftSign, false);
    }

    public void CollectedBreadge()
    {
        gotBreadge = true;
    }




}