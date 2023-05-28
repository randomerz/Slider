using UnityEngine;
using static MGSimulator;

public class MGUI : MonoBehaviour
{
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private MGUISquare[] squares;
    [SerializeField] private GameObject trackerPrefab;

    public MGUISquare[] Squares => squares;

    private void Start()
    {
        MGSimulator.OnUnitSpawn += OnUnitSpawn;
    }

    private void OnDisable()
    {
        MGSimulator.OnUnitSpawn -= OnUnitSpawn;
    }

    public MGUISquare GetSquare(MGSpace space)
    {
        return GetSquare(space.GetPosition());
    }

    public MGUISquare GetSquare(Vector2Int pos)
    {
        int index = pos.y * width + pos.x;
        return squares[index];
    }

    public MGUISquare GetSquare(int x, int y)
    {
        return GetSquare(new Vector2Int(x, y));
    }

    private void OnUnitSpawn(MGUnit unit)
    {
        CreateTracker(unit);
    }

    private void CreateTracker(MGUnit unit)
    {
        GameObject trackerGO = GameObject.Instantiate(trackerPrefab, this.transform);
        MGUIUnitTracker tracker = trackerGO.GetComponent<MGUIUnitTracker>();
        tracker.Init(unit);
    }

    //private void DestroyTracker(MGUnitData.Data unit)
    //{
    //    MGUIUnitTracker tracker = _unitTrackers[unit];
    //    _unitTrackers.Remove(unit);
    //    Destroy(tracker.gameObject);
    //}
}
