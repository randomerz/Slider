using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Assumptions made:
/// - Tiles with lights can cast in all directions
/// - Refresh after every artifact move
/// - 
/// </summary>
public class CaveArtifactLightSim : MonoBehaviour
{
    public LightManager lightManager;
    public CaveSTile[] caveStiles; // in order of island id
    public ArtifactTileButton[] artifactTileButtons; // in order from top-left to bottom-right

    private Dictionary<CaveLight, int> stileLights = new Dictionary<CaveLight, int>();
    private Dictionary<CaveLight, Vector2Int> worldLights = new Dictionary<CaveLight, Vector2Int>();

    private bool[,] isLit = new bool[,] {
        {false, false, false}, 
        {false, false, false}, 
        {false, false, false}, 
    };

    private void Awake() 
    {
        Initialize();
    }

    private void Start()
    {
        UpdateLightSim();
    }

    private void OnEnable()
    {
        SGrid.OnSTileEnabled += STileEnabled;
        UIArtifact.MoveMadeOnArtifact += ButtonInteract;
        CaveLight.OnLightSwitched += LightSwitched;
    }

    private void OnDisable()
    {
        SGrid.OnSTileEnabled -= STileEnabled;
        UIArtifact.MoveMadeOnArtifact -= ButtonInteract;
        CaveLight.OnLightSwitched -= LightSwitched;
    }

    private void STileEnabled(object sender, SGrid.OnSTileEnabledArgs e) => UpdateLightSim();
    private void ButtonInteract(object sender, System.EventArgs e) => UpdateLightSim();
    private void LightSwitched(object sender, CaveLight.OnLightSwitchedArgs e) => UpdateLightSim();

    public void Initialize()
    {
        foreach (CaveLight light in lightManager.lights)
        {
            STile stile = light.GetComponentInParent<STile>();
            if (stile != null)
            {
                stileLights[light] = stile.islandId;
            }
            else
            {
                worldLights[light] = light.GetPos();
            }
        }
    }

    public bool GetIsLitAt(int x, int y)
    {
        return isLit[x, y];
    }

    public void UpdateLightSim()
    {
        // clear grid
        isLit = new bool[,] {
            {false, false, false}, 
            {false, false, false}, 
            {false, false, false}, 
        };

        foreach (CaveLight light in worldLights.Keys)
        {
            ProjectLight(light, worldLights[light]);
        }
        foreach (CaveLight light in stileLights.Keys)
        {
            int islandId = stileLights[light];
            ArtifactTileButton button = UIArtifact.GetInstance().GetButton(islandId);
            ProjectLight(light, new Vector2Int(button.x, button.y));
        }

        UpdateButtons();
    }

    public void UpdateButtons()
    {
        foreach (ArtifactTileButton button in artifactTileButtons)
        {
            ArtifactTBPluginLight lightPlugin = button.GetComponent<ArtifactTBPluginLight>();
            if (lightPlugin == null)
            {
                Debug.LogError("Plugin could not be found!");
                continue;
            }
            lightPlugin.SetLit(isLit[button.x, button.y]);

            if (SGrid.Current.CheckCompletion)
            {
                button.SetComplete(isLit[button.x, button.y]);
            }
        }
    }

    private void ProjectLight(CaveLight light, Vector2Int position)
    {
        if (!light.isActiveAndEnabled || !light.LightOn)
            return;

        Vector2Int[] directions = new Vector2Int[] {
            Vector2Int.right,
            Vector2Int.up,
            Vector2Int.left,
            Vector2Int.down,
        };

        if (IsInBounds(position))
        {
            isLit[position.x, position.y] = true;
        }

        foreach (Vector2Int direction in directions)
        {
            Vector2Int pos = position + direction;
            if (IsInBounds(pos))
            {
                ArtifactTileButton button = UIArtifact.GetButton(pos.x, pos.y);
                if (button == null || !button.TileIsActive)
                {
                    isLit[pos.x, pos.y] = true;
                    continue;
                }
                
                int islandId = button.islandId;
                CaveSTile caveSTile = caveStiles[islandId - 1];
                if (caveSTile.validDirsForLight.Contains(-direction))
                {
                    isLit[pos.x, pos.y] = true;
                    continue;
                }
            }
        }
    }

    private bool IsInBounds(Vector2 position)
    {
        return 0 <= position.x && position.x < 3 && 0 <= position.y && position.y < 3;
    }
}
