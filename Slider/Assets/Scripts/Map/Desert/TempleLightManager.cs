using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Tilemaps;

/* L: Responsible for storing heightMask, lightMask, and updating materials based on the lightMask. */
/*L: IMPORTANT THINGS: 
 * - If you use SetPixel on a Texture2D you HAVE to call Apply() or else it won't update
 * - ALWAYS call UpdateMaterials() any time you change the heightMask or lightMask.
*/
public class TempleLightManager : MonoBehaviour
{
    private const string caveShaderName = "Shader Graphs/CaveTileLightShader";

   // [SerializeField] private CaveLight[] lights;
    [SerializeField] private TempleLight[] lights;
   // [SerializeField] private CaveSTile[] stiles;

   // public GameObject tilesRoot;
    [SerializeField] private Tilemap worldBorderColliderTilemap;
    //[SerializeField] private Tilemap worldLightColliderTilemap;

   // private Texture2D _heightMask;
    private Texture2D _lightMask;
    private List<Material> _caveLightMaterials;

    private int _worldToMaskDX;
    private int _worldToMaskDY;
    private int _maskSizeX;
    private int _maskSizeY;

    //L: NOTE These should be READ ONLY, but I'm not using ReadOnlyCollection since it would require an entire extra copy for the SetPixels method and we want it to be optimized.
    private Color[] _allBlack = null;
   // private Color[] _allBlackStiles = null;
    private bool _updateLightingFlag = false;
    private Shader _caveShader;

    public static TempleLightManager instance;

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
        SGrid.OnSTileEnabled += OnSTileEnabled;
    }

    private void OnDisable()
    {
        SGrid.OnSTileEnabled -= OnSTileEnabled;
    }

    private void OnSTileEnabled(object sender, SGrid.OnSTileEnabledArgs e)
    {
        //UpdateLighting();
    }

    public void SetLightFlag()
    {
        _updateLightingFlag = true;
    }

    private void Update()
    {
    
        //if (_updateLightingFlag)
       //{
         //   _updateLightingFlag = false;
            UpdateLighting(true);
        //}    
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
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
        InitLightMask();

        if (_caveLightMaterials == null || _caveLightMaterials.Count == 0)
        {
            FindMaterials();
        }
        UpdateMaterials();
    }

    private void UpdateLighting(bool dynamicOnly = false)
    {
        UpdateLightMask(dynamicOnly);
        UpdateMaterials();
    }

    private void SetTileMapSize()
    {
        _worldToMaskDX = -worldBorderColliderTilemap.cellBounds.x;
        _worldToMaskDY = -worldBorderColliderTilemap.cellBounds.y;
        _maskSizeX = worldBorderColliderTilemap.cellBounds.xMax + _worldToMaskDX;
        _maskSizeY = worldBorderColliderTilemap.cellBounds.yMax + _worldToMaskDY;
    }


    #region Light Mask Updates
    private void InitLightMask()
    {
        //L: Set all pixels to black (only do this once!)
        _lightMask = new Texture2D(_maskSizeX, _maskSizeY);
        print(_maskSizeX);
        print(_maskSizeY);
        UpdateLightMaskWithLights(false);
        _lightMask.Apply();
    }

    private void UpdateLightMask(bool dynamicOnly)
    {
        UpdateLightMaskWithLights(dynamicOnly);
        _lightMask.Apply();
    }

    private void UpdateLightMaskWithLights(bool dynamicOnly)
    {
        Color[] lightMaskBuffer = new Color[_maskSizeX*_maskSizeY];
        GetAllBlack().CopyTo(lightMaskBuffer, 0);
        foreach (TempleLight l in lights)
        {
            l.UpdateLightMask(ref lightMaskBuffer, _worldToMaskDX, _worldToMaskDY, _maskSizeX, _maskSizeY, dynamicOnly);
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

            _allBlack = new Color[totalMaskSize];
            for (int i = 0; i < totalMaskSize; i++)
            {
                _allBlack[i] = Color.black;
            }
        }

        return _allBlack;
    }


    #region Draw Gizmos
    private void OnDrawGizmosSelected()
    {
        DrawLightMask();
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