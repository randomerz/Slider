using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonMirage : MonoBehaviour
{
    [SerializeField] private Material regularMaterial;
    [SerializeField] private Material mirageMaterial;
    [SerializeField] private Image buttonImage;
    private bool mirageEnabled = false;

    public void SetMirageEnabled(bool enabled)
    {
        mirageEnabled = enabled;
        buttonImage.material = mirageEnabled ? mirageMaterial : regularMaterial;
    }
}
