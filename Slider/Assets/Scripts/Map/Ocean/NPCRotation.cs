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

    public bool gotBreadge = false; //saved in oceangrid.cs maybe need to update to Savable in the future
    public bool gotCatbeardTreasure = false;

    public void OnEnable()
    {
        ShopManager.OnTurnedItemIn += ChangeNPCS;
        AnchorFirstAppearance.OnAnchorAcquire += MoveAmberOak;
        InitTavern();
    }

    public void OnDisable()
    {
        ShopManager.OnTurnedItemIn -= ChangeNPCS;
        AnchorFirstAppearance.OnAnchorAcquire -= MoveAmberOak;
    }



    public void InitTavern()
    {
        List<Collectible.CollectibleData> collectibles = PlayerInventory.Instance.GetCollectiblesList();
        foreach (Collectible.CollectibleData item in collectibles)
        {
            switch (item.name)
            {
                case "A Golden Fish": //fisherman joins the tavern
                    rotationUpdates.Add("fisherman");
                    break;

                case "A Peculiar Rock": //alien
                    rotationUpdates.Add("alien");
                    break;

                case "Cat Beard's Treasure"://dice ppl leave
                    rotationUpdates.Add("diceGirl");
                    rotationUpdates.Add("catBeard");
                    break;

                case "A Magical Gem": //fezziwig joins
                    rotationUpdates.Add("fezziwig");
                    break;
                default:
                    break;
            }
            if (PlayerInventory.Instance.GetHasCollectedAnchor())
            {
                amberOak.Teleport(leftSign);
            }
            if (SaveSystem.Current.GetBool("oceanPickedUpCoconut"))
            {
                MovePorker();
            }

            traveling_merchant.Teleport(off_camera);
            alien.Teleport(off_camera);
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

            case "Cat Beard's Treasure"://dice ppl leave
                rotationUpdates.Add("diceGirl");
                rotationUpdates.Add("catBeard");
                gotCatbeardTreasure = true;
                break;

            case "A Magical Gem": //fezziwig joins
                rotationUpdates.Add("fezziwig");
                break;
            default:
                break;
        }
    }

    public void UpdateTavern()
    {
        float rng = UnityEngine.Random.Range(0f, 1f);
        if (rng < .1f && !gotBreadge && gotCatbeardTreasure)
        {
            traveling_merchant.Teleport(leftEntrance);
        }
        else
        {
            traveling_merchant.Teleport(off_camera);
        }
        foreach (string person in rotationUpdates)
        {
            switch (person)
            {
                case "fisherman": //fisherman joins the tavern
                    fisherman.makeFaceRight();
                    fisherman.Teleport(left_left_table);
                    break;

                case "alien": //alien joins
                    alien.Teleport(leftSign);
                    break;

                case "diceGirl"://dice ppl leave, catberad and broke replace them
                    diceGirl.Teleport(off_camera);
                    diceGuy.Teleport(off_camera);
                    amberOak.Teleport(leftDice);
                    catBeard.Teleport(rightDice);
                    break;

                case "fezziwig": //fezziwig joins
                    fezziwig.Teleport(rightSign);
                    break;
                case "porker"://move porker to the coconuts and change his dialogue
                    porker.Teleport(coconuts);
                    break;


                default:
                    break;
            }
        }
        rotationUpdates.Clear();
    }

    public void MovePorker()
    {
        porker.Teleport(coconuts);
    }

    public void MoveAmberOak(object sender, System.EventArgs e)
    {
        porker.Teleport(rightEntrance);
        ike.Teleport(IkeSpot);
        amberOak.Teleport(leftSign);
    }

    public void CollectedBreadge()
    {
        gotBreadge = true;
    }




}