using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TavernJukebox : MonoBehaviour, ISavable
{
    private int currentIndex;
    private const int NUMBER_OF_SONGS = 14;

    private readonly float[] songBPMs = new float[] {
        120f,   // Tavern
        85f,    // Ocean
        85f,    // Ocean Island - its actually 170
        100f,   // Village
        95f,    // Caves - i think
        170f,   // Jungle
        177f,   // Casino
        116f,   // Factory - probably wrong T ^ T
        105f,   // Military
        166f,   // Mountain - i think
        80f,    // MagiTech Future
        80f,    // MagiTech Past
        150f,   // Menu
        120f,   // Trailer
    };
    
    [SerializeField] private NPC npc;

    void Start()
    {
        SetSong(currentIndex); // should come from load
    }

    public void Save()
    {
        SaveSystem.Current.SetInt("oceanTavernJukeboxIndex", currentIndex);
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
