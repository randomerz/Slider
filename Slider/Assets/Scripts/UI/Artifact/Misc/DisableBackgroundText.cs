using UnityEngine;
using TMPro;

public class DisableBackgroundText : MonoBehaviour
{
    public TextMeshProUGUI text;
    public TMP_FontAsset disableIfThisIsNotMyAsset;

    private void OnEnable()
    {
        text.enabled = text.font == disableIfThisIsNotMyAsset;
    }
}