using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DG_HumanPlayer : DG_Player
{
    [SerializeField] private DG_PlayerBet playerBet;
    [SerializeField] private DG_PlayerPush playerPush;

    [SerializeField] private GameObject playDirectionChoiceUI;
    [SerializeField] private GameObject humanPlayerTurnUI;
    [SerializeField] private GameObject BsButtonUI;
    [SerializeField] private GameObject ExactButtonUI;


    public override IEnumerator TakeTurn()
    {
        if (!alreadyPushedThisRound && diceHolder.HasMoreThanOneDieFace())
        {
            playerPush.gameObject.SetActive(true);
        }
        humanPlayerTurnUI.SetActive(true);
        if (DG_CurrentBet.instance.playerWhoBet == null) { BsButtonUI.SetActive(false); ExactButtonUI.SetActive(false); }
        else { BsButtonUI.SetActive(true); ExactButtonUI.SetActive(true); }

        yield return new WaitUntil(() => (hasBetBeenPlaced || hasBSBeenCalled || hasExactBeenCalled) );

        if (hasBetBeenPlaced)
        {
            Debug.Log("Human Player placed Bet");
            hasBetBeenPlaced = false;
            hasBSBeenCalled = false;
            hasExactBeenCalled = false;
            humanPlayerTurnUI.SetActive(false);
            DG_CurrentBet.instance.SetCurrentBet(this, betPlaced[0], betPlaced[1]);
        }
        else if (hasBSBeenCalled)
        {
            Debug.Log("Human Player called BS");
            hasBetBeenPlaced = false;
            hasBSBeenCalled = false;
            hasExactBeenCalled = false;
            humanPlayerTurnUI.SetActive(false);
            yield return DG_GameManager.instance.BSOrExactCalled(this);
        }
        else if (hasExactBeenCalled)
        {
            Debug.Log("Human Player called Exact");
            hasBetBeenPlaced = false;
            hasBSBeenCalled = false;
            hasExactBeenCalled = false;
            humanPlayerTurnUI.SetActive(false);
            yield return DG_GameManager.instance.BSOrExactCalled(this, true);
        }
    }

    public override IEnumerator ChoosePlayDirection()
    {
        playDirectionChoiceUI.SetActive(true);
        yield return new WaitUntil(() => hasDirectionBeenChosen);
        hasDirectionBeenChosen = false;
        playDirectionChoiceUI.SetActive(false);

        Debug.Log("Human Player chose play direction: " + chosenPlayDirection);

        DG_GameManager.instance.currentPlayDirection = chosenPlayDirection;

    }

    private DGPlayDirection chosenPlayDirection;
    private bool hasDirectionBeenChosen = false;

    public void OnChooseClockWiseDirectionButtonPressed()
    {
        chosenPlayDirection = DGPlayDirection.ClockWise;
        hasDirectionBeenChosen = true;
    }
    public void OnChooseCounterClockWiseDirectionButtonPressed()
    {
        chosenPlayDirection = DGPlayDirection.CounterClockWise;
        hasDirectionBeenChosen = true;
    }

    private int[] betPlaced = new int[2];
    private bool hasBetBeenPlaced = false;

    public void OnBetButtonPressed()
    {
        betPlaced[0] = playerBet.NumDiceBet;
        betPlaced[1] = playerBet.FaceNumBet;
        hasBetBeenPlaced = true;
    }

    private bool hasBSBeenCalled = false;

    public void OnCallBSButtonPressed()
    {
        hasBSBeenCalled = true;
    }

    private bool hasExactBeenCalled = false;

    public void OnExactButtonPressed()
    {
        hasExactBeenCalled = true;
    }

    public void OnPushButtonPressed()
    {
        int faceNumPushed = playerPush.FaceNumPushed;
        if (PlayerHasADieOfFace(faceNumPushed))
        {
            alreadyPushedThisRound = true;
            PushDiceOfFace(faceNumPushed);
            playerPush.gameObject.SetActive(false);
            playerBet.LockFace(faceNumPushed);
        }
    }
}
