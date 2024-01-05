using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DiceGizmo : MonoBehaviour, ISavable
{
    [HideInInspector] public STile myStile;
    [SerializeField] private string saveString;
    [SerializeField] private int diceIndex;
    [SerializeField] private bool onlySoundInHouse;
    [SerializeField] private DiceGizmo otherDice;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private TextMeshProUGUI bgText;
    [SerializeField] private Transform baseTransform;
    [SerializeField] private SpriteRenderer sr;

    public int value;
    public Sprite[] sprites;
    public Sprite[] rotatedSprites;
    public Sprite[] bumpSprites;

    private Coroutine diceShakeCoroutine;
    //public Animator animator; // this is only based on Tree animator controller rn
    //Chen: Should the above be changed to the Dice animator controller or something?
    private bool animationFinished = false;

    private void Awake()
    {
        if (myStile == null)
        {
            FindSTile();
        }
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
    
    public void Load(SaveProfile profile)
    {
        value = profile.GetInt(saveString, 1);
        UpdateSprite();

        text.text = value.ToString();
        bgText.text = value.ToString();
    }

    public void Save()
    {
        SaveSystem.Current.SetInt(saveString, value);
    }

    public void CheckSeven(Condition c)
    {
        c.SetSpec(otherDice != null && value + otherDice.value == 7);
    }

    public void CheckNotSeven(Condition c)
    {
        c.SetSpec(otherDice != null && value + otherDice.value != 7);
    }

    public void CheckNine(Condition c)
    {
        c.SetSpec(otherDice != null && value + otherDice.value == 9);
    }

    public void CheckElevens(Condition c)
    {
        c.SetSpec(otherDice != null && value + otherDice.value == 11);
    }

    public void DiceShakeFinished(Condition c)
    {
        c.SetSpec(animationFinished);
    }

    public void changeValue(int num) //Don't want to change yet due to Desert/Ocean Scene references
    {
        animationFinished = false;
        value = num;
        if (value >= 7) value = 1;

        if (!onlySoundInHouse || (onlySoundInHouse && Player.GetIsInHouse()))
        {
            float pitch = (value != 1) ? 1f : 0.75f;
            AudioManager
                .PickSound("Dice Shake")
                .WithAttachmentToTransform(transform)
                .WithPitch(pitch)
                .WithPriorityOverDucking(true)
                .AndPlay();
        }
        
        UpdateSprite();

        text.text = "";
        bgText.text = "";

        if (diceShakeCoroutine != null) StopCoroutine(diceShakeCoroutine);
        diceShakeCoroutine = StartCoroutine(Shake(7));
    }

    public void BumpValue()
    {
        animationFinished = false;
        value += 1;
        if (value >= 7) value = 1;

        if (!onlySoundInHouse || (onlySoundInHouse && Player.GetIsInHouse()))
        {
            float pitch = (value != 1) ? 1f : 0.75f;
            AudioManager
                .PickSound("Dice Bump")
                .WithAttachmentToTransform(transform)
                .WithPitch(pitch)
                .WithPriorityOverDucking(true)
                .AndPlay();
        }
        
        UpdateSprite();

        text.text = "";
        bgText.text = "";

        if (diceShakeCoroutine != null) StopCoroutine(diceShakeCoroutine);
        diceShakeCoroutine = StartCoroutine(AnimateBump());
    }

    private IEnumerator AnimateBump()
    {
        for (int i = 0; i < bumpSprites.Length; i++)
        {
            sr.sprite = bumpSprites[i];
            yield return new WaitForSeconds(0.125f);
        }
        UpdateSprite();

        text.text = value.ToString();
        bgText.text = value.ToString();
        animationFinished = true;
        diceShakeCoroutine = null;
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
        UpdateSprite();

        text.text = value.ToString();
        bgText.text = value.ToString();
        animationFinished = true;
        diceShakeCoroutine = null;
    }

    public void UpdateSprite()
    {
        sr.sprite = sprites[value - 1];
    }


    public void OnStileChangeDir(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        if (e.stile.islandId == myStile.islandId)
        {
            BumpValue();
        }
    }

    private void UpdateDiceOnAnchorInteract(object sender, Anchor.OnAnchorInteractArgs e)
    {
        if (e.stile == myStile && e.drop)
        {
            changeValue(1);
        }
    }

    private void FindSTile()
    {
        myStile = transform.GetComponentInParent<STile>();

        if (myStile == null)
            Debug.LogWarning("Something went wrong in finding stile!");
    }
}
