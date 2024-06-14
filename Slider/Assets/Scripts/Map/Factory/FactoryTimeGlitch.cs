using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FactoryTimeGlitch : MonoBehaviour
{
    public const string TIME_GLITCH_SAVE_STRING = "factoryTimeGlitchOccured";
    public const int TIME_GLITCH_ISLAND_ID = 7;

    [SerializeField] private GameObject objectsFromPastRoot;
    [SerializeField] private GameObject objectsFromFutureRoot;

    [SerializeField] private STileTilemap stileTilemap;
    [SerializeField] private Tilemap newMaterialsTilemap;

    [SerializeField] private UIHousingTracker housingTracker;

    private void Start()
    {
        if (SaveSystem.Current.GetBool(TIME_GLITCH_SAVE_STRING))
        {
            UpdateMap();
        }
    }

    public void DoTimeGlitch()
    {
        if (SaveSystem.Current.GetBool(TIME_GLITCH_SAVE_STRING))
        {
            return;
        }

        // Maybe it will be a coroutine in the future
        TimeGlitch();
    }
    
    private void TimeGlitch()
    {
        SaveSystem.Current.SetBool(TIME_GLITCH_SAVE_STRING, true);

        UIEffects.FlashWhite();
        AudioManager.Play("Slide Explosion");
        AudioManager.Play("Hurt");

        UpdateMap();

        // TODO: Update UI icons

        SGrid.Current.gridTilesExplored.SetTileExplored(TIME_GLITCH_ISLAND_ID, false);
    }
    
    private void UpdateMap()
    {
        objectsFromPastRoot.SetActive(false);
        objectsFromFutureRoot.SetActive(true);

        stileTilemap.materials = newMaterialsTilemap;

        housingTracker.enabled = true;
    }
}