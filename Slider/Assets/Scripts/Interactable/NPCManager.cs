using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCManager : MonoBehaviour
{
    private Dictionary<GameObject, int> voicelines = new Dictionary<GameObject, int>();
    public GameObject[] npcs = new GameObject[8];

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

    public int getVoiceLineNumber(string name)
    {
        //Debug.Log(name);
        foreach (KeyValuePair<GameObject, int> e in voicelines)
        {
            if (e.Key.GetComponent<NPC>().characterName == name)
            {
                return e.Value;
            } 
        }
        return -1;
    }

    public void changeVoiceLine(string name, int val)
    {
        foreach (KeyValuePair<GameObject, int> e in voicelines)
        {
            if (e.Key.GetComponent<NPC>().characterName == name)
            {
                voicelines[e.Key] = val;
            }
        }
    }

}
