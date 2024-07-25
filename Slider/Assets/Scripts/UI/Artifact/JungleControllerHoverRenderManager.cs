using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JungleControllerHoverRenderManager : MonoBehaviour
{
    private const string SLIDER_COLLECTIBLE = "Slider 2 & 3";

    [SerializeField] private Image button2Default;
    [SerializeField] private Image button2Solid;
    [SerializeField] private Image button2Dashed;
    [SerializeField] private Image button3Default;
    [SerializeField] private Image button3Solid;
    [SerializeField] private Image button3Dashed;

    // Start is called before the first frame update
    void Start()
    {
        bool hasSlider = PlayerInventory.Contains(SLIDER_COLLECTIBLE);

        button2Default.enabled = !hasSlider;
        button3Default.enabled = !hasSlider;

        button2Solid.gameObject.SetActive(hasSlider);
        button2Dashed.gameObject.SetActive(hasSlider);

        button3Solid.gameObject.SetActive(hasSlider);
        button3Dashed.gameObject.SetActive(hasSlider);
    }

    public void SetTile2Selected()
    {
        bool hasSlider = PlayerInventory.Contains(SLIDER_COLLECTIBLE);
        
        button2Default.enabled = !hasSlider;
        button3Default.enabled = !hasSlider;

        if (!hasSlider)
        {
            return;
        }

        button3Default.gameObject.SetActive(true);

        button2Solid.gameObject.SetActive(true);
        button2Dashed.gameObject.SetActive(false);

        button3Solid.gameObject.SetActive(false);
        button3Dashed.gameObject.SetActive(true);
    }

    public void SetTile3Selected()
    {
        bool hasSlider = PlayerInventory.Contains(SLIDER_COLLECTIBLE);
        
        button2Default.enabled = !hasSlider;
        button3Default.enabled = !hasSlider;

        if (!hasSlider)
        {
            return;
        }

        button2Default.gameObject.SetActive(true);

        button2Solid.gameObject.SetActive(false);
        button2Dashed.gameObject.SetActive(true);

        button3Solid.gameObject.SetActive(true);
        button3Dashed.gameObject.SetActive(false);
    }

    public void OnDeselect()
    {
        button2Default.gameObject.SetActive(false);
        button3Default.gameObject.SetActive(false);
    }
}
