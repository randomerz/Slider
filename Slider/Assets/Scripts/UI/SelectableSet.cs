using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attach this to a UI object and put the selectables into the selectables array.
/// This is a replacement for the old setup of putting all of the selectable sets into the
/// UINavigationManager component by hand. Use this instead!
/// </summary>
/// <remarks>Author: Travis</remarks>
public class SelectableSet : MonoBehaviour
{
    [SerializeField] private Selectable[] selectables;
    public Selectable[] Selectables { get => selectables; }

    [SerializeField] private List<SelectableSet> subSelectableSets;
    public SelectableSet[] SubSelectableSets { get => subSelectableSets.ToArray(); }

    private void Awake()
    {
        if (selectables.Length == 0)
        {
            Debug.LogWarning($"A SelectableSet on {gameObject.name} ({gameObject.GetInstanceID()}) contained no selectables. This likely indicates an error.");
        }
    }

    public void AddSubSelectableSet(SelectableSet selectableSet)
    {
        subSelectableSets.Add(selectableSet);
    }

    public void RemoveSubSelectableSet(SelectableSet selectableSet)
    {
        subSelectableSets.Remove(selectableSet);
    }
}
