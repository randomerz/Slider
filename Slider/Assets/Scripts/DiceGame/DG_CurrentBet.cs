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
        int[] bet = new int[2];
        int currNumDiceBet = NumDiceBet;
        int currFaceNumBet = FaceNumBet;

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
