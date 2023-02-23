using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Tilemaps;

/* L: Responsible for storing heightMask, lightMask, and updating materials based on the lightMask. */
/*L: IMPORTANT THINGS: 
 * - If you use SetPixel on a Texture2D you HAVE to call Apply() or else it won't update
 * - ALWAYS call UpdateMaterials() any time you change the heightMask or lightMask.
*/
public class LightManager : MonoBehaviour
{
    private const string caveShaderName = "Shader Graphs/CaveTileLightShader";

    public CaveLight[] lights;
    [SerializeField] private CaveSTile[] stiles;

    public GameObject tilesRoot;
    [SerializeField] private Tilemap worldBorderColliderTilemap;
    [SerializeField] private Tilemap worldLightColliderTilemap;

    private Texture2D _heightMask;
    private Texture2D _lightMask;
    private List<Material> _caveLightMaterials;

    private int _worldToMaskDX;
    private int _worldToMaskDY;
    private int _maskSizeX;
    private int _maskSizeY;

    //L: NOTE These should be READ ONLY, but I'm not using ReadOnlyCollection since it would require an entire extra copy for the SetPixels method and we want it to be optimized.
    private Color[] _allBlack = null;
    private Color[] _allBlackStiles = null;
    private bool _updateLightingFlag = false;
    private Shader _caveShader;

    public static LightManager instance;

    public static event System.EventHandler OnLightingUpdate;

    void Awake()
    {
        //There can only be one LightManager in the scene at a time, but we don't want it to be persistent between scenes since there's overhead + it causes issues.
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializeLighting();
    }

    private void OnEnable()
    {
        SGridAnimator.OnSTileMoveEnd += OnSTileMoveEnd;
        SGrid.OnSTileEnabled += OnSTileEnabled;
        CaveLight.OnLightSwitched += OnLightSwitched;
    }

    private void OnDisable()
    {
        SGridAnimator.OnSTileMoveEnd -= OnSTileMoveEnd;
        SGrid.OnSTileEnabled -= OnSTileEnabled;
        CaveLight.OnLightSwitched -= OnLightSwitched;
    }

    private void LateUpdate()
    {
        if (_updateLightingFlag || SGrid.Current.TilesMoving())
        {
            _updateLightingFlag = false;
            UpdateLighting();
        }
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    private void OnSTileMoveEnd(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        _updateLightingFlag = true;
    }

    private void OnSTileEnabled(object sender, SGrid.OnSTileEnabledArgs e)
    {
        _updateLightingFlag = true;
    }

    private void OnLightSwitched(object sender, CaveLight.OnLightSwitchedArgs e)
    {
        _updateLightingFlag = true;
    }

    //x and y are in tilemap cell coordinates
    public bool GetLightMaskAt(int x, int y)
    {
        //Debug.Log(lightMask.GetPixel(x + worldToMaskDX, y + worldToMaskDY));
        return _lightMask.GetPixel(x + _worldToMaskDX, y + _worldToMaskDY).r > 0.5f;
    }

    //Get the light mask at a tile within a tilemap. pos is in cell coordinates with respect to tm. 
    public bool GetLightMaskAt(Tilemap tm, Vector3Int pos)
    {
        Vector3 posInWorld = tm.CellToWorld(pos);
        Vector2Int tilePos = TileUtil.WorldToTileCoords(posInWorld);
        return GetLightMaskAt(tilePos.x, tilePos.y);
    }

    private void InitializeLighting()
    {
        SetTileMapSize();

        InitHeightMask();
        InitLightMask();

        if (_caveLightMaterials == null || _caveLightMaterials.Count == 0)
        {
            FindMaterials();
        }
        UpdateMaterials();

        OnLightingUpdate?.Invoke(this, null);
    }

    private void UpdateLighting()
    {
        UpdateHeightMask();
        UpdateLightMask();
        UpdateMaterials();

        OnLightingUpdate?.Invoke(this, null);
    }

    private void SetTileMapSize()
    {
        _worldToMaskDX = -worldBorderColliderTilemap.cellBounds.x;
        _worldToMaskDY = -worldBorderColliderTilemap.cellBounds.y;
        _maskSizeX = worldBorderColliderTilemap.cellBounds.xMax + _worldToMaskDX;
        _maskSizeY = worldBorderColliderTilemap.cellBounds.yMax + _worldToMaskDY;
    }

    #region Height Mask Updates
    private void InitHeightMask()
    {
        _heightMask = new Texture2D(_maskSizeX, _maskSizeY);

        UpdateWorldHeightMask();
        UpdateStilesHeightMask();

        _heightMask.Apply();
    }

    private void UpdateHeightMask()
    {
        UpdateStilesHeightMask();
        _heightMask.Apply();
    }

    private void UpdateStilesHeightMask()
    {
        _heightMask.SetPixels(-8 + _worldToMaskDX, -8 + _worldToMaskDY, 51, 51, GetAllBlackStiles());

        foreach (CaveSTile stile in stiles)
        {
            UpdateHeightMaskBufferWithStile(stile);
        }
    }

    private void UpdateHeightMaskBufferWithStile(CaveSTile stile)
    {
        if (stile.isTileActive)
        {
            Color[] buffer = stile.HeightMask;
            int xMin = (int) (-stile.STILE_WIDTH / 2 + stile.transform.position.x + _worldToMaskDX);
            int yMin = (int) (-stile.STILE_WIDTH / 2 + stile.transform.position.y + _worldToMaskDY);
            _heightMask.SetPixels(xMin, yMin, stile.STILE_WIDTH, stile.STILE_WIDTH, buffer);
        }
    }

    private void UpdateWorldHeightMask()
    {
        Color[] buffer = new Color[_maskSizeX * _maskSizeY];

        for (int y = 0; y < _maskSizeY; y++)
        {
            for (int x = 0; x < _maskSizeX; x++)
            {
                bool tileExists = worldLightColliderTilemap.GetTile(new Vector3Int(x - _worldToMaskDX, y - _worldToMaskDY, 0)) != null;
                buffer[y * _maskSizeX + x] = tileExists ? Color.white : Color.black;
            }
        }

        _heightMask.SetPixels(buffer);
    }
    #endregion

    #region Light Mask Updates
    private void InitLightMask()
    {
        //L: Set all pixels to black (only do this once!)
        _lightMask = new Texture2D(_maskSizeX, _maskSizeY);

        UpdateLightMaskWithLights(true);
        _lightMask.Apply();
    }

    private void UpdateLightMask()
    {
        UpdateLightMaskWithLights(false);
        _lightMask.Apply();
    }

    private void UpdateLightMaskWithLights(bool fromInit)
    {
        Color[] lightMaskBuffer = new Color[_maskSizeX*_maskSizeY];
        GetAllBlack().CopyTo(lightMaskBuffer, 0);
        foreach (CaveLight l in lights)
        {
            bool lightOn = fromInit ? l.lightOnStart : l.LightOn;
            if (lightOn && l.LightActiveInScene())
            {
                //Debug.Log($"Updated Light: {l.gameObject.name}");

                // Vector2Int testPos = new Vector2Int(29+_worldToMaskDX, -7+_worldToMaskDY);
                //Debug.Log($"Height Mask Test Pixel: {_heightMask.GetPixel(testPos.x, testPos.y).r > 0.5f}");
                l.UpdateLightMask(ref lightMaskBuffer, _heightMask, _worldToMaskDX, _worldToMaskDY, _maskSizeX, _maskSizeY);
            }
        }
        _lightMask.SetPixels(lightMaskBuffer);
    }
    #endregion

    private void FindMaterials()
    {
        _caveLightMaterials = new List<Material>();
        _caveShader = Shader.Find(caveShaderName);

        Renderer[] allRenderers = FindObjectsOfType<Renderer>(true);
        foreach (Renderer r in allRenderers)
        {
            if (r.material.shader == _caveShader)
            {
                _caveLightMaterials.Add(r.material);
                //Debug.Log(r.gameObject.name);
            }
        }
    }

    private void UpdateMaterials()
    {
        foreach (Material m in _caveLightMaterials)
        {
            m.SetTexture("_LightMask", _lightMask);
            m.SetVector("_MaskOffset", new Vector4(_worldToMaskDX, _worldToMaskDY));
            m.SetVector("_MaskSize", new Vector4(_maskSizeX, _maskSizeY));
        }
    }

    private Color[] GetAllBlack()
    {
        if (_allBlack == null)
        {
            int totalMaskSize = _maskSizeX * _maskSizeY;
            Color black = Color.black;

            _allBlack = new Color[totalMaskSize];
            for (int i = 0; i < totalMaskSize; i++)
            {
                _allBlack[i] = black;
            }
        }

        return _allBlack;
    }

    private Color[] GetAllBlackStiles()
    {
        if (_allBlackStiles == null)
        {
            int totalSize = 2601;   //3*3*17*17
            Color black = Color.black;

            _allBlackStiles = new Color[totalSize];
            for (int i = 0; i < totalSize; i++)
            {
                _allBlackStiles[i] = black;
            }
        }

        return _allBlackStiles;
    }

    #region Draw Gizmos
    private void OnDrawGizmosSelected()
    {
        DrawLightMask();
        DrawHeightMask();
    }

    private void DrawHeightMask()
    {
        Gizmos.color = Color.blue; 
        for (int x = 0; x < _maskSizeX; x++)
        {
            for (int y = 0; y < _maskSizeY; y++)
            {
                if (_heightMask.GetPixel(x, y).r > 0.5f)
                {
                    Gizmos.DrawSphere(new Vector3(x-_worldToMaskDX, y-_worldToMaskDY, 0), 0.2f);
                }
            }
        }
    }

    private void DrawLightMask()
    {
        //draw lightmask
        Gizmos.color = Color.yellow;
        for (int x = -_worldToMaskDX; x < _maskSizeX - _worldToMaskDX; x++)
        {
            for (int y = -_worldToMaskDY; y < _maskSizeY - _worldToMaskDY; y++)
            {
                if (GetLightMaskAt(x, y))
                {
                    Gizmos.DrawSphere(new Vector3(x, y, 0), 0.2f);
                }
            }
        }
    }
    #endregion
}