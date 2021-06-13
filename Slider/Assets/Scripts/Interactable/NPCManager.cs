using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCManager : MonoBehaviour
{
    private Dictionary<GameObject, int> voicelines = new Dictionary<GameObject, int>();
    public GameObject[] npcs = new GameObject[8];
    public static int currSliders = 8;

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
        Debug.Log("in method");
        switch(Name)
        {
            case "Pierre":
                if (currSliders == 7 && (EightPuzzle.GetGrid()[0, 2].islandId == 6 && EightPuzzle.GetGrid()[1, 2].islandId == 2 && EightPuzzle.GetGrid()[2, 2].islandId == 4 && EightPuzzle.GetGrid()[2, 1].islandId == 7))
                {
                    voicelines[npcs[0]] = 1;
                }
                else if (currSliders == 8)
                {
                    Debug.Log("Enter if statement");
                    voicelines[npcs[0]] = 2;
                }
                else if (currSliders == 9)
                {
                    voicelines[npcs[0]] = 3;
                }
                break;
            case "Kevin":
                break;
            case "Felicia":
                break;
            case "Sam":
                break;
            case "Archibald":
                break;
            case "Romeo":
                break;
            case "Juliet":
                break;
            case "Fezziwig":
                break;
            case "Mayor":
            case "Chef":
                break;
            default:
                break;
        }
    }
}
