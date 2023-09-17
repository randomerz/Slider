using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DG_Player : MonoBehaviour
{
    public string playerName = "defaultName";

    [SerializeField] protected DG_DiceHolder diceHolder;

    [SerializeField] protected GameObject diceCover;

    [SerializeField] protected Sprite charIcon;
    public Sprite GetCharIcon() { return charIcon; }


    protected bool alreadyPushedThisRound = false;

    protected void Start()
    {
        DG_GameManager.instance.RoundStarted += OnRoundStarted;
        if (charIcon== null) //if no char icon defined just use the player sprite
        {
            charIcon = GetComponent<Image>().sprite;
        }
    }


    private void OnRoundStarted()
    {
        alreadyPushedThisRound = false;
    }

    public bool PlayerHasADieOfFace(int faceNum)
    {
        foreach(int face in diceHolder.GetDiceFaces()) 
        { 
            if (face == 1 || face == faceNum)
            {
                return true;
            }
        }
        return false;
    }

    public void PushDiceOfFace(int faceNum)
    {
        List<int> shownFaces = diceHolder.ShowDiceOfFaceToAllPlayers(faceNum);
        DG_GameManager.instance.visibleDiceFaces.AddRange(shownFaces);

        foreach (DG_Die die in diceHolder.GetDice())
        {
            if (die.FaceNum != 1 && die.FaceNum != faceNum)
            {
                DG_GameManager.instance.RemoveDiceFaceFromCurrentTotalDiceFaces(die.FaceNum);
                die.Roll();
            }
        }
    }

    public void ObscureDice() 
    { 
        diceCover.SetActive(true);
        diceHolder.CoverAllDice();
    }
    public void RevealDice() { diceCover.SetActive(false); }
    public List<int> GetDiceFaces(){ return diceHolder.GetDiceFaces(); }

    public bool eliminated = false;
    public void RemoveADie()
    { 
        diceHolder.RemoveADie();
        if (diceHolder.GetDice().Count == 0)
        {
            eliminated = true;
            DG_GameManager.instance.EliminatePlayer(this);
        }
    }

    public void RollAllDice(){diceHolder.RollAllDice();}

    public void HighlightDiceOfFace(int face, bool includeOnes = true) {  diceHolder.HighlightDiceOfFace(face, includeOnes); }
    public void RemoveAllHighlights() { diceHolder.RemoveAllHighlights(); }

    public virtual IEnumerator TakeTurn()
    {
        yield return null;
    }

    public virtual IEnumerator ChoosePlayDirection()
    {
        yield return null;
    }
}
