using UnityEngine;

public class OpenGuideHandler : MonoBehaviour
{
    public void OpenGuideWebsite()
    {
        Application.OpenURL("https://www.boomo.me/walkthroughs/slider.html");
    }

    public void OpenFeedbackFormHTTP()
    {
        Application.OpenURL("https://forms.gle/J14mhR7ysbVUGpvZ9");
        
    }
}
