using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCRotation : MonoBehaviour
{
    private List<string> rotationUpdates = new List<string> ();
    [SerializeField] NPC diceGirl;
    [SerializeField] NPC diceGuy;
    [SerializeField] NPC fisherman;
    [SerializeField] NPC alien;
    [SerializeField] NPC porker;
    [SerializeField] NPC traveling_merchant;
    [SerializeField] NPC catBeard;
    [SerializeField] NPC fezziwig;
    [SerializeField] NPC amberOak;
    [SerializeField] NPC muncher;
    [SerializeField] Transform tile2Transform;
    Transform tavernTransform;

    public bool gotBreadge = false;
    public bool gotCatbeardTreasure = false;

    public Dictionary<string,Vector3> tavernLoc = new Dictionary<string,Vector3>(){
        {"left_dice", new Vector3(-6, 4, 0)},
        {"right_dice", new Vector3(-1.5f, 5, 0)},
        {"left_entrance", new Vector3(-2, -2, 0)},
        {"right_entrance", new Vector3(4, 2, 0)},
        {"left_sign", new Vector3(-1.5f, 1.5f, 0)},
        {"left_left_table", new Vector3(-6, 1.5f, 0)},
        {"right_sign", new Vector3(3f, 1.5f, 0)},
        {"outside", new Vector3(20,0,0)},
        {"coconuts", new Vector3(6.75f, 4f, 0)},
        {"right_right_table", new Vector3(8.5f, 0.5f, 0)}  
    };
    public void OnEnable()
    {
        ShopManager.OnTurnedItemIn += ChangeNPCS;
        AnchorFirstAppearance.OnAnchorAcquire += MoveAmberOak;
        tavernTransform = diceGirl.transform.parent;
        InitTavern();
    }

    public void OnDisable()
    {
        ShopManager.OnTurnedItemIn -= ChangeNPCS;
    }



    public void InitTavern()
    {
        List<Collectible.CollectibleData> collectibles = PlayerInventory.Instance.GetCollectiblesList();
        foreach(Collectible.CollectibleData item in collectibles)
        {
            switch(item.name)
            {
                case "A Golden Fish": //fisherman joins the tavern
                    rotationUpdates.Add("fisherman");
                    break;

                case "A Peculiar Rock": //alien
                    rotationUpdates.Add("alien");
                    break;
                
                case "Cat Beard's Treasure"://dice ppl leave
                    Debug.Log("adding dice duo to change list");
                    rotationUpdates.Add("diceGirl");
                    rotationUpdates.Add("catBeard");
                    break;
                
                case "A Magical Gem": //fezziwig joins
                    rotationUpdates.Add("fezziwig");
                    break;
                default:
                    break;
            }
            if(PlayerInventory.Instance.GetHasCollectedAnchor()){
                Debug.Log("adding dice duo to change list");
                amberOak.MoveLocal(amberOak.transform.parent, tavernLoc["left_sign"]);
            }
            if(SaveSystem.Current.GetBool("oceanPickedUpCoconut"))
                MovePorker();

            traveling_merchant.MoveLocal(traveling_merchant.transform.parent, tavernLoc["outside"]);
            alien.MoveLocal(alien.transform.parent, tavernLoc["outside"]);
        }
    }


   //whenever player turns in an item, depending on item, bring npcs/in and out of tavern
    public void ChangeNPCS(object sender, ShopManager.OnTurnedItemInArgs args )
    {
        switch(args.item){
            case "A Golden Fish": //fisherman joins the tavern
                rotationUpdates.Add("fisherman");
                break;

            case "A Peculiar Rock": //alien
                rotationUpdates.Add("alien");
                break;
            
            case "Cat Beard's Treasure"://dice ppl leave
                Debug.Log("adding dice duo to change list");
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

    public void UpdateTavern(){
        Debug.Log("Updating tavern npcs!");
        float rng = UnityEngine.Random.Range(0f, 1f);
        Debug.Log("RNG: " + rng + " gotCatbeardTreasure: " + gotCatbeardTreasure + " gotBreadge: " + gotBreadge);
        if(rng < .1f && !gotBreadge && gotCatbeardTreasure){
            traveling_merchant.MoveLocal(traveling_merchant.transform.parent, tavernLoc["left_entrance"]);
        }else{
            traveling_merchant.MoveLocal(traveling_merchant.transform.parent, tavernLoc["outside"]);
        }
        foreach(string person in rotationUpdates){
            Debug.Log("Updating "+ person);
            switch(person){
            case "fisherman": //fisherman joins the tavern
                fisherman.MoveLocal(fisherman.transform.parent, tavernLoc["right_sign"]);
                break;

            case "alien": //alien joins
                alien.MoveLocal(alien.transform.parent, tavernLoc["left_left_table"]);
                break;
            
            case "diceGirl"://dice ppl leave, catberad and broke replace them
                Debug.Log("Evicting the dice duo!");
                diceGirl.MoveLocal(diceGirl.transform, tavernLoc["outside"]);
                diceGuy.MoveLocal(diceGuy.transform, tavernLoc["outside"]);
                amberOak.MoveLocal(tavernTransform, tavernLoc["left_dice"]);
                catBeard.MoveLocal(tavernTransform, tavernLoc["right_dice"]);
                break;
            
            case "fezziwig": //fezziwig joins
                fezziwig.MoveLocal(fezziwig.transform.parent, tavernLoc["right_sign"]);
                break;
            case "porker"://move porker to the coconuts and change his dialogue
                porker.MoveLocal(tile2Transform, tavernLoc["coconutArea"]);
                break;
            

            default:
                break;
        }
        }
        rotationUpdates.Clear();
    }

    public void MovePorker(){
        Debug.Log("coconuts");
        porker.MoveLocal(tile2Transform, tavernLoc["coconuts"]);
    }

    public void MoveAmberOak(object sender, System.EventArgs e){
        Debug.Log("moving AmberOak");
        amberOak.MoveLocal(amberOak.transform.parent, tavernLoc["left_sign"]);
    }

    public void CollectedBreadge(){
        gotBreadge = true;
    }

    


}