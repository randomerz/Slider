using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCManager : MonoBehaviour
{
    private Dictionary<GameObject, int> voicelines = new Dictionary<GameObject, int>();
    public GameObject[] npcs = new GameObject[11];
    public static int currSliders = 1;
    public static bool fishOn = false;
    public static bool LoversOnFirstTime = false;
    public static bool hasBeenDug = false;
    public static bool firstTimeFezziwigCheck = false;
    public static bool firstTimeArchibaldCheck = false;
    public static bool firstTimePierreCheck = false;
    public static bool firstTimeKevinCheck = false;


    void Start()
    {
        voicelines.Add(npcs[0], 0);
        voicelines.Add(npcs[1], 0);
        voicelines.Add(npcs[2], 0);
        voicelines.Add(npcs[3], 0);
        voicelines.Add(npcs[4], 0);
        voicelines.Add(npcs[5], 0);
        voicelines.Add(npcs[6], 0);
        voicelines.Add(npcs[7], 0);
        voicelines.Add(npcs[8], 0);
        voicelines.Add(npcs[9], 0);
        voicelines.Add(npcs[10], 0);
    }

    public void ChangeWorldState()
    {
        currSliders++;
    }

    public int getVoiceLineNumber(string name)
    {
        //Debug.Log(name);
        int counter = 0;
        foreach (KeyValuePair<GameObject, int> e in voicelines)
        {
            if (e.Key.GetComponent<NPC>().characterName == name)
            {
                break;
            }
            counter++;
        }
        CheckWorldState(name);
        return voicelines[npcs[counter]];
    }

    public void changeVoiceLine(string name, int val)
    {
        int counter = 0;
        foreach (KeyValuePair<GameObject, int> e in voicelines)
        {
            if (e.Key.GetComponent<NPC>().characterName == name)
            {
                voicelines[npcs[counter]] = val;
            }
            counter++;
        }
    }

    public void CheckWorldState(string Name)
    {
        switch (Name)
        {
            case "Pierre":
                if (!firstTimePierreCheck && currSliders == 7 && fishOn && VillageGrid.instance.CheckRiver())
                {
                    voicelines[npcs[0]] = 1;
                    // ItemManager.ActivateNextItem();
                    VillageGrid.instance.ActivateSliderCollectible(8);
                    firstTimePierreCheck = true;
                }
                else if (firstTimePierreCheck && currSliders == 7 && fishOn && VillageGrid.instance.CheckRiver())
                {
                    voicelines[npcs[0]] = 1;
                }
                else if (currSliders == 8)
                {
                    voicelines[npcs[0]] = 2;
                }
                else if (currSliders == 9)
                {
                    voicelines[npcs[0]] = 3;
                }
                break;
            case "Kevin":
                if (!firstTimeKevinCheck && currSliders == 4 && KnotBox.PuzzleComplete())
                {
                    voicelines[npcs[1]] = 1;
                    // ItemManager.ActivateNextItem();
                    VillageGrid.instance.ActivateSliderCollectible(5);
                    firstTimeKevinCheck = true;
                }
                else if (firstTimeKevinCheck && currSliders == 4 && KnotBox.PuzzleComplete())
                {
                    voicelines[npcs[1]] = 1;
                }
                else if (currSliders > 4 && currSliders < 9)
                {
                    voicelines[npcs[1]] = 2;
                }
                else if (currSliders == 9)
                {
                    voicelines[npcs[1]] = 3;
                }
                break;
            case "Felicia":
                if (currSliders > 6 && currSliders < 9 && PlayerInventory.Contains("Dig"))
                {
                    voicelines[npcs[2]] = 1;
                }
                else if (currSliders == 9)
                {
                    voicelines[npcs[2]] = 2;
                }
                break;
            case "Sam":
                if (!fishOn && currSliders == 7 && VillageGrid.instance.CheckRiver())
                {
                    voicelines[npcs[3]] = 1;
                    AudioManager.Play("Puzzle Complete");
                    fishOn = true;
                }
                else if (fishOn && currSliders == 7 && VillageGrid.instance.CheckRiver())
                {
                    voicelines[npcs[3]] = 1;
                }
                else if (currSliders == 8)
                {
                    voicelines[npcs[3]] = 2;
                }
                else if (currSliders == 9)
                {
                    voicelines[npcs[3]] = 3;
                }
                break;
            case "Archibald":
                if (!firstTimeArchibaldCheck && currSliders == 3 && PlayerInventory.Contains("Coffee"))
                {
                    voicelines[npcs[4]] = 1;
                    // ItemManager.ActivateNextItem();
                    VillageGrid.instance.ActivateSliderCollectible(4);
                    firstTimeArchibaldCheck = true;
                }
                else if (firstTimeArchibaldCheck && currSliders == 3 && PlayerInventory.Contains("Coffee"))
                {
                    voicelines[npcs[4]] = 1;
                }
                else if (currSliders > 3 && currSliders < 9)
                {
                    voicelines[npcs[4]] = 2;
                }
                else if (currSliders == 9)
                {
                    voicelines[npcs[4]] = 3;
                }
                break;
            case "Romeo":
                if (currSliders >= 5 && currSliders < 9 && VillageGrid.instance.CheckLovers())
                {
                    voicelines[npcs[5]] = 1;
                    if (!LoversOnFirstTime)
                    {
                        // ItemManager.ActivateNextItem();
                        VillageGrid.instance.ActivateSliderCollectible(6);
                        LoversOnFirstTime = true;
                    }
                }
                else if (currSliders == 9)
                {
                    voicelines[npcs[5]] = 2;
                }
                else
                {
                    voicelines[npcs[5]] = 0;
                }
                break;
            case "Juliet":
                if (currSliders >= 5 && currSliders < 9 && VillageGrid.instance.CheckLovers())
                {
                    voicelines[npcs[6]] = 1;
                    if (!LoversOnFirstTime)
                    {
                        // ItemManager.ActivateNextItem();
                        VillageGrid.instance.ActivateSliderCollectible(6);
                        LoversOnFirstTime = true;
                    }

                }
                else if (currSliders == 9)
                {
                    voicelines[npcs[6]] = 2;
                }
                else
                {
                    voicelines[npcs[6]] = 0;
                }
                break;
            case "Fezziwig":
                if (firstTimeFezziwigCheck)
                {
                    voicelines[npcs[7]] = 1;
                }
                if (currSliders == 9)
                {
                    voicelines[npcs[7]] = 2;
                }
                break;
            default:
                break;
        }
    }
}
