using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Printer : MonoBehaviour
{
    [SerializeField]
    public GameObject wallObject;
    public GameObject floorObject;
    public GameObject wireObject;
    public GameObject tileItem;
    public GameObject rocketItem;
    public ParticleSystem poof;
    public ParticleSystem poof2;
    public Animator bodyAnim;
    public Animator headAnim;

    private bool startedPrinting;

    // Start is called before the first frame update
    // Update is called once per frame
    private bool walls = false;
    private bool floor = false;
    private bool wires = false;

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
    }

    void Update()
    {
        CheckParts();
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
        tileItem.SetActive(true);
        bodyAnim.speed = 2.5f;
        headAnim.speed = 2.5f;
        yield return new WaitForSeconds(1f);
        poof2.Play();
        bodyAnim.speed = 4f;
        headAnim.speed = 4f;
        yield return new WaitForSeconds(3.5f);
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
            operatorMessage = "I'll just need these materials: the base, the walls, and the wires!!!";
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
                mlist.Add(" the base");
                operatorHint = "The base is right here to my left, but you'll probably need that Conductive Bob somehow!!!!";
            }
            if (!walls)
            {
                mlist.Add(" the walls");
                operatorHint = "The walls are up behind that giant door!!!!";
            }
            if (!wires)
            {
                mlist.Add(" the wires");
                operatorHint = "The wires are in the bottom-right of the Factory!!!!";
            }
            operatorMessage = $"It still needs{string.Join(',', mlist)}!!!";
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
