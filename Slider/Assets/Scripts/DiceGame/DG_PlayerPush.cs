using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DG_PlayerPush : MonoBehaviour
{
    [SerializeField] private DG_Die showDie;

    public int FaceNumPushed { get; private set; }


    private void SetCurrentPush(int faceNum)
    {
        FaceNumPushed = faceNum;
        showDie.ForceDieFace(faceNum);
    }

    private void OnEnable()
    {
        SetCurrentPush(1);
    }

    public void IncrementFaceNumBet()
    {
        if (FaceNumPushed >= 6) { return; }
        SetCurrentPush(FaceNumPushed + 1);
    }

    public void DecrementFaceNumBet()
    {
        if (FaceNumPushed <= 1) { return; }
        SetCurrentPush(FaceNumPushed - 1);
    }
}