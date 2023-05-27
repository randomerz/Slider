using System.Xml;
using UnityEngine;
using static MGSimulator;

public class MGUI : MonoBehaviour
{
    [SerializeField] private MGUISquare[] squares;
    [SerializeField] private GameObject trackerPrefab;

    private void Start()
    {
        MGSimulator.OnUnitSpawn += OnUnitSpawn;
    }

    private void OnDisable()
    {
        MGSimulator.OnUnitSpawn -= OnUnitSpawn;
    }

    public int GetSquareIndexFromPos(int x, int y)
    {
        return 3 * y + x;
    }

    private void OnUnitSpawn(MGUnit unit)
    {
        CreateTracker(unit);
    }

    private void CreateTracker(MGUnit unit)
    {
        GameObject trackerGO = GameObject.Instantiate(trackerPrefab, this.transform);
        MGUIUnitTracker tracker = trackerGO.GetComponent<MGUIUnitTracker>();

        Vector2Int trackerPos = unit.CurrSpace.GetPosition();
        MGUISquare trackerSquare = squares[GetSquareIndexFromPos(trackerPos.x, trackerPos.y)];
        tracker.SetSquare(trackerSquare);
        tracker.SetData(unit.Data);
    }

    //private void DestroyTracker(MGUnitData.Data unit)
    //{
    //    MGUIUnitTracker tracker = _unitTrackers[unit];
    //    _unitTrackers.Remove(unit);
    //    Destroy(tracker.gameObject);
    //}
}
