using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Animation event moment :(
public class UIArtifactmenuDisabler : MonoBehaviour
{
    public UIArtifactMenus uiArtifactMenus;

    public void DisableArtPanel()
    {
        uiArtifactMenus.DisableArtPanel();
    }
}
