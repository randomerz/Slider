using TMPro;
using UnityEngine;

public class GetVersion : MonoBehaviour
{
    public TextMeshProUGUI versionText;

    private void Awake()
    {
        versionText.text = $"v{Application.version}";
    }
}