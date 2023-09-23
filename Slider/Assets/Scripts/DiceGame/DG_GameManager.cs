using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DG_GameManager : MonoBehaviour
{
    public static DG_GameManager instance { get; private set; }
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [SerializeField] private List<DG_AIPersonality> AIPersonalities = new List<DG_AIPersonality>();
    [SerializeField] private List<DG_Player> players;
    public List<DG_Player> GetPlayers() {  return players; }

    [SerializeField] private TextMeshProUGUI diceOnTableTextMesh;
    private int numDiceOnTable;

    public event Action RoundStarted;

    public List<int> visibleDiceFaces = new List<int>();
    public List<int> CurrentTotalDiceFaces { get; private set; }
    public void AddDiceFaceToCurrentTotalDiceFaces(int face) {  CurrentTotalDiceFaces.Add(face); }
    public void RemoveDiceFaceFromCurrentTotalDiceFaces(int face) { CurrentTotalDiceFaces.Remove(face); }

    public DGPlayDirection currentPlayDirection;

    private void Start()
    {
        CurrentTotalDiceFaces= new List<int>();
        StartGame();
    }

    public void EliminatePlayer(DG_Player player)
    {
        players.Remove(player);
        player.gameObject.SetActive(false);
    }

    public void StartGame()
    {
        numDiceOnTable = (players.Count * 6);
        diceOnTableTextMesh.text = numDiceOnTable.ToString() + " Dice On Table";
        AssignAIPersonalities();
        StartCoroutine(StartRound(players[0]));
    }

    private void AssignAIPersonalities()
    {

        AIPersonalities = Shuffle(AIPersonalities);
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i] is DG_AIPlayer)
            {
                DG_AIPlayer ai_player = (DG_AIPlayer)players[i];
                ai_player.SetPersonality(AIPersonalities[i]);
            }
        }
    }

    private static System.Random rng = new System.Random();
    public static List<T> Shuffle<T>(List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
        return list;
    }

    public IEnumerator StartRound(DG_Player startingPlayer)
    {
        visibleDiceFaces.Clear();
        DG_CurrentBet.instance.OnNewRoundStart();
        
        CurrentTotalDiceFaces.Clear();
        RollAllPlayerDice();

        RoundStarted?.Invoke();

        yield return startingPlayer.TakeTurn();

        yield return startingPlayer.ChoosePlayDirection();

        DG_Player nextPlayer = startingPlayer;
        while (!bSCalled)
        {
            if (currentPlayDirection == DGPlayDirection.ClockWise)
            {
                int i = players.IndexOf(nextPlayer) + 1;
                if (i >= players.Count) { i = 0; }
                nextPlayer = players[i];
            }
            else if (currentPlayDirection == DGPlayDirection.CounterClockWise)
            {
                int i = players.IndexOf(nextPlayer) - 1;
                if (i < 0) { i = players.Count - 1; }
                nextPlayer = players[i];
            }
            yield return nextPlayer.TakeTurn();
        }
        bSCalled = false;

        if (!HumanPlayerStillInGame())
        {
            Debug.Log("GAME OVER! YOU LOSE!");
        }
        else if (players.Count == 1)
        {
            Debug.Log("GAME OVER! YOU WIN!");
        }

        if (playerWhoLostLastRound == null || playerWhoLostLastRound.eliminated == true)
        {
            StartCoroutine(StartRound(players[UnityEngine.Random.Range(0,players.Count)]));
        }
        else
        {
            StartCoroutine(StartRound(playerWhoLostLastRound));
        }
    }

    private bool HumanPlayerStillInGame()
    {
        foreach (DG_Player player in players)
        {
            if (player is DG_HumanPlayer)
            {
                return true;
            }
        }
        return false;
    }

    private void RollAllPlayerDice()
    {
        foreach(DG_Player player in players)
        {
            player.RollAllDice();
        }
    }

    private bool bSCalled = false;

    private DG_Player playerWhoLostLastRound;

    public IEnumerator BSOrExactCalled(DG_Player playerCallingBS, bool exactRequired = false)
    {
        bSCalled = true;
        ObscureAllDice(); //for fix bug :)
        //RemoveAllHighlights();
        //RevealAIDice();
        RevealAllDice();
        yield return new WaitForSeconds(1);
        HighlightAllDiceOfFace(DG_CurrentBet.instance.FaceNumBet);

        yield return new WaitForSeconds(5);

        if (exactRequired)
        {
            if (IsCurrentBetExact())
            {
                Debug.Log(playerCallingBS.playerName + " was exact. No one loses a die.");
                playerWhoLostLastRound = playerCallingBS;
            }
            else
            {
                Debug.Log(playerCallingBS.playerName + " lost the round.");
                playerCallingBS.RemoveADie();
                numDiceOnTable--;
                diceOnTableTextMesh.text = numDiceOnTable.ToString() + " Dice On Table";
                playerWhoLostLastRound = playerCallingBS;
            }
        }
        else if (!exactRequired && IsCurrentBetTrue())
        {
            Debug.Log(playerCallingBS.playerName + " lost the round.");
            playerCallingBS.RemoveADie();
            numDiceOnTable--;
            diceOnTableTextMesh.text = numDiceOnTable.ToString() + " Dice On Table";
            playerWhoLostLastRound = playerCallingBS;
        }
        else
        {
            DG_Player playerWhoBet = DG_CurrentBet.instance.playerWhoBet;
            Debug.Log(playerWhoBet.playerName + " lost the round.");
            playerWhoBet.RemoveADie();
            numDiceOnTable--;
            diceOnTableTextMesh.text = numDiceOnTable.ToString() + " Dice On Table";
            playerWhoLostLastRound = playerWhoBet;
        }
        yield return new WaitForSeconds(2);
        RemoveAllHighlights();
        //ObscureAIDice();
        ObscureAllDice();
    }

    private void HighlightAllDiceOfFace(int face)
    {
        foreach (DG_Player player in players)
        {
            player.HighlightDiceOfFace(face);
        }
    }
    private void RemoveAllHighlights()
    {
        foreach (DG_Player player in players)
        {
            player.RemoveAllHighlights();
        }
    }

    private void RevealAllDice()
    {
        foreach (DG_Player player in players)
        {
            player.RevealDice();
        }
    }

    private void RevealAIDice()
    {
        foreach (DG_Player player in players)
        {
            if (player is DG_AIPlayer)
            {
                player.RevealDice();
            }
        }
    }
    private void ObscureAllDice()
    {
        foreach (DG_Player player in players)
        {
            player.ObscureDice();
        }
    }
    private void ObscureAIDice()
    {
        foreach (DG_Player player in players)
        {
            if (player is DG_AIPlayer)
            {
                player.ObscureDice();
            }
        }
    }

    private bool IsCurrentBetTrue()
    {
        List<int> totalDiceFaces = new List<int>();
        foreach (DG_Player player in players)
        {
            totalDiceFaces.AddRange(player.GetDiceFaces());
        }

        int faceNumRequired = DG_CurrentBet.instance.FaceNumBet;
        int numDiceRequired = DG_CurrentBet.instance.NumDiceBet;
        int numMatchingDiceFound = 0;

        foreach (int diceFace in totalDiceFaces)
        {
            if (diceFace == faceNumRequired || diceFace == 1)
            {
                numMatchingDiceFound++;
            }
            if (numMatchingDiceFound >= numDiceRequired)
            {
                Debug.Log("Found at least " + numMatchingDiceFound + " dice of face " + faceNumRequired + " out of requireed " + numDiceRequired);
                return true;
            }
        }
        Debug.Log("only " + numMatchingDiceFound + " dice of face " + faceNumRequired + " found, out of needed " + numDiceRequired);
        return false;
    }

    private bool IsCurrentBetExact()
    {
        List<int> totalDiceFaces = new List<int>();
        foreach (DG_Player player in players)
        {
            totalDiceFaces.AddRange(player.GetDiceFaces());
        }

        int faceNumRequired = DG_CurrentBet.instance.FaceNumBet;
        int numDiceRequired = DG_CurrentBet.instance.NumDiceBet;
        int numMatchingDiceFound = 0;

        foreach (int diceFace in totalDiceFaces)
        {
            if (diceFace == faceNumRequired || diceFace == 1)
            {
                numMatchingDiceFound++;
            }
        }

        if (numMatchingDiceFound == numDiceRequired)
        {
            Debug.Log("exactly " + numMatchingDiceFound + " dice of face " + faceNumRequired + " found, out of needed " + numDiceRequired);
            return true;
        }
        else
        {
            Debug.Log(numMatchingDiceFound + " dice of face " + faceNumRequired + " found, but it needed to be exactly " + numDiceRequired);
            return false;
        }
    }

}
public enum DGPlayDirection { ClockWise, CounterClockWise }
