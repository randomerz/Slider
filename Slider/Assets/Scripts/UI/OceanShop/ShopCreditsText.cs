using UnityEngine;
using TMPro;

public class ShopCreditsText : MonoBehaviour 
{
    public ShopManager shopManager;
    public TextMeshProUGUI creditsText;

    private void Update() 
    {
        creditsText.text = string.Format("{0} Credits", shopManager.GetCredits());
    }
}