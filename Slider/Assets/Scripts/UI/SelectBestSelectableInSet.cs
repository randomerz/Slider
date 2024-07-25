using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SelectBestSelectableInSet : MonoBehaviour
{
    [SerializeField] private List<Selectable> selectables;

    public void SelectBestSelectable()
    {
        Selectable bestSelectableInCurrentMenu = selectables.Where(selectable => selectable.isActiveAndEnabled && selectable.IsInteractable()).First();
        CoroutineUtils.ExecuteAfterEndOfFrame(() => bestSelectableInCurrentMenu.Select(), this);
        Debug.Log(bestSelectableInCurrentMenu.gameObject.name);
    }
}
