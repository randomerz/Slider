using UnityEngine;

public class LocalizationInjector : MonoBehaviour
{
    void Start()
    {
        Refresh();
    }

    public void Refresh()
    {
        LocalizationLoader.LocalizePrefab(gameObject);
    }
}
