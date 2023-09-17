using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class DG_DiceHolder : MonoBehaviour
{
    [SerializeField]
    private List<DG_Die> Dice;
    [SerializeField] private GameObject diceCover;
    public List<DG_Die> GetDice() { return Dice; }


    public void WhiteHighlightDiceOfFace(int face, bool includeOnes = true)
    {
        foreach (DG_Die die in Dice)
        {
            if (die.FaceNum == face || (includeOnes && die.FaceNum == 1))
            {
                die.ShowWhiteHighlight();
            }
        }
    }
    public void HighlightDiceOfFace(int face, bool includeOnes = true)
    {
        foreach (DG_Die die in Dice)
        {
            if(die.FaceNum == face || (includeOnes && die.FaceNum == 1))
            {
                die.ShowHighlight();
            }
        }
    }
    public void RemoveAllHighlights()
    {
        foreach (DG_Die die in Dice)
        {
            die.HideHighlight();
        }
    }

    public void CoverAllDice()
    {
        foreach (DG_Die die in Dice)
        {
            if (!die.covered)
            {
                CoverDie(die);
            }
        }
        diceCover.transform.SetAsLastSibling();
    }

    public List<int> ShowDiceOfFaceToAllPlayers(int faceNum)
    {
        List<int> shownDice = new List<int>();
        foreach(DG_Die die in Dice)
        {
            if (die.FaceNum == faceNum || die.FaceNum == 1)
            {
                ShowDieToAllPlayers(die);
                shownDice.Add(die.FaceNum);
            }
        }
        return shownDice;
    }

    private void ShowDieToAllPlayers(DG_Die die)
    {
        die.ShowWhiteHighlight();
        die.transform.SetParent(diceCover.transform);
        die.covered = false;
    }

    private void CoverDie(DG_Die die)
    {
        die.HideHighlight();
        die.transform.SetParent(this.transform);
        die.covered = true;
    }

    //for convenience
    private List<int> DiceFaces = new List<int>();
    public List<int> GetDiceFaces()
    {
        DiceFaces.Clear();
        foreach (DG_Die die in Dice)
        {
            DiceFaces.Add(die.FaceNum);
        }
        return DiceFaces;
    }
    //for ai
    public int GetMostCommonFace()
    {
        int? modeValue =
         DiceFaces
         .GroupBy(x => x)
         .OrderByDescending(x => x.Count()).ThenBy(x => x.Key)
         .Select(x => (int?)x.Key)
         .FirstOrDefault();
        return (int)modeValue;
    }
    public int GetNumDieOfFace(int faceNum)
    {
        int amount = 0;
        foreach (DG_Die die in Dice)
        {
            if (die.FaceNum == faceNum)
            {
                amount++;
            }
        }
        return amount;
    }

    public List<DG_Die> RollAllDice()
    {
        DiceFaces.Clear();
        foreach (DG_Die die in Dice) 
        {
            die.Roll();
            DiceFaces.Add(die.FaceNum);
        }


        return Dice;
    }

    public void RemoveADie()
    {
        DG_Die dieToRemove = Dice[0];
        Dice.Remove(dieToRemove);
        dieToRemove.gameObject.SetActive(false);
    }

    public bool HasMoreThanOneDieFace()
    {
        if (Dice.Count < 2) { return false; }

        int lastFace = -1;
        foreach(int face in DiceFaces)
        {
            if (lastFace == -1)
            {
                lastFace = face;
            }
            else if (lastFace != face) 
            {
                return true;
            }
        }
        return false;
    }
}
