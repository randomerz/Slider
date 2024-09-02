using UnityEngine;

public class LocalizationInjector : MonoBehaviour
{
    // AT: no longer used, Refresh should only be called through LocalizationLoader
    void Start()
    {
        // Refresh();
    }

    public void Refresh()
    {
        LocalizationLoader.LocalizePrefab(gameObject);
    }
}
