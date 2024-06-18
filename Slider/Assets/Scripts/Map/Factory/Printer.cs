using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Printer : MonoBehaviour, IDialogueTableProvider
{
    public GameObject wallObject;
    public GameObject floorObject;
    public GameObject wireObject;
    public GameObject tileItem;
    public GameObject rocketItem;
    public ParticleSystem poof;
    public ParticleSystem poof2;
    public Animator bodyAnim;
    public Animator headAnim;

    public PlayerConditionals playerConditionals;

    private bool startedPrinting;

    // Start is called before the first frame update
    // Update is called once per frame
    private bool walls = false;
    private bool floor = false;
    private bool wires = false;
    
    #region Localization

    enum OperatorText
    {
        First,
        BaseHint,
        WallHint,
        WireHint,
        BaseMsg,
        WallMsg,
        WireMsg,
        ListMaterialMsgBeginning,
        ListMaterialMsgPunctuation
    }
    public Dictionary<string, (string original, string translated)> TranslationTable { get; }

    private Dictionary<string, (string original, string translated)> _translationTable =
        IDialogueTableProvider.InitializeTable(
            new Dictionary<OperatorText, string>
            {
                { OperatorText.First, "I'll just need these materials: the base, the walls, and the wires!!!" },
                { OperatorText.BaseHint, "The base is right here to my left, but you'll probably need that Conductive Bob somehow!!!!" },
                { OperatorText.WallHint, "The walls are up behind that giant door!!!!" },
                { OperatorText.WireHint, "The wires are in the bottom-right of the Factory!!!!" },
                { OperatorText.BaseMsg, " the base" },
                { OperatorText.WallMsg, " the walls" },
                { OperatorText.WireMsg, " the wires" },
                { OperatorText.ListMaterialMsgBeginning, "It still needs" },
                { OperatorText.ListMaterialMsgPunctuation, "!!!" },
            });
    #endregion

    private void OnEnable()
    {
        PowerCrystal.blackoutStarted += HandleBlackoutStarted;
        PowerCrystal.blackoutEnded += HandleBlackoutEnded;
    }

    private void OnDisable()
    {
        PowerCrystal.blackoutStarted -= HandleBlackoutStarted;
        PowerCrystal.blackoutEnded -= HandleBlackoutEnded;
    }

    private void HandleBlackoutStarted()
    {
        bodyAnim.speed = 0;
        headAnim.speed = 0;
    }

    private void HandleBlackoutEnded()
    {
        bodyAnim.speed = 1;
        headAnim.speed = 1;
    }

    void Start()
    {
        CheckParts();

        if (PlayerInventory.Contains("Slider 5", Area.Factory))
        {
            playerConditionals.DisableConditionals();
        }
    }

    void Update()
    {
        if (!PlayerInventory.Contains("Slider 5", Area.Factory))
        {
            CheckParts();
        }
    }

    public void StartPoof()
    {
        if (!PlayerInventory.Contains("Slider 5", Area.Factory) && walls && floor && wires)
        {
            if (startedPrinting)
                return;

            startedPrinting = true;
            StartCoroutine(PoofCoroutine());
        }
        else
        {
            AudioManager.Play("Artifact Error");
        }
    }

    private IEnumerator PoofCoroutine()
    {

        rocketItem.SetActive(false);
        poof.Play();
        AudioManager.Play("Hat Click", transform);
        tileItem.SetActive(true);
        bodyAnim.speed = 2.5f;
        headAnim.speed = 2.5f;
        yield return new WaitForSeconds(1f);
        poof2.Play();
        bodyAnim.speed = 4f;
        headAnim.speed = 4f;
        for (float i = 0; i < 3.5f; i += 0.25f)
        {
            AudioManager.PlayWithVolume("Hat Click", 0.25f);
            yield return new WaitForSeconds(0.25f);
        }
        poof2.Stop();
        headAnim.speed = 2.5f;
        bodyAnim.speed = 2.5f;
        SGrid.Current.ActivateSliderCollectible(5);
        yield return new WaitForSeconds(0.5f);
        headAnim.speed = 1f;
        bodyAnim.speed = 1f;
        poof.Play();
        rocketItem.SetActive(true);
        tileItem.SetActive(false);
        playerConditionals.DisableConditionals();
    }

    public void CheckParts()
    {
        string operatorMessage = "";
        string operatorHint = "";
        walls = PlayerInventory.Contains("Slider Walls");
        floor = PlayerInventory.Contains("Slider Base");
        wires = PlayerInventory.Contains("Slider Wires");
        SetActives();
        if (!floor && !walls && !wires)
        {
            // first message
            operatorMessage = this.GetLocalizedSingle(OperatorText.First);
        }
        else if (floor && walls && wires)
        {
            // ready to print
            SaveSystem.Current.SetBool("factoryBuildATileReady", true);
        }
        else
        {
            List<string> mlist = new List<string>();
            if (!floor)
            {
                mlist.Add(this.GetLocalizedSingle(OperatorText.BaseMsg));
                operatorHint = this.GetLocalizedSingle(OperatorText.BaseHint);
            }
            if (!walls)
            {
                mlist.Add(this.GetLocalizedSingle(OperatorText.WallMsg));
                operatorHint = this.GetLocalizedSingle(OperatorText.WallHint);
            }
            if (!wires)
            {
                mlist.Add(this.GetLocalizedSingle(OperatorText.WireMsg));
                operatorHint = this.GetLocalizedSingle(OperatorText.WireHint);
            }
            operatorMessage = 
                this.GetLocalizedSingle(OperatorText.ListMaterialMsgBeginning) 
                + string.Join(',', mlist) 
                + this.GetLocalizedSingle(OperatorText.ListMaterialMsgPunctuation);
        }
        SaveSystem.Current.SetString("FactoryPrinterParts", operatorMessage);
        SaveSystem.Current.SetString("FactoryPrinterPartsHint", operatorHint);

        // if only one left
        if ((!floor && walls && wires) ||
            (floor && !walls && wires) ||
            (floor && walls && !wires))
        {
            SaveSystem.Current.SetBool("factoryBuildATileHint", true);
        }
    }

    private void SetActives()
    {
        wallObject.SetActive(walls);
        floorObject.SetActive(floor);
        wireObject.SetActive(wires);
    }
}
