using UnityEngine;

public class LocalizationInjector : MonoBehaviour
{
    private string locale = null;
    
    void Start()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (locale == null || !locale.Equals(LocalizationLoader.CurrentLocale))
        {
            LocalizationLoader.LocalizePrefab(gameObject);
            locale = LocalizationLoader.CurrentLocale;
        }
    }
}
