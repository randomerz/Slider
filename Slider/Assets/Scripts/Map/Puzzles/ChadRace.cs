using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;

// Chad race should be attatched to chad
public class ChadRace : MonoBehaviour
{
    enum State {
        NotStarted,
        Started,
        Running,
        Cheated,
        ChadWon,
        PlayerWon
    };

    public UnityEvent onRaceWon;
    public Transform finishingLine;
    public Transform player;
    public float speed;
    public Transform startStileObjects;
    public SGrid jungleGrid;
    public bool tilesAdjacent;
    public Animator chadimator;
    public NPC npcScript;
    public DialogueConditionals countDownDialogue;

    private Vector2 chadStartLocal;
    private Vector2 playerStart;
    private Vector2 endPoint;
    private Vector2 chadEndLocal;
    private State raceState;
    private bool inStart;
    private float startTime;

    // Keeps track if this is the first time the play has tried the race with the current tile positions
    private bool firstTime;

    private float jungleChadEnd;

    // Start is called before the first frame update
    void Start()
    {
        // Setting all the starting params
        tilesAdjacent = false;
        inStart = false;
        firstTime = true;
        raceState = State.NotStarted;
        // Chad's start location relative to the starting stile will always be the same
        chadStartLocal = transform.localPosition;

        // Setting the chadimator initial bools
        chadimator.SetBool("isWalking", false);
        chadimator.SetBool("isSad", false);

        // Setting the first time dialogue
        countDownDialogue.dialogue = "Bet I could beat you to the bell (e to start)";
        npcScript.dconds.Add(countDownDialogue);
    }

    // Update is called once per frame
    void Update()
    {
        switch (raceState) {
            case State.Started:
                player.position = playerStart;
                float timeDiff = Time.unscaledTime - startTime;
                if (timeDiff < 3) {
                    DisplayAndTriggerDialogue((int)(4 - (Time.unscaledTime - startTime)) + "");
                } else {
                    DisplayAndTriggerDialogue("GO!");
                    chadimator.SetBool("isWalking", true);
                    raceState = State.Running;

                    // AudioManager.SetMusicParameter("Jungle", "JungleChadStarted", 1); // magnet to start of race
                    // AudioManager.SetMusicParameter("Jungle", "JungleChadWon", 0);
                    StartCoroutine(SetParameterTemporary("JungleChadStarted", 1, 0));
                }
                break;
            case State.Running:
                if (!tilesAdjacent) {
                    // The player has cheated
                    chadEndLocal = transform.localPosition;
                    raceState = State.Cheated;
                } else if (transform.position.y <= endPoint.y) {
                    MoveChad();
                } else {
                    // Chad has made it to the finish line
                    onRaceWon.Invoke();
                    chadEndLocal = transform.localPosition;
                    raceState = State.ChadWon;
                }
                
                break;
            case State.Cheated:
                chadimator.SetBool("isWalking", false);
                transform.localPosition = chadEndLocal;
                firstTime = true;
                break;
            case State.ChadWon:
                chadimator.SetBool("isWalking", false);
                countDownDialogue.dialogue = "Pfft, too easy. Come back when you're fast enough to compete with me. (e to start)";
                transform.localPosition = chadEndLocal;

                // AudioManager.SetMusicParameter("Jungle", "JungleChadStarted", 0);
                // AudioManager.SetMusicParameter("Jungle", "JungleChadWon", 1);
                jungleChadEnd = 1;
                break;
            case State.PlayerWon:
                // Not entirely sure why, but an offset of .5 in x gets chad in the right spot at the end
                chadEndLocal = finishingLine.localPosition - new Vector3(.5f, 2, 0);
                if (transform.localPosition.y < chadEndLocal.y) {
                    MoveChad();
                } else {
                    transform.localPosition = chadEndLocal;
                    chadimator.SetBool("isWalking", false);
                    chadimator.SetBool("isSad", true);

                    // AudioManager.SetMusicParameter("Jungle", "JungleChadStarted", 0);
                    // AudioManager.SetMusicParameter("Jungle", "JungleChadWon", 2);
                    jungleChadEnd = 1;
                }
                break;
        }
    }

    public void PlayerEnteredEnd() {
        if (raceState == State.Running) {
            raceState = State.PlayerWon;
            onRaceWon.Invoke();
        }
    }

    // Invoked by the NPC collider when the player is within the NPC's interact range
    public void InStartPosition() {
        inStart = true;
    }

    // Invoked by the NPC collider when the player exits the NPC's interact range
    public void NotInStartPosition() {
        inStart = false;
    }

    // Invoked by Player Conditionals on success
    public void StartQueued() {
        if (inStart && tilesAdjacent && (raceState != State.Started && raceState != State.Running && raceState != State.PlayerWon)) {
            endPoint = finishingLine.position - new Vector3(0, 1, 0);
            transform.parent = startStileObjects;
            transform.localPosition = chadStartLocal;
            raceState = State.Started;
            playerStart = new Vector2(transform.position.x, transform.position.y - 1);
            startTime = Time.unscaledTime;
        }
    }

    // Conditionals stuff for Chad Dialogue
    public void CurrentlyRunning(Conditionals.Condition cond) {
        cond.SetSpec(raceState == State.Running);
    }

    public void PlayerWon(Conditionals.Condition cond) {
        cond.SetSpec(raceState == State.PlayerWon);
    }

    public void Cheated(Conditionals.Condition cond) {
        cond.SetSpec(raceState == State.Cheated);
    }

    // Private helper methods
    private void MoveChad() {
        // Chad goes all the way in the x direction before going in the y direction
        // Assume that the target location is up and to the right of the starting point
        Vector3 targetDirection = transform.position.x >= endPoint.x ? new Vector3(0,1,0) : new Vector3(1,0,0);
        transform.position += + speed * targetDirection * Time.deltaTime;

        // Assigns chad's current parent to the objects of the stile that he is currently over
        transform.parent = jungleGrid.GetStileUnderneath(gameObject).transform.GetChild(0);
    }

    private void DisplayAndTriggerDialogue(string message) {
        countDownDialogue.dialogue = message;
        npcScript.TypeNextDialogue();
    }


    private IEnumerator SetParameterTemporary(string parameterName, float value1, float value2)
    {
        Debug.Log("Param update to " + value1);
        AudioManager.SetMusicParameter("Jungle", parameterName, value1);

        yield return new WaitForSeconds(1);
        
        Debug.Log("Param update to " + value2);
        AudioManager.SetMusicParameter("Jungle", parameterName, value2);
    }

    public void UpdateChadEnd()
    {
        if (jungleChadEnd == 1)
        {
            Debug.Log("peep ee poo poo");
            StartCoroutine(SetParameterTemporary("JungleChadEnd", 1, 0));
            jungleChadEnd = 0;
        }
    }
}
