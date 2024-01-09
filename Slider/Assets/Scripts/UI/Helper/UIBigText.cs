using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIBigText : MonoBehaviour
{
    public TextMeshProUGUI[] texts;
    
    public void SetText(string text)
    {
        foreach (TextMeshProUGUI t in texts)
        {
            t.text = text;
        }
    }
}
