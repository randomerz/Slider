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

    protected void Start()
    {
        if (charIcon== null) //if no char icon defined just use the player sprite
        {
            charIcon = GetComponent<Image>().sprite;
        }
    }

    public void ObscureDice() { diceCover.SetActive(true); }
    public void RevealDice() { diceCover.SetActive(false); }
    public List<int> GetDiceFaces(){ return diceHolder.GetDiceFaces(); }

    public void RemoveADie(){ diceHolder.RemoveADie();}

    public void RollAllDice(){diceHolder.RollAllDice();}

    public void HighlightDiceOfFace(int face, bool includeOnes = true) {  diceHolder.HighlightDiceOfFace(face, includeOnes); }
    public void RemoveAllHighlights() { diceHolder.RemoveAllHighlights(); }

    public virtual IEnumerator TakeTurn()
    {
        yield return null;
    }

    public void CallBullShit()
    {

    }

    public virtual IEnumerator ChoosePlayDirection()
    {
        yield return null;
    }
}
