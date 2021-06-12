using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCManager : MonoBehaviour
{
    private Dictionary<NPC, int> voicelines = new Dictionary<NPC, int>();
    public NPC[] npcs = new NPC[8];

    private void Awake()
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
        foreach (KeyValuePair<NPC, int> e in voicelines)
        {
            if (e.Key.characterName == name)
            {
                return e.Value;
            } 
        }
        return -1;
    }

    public void changeVoiceLine(string name)
    {
        foreach (KeyValuePair<NPC, int> e in voicelines)
        {
            if (e.Key.characterName == name)
            {
                voicelines[e.Key] = e.Value + 1;
            }
        }
    }

}
