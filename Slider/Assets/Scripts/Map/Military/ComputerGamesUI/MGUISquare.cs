using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MGUISquare : MonoBehaviour
{
    [SerializeField] private int x;
    [SerializeField] private int y;
    [SerializeField] private Sprite supplyImage;
    [SerializeField] private GameObject trackerPrefab;

    private Dictionary<MGUnits.Unit, MGUIUnitTracker> _unitTrackers;

    private MGSimulator _sim;
    private Image _displayImg;

    private MGSpace _mgSpace;

    private void Awake()
    {
        _unitTrackers = new Dictionary<MGUnits.Unit, MGUIUnitTracker>();
        _sim = FindObjectOfType<MGSimulator>();
        _displayImg = GetComponent<Image>();

        MGSimulator.AfterInit += SetupEventListeners;
    }

    private void SetupEventListeners(object sender, System.EventArgs e)
    {
        _mgSpace = _sim.GetSpace(x, y);
        _mgSpace.OnSupplyDropSpawn += OnSupplyDrop;
        _mgSpace.OnUnitsChanged += UpdateUnits;
    }

    private void OnDisable()
    {
        Debug.Log("Disabled");
        _mgSpace.OnSupplyDropSpawn -= OnSupplyDrop;
    }

    private void OnSupplyDrop()
    {
        Debug.Log("Supply Drop UI Updated.");
        SetSupplyTile(true);
    }

    private void UpdateUnits(MGUnits.Unit unit, int quantity)
    {
        //Check Deletion
        if (quantity <= 0)
        {
            if (_unitTrackers.ContainsKey(unit))
            {
                DestroyTracker(unit);
            }

            return;
        }

        //Check Creation
        if (!_unitTrackers.ContainsKey(unit))
        {
            CreateTracker(unit);
        }

        //Update Tracker Count.
        _unitTrackers[unit].SetCount(quantity);
    }

    private void CreateTracker(MGUnits.Unit unit)
    {
        GameObject trackerGO = GameObject.Instantiate(trackerPrefab, this.transform);
        MGUIUnitTracker tracker = trackerGO.GetComponent<MGUIUnitTracker>();
        tracker.SetAllegiance(unit.side);
        //tracker.transform.position += Vector3.left * 5 * (unit.side == MGUnits.Allegiance.Ally ? -1f : 1f);
        _unitTrackers[unit] = tracker;
    }

    private void DestroyTracker(MGUnits.Unit unit)
    {
        MGUIUnitTracker tracker = _unitTrackers[unit];
        _unitTrackers.Remove(unit);
        Destroy(tracker.gameObject);
    }

    public void SetSupplyTile(bool enabled)
    {
        _displayImg.sprite = supplyImage;
    }
}
