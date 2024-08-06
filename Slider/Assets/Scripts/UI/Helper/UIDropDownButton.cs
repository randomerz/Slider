using UnityEngine;
using UnityEngine.UI;

public class UIDropDownButton : MonoBehaviour
{
    public UIDropDownController myParent;
    public Toggle myToggle;

    private void Awake()
    {
        myParent.RegisterItem(this);
    }

    private void OnDestroy()
    {
        myParent.UnregisterItem(this);
    }
}