using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class DG_Die : MonoBehaviour
{
    [SerializeField] private Image image;

    [SerializeField] private Sprite dieFace1;
    [SerializeField] private Sprite dieFace2;
    [SerializeField] private Sprite dieFace3;
    [SerializeField] private Sprite dieFace4;
    [SerializeField] private Sprite dieFace5;
    [SerializeField] private Sprite dieFace6;

    [SerializeField] private GameObject highlight;
    [SerializeField] private GameObject whiteHighlight;

    public int FaceNum { get; private set; }

    public void ShowWhiteHighlight() { HideHighlight(); whiteHighlight.SetActive(true); }
    public void ShowHighlight() { HideHighlight(); highlight.SetActive(true); }
    public void HideHighlight() { highlight.SetActive(false); whiteHighlight.SetActive(false); }

    public bool covered = true;

    public int Roll()
    {
        FaceNum = Random.Range(1, 7);
        SetFaceShown(FaceNum);
        DG_GameManager.instance.AddDiceFaceToCurrentTotalDiceFaces(FaceNum);
        return FaceNum;
    }

    public void ForceDieFace(int forcedFaceNum)
    {
        FaceNum = forcedFaceNum;
        SetFaceShown(forcedFaceNum);
    }

    private void SetFaceShown(int faceNum = 0)
    {
        if (faceNum == 0)
        {
            faceNum = FaceNum;
        }
        switch(faceNum)
        {
            case 1:
                image.sprite = dieFace1;
                break;
            case 2:
                image.sprite = dieFace2;
                break;
            case 3:
                image.sprite = dieFace3;
                break;
            case 4:
                image.sprite = dieFace4;
                break;
            case 5:
                image.sprite = dieFace5;
                break;
            case 6:
                image.sprite = dieFace6;
                break;
        }
    }
}
