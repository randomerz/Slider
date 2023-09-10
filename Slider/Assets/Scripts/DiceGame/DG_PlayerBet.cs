using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DG_PlayerBet : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI numDiceTextMesh;
    [SerializeField] private DG_Die showDie;

    public int FaceNumBet { get; private set; }
    public int NumDiceBet { get; private set; }


    private void SetCurrentBet(int numDice, int faceNum)
    {
        FaceNumBet = faceNum;
        NumDiceBet = numDice;

        numDiceTextMesh.text = numDice.ToString();
        showDie.ForceDieFace(faceNum);

    }

    private void OnEnable()
    {
        int[] bet = DG_CurrentBet.instance.GetLowestPossibleBet();
        SetCurrentBet(bet[0], bet[1]);
    }

    public void IncrementNumDiceBet()
    {
        if (DG_CurrentBet.instance.NewBetIsValid(NumDiceBet + 1, FaceNumBet))
        {
            SetCurrentBet(NumDiceBet + 1, FaceNumBet);
        }
    }
    public void IncrementFaceNumBet()
    {
        if (DG_CurrentBet.instance.NewBetIsValid(NumDiceBet, FaceNumBet + 1))
        {
            SetCurrentBet(NumDiceBet, FaceNumBet + 1);
        }
    }
    public void DecrementNumDiceBet()
    {
        if (DG_CurrentBet.instance.NewBetIsValid(NumDiceBet - 1, FaceNumBet))
        {
            SetCurrentBet(NumDiceBet - 1, FaceNumBet);
        }
    }
    public void DecrementFaceNumBet()
    {
        if (DG_CurrentBet.instance.NewBetIsValid(NumDiceBet, FaceNumBet - 1))
        {
            SetCurrentBet(NumDiceBet, FaceNumBet - 1);
        }
    }
}
