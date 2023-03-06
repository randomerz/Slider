using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TavernJukebox : MonoBehaviour, ISavable
{
    private int currentIndex;
    private const int NUMBER_OF_SONGS = 14;
    
    [SerializeField] private NPC npc;

    void Start()
    {
        SetSong(currentIndex); // should come from load
    }

    public void Save()
    {
        SaveSystem.Current.SetInt("oceanTavernJukeboxIndex", NUMBER_OF_SONGS);
    }

    public void Load(SaveProfile profile)
    {
        currentIndex = profile.GetInt("oceanTavernJukeboxIndex");
    }

    
    public void IncrementSong()
    {
        currentIndex = (currentIndex + 1) % NUMBER_OF_SONGS;
        SetSong(currentIndex);
    }

    private void SetSong(int index)
    {
        SaveSystem.Current.SetString("oceanTavernJukeboxString", $"{index + 1} / {NUMBER_OF_SONGS}");
        npc.TypeCurrentDialogue();
        AudioManager.SetMusicParameter("Ocean Tavern", "OceanTrackNumber", index);
    }

}
