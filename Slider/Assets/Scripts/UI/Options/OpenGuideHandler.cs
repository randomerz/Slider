using UnityEngine;

public class OpenGuideHandler : MonoBehaviour
{
    public void OpenGuideWebsite()
    {
        string locale = (string)SettingsManager.Setting(Settings.Locale).GetCurrentValue();
        string url = "https://www.boomo.me/walkthroughs/slider.html";
        switch (locale)
        {
            case "한국어":
                url = "https://www.boomo.me/walkthroughs/slider-kr.html";
                break;
            case "简体中文":
                break;
        }
        Application.OpenURL(url);
    }

    public void OpenFeedbackFormHTTP()
    {
        Application.OpenURL("https://forms.gle/J14mhR7ysbVUGpvZ9");
        
    }
}
