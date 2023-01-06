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

    // Start is called before the first frame update
    // Update is called once per frame
    private bool walls = false;
    private bool floor = false;
    private bool wires = false;
    void Awake()
    {
        
        CheckParts();
    }
    void Update()
    {
        
    }

    public void StartPoof()
    {
        if (!PlayerInventory.Contains("Slider 5") && walls && floor && wires)
        {
            StartCoroutine(PoofCoroutine());
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
        walls = PlayerInventory.Contains("Walls");
        floor = PlayerInventory.Contains("Floor");
        wires = PlayerInventory.Contains("Wires");
        SetActives();
        if (!floor && !walls && !wires)
        {
            // first message
            operatorMessage = "I'll just need these materials: the floor, the walls, and the wires!!!";
        }
        else if (floor && walls && wires)
        {
            // ready to print
            operatorMessage = "Radical, let me turn it on!!!!";
        }
        else
        {
            List<string> mlist = new List<string>();
            if (!floor)
            {
                mlist.Add(" the floor");
            }
            if (!walls)
            {
                mlist.Add(" the walls");
            }
            if (!wires)
            {
                mlist.Add(" the wires");
            }
            operatorMessage = $"It still needs{string.Join(',', mlist)}!!!";
        }
        SaveSystem.Current.SetString("FactoryPrinterParts", operatorMessage);
    }

    private void SetActives()
    {
        wallObject.SetActive(walls);
        floorObject.SetActive(floor);
        wireObject.SetActive(wires);
    }
}
