using UnityEngine;

public class TileCountBasedTracker : MonoBehaviour 
{
    [SerializeField] private GameObject trackedObject;
    [SerializeField] private int enableTrackerOnTileNumber;
    [SerializeField] private int disableTrackerOnTileNumber;
    private bool areTrackersOn;

    private void Start() 
    {
        if (enableTrackerOnTileNumber <= SGrid.Current.GetNumTilesCollected() &&
            SGrid.Current.GetNumTilesCollected() < disableTrackerOnTileNumber)
        {
            SetTrackers(true);
        }
    }

    private void OnEnable() 
    {
        SGrid.OnSTileEnabled += OnSTileEnabled;
    }

    private void OnDisable() 
    {
        SGrid.OnSTileEnabled -= OnSTileEnabled;
    }

    private void OnSTileEnabled(object sender, SGrid.OnSTileEnabledArgs e)
    {
        if (e.stile.islandId == enableTrackerOnTileNumber)
        {
            SetTrackers(true);
        }
        else if (e.stile.islandId >= disableTrackerOnTileNumber)
        {
            SetTrackers(false);
        }
    }

    private void SetTrackers(bool value)
    {
        if (value != areTrackersOn)
        {
            areTrackersOn = value;
            if (areTrackersOn)
            {
                UITrackerManager.AddNewTracker(gameObject);
            }
            else
            {
                UITrackerManager.RemoveTracker(gameObject);
            }
        }
    }
}