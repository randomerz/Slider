using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MGUISquare : MonoBehaviour
{
    [SerializeField] private int x;
    [SerializeField] private int y;
    [SerializeField] private Sprite supplyImage;

    private MGSimulator _sim;
    private Image _displayImg;

    private MGSpace _mgSpace;

    private void Awake()
    {
        _sim = FindObjectOfType<MGSimulator>();
        _displayImg = GetComponent<Image>();

        MGSimulator.AfterInit += SetupEventListeners;
    }

    private void SetupEventListeners(object sender, System.EventArgs e)
    {
        _mgSpace = _sim.GetSpace(x, y);
        MGSimulator.OnUnitSpawn += OnUnitSpawn;
        //_mgSpace.OnSupplyDropSpawn += OnSupplyDrop;
    }

    private void OnDisable()
    {
        MGSimulator.OnUnitSpawn -= OnUnitSpawn;
        //_mgSpace.OnSupplyDropSpawn -= OnSupplyDrop;
    }

    private void OnSupplyDrop()
    {
        Debug.Log("Supply Drop UI Updated.");
        SetSupplyTile(true);
    }

    private void OnUnitSpawn(MGUnit unit)
    {

    }

    //private void UpdateUnits(MGUnitData.Data unit, int quantity)
    //{
    //    //Check Deletion
    //    if (quantity <= 0)
    //    {
    //        if (_unitTrackers.ContainsKey(unit))
    //        {
    //            DestroyTracker(unit);
    //        }

    //        return;
    //    }

    //    //Check Creation
    //    if (!_unitTrackers.ContainsKey(unit))
    //    {
    //        CreateTracker(unit);
    //    }

    //    //Update Tracker Count.
    //    _unitTrackers[unit].SetCount(quantity);
    //}

    public void SetSupplyTile(bool enabled)
    {
        _displayImg.sprite = supplyImage;
    }
}
