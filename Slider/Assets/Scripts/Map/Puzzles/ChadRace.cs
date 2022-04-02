using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;

// Chad race should be attatched to chad
public class ChadRace : MonoBehaviour
{
    public UnityEvent onPlayerWin;
    public Transform finishingLine;
    public Transform player;
    public float speed;

    public bool tilesAdjacent;

    public Animator chadimator;

    //Scuffed dialogue stuff
    public NPC npcScript;
    public DialogueConditionals countDownDialogue;

    

    // The local position of the chad so he goes back to his spot on the STile
    private Vector2 chadStartLocal;

    // The global starting position of the player
    private Vector2 playerStart;

    //The global position of the endpoint so chad can calculate where he needs to go
    private Vector2 endPoint;

    // True when player is in the start position
    private bool inStart;

    // True when the race has first started, turns false once the race is running
    private bool started;

    // True when the race is running
    private bool running;

    // Keeps track of when the race was started
    private float startTime;

    // Keeps track of if player won
    private bool playerWon;

    // Keeps track if this is the first time the play has tried the race with the current tile positions
    private bool firstTime;

    // Keeps track of if the player cheated
    private bool cheated;

    // Start is called before the first frame update
    void Start()
    {
        // Setting all the starting params
        tilesAdjacent = false;
        inStart = false;
        started = false;
        running = false;
        playerWon = false;
        firstTime = true;

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
        if (started) {
            player.position = playerStart;
            // Wait 3 seconds after first starts to run the race
            if (Time.unscaledTime - startTime >= 3) {
                countDownDialogue.dialogue = "GO!";
                npcScript.TriggerDialogue();
                running = true;
                chadimator.SetBool("isWalking", true);
                started = false;
            } else {
                countDownDialogue.dialogue = (int)(4 - (Time.unscaledTime - startTime)) + "";
                npcScript.TriggerDialogue();
            }
        }
        if (running) {
            // The player has tried to cheat
            if (!tilesAdjacent) { 
                chadimator.SetBool("isWalking", false); 
                running = false;
                firstTime = true;
                cheated = true;
                transform.localPosition = chadStartLocal;
            } else if (transform.position.y <= endPoint.y) {
                // Chad goes all the way in the x direction before going in the y direction
                // Assume that the target location is up and to the right of the starting point
                Vector3 targetDirection = transform.position.x >= endPoint.x ? new Vector3(0,1,0) : new Vector3(1,0,0);
                transform.position += + speed * targetDirection * Time.deltaTime;
            } else {
                // Chad has made it to the finish line
                chadimator.SetBool("isWalking", false);
                countDownDialogue.dialogue = "Pfft, too easy. Come back when you're fast enough (e to start)";
                running = false;
            }
        } else if (!firstTime && !tilesAdjacent) {
            transform.localPosition = chadStartLocal;
        }
    }

    public void PlayerEnteredEnd() {
        if (running) {
            playerWon = true;
            running = false;
            onPlayerWin.Invoke();
            chadimator.SetBool("isWalking", false);
            chadimator.SetBool("isSad", true);
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
        if (inStart && tilesAdjacent && !started && !running && !playerWon) {
            if (firstTime) {
                chadStartLocal = transform.localPosition;
                endPoint = finishingLine.position - new Vector3(0, 1, 0);
            }  else {
                transform.localPosition = chadStartLocal;
            }
            cheated = false;
            firstTime = false;
            started = true;
            playerStart = new Vector2(transform.position.x, transform.position.y - 1);
            startTime = Time.unscaledTime;
        }
    }

    // Conditionals stuff for Chad Dialogue
    public void CurrentlyRunning(Conditionals.Condition cond) {
        cond.SetSpec(running);
    }

    public void PlayerWon(Conditionals.Condition cond) {
        cond.SetSpec(playerWon);
    }

    public void Cheated(Conditionals.Condition cond) {
        cond.SetSpec(cheated);
    }


}
