using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DiceGizmo : MonoBehaviour
{
    public STile myStile;
    [SerializeField] private DiceGizmo otherDice;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private TextMeshProUGUI bgText;

    public int value;
    public Sprite[] sprites;

    private SpriteRenderer sr;
    //public Animator animator; // this is only based on Tree animator controller rn
    //Chen: Should the above be changed to the Dice animator controller or something?

    private void Awake()
    {
        if (myStile == null)
        {
            FindSTile();
        }
        sr = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        if (myStile != null)
            SGridAnimator.OnSTileMoveEnd += OnStileChangeDir;
        Anchor.OnAnchorInteract += UpdateDiceOnAnchorInteract;
    }

    private void OnDisable()
    {
        if (myStile != null)
            SGridAnimator.OnSTileMoveEnd -= OnStileChangeDir;
        Anchor.OnAnchorInteract -= UpdateDiceOnAnchorInteract;
    }

    public void CheckElevens(Condition c)
    {
        c.SetSpec(otherDice != null && value + otherDice.value == 11);
    }

    public void changeValue(int num) //Don't want to change yet due to Desert/Ocean Scene references
    {
        value = num;
        if (value >= 7) value = 1;
        sr.sprite = sprites[value - 1];
        text.text = value.ToString();
        bgText.text = value.ToString();
    }
    public void OnStileChangeDir(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        if (e.stile.islandId == myStile.islandId)
        {
            changeValue(value + 1);
        }
    }

    private void UpdateDiceOnAnchorInteract(object sender, Anchor.OnAnchorInteractArgs e)
    {
        if (myStile.hasAnchor) //There could be an edge case where hasAnchor hasn't quite updated but it's dice
        {
            changeValue(1);
        }
    }

    private void FindSTile()
    {
        Transform curr = transform;
        int i = 0;
        while (curr.parent != null && i < 100)
        {
            if (curr.GetComponent<STile>() != null)
            {
                myStile = curr.GetComponent<STile>();
                return;
            }

            // Debug.Log(curr.name);
            curr = curr.parent;
            i += 1;
        }

        if (i == 100)
            Debug.LogWarning("something went wrong in finding stile!");
    }

}
