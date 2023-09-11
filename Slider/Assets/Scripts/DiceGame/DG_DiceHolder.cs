using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class DG_DiceHolder : MonoBehaviour
{
    [SerializeField]
    private List<DG_Die> Dice;
    public List<DG_Die> GetDice() { return Dice; }

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
        Dice.Remove(Dice[0]);
        Dice[0].gameObject.SetActive(false);
    }
}
