using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoggySeasManager : MonoBehaviour, ISavable
{
    private Vector2Int[] correctPath =
    {
        Vector2Int.left,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.left,
        Vector2Int.up,
        Vector2Int.left,
    };
    private int playerIndex = 0;
    private Vector2Int playerMovement;
    private int lastIslandId = 1;
    private bool foggyCompleted;
    public FogAnimationController fogAnimationController6;
    public FogAnimationController fogAnimationController7;
    public GameObject fogIsland;
    private int fogIslandId; //tile which fog island was found on

    [Header("Foggy Progress Notes")]
    [SerializeField] private List<SpriteRenderer> progressNotes;
    [SerializeField] private Sprite emptyNote, fullNote;
    [SerializeField] private GameObject sparklePrefab;

    public void Save()
    {
        SaveSystem.Current.SetBool("oceanFoggyIslandReached", foggyCompleted);
    }

    public void Load(SaveProfile profile)
    {
        foggyCompleted = profile.GetBool("oceanFoggyIslandReached");
    }


    private void Start()
    {
        fogIsland.SetActive(false);
        foreach (SpriteRenderer note in progressNotes)
        {
            note.enabled = false;
        }
    }

    private void Update()
    {
        UpdatePlayerMovement();
    }

    private void UpdatePlayerMovement()
    {
        if (Player.GetInstance().GetSTileUnderneath() == null)
        {
            if (lastIslandId == 6 || lastIslandId == 7)
            {
                lastIslandId = 1;
                FailFoggy();
                SetProgressRingActive(false);
            }
            return;
        }

        int currentIslandId = Player.GetInstance().GetSTileUnderneath().islandId;

        if ((currentIslandId == 6 || currentIslandId == 7) && !foggyCompleted)
        {
            SetProgressRingActive(true);
            SaveSystem.Current.SetBool("OceanEnteredFoggy", true);
        }

        if (EnteredNewFoggyTile(currentIslandId))
        {
            CheckNewFoggyTile(currentIslandId);
        }

        lastIslandId = currentIslandId;
    }

    private bool EnteredNewFoggyTile(int currentIslandId)
    {
        return currentIslandId != lastIslandId && (lastIslandId == 6 || lastIslandId == 7);
    }

    private void CheckNewFoggyTile(int currentIslandId)
    {
        fogAnimationController6.SetIsVisible(true);
        fogAnimationController7.SetIsVisible(true);
        fogIsland.SetActive(false);

        if (currentIslandId != 6 && currentIslandId != 7)
        {
            FailFoggy();
            SetProgressRingActive(false);
        }
        else
        {
            STile current = SGrid.Current.GetStile(currentIslandId);
            STile old = SGrid.Current.GetStile(lastIslandId);

            Vector2Int currentPos = new Vector2Int(current.x, current.y);
            Vector2Int oldPos = new Vector2Int(old.x, old.y);

            playerMovement = currentPos - oldPos;
            CheckFoggySeas();
        }
    }



    public void CheckFoggySeas()
    {
        bool correct = FoggyCorrectMovement();
        
        if (SGrid.Current.GetStile(6).isTileActive 
            && SGrid.Current.GetStile(7).isTileActive 
            && foggyCompleted)
        {
            if(Player.GetInstance().GetSTileUnderneath().islandId == fogIslandId && correct) 
            {
                ActivateIsland();
            }
        }
        else
        {
            fogAnimationController6.SetIsVisible(true);
            fogAnimationController7.SetIsVisible(true);
        }
        
    }

    public void ActivateIsland()
    {
        STile playerStile = Player.GetInstance().GetSTileUnderneath();
        fogIsland.transform.position = playerStile.transform.position;
        fogIsland.transform.SetParent(playerStile.transform);
        fogIsland.SetActive(true);
        SetProgressRingActive(false);

        if (fogIslandId == 6)
            fogAnimationController6.SetIsVisible(false);
        else
            fogAnimationController7.SetIsVisible(false);
    }

    private void FoggySeasAudio()
    {
        AudioManager.PlayWithPitch("Puzzle Complete", 0.5f + playerIndex * 0.1f);
        AudioManager.SetGlobalParameter("OceanFoggyProgress", playerIndex);
    }

    public bool FoggyCorrectMovement()
    {
        if(playerIndex == correctPath.Length - 1 && !foggyCompleted && correctPath[playerIndex] == playerMovement)
        {
            playerIndex++;
            FoggyCompleted();
            return true; 
        }
        else if (0 <= playerIndex && playerIndex < correctPath.Length - 1 && correctPath[playerIndex] == playerMovement)
        {
            playerIndex++;
            FoggySeasAudio();
            progressNotes[playerIndex - 1].sprite = fullNote;
            return false;
        }
        else
        {
            FailFoggy();
            return false;
        }

    }

    private void FailFoggy()
    {
        if (playerIndex != 0 && playerIndex != 6)
        {
            AudioManager.Play("Artifact Error");
        }

        playerIndex = 0;
        foggyCompleted = false; // DC: idk why we made it only completable once
        AudioManager.SetGlobalParameter("OceanFoggyProgress", 0);
        foreach (SpriteRenderer note in progressNotes)
        {
            if (note.sprite.Equals(fullNote))
            {
                Instantiate(sparklePrefab, note.gameObject.transform.position, Quaternion.identity);
            }
            note.sprite = emptyNote;
        }
    }

    private void FoggyCompleted()
    {
        foggyCompleted = true;
        fogIslandId = Player.GetInstance().GetSTileUnderneath().islandId;
        for(int i =0; i < correctPath.Length; i++)
            progressNotes[i].sprite = emptyNote;
    }

    private void SetProgressRingActive(bool active)
    {
        foreach (SpriteRenderer note in progressNotes)
        {
            note.enabled = active;
            if (!active)
            {
                Instantiate(sparklePrefab, note.gameObject.transform.position, Quaternion.identity);
            }
        }
    }
}
