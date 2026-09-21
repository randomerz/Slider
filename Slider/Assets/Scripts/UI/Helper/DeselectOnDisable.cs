using UnityEngine;
using UnityEngine.EventSystems;

// on controller mode, when some buttons are selected but disabled, they stay as the event systems selected object
public class DeselectOnDisable : MonoBehaviour 
{
    public void Deselect()
    {
        if (UIArtifactMenus.IsArtifactOpen() && Controls.UsingControllerOrKeyboardOnly())
        {
            if (EventSystem.current.currentSelectedGameObject == gameObject)
            {
                EventSystem.current.SetSelectedGameObject(null);
                UIArtifact.GetInstance().HandleControllerCheck(this, null);
            }
        }
    }
}