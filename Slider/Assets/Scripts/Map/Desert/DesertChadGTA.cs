using UnityEngine;

public class DesertChadGTA : MonoBehaviour
{
    public const string CHAD_STARTED_HEIST_SAVE_STRING = "DesertChadStartedHeist";
    public const string CHAD_KEEPING_WATCH_SAVE_STRING = "DesertChadKeepingWatch";
    public const string CASINO_HEIGHT_HAPPENED_SAVE_STRING = "DesertCasinoAlreadyHappened";

    public Transform chadWalkStart;
    public ChadFollowPlayer chadFollowPlayer;
    public NPC chadNPC;
    
    private float timeUntilTurn = 1;
    private bool shouldFaceRight;

    private void OnDisable()
    {
        if (SaveSystem.Current.GetBool(CHAD_KEEPING_WATCH_SAVE_STRING))
        {
            SaveSystem.Current.SetBool(CASINO_HEIGHT_HAPPENED_SAVE_STRING, true);
        }
    }

    private void Update()
    {
        if (SaveSystem.Current.GetBool(CHAD_KEEPING_WATCH_SAVE_STRING) && !SaveSystem.Current.GetBool(CASINO_HEIGHT_HAPPENED_SAVE_STRING))
        {
            timeUntilTurn -= Time.deltaTime;
            if (timeUntilTurn < 0)
            {
                timeUntilTurn = 1;
                shouldFaceRight = !shouldFaceRight;
                chadNPC.SetFacingRight(shouldFaceRight);
            }
        }
    }

    public void StartCasinoHeist()
    {
        chadFollowPlayer.SetFollowingPlayer(false);
        chadWalkStart.transform.position = chadNPC.transform.position;
        SaveSystem.Current.SetBool(CHAD_STARTED_HEIST_SAVE_STRING, true);
        CoroutineUtils.ExecuteAfterEndOfFrame(() => chadNPC.TypeCurrentDialogue(), this);
    }
}