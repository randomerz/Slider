using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;

// Chad race should be attatched to chad
public class ChadRace : MonoBehaviour
{
    public UnityEvent onPlayerWin;
    public Transform chad;
    public Transform player;
    public float speed;

    public bool tilesAdjacent;

    // The local position of the chad so he goes back to his spot on the STile
    private Vector2 chadStart;

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

    // Start is called before the first frame update
    void Start()
    {
        tilesAdjacent = false;
        inStart = false;
        started = false;
        running = false;
        playerWon = false;
        firstTime = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (started) {
            player.position = playerStart;
            // Wait 3 seconds after first starts to run the race
            if (Time.unscaledTime - startTime >= 3) {
                running = true;
                started = false;
            }
        }
        if (running) {
            // The player has tried to cheat
            if (!tilesAdjacent) {   
                running = false;
                firstTime = true;
                chad.localPosition = chadStart;
            }
            if (chad.position.y <= endPoint.y) {
                // Chad goes all the way in the x direction before going in the y direction
                // Assume that the target location is up and to the right of the starting point
                Vector3 targetDirection = chad.position.x >= endPoint.x ? new Vector3(0,1,0) : new Vector3(1,0,0);
                chad.position += + speed * targetDirection * Time.deltaTime;
            } else {
                // Chad has made it to the finish line
                running = false;
            }
        }
        
    }

    public void PlayerEnteredEnd() {
        if (running) {
            playerWon = true;
            running = false;
            onPlayerWin.Invoke();
            chad.localPosition = chadStart;
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
                chadStart = chad.localPosition;
                endPoint = transform.position;
            }  else {
                chad.localPosition = chadStart;
            }
            firstTime = false;
            started = true;
            playerStart = new Vector2(chad.position.x, chad.position.y - 1);
            startTime = Time.unscaledTime;
        }
    }
    
}
