using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : MonoBehaviour
{
    public string characterName;

    public NPC(string cName)
    {
        characterName = cName;
    }

    public int GetCurrentDialogueNumber()
    {
        return FindObjectOfType<NPCManager>().getVoiceLineNumber(characterName);
    }
}