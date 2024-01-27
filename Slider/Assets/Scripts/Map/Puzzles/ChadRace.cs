using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;
using UnityEditor;

// Chad race should be attatched to chad
public class ChadRace : MonoBehaviour, ISavable
{
    enum State {
        TrackNotSetup,
        NotStarted,
        Started,
        Running,
        Cheated,
        CheatedTrackBroken,
        CheatedTrackFixed,
        ChadWon,
        PlayerWon,
        RaceEnded
    };

    public Transform startingLine;
    public Transform finishingLine;
    public Transform player;
    public float speed;
    public Transform startStileObjects;
    public SGrid jungleGrid;
    public bool tilesAdjacent;
    public Animator chadimator;
    public NPC npcScript;
    public GameObject talkingCanvas;
    public PlayerConditionals playerConditional;
    public GameObject startingFlag;

    private Vector2 chadStartLocal;
    private Vector2 playerStart;
    private Vector2 endPoint;
    private State raceState;
    private float startTime;
    private int dialogueCurrentTime;

    [SerializeField] private ParticleSystem[] speedLinesList;
    private bool speedLinesOn;

    private const string SAVE_STRING_PLAYER_WON = "JungleChadRacePlayerWon";

    private void OnEnable()
    {
        SGrid.OnGridMove += CheckChad;
        SGrid.OnSTileEnabled += CheckChad;
    }

    private void OnDisable()
    {
        SGrid.OnGridMove -= CheckChad;
        SGrid.OnSTileEnabled -= CheckChad;
    }

    // Start is called before the first frame update
    void Start()
    {
        // Setting all the starting params
        tilesAdjacent = false;
        raceState = State.NotStarted;
        // Chad's start location relative to the starting stile will always be the same
        chadStartLocal = transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        switch (raceState) {
            case State.TrackNotSetup:
                if (tilesAdjacent)
                {
                    DisplayAndTriggerDialogue("Ready to race?");
                    raceState = State.NotStarted;
                }
                break;

            case State.NotStarted:
                if (!tilesAdjacent)
                {
                    DisplayAndTriggerDialogue("Race time! Set up a track to the bell.");
                    raceState = State.TrackNotSetup;
                }
                ActivateSpeedLines(false);
                break;

            case State.Started:
                HandleStartCountdown();
                break;

            case State.Running:

                if (!tilesAdjacent) 
                {
                    // The player has cheated
                    AudioManager.Play("Record Scratch");
                    StartCoroutine(SetParameterTemporary("JungleChadEnd", 1, 0));
                    // chadEndLocal = transform.localPosition;
                    DisplayAndTriggerDialogue("Hey, no changing the track before the race is done!");
                    ActivateSpeedLines(false);
                    raceState = State.Cheated;
                } 
                else if (transform.position.y <= endPoint.y) 
                {
                    MoveChad();
                }
                else 
                {
                    // Chad has made it to the finish line
                    OnRaceWin();
                    // chadEndLocal = transform.localPosition;
                    raceState = State.ChadWon;
                    StartCoroutine(SetParameterTemporary("JungleChadEnd", 1, 0));
                    DisplayAndTriggerDialogue("Pfft, too easy. Come back when you're fast enough to compete with me.");
                }

                break;

            case State.Cheated:
                chadimator.SetBool("isWalking", false);
                
                if (tilesAdjacent)
                {
                    DisplayAndTriggerDialogue("Wanna try again, bozo?");
                    raceState = State.CheatedTrackFixed;
                }
                else
                {
                    CheckChad(jungleGrid, null);
                }
                break;

            case State.CheatedTrackBroken:
                if (tilesAdjacent)
                {
                    DisplayAndTriggerDialogue("Wanna try again, bozo?");
                    raceState = State.CheatedTrackFixed;
                }
                break;

            case State.CheatedTrackFixed:
                if (!tilesAdjacent)
                {
                    DisplayAndTriggerDialogue("Reset the track to the bell.");
                    raceState = State.CheatedTrackBroken;
                }
                break;

            case State.ChadWon:
                chadimator.SetBool("isWalking", false);
                transform.position = finishingLine.transform.position;

                break;

            case State.PlayerWon:
                // Not entirely sure why, but an offset of .5 in x gets chad in the right spot at the end
                if (transform.localPosition.y < finishingLine.localPosition.y) 
                {
                    MoveChad();
                } else 
                {
                    transform.localPosition = finishingLine.localPosition;
                    chadimator.SetBool("isWalking", false);
                    chadimator.SetBool("isSad", true);

                    if (PlayerInventory.Contains(jungleGrid.GetCollectible("Boots")))
                    {
                        raceState = State.RaceEnded;
                    }
                }
                break;

            case State.RaceEnded:
                break;

        }

        if (raceState != State.Started && raceState != State.Running && !playerConditional.onActionEnabled)
        {
            playerConditional.EnableConditionals();
        }
    }

    public void Save()
    {
        if (raceState == State.PlayerWon)
        {
            SaveSystem.Current.SetBool(SAVE_STRING_PLAYER_WON, true);
        }
    }

    public void Load(SaveProfile profile)
    {
        if (profile.GetBool(SAVE_STRING_PLAYER_WON))
        {
            raceState = State.PlayerWon;
        }

        if (profile.GetBool(JungleShapeManager.GetSaveString("Flag")))
        {
            PlaceStartingFlag(true);
        }
        else
        {
            startingFlag.SetActive(false);
        }
    }

    public void PlaceStartingFlag(bool fromSave=false)
    {
        startingFlag.SetActive(true);

        if (!fromSave)
        {
            ParticleManager.SpawnParticle(ParticleType.SmokePoof, startingFlag.transform.position, startingFlag.transform);
            AudioManager.Play("Hat Click");
        }
    }

    private void HandleStartCountdown()
    {
        SaveSystem.Current.SetBool("jungleChadRaceHasStarted", true);
        player.position = playerStart;

        int timeSinceStart = (int)(Time.time - startTime);

        if (timeSinceStart < 3) 
        {
            if (dialogueCurrentTime != 3 - timeSinceStart)
            {
                dialogueCurrentTime = 3 - timeSinceStart;  // 3... 2... 1...
                // Just changed
                AudioManager.Play("Pop");
            }
            DisplayAndTriggerDialogue(dialogueCurrentTime.ToString());
        } 
        else 
        {
            if (dialogueCurrentTime != 0)
            {
                dialogueCurrentTime = 0;
                AudioManager.PlayWithPitch("Pop", 1.2f);
                ParticleManager.SpawnParticle(ParticleType.MiniSparkle, startingLine.transform.position + new Vector3(0, 0.5f), startingLine.transform);
                ActivateSpeedLines(true);
            }
            DisplayAndTriggerDialogue("GO!");
            chadimator.SetBool("isWalking", true);
            StartCoroutine(SetParameterTemporary("JungleChadStarted", 1, 0));

            raceState = State.Running;
        }
    }

    public void CheckChad(object sender, System.EventArgs e)
    {
        if (SGrid.Current.GetGrid() != null)
            tilesAdjacent = CheckGrid.row(SGrid.GetGridString(), "523");
    }

    
    public void PlayerEnteredEnd() 
    {
        if (raceState == State.Running) {
            raceState = State.PlayerWon;

            StartCoroutine(SetParameterTemporary("JungleChadEnd", 1, 0));
            OnRaceWin();
        }
    }

    // Invoked by Player Conditionals on success
    public void StartQueued() {
        if (tilesAdjacent && 
            SaveSystem.Current.GetBool(JungleShapeManager.GetSaveString("Flag")) &&
            raceState != State.Started && 
            raceState != State.Running && 
            raceState != State.PlayerWon && 
            raceState != State.RaceEnded)
        {
            endPoint = finishingLine.position;
            transform.parent = startStileObjects;
            transform.localPosition = chadStartLocal;
            playerStart = new Vector2(transform.position.x, transform.position.y - 1);
            startTime = Time.time;
            playerConditional.DisableConditionals();
            
            raceState = State.Started;
        }
    }

    public void OnRaceWin()
    {
        AudioManager.Play("Puzzle Complete");
        ParticleManager.SpawnParticle(ParticleType.MiniSparkle, finishingLine.transform.position + Vector3.up, finishingLine.transform);
        ActivateSpeedLines(false);
    }

    // Conditionals stuff for Chad Dialogue
    public void TrackNotSetup(Condition cond) => cond.SetSpec(raceState == State.TrackNotSetup);
    public void NotStarted(Condition cond) => cond.SetSpec(raceState == State.NotStarted);
    public void CurrentlyRunning(Condition cond) => cond.SetSpec(raceState == State.Running);
    public void PlayerWon(Condition cond) => cond.SetSpec(raceState == State.PlayerWon);
    public void Cheated(Condition cond) => cond.SetSpec(raceState == State.Cheated);
    public void ChadWon(Condition cond) => cond.SetSpec(raceState == State.ChadWon);
    public void RaceEnded(Condition cond) => cond.SetSpec(raceState == State.RaceEnded);

    // Private helper methods
    private void MoveChad() 
    {
        // Chad goes all the way in the x direction before going in the y direction
        // Assume that the target location is up and to the right of the starting point
        Vector3 targetDirection = transform.position.x >= endPoint.x ? Vector3.up : Vector3.right;
        transform.position += speed * Time.deltaTime * targetDirection;

        // Assigns chad's current parent to the objects of the stile that he is currently over
        transform.parent = SGrid.GetSTileUnderneath(gameObject).transform;
    }

    private void ActivateSpeedLines(bool activate)
    {
        if (activate == speedLinesOn)
        {
            return;
        }

        speedLinesOn = activate;

        foreach (ParticleSystem lines in speedLinesList)
        {
            if (speedLinesOn)
            {
                lines.Play();
            }
            else
            {
                lines.Stop();
            }
        }
        
    }

    private void DisplayAndTriggerDialogue(string message) 
    {
        if (SaveSystem.Current.GetString("jungleChadSpeak") == message)
        {
            return;
        }

        SaveSystem.Current.SetString("jungleChadSpeak", message);
        npcScript.TypeCurrentDialogueSafe();
    }

    private IEnumerator SetParameterTemporary(string parameterName, float value1, float value2)
    {
        AudioManager.SetGlobalParameter(parameterName, value1);

        yield return new WaitForSeconds(1);
        
        AudioManager.SetGlobalParameter(parameterName, value2);
    }

    // agony
    public void SetCanvasInFallenPosition(bool isFallen)
    {
        talkingCanvas.transform.localPosition = isFallen ? new Vector3(-0.75f, 0) : Vector3.zero;
    }

    public void HealChad()
    {
        SaveSystem.Current.SetBool("jungleChadIsHealed", true);
        ParticleManager.SpawnParticle(ParticleType.SmokePoof, transform.position, transform);
        chadimator.SetBool("isCrutch", true);
    }
}
