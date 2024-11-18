using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIBigText : MonoBehaviour
{
    public TextMeshProUGUI primaryText;
    public TextMeshProUGUI[] texts;

    private bool wasUpdated;

    private void Start()
    {
        // Late start
        CoroutineUtils.ExecuteAfterEndOfFrame(() => {
            if (!wasUpdated && primaryText != null)
            {
                SetText(primaryText.text);
            }
        }, this);
    }
    
    public void SetText(string text)
    {
        wasUpdated = true;
        foreach (TextMeshProUGUI t in texts)
        {
            t.text = text;
        }
    }
}
