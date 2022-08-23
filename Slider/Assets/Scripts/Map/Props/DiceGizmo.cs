using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DiceGizmo : MonoBehaviour
{
    public STile myStile;
    [SerializeField] private int diceIndex;
    [SerializeField] private bool onlySoundInHouse;
    [SerializeField] private DiceGizmo otherDice;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private TextMeshProUGUI bgText;
    [SerializeField] private Transform baseTransform;

    public int value;
    public Sprite[] sprites;
    public Sprite[] rotatedSprites;

    private SpriteRenderer sr;
    private Coroutine diceShakeCoroutine;
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

        if (!onlySoundInHouse || (onlySoundInHouse && Player.GetIsInHouse()))
        {
            if (value != 1) AudioManager.Play("Dice Shake");
            else AudioManager.PlayWithPitch("Dice Shake", 0.75f);
        }
        
        sr.sprite = sprites[value - 1];

        text.text = "";
        bgText.text = "";

        if (diceShakeCoroutine != null) StopCoroutine(diceShakeCoroutine);
        diceShakeCoroutine = StartCoroutine(Shake(7));
    }

    public IEnumerator Shake(int times)
    {
        for (int i = 0; i < times; i++)
        {
            transform.position = baseTransform.position + Random.insideUnitSphere *.2f;
            if ((i + diceIndex) % 2 == 0)
                sr.sprite = rotatedSprites[Random.Range(0, 6)];
            else
                sr.sprite = sprites[Random.Range(0, 6)];

            yield return new WaitForSeconds(.25f);
        }
        yield return new WaitForSeconds(.25f);

        transform.position = baseTransform.position;
        sr.sprite = sprites[value - 1];

        text.text = value.ToString();
        bgText.text = value.ToString();

        diceShakeCoroutine = null;
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
        if (myStile.hasAnchor && e.drop) //There could be an edge case where hasAnchor hasn't quite updated but it's dice
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
