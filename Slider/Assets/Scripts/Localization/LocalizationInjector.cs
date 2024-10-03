using UnityEngine;

public class LocalizationInjector : MonoBehaviour
{
    public GameObject prefabVariantParent = null;
    
    public void Refresh()
    {
        LocalizationLoader.LocalizePrefab(gameObject, prefabVariantParent == null ? gameObject : prefabVariantParent);
    }
}
