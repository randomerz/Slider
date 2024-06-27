using System.Linq;
using UnityEngine;

public partial class ChadChirp : MonoBehaviour, ISavable
{
    public class ChadChirpArgs : System.EventArgs
    {
        public string id;
    }

    public static System.EventHandler<ChadChirpArgs> OnTryChirp; // First come first serve!

    private const string CHIRP_SAVE_STRING = "MiscChadFollowPlayerChirp";
    private const int NUMBER_OF_SMALL_TALKS = 3;

    private const float CAN_CHIRP_COOLDOWN = 3.5f; // for special events, like meeting the money wizard
    private static float timeUntilCanChirp;
    private const float WANT_CHIRP_COOLDOWN = 30f; // for random stuff like "woah ill never get used to that portal"
    private static float timeUntilWantsToChirp;
    private Coroutine smalltalkCoroutine;
    private const float SMALL_TALK_COOLDOWN = 120f;
    private const float SMALL_TALK_RETRY_COOLDOWN = 5f;
    
    [Header("References")]
    public NPC npc;
    public ChadFollowPlayer chadFollowPlayer;

    private void Start()
    {
        SaveSystem.Current.SetString(CHIRP_SAVE_STRING, "Onwards!"); // fallback case

        RestartSmallTalk();
        HandleChangeScene();
    }

    private void OnEnable()
    {
        OnTryChirp += TryChirp;
        Anchor.OnAnchorInteract += OnAnchorInteract;
    }

    private void OnDisable()
    {
        OnTryChirp -= TryChirp;
        Anchor.OnAnchorInteract -= OnAnchorInteract;
    }

    public void Save()
    {
        foreach (ChadChirpData d in ChadChirpData.chirpDataList)
        {
            SaveSystem.Current.SetBool(ChadChirpData.GetChirpUsedSaveString(d), d.hasBeenUsed);
        }
    }

    public void Load(SaveProfile profile)
    {
        for (int i = 0; i < ChadChirpData.chirpDataList.Count; i++)
        {
            ChadChirpData d = ChadChirpData.chirpDataList[i];
            d.hasBeenUsed = profile.GetBool(ChadChirpData.GetChirpUsedSaveString(d));
        }
    }

    private void Update()
    {
        timeUntilCanChirp -= Time.deltaTime;
        timeUntilWantsToChirp -= Time.deltaTime;
    }


    public void TryChirp(object sender, ChadChirpArgs e)
    {
        TryChirp(e.id);
    }

    public void TryChirp(string id)
    {
        if (!chadFollowPlayer.isFollowingEnabled)
        {
            return;
        }

        if (npc.IsDialogueBoxActive())
        {
            return;
        }

        ChadChirpData data = ChadChirpData.chirpDataList.FirstOrDefault(d => d.id == id);

        if (data == null)
        {
            Debug.LogError($"Could not find '{id}' in list of available chirps!");
            return;
        }

        if (!data.canBeRepeated && data.hasBeenUsed)
        {
            return;
        }

        switch (data.priority)
        {
            case 2:
                if (data.canBeRepeated && data.hasBeenUsed)
                {
                    if (timeUntilWantsToChirp > 0)
                    {
                        return;
                    }
                }
                else
                {
                    if (timeUntilCanChirp > 0)
                    {
                        return;
                    }
                }
                break;

            case 1:
            default:
                if (timeUntilWantsToChirp > 0)
                {
                    return;
                }
                break;
        }

        data.hasBeenUsed = true;
        timeUntilCanChirp = CAN_CHIRP_COOLDOWN;
        timeUntilWantsToChirp = WANT_CHIRP_COOLDOWN;
        RestartSmallTalk();
        TypeDialogue(data.text);
    }

    public void TypeDialogue(string text)
    {
        SaveSystem.Current.SetString(CHIRP_SAVE_STRING, text);
        npc.TypeCurrentDialogue();
    }

    private void RestartSmallTalk()
    {
        if (smalltalkCoroutine != null)
        {
            StopCoroutine(smalltalkCoroutine);
        }

        smalltalkCoroutine = CoroutineUtils.ExecuteAfterDelay(() => TrySmallTalk(), this, SMALL_TALK_COOLDOWN);
    }

    private void TrySmallTalk()
    {
        int rand = 1 + Random.Range(0, NUMBER_OF_SMALL_TALKS);
        string id = $"RandomSmallTalk{rand}";
        ChadChirpData data = ChadChirpData.chirpDataList.FirstOrDefault(d => d.id == id);

        if (data == null)
        {
            Debug.LogError($"Number of small talks might be misconfigured!");
            smalltalkCoroutine = null;
            return;
        }

        if (data.hasBeenUsed)
        {
            // Retry
            smalltalkCoroutine = CoroutineUtils.ExecuteAfterDelay(() => TrySmallTalk(), this, SMALL_TALK_RETRY_COOLDOWN);
            return;
        }

        smalltalkCoroutine = null;
        TryChirp(id);
    }



    private void HandleChangeScene()
    {
        Area lastArea = SceneSpawns.lastArea;
        Area currentArea = SGrid.Current.GetArea();
        
        string id = GetIDToUse(lastArea, currentArea);

        if (id != null)
        {
            CoroutineUtils.ExecuteAfterDelay(() => TryChirp(id), this, 1.75f);
        }
    }

    private string GetIDToUse(Area lastArea, Area currentArea)
    {
        if (lastArea == Area.Jungle)
        {
            return "CameFromJungle";
        }
        if (lastArea == Area.Military)
        {
            return "CameFromMilitary";
        }
        if (currentArea == Area.Desert)
        {
            return "ArrivedInDesert";
        }
        if (currentArea == Area.Factory && !FactoryGrid.PlayerInPast)
        {
            return "ArrivedInFactoryPresent";
        }
        if (currentArea == Area.MagiTech && !MagiTechGrid.IsInPast(Player.GetInstance().transform))
        {
            return "ArrivedInMagiTechPresent";
        }
        return null;
    }

    private void OnAnchorInteract(object sender, Anchor.OnAnchorInteractArgs e)
    {
        if (e.drop)
        {
            TryChirp("UsedAnchor");
        }
    }

    private void OnTimeChange(object sender, Portal.OnTimeChangeArgs e)
    {
        if (!e.betweenAreas)
        {
            TryChirp("WentThroughPortal");
        }
    }
}