using UnityEngine;

public class OpenGuideHandler : MonoBehaviour
{
    public void OpenGuideWebsite()
    {
        string locale = (string)SettingsManager.Setting(Settings.Locale).GetCurrentValue();
        Debug.Log("Locale: " + locale);
        string url = "https://www.boomo.me/walkthroughs/slider.html";
        switch (locale)
        {
            case "Korean":
                url = "https://www.boomo.me/walkthroughs/slider-kr.html";
                break;
            case "Chinese":
                url = "https://www.boomo.me/walkthroughs/slider-cn.html";
                break;
        }
        Application.OpenURL(url);
    }

    public void OpenFeedbackFormHTTP()
    {
        Application.OpenURL("https://forms.gle/J14mhR7ysbVUGpvZ9");
        
    }
}
