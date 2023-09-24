using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    [SerializeField] private Button[] faceNumBetArrows;

    public void LockFace(int faceNum)
    {
        SetCurrentBet(DG_CurrentBet.instance.GetLowestPossibleNumDiceBetOfFace(faceNum), faceNum);

        foreach (Button button in faceNumBetArrows)
        {
            //button.enabled = false;
            button.gameObject.SetActive(false);
        }
    }

    private void UnlockFaceArrows()
    {
        foreach (Button button in faceNumBetArrows)
        {
            //button.enabled = true;
            button.gameObject.SetActive(true);
        }
    }

    private void OnEnable()
    {
        int[] bet = new int[2];
        if (DG_CurrentBet.instance.FaceNumBet == 1)
        {
            bet[0] = DG_CurrentBet.instance.NumDiceBet + 1;
            bet[1] = 1;
        }
        else
        {
            bet = DG_CurrentBet.instance.GetLowestPossibleBet();
        }

        SetCurrentBet(bet[0], bet[1]);
        UnlockFaceArrows();
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
        else if (DG_CurrentBet.instance.FaceNumBet == 1) //switching face off 1s rule
        {
            int numDiceLastBet = DG_CurrentBet.instance.NumDiceBet;
            if (DG_CurrentBet.instance.NewBetIsValid((numDiceLastBet * 2) + 1, FaceNumBet + 1))
            {
                SetCurrentBet((numDiceLastBet * 2) + 1, FaceNumBet + 1);
            }
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
