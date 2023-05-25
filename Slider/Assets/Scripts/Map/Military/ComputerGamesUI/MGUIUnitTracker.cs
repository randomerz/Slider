using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MGUIUnitTracker : MonoBehaviour
{
    //[SerializeField] private TMPro.TextMeshProUGUI text;
    //[SerializeField] private int count;
    [SerializeField] private Color allyColor;
    [SerializeField] private Color enemyColor;

    private Image _image;

    private void Awake()
    {
        _image = GetComponent<Image>();
    }

    public void SetAllegiance(MGUnits.Allegiance side)
    {
        switch (side)
        {
            case MGUnits.Allegiance.Ally:
                _image.color = allyColor;
                break;
            default:
                _image.color = enemyColor;
                break;
        }
    }

    public void SetCount(int count)
    {
        //this.count = count;

        ////TODO: Set text field to new count.
        //text.text = count.ToString();
    }
}
