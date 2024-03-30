using System.Collections.Generic;
using UnityEngine;

public class SGridTilesExplored : MonoBehaviour
{
    // Format: AreaExplored_Village_01
    // Prefix_CurrentArea_NumberWithZeroFill
    private const string TILE_EXPLORED_PREFIX = "TilesExplored";
    // Needs to be maintained
    private bool areAllTilesExplored;
    private int lastStileUnderPlayer = -1;
    private Area myArea;

    private void Start()
    {
        myArea = SGrid.Current.GetArea();
        UpdateAllTilesExplored(null, null);
    }

    private void OnEnable()
    {
        SGrid.OnSTileCollected += UpdateAllTilesExplored;
    }

    private void OnDisable()
    {
        SGrid.OnSTileCollected -= UpdateAllTilesExplored;
    }

    private void Update()
    {
        if (!areAllTilesExplored)
        {
            STile stileUnderPlayer = Player.GetInstance().GetSTileUnderneath();
            if (stileUnderPlayer != null)
            {
                if (stileUnderPlayer.islandId != lastStileUnderPlayer)
                {
                    lastStileUnderPlayer = stileUnderPlayer.islandId;
                    SetTileExplored(myArea, lastStileUnderPlayer);
                    UpdateAllTilesExplored(null, null);
                }
            }
        }
    }

    public bool IsTileExplored(int stileId)
    {
        return IsTileExplored(myArea, stileId);
    }

    public bool IsTileExplored(Area area, int stileId)
    {
        return SaveSystem.Current.GetBool(BuildSaveString(area, stileId));
    }

    public void SetTileExplored(int stileId, bool explored=true)
    {
        SetTileExplored(myArea, stileId, explored);
    }

    public virtual void SetTileExplored(Area area, int stileId, bool explored=true)
    {
        SaveSystem.Current.SetBool(BuildSaveString(area, stileId), explored);

        if (!explored)
        {
            areAllTilesExplored = false;
        }
    }

    protected string BuildSaveString(Area area, int stileId)
    {
        return $"{TILE_EXPLORED_PREFIX}_{area.ToString()}_{stileId.ToString("D2")}";
    }

    private void UpdateAllTilesExplored(object sender, SGrid.OnSTileEnabledArgs e)
    {
        int numTilesCollected = SGrid.Current.GetActiveTiles().Count;
        for (int i = 1; i <= numTilesCollected; i++)
        {
            if (!IsTileExplored(myArea, i))
            {
                areAllTilesExplored = false;
                return;
            }
        }
        areAllTilesExplored = true;
    }
}