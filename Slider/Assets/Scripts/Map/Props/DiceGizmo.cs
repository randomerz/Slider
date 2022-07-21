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

    private void Update()
    {
        if (myStile.hasAnchor)
        {
            value = 1;
        }
    }

    private void OnEnable()
    {
        if (myStile != null)
            SGridAnimator.OnSTileMoveEnd += OnStileChangeDir;
    }

    private void OnDisable()
    {
        if (myStile != null)
            SGridAnimator.OnSTileMoveEnd -= OnStileChangeDir;
    }

    public void CheckElevens(Condition c)
    {
        c.SetSpec(value + otherDice.value == 11);
    }

    public void changeValue(int num)
    {
        value = num;
        sr.sprite = sprites[value - 1];
        text.text = value.ToString();
        bgText.text = value.ToString();
    }
    public void OnStileChangeDir(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        if (e.stile.islandId == myStile.islandId)
        {
            value++;
            if (value == 7)
            {
                value = 1;
            }
            sr.sprite = sprites[value - 1];
            text.text = value.ToString();
            bgText.text = value.ToString();
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
