using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DG_CurrentBet : MonoBehaviour
{
    public static DG_CurrentBet instance { get; private set; }

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

    [SerializeField] private Image charIconImage;
    [SerializeField] private TextMeshProUGUI numDiceTextMesh;
    [SerializeField] private DG_Die showDie;

    public DG_Player playerWhoBet { get; private set; }
    public int NumDiceBet { get; private set; }
    public int FaceNumBet { get; private set; }

    public List<Bet> betsThisRound { get; private set; }

    public int[] GetMostCommonFaceBetThisRound()
    {
        List<int> facesBet = new List<int>();
        foreach (Bet bet in betsThisRound)
        {
            facesBet.Add(bet.faceNumBet);
        }


        // Initialize the return value
        int mode = default(int);
        int amount = 0;

        // Test for a null reference and an empty list
        if (facesBet != null && facesBet.Count() > 0)
        {
            // Store the number of occurences for each element
            Dictionary<int, int> counts = new Dictionary<int, int>();

            // Add one to the count for the occurence of a character
            foreach (int element in facesBet)
            {
                if (counts.ContainsKey(element))
                    counts[element]++;
                else
                    counts.Add(element, 1);
            }

            // Loop through the counts of each element and find the 
            // element that occurred most often
            int max = 0;

            foreach (KeyValuePair<int, int> count in counts)
            {
                if (count.Value > max)
                {
                    // Update the mode
                    mode = count.Key;
                    max = count.Value;
                    amount = count.Value;
                }
            }
        }
        int[] answer = { mode, amount };
        return answer;
    }

    public void OnNewRoundStart()
    {
        if (betsThisRound == null)
        {
            betsThisRound = new List<Bet>();
        }
        else
        {
            betsThisRound.Clear();
        }
        SetCurrentBet(null, 0, 1);
    }


    //Todo: show current face of betting player
    public void SetCurrentBet(DG_Player player, int numDice, int faceNum)
    {
        playerWhoBet = player;
        NumDiceBet= numDice;
        FaceNumBet= faceNum;

        if (player != null) 
        { 
            ShowUI();
            charIconImage.sprite = playerWhoBet.GetCharIcon();
            Bet bet = new Bet(player, numDice, faceNum);
            betsThisRound.Add(bet);
        }

        numDiceTextMesh.text = numDice.ToString();
        showDie.ForceDieFace(faceNum);

        if (player == null) { HideUI(); }
    }

    public void HideUI()
    {
        charIconImage.gameObject.SetActive(false);
        numDiceTextMesh.gameObject.SetActive(false);
        showDie.gameObject.SetActive(false);
    }
    public void ShowUI()
    {
        charIconImage.gameObject.SetActive(true);
        numDiceTextMesh.gameObject.SetActive(true);
        showDie.gameObject.SetActive(true);
    }

    public bool NewBetIsValid(int numDice, int faceNum)
    {
        if (faceNum < 1 || faceNum > 6) { return false; }
        if (numDice < 1 || numDice > 99) { return false; }

        if (numDice < NumDiceBet) { return false; }
        if (faceNum <= FaceNumBet && numDice <= NumDiceBet) { return false; }
        if (FaceNumBet == 1 && faceNum != 1 && numDice < (NumDiceBet * 2) + 1) { return false; } //switching face off 1s rule
        return true;
    }

    public int[] GetLowestPossibleBet()
    {
        int[] bet = new int[2];
        int currNumDiceBet = NumDiceBet;
        int currFaceNumBet = FaceNumBet;

        if (currFaceNumBet < 6)
        {
            bet[0] = currNumDiceBet;
            bet[1] = currFaceNumBet + 1;
        }
        else
        {
            bet[0] = currNumDiceBet + 1;
            bet[1] = 2;//betting on ones is sticky so start at twos
        }
        return bet;
    }

    public int GetLowestPossibleNumDiceBetOfFace(int faceNum) 
    {
        int currNumDiceBet = NumDiceBet;
        int currFaceNumBet = FaceNumBet;

        if (currFaceNumBet == 1 && faceNum != 1) //switching face off 1s rule
        {
            return (currNumDiceBet * 2) + 1;
        }

        if (faceNum > currFaceNumBet)
        {
            return currNumDiceBet;
        }
        else
        {
            return currNumDiceBet + 1;
        }
    }

    public float GetProbabilityOfCurrentBet(List<int> knownFaces = null)
    {
        return GetProbabilityOfBet(NumDiceBet, FaceNumBet, knownFaces);
    }

    public float GetProbabilityOfBet(int numDiceBet, int faceNumBet, List<int> knownFaces = null)
    {
        List<int> dice = DG_GameManager.instance.CurrentTotalDiceFaces;

        if (numDiceBet > dice.Count) { return 0; }

        // Calculate the total number of dice faces currently in the game
        int totalDiceFaces = dice.Count;

        // Calculate the probability using the binomial probability formula
        float probability = 0;

        // Adjust the likelihood of faceNumBet being rolled
        float likelihoodOfSingleDieBeingFace = 1.0f / 6;
        if (faceNumBet != 1)
        {
            // Adjust the likelihood due to wildcards
            likelihoodOfSingleDieBeingFace = 2.0f / 6;
        }

        //naive bet
        if (knownFaces == null)
        {

            for (int k = numDiceBet; k <= totalDiceFaces; k++)
            {
                // Calculate the binomial coefficient (n choose k)
                int binomialCoefficient = BinomialCoefficient(totalDiceFaces, k);

                // Calculate the probability of exactly k successes
                float probabilityOfKSuccesses = Mathf.Pow(likelihoodOfSingleDieBeingFace, k) *
                                                Mathf.Pow(1.0f - likelihoodOfSingleDieBeingFace, totalDiceFaces - k);

                // Add the probability of at least k successes
                probability += binomialCoefficient * probabilityOfKSuccesses;
            }
            Debug.Log("naive probability of bet [" + NumDiceBet + ", " + FaceNumBet + "] is " + probability * 100 + "%");
            return probability;
        }
        else
        {
            //the bet is effectively lowered by one die for each we already know matches
            int numUnknownDiceBet = numDiceBet;
            foreach (int dieFace in knownFaces)
            {
                if (dieFace == faceNumBet || dieFace == 1)
                {
                    numUnknownDiceBet--;
                }
            }
            //the number on the table is effectively lowered to only the dice we don't know about
            int unknownDiceFaces = totalDiceFaces - knownFaces.Count;

            for (int k = numUnknownDiceBet; k <= unknownDiceFaces; k++)
            {
                // Calculate the binomial coefficient (n choose k)
                int binomialCoefficient = BinomialCoefficient(unknownDiceFaces, k);

                // Calculate the probability of exactly k successes
                float probabilityOfKSuccesses = Mathf.Pow(likelihoodOfSingleDieBeingFace, k) *
                                                Mathf.Pow(1.0f - likelihoodOfSingleDieBeingFace, unknownDiceFaces - k);

                // Add the probability of at least k successes
                probability += binomialCoefficient * probabilityOfKSuccesses;
            }

            /*
            for (int k = numDiceBet; k <= totalDiceFaces; k++)
            {
                // Calculate the binomial coefficient (n choose k)
                int binomialCoefficient = BinomialCoefficient(totalDiceFaces, k);

                // Calculate the probability of exactly k successes
                float probabilityOfKSuccesses = 1.0f;

                // Adjust the likelihood of each die being the specified face value
                foreach (int dieFace in knownFaces)
                {
                    if (dieFace == faceNumBet)
                    {
                        probabilityOfKSuccesses *= likelihoodOfSingleDieBeingFace;
                    }
                    else if (dieFace == 1)
                    {
                        probabilityOfKSuccesses *= likelihoodOfSingleDieBeingFace * 2.0f / 6; // Adjust for wildcards
                    }
                    else
                    {
                        probabilityOfKSuccesses *= (1.0f - likelihoodOfSingleDieBeingFace);
                    }
                }

                // Add the probability of at least k successes
                probability += binomialCoefficient * probabilityOfKSuccesses;
            }
            */
            Debug.Log("non-naive probability of bet [" + NumDiceBet + ", " + FaceNumBet + "] is " + probability * 100 + "%");
            return probability;
        }
    }

    int BinomialCoefficient(int n, int k)
    {
        if (k < 0 || k > n)
        {
            return 0;
        }
        int result = 1;
        for (int i = 1; i <= k; i++)
        {
            result *= (n - i + 1);
            result /= i;
        }
        return result;
    }
}
public struct Bet
{
    public DG_Player player;
    public int numDiceBet;
    public int faceNumBet;

    public Bet(DG_Player player, int numDice, int faceNum) : this()
    {
        this.player = player;
        this.numDiceBet = numDice;
        this.faceNumBet = faceNum;
    }
}
