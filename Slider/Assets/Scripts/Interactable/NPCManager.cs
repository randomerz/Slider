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
        switch(Name)
        {
            case "Pierre":
                if (currSliders == 7 && fishOn && (EightPuzzle.GetGrid()[0, 2].islandId == 6 && EightPuzzle.GetGrid()[1, 2].islandId == 2 && EightPuzzle.GetGrid()[2, 2].islandId == 4 && EightPuzzle.GetGrid()[2, 1].islandId == 7))
                {
                    voicelines[npcs[0]] = 1;
                    ItemManager.ActivateNextItem();
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
                if (currSliders == 4 && (true))
                {
                    voicelines[npcs[1]] = 1;
                    ItemManager.ActivateNextItem();
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
                if (currSliders == 6 && Item.hasBeenDug)
                {
                    voicelines[npcs[2]] = 1;
                }
                else if (currSliders == 9)
                {
                    voicelines[npcs[2]] = 2;
                }
                break;
            case "Sam":
                if (currSliders == 7 && (EightPuzzle.GetGrid()[0, 2].islandId == 6 && EightPuzzle.GetGrid()[1, 2].islandId == 2 && EightPuzzle.GetGrid()[2, 2].islandId == 4 && EightPuzzle.GetGrid()[2, 1].islandId == 7))
                {
                    voicelines[npcs[3]] = 1;
                    fishOn = true;
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
                if (currSliders == 3 && Item.coffeeHasBeenDrunk)
                {
                    voicelines[npcs[4]] = 1;
                    ItemManager.ActivateNextItem();
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
                if (currSliders >= 5 &&  CheckLovers())
                {
                    voicelines[npcs[5]] = 1;
                    if (!LoversOnFirstTime)
                    {
                        ItemManager.ActivateNextItem();
                        LoversOnFirstTime = true;
                    }
                }
                else if (currSliders == 9)
                {
                    voicelines[npcs[5]] = 2;
                }else
                {
                    voicelines[npcs[5]] = 0;
                }
                break;
            case "Juliet":
                if (currSliders >= 5 && CheckLovers())
                {
                    voicelines[npcs[6]] = 1;
                    if (!LoversOnFirstTime)
                    {
                        ItemManager.ActivateNextItem();
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
                if (currSliders == 9)
                {
                    voicelines[npcs[7]] = 1;
                }
                break;
            default:
                break;
        }
    }

    public static bool CheckQRCode()
    {
        if (hasBeenDug)
        {
            return false;
        }
        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                if (EightPuzzle.GetGrid()[x, y].islandId == 6 && !EightPuzzle.GetGrid()[x, y].isEmpty)
                {
                    if (y != 2 && x != 2 && EightPuzzle.GetGrid()[x, y + 1].islandId == 3 && EightPuzzle.GetGrid()[x + 1, y].islandId == 2 && EightPuzzle.GetGrid()[x + 1, y + 1].islandId == 1)
                    {
                        hasBeenDug = true;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }
        return false;
    }

    public bool CheckLovers()
    {
        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                if (EightPuzzle.GetGrid()[x, y].islandId == 5 && !EightPuzzle.GetGrid()[x, y].isEmpty)
                {
                    if (x != 0 && EightPuzzle.GetGrid()[x - 1, y].islandId == 1)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }
        return false;
    }

    public static bool CheckFinalPlacements()
    {
        if (EightPuzzle.GetGrid()[0, 0].islandId == 1 && !EightPuzzle.GetGrid()[0, 0].isEmpty &&
            EightPuzzle.GetGrid()[1, 0].islandId == 5 && !EightPuzzle.GetGrid()[1, 0].isEmpty &&
            EightPuzzle.GetGrid()[2, 0].islandId == 3 && !EightPuzzle.GetGrid()[2, 0].isEmpty &&
            EightPuzzle.GetGrid()[0, 1].islandId == 8 && !EightPuzzle.GetGrid()[0, 1].isEmpty &&
            EightPuzzle.GetGrid()[2, 1].islandId == 7 && !EightPuzzle.GetGrid()[2, 1].isEmpty &&
            EightPuzzle.GetGrid()[0, 2].islandId == 6 && !EightPuzzle.GetGrid()[0, 2].isEmpty &&
            EightPuzzle.GetGrid()[1, 2].islandId == 2 && !EightPuzzle.GetGrid()[1, 2].isEmpty &&
            EightPuzzle.GetGrid()[2, 2].islandId == 4 && !EightPuzzle.GetGrid()[2, 2].isEmpty)
        {
            return true;
        }
        return false;
    }
}
