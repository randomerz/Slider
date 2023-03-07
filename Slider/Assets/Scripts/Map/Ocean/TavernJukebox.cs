using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TavernJukebox : MonoBehaviour, ISavable
{   
    [SerializeField] private NPC npc;
    [SerializeField] private Animator animator;
    [SerializeField] private ParticleSystem particlesLeft;
    [SerializeField] private ParticleSystem particlesRight;
    private bool isPlayingLeftParticles;

    private int currentIndex;
    private const int NUMBER_OF_SONGS = 14;

    private float timeUntilBump = 0;
    private float timeBetweenBumps = 1;
    private const string BUMP_ANIMATION_NAME = "Bump";

    private readonly float[] songBPMs = new float[] {
        120f,   // Tavern
        85f*2f/3f,// Ocean 85 bpm 3/4
        170f/3f,// Ocean Island - its actually 170 bpm 3/4
        100f,   // Village
        47.5f,  // Caves - i think is 95
        140f,   // Jungle - 140
        176f,   // Casino - 88
        116f,   // Factory
        105f,   // Military
        41.5f,  // Mountain - i think is 166
        80f,    // MagiTech Future
        80f,    // MagiTech Past
        150f,   // Menu
        120f,   // Trailer
    };


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

    private void Update() 
    {
        if (timeUntilBump <= 0)
        {
            timeUntilBump += timeBetweenBumps;
            if (timeUntilBump <= 0)
            {
                // still < 0; this means the computer is really slow
                timeUntilBump += ((int)(timeUntilBump / timeBetweenBumps) + 1) * timeBetweenBumps;
            }
            
            Bump();
        }
        
        timeUntilBump -= Time.deltaTime;
    }

    private void Bump()
    {
        animator.Play(BUMP_ANIMATION_NAME, -1, 0);

        if (isPlayingLeftParticles)
            particlesLeft.Play();
        else
            particlesRight.Play();
        isPlayingLeftParticles = !isPlayingLeftParticles; 
    }

    
    public void IncrementSong()
    {
        currentIndex = (currentIndex + 1) % NUMBER_OF_SONGS;
        SetSong(currentIndex);

        AudioManager.Play("UI Click");
    }

    private void SetSong(int index)
    {
        SaveSystem.Current.SetString("oceanTavernJukeboxString", $"{index + 1} / {NUMBER_OF_SONGS}");
        npc.TypeCurrentDialogue();
        AudioManager.SetMusicParameter("Ocean Tavern", "OceanTrackNumber", index);

        timeBetweenBumps = 60f / songBPMs[index];
        timeUntilBump = 3f / 12; // 3 frame offset
    }

}
