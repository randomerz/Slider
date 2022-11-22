using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CaveMossManager : MonoBehaviour
{
    private const float colliderForgiveness = 0.25f;

    [SerializeField] private Tilemap mossMap;
    [SerializeField] private Tilemap recededMossMap;
    public Tilemap mossCollidersMap;
    [SerializeField] private STile stile;
    [SerializeField] private float mossFadeSpeed = 3f;

    private PlayerMoveOffMoss _playerMoveOffMoss;
    private float _updateMossTimer;

    public class MossIsGrowingArgs : System.EventArgs
    {
        public STile stile;
        public Tilemap mossMap;
        public Vector3Int cellPos;
        public bool isGrowing;
    }

    public class MossUpdatedArgs : System.EventArgs
    {
        public STile stile;
    }

    public static event System.EventHandler<MossUpdatedArgs> MossUpdated;

    private void Awake()
    {
        if (stile == null)
        {
            stile = GetComponentInParent<CaveSTile>();
        }

        _playerMoveOffMoss = mossCollidersMap.GetComponent<PlayerMoveOffMoss>();
    }

    private void Start()
    {
        InitMoss();
    }

    private void OnEnable()
    {
        LightManager.OnLightingUpdate += HandleLightingUpdate;
    }

    private void OnDisable()
    {
        LightManager.OnLightingUpdate -= HandleLightingUpdate;
    }

    private void Update()
    {

        if (_updateMossTimer >= 0f)
        {
            UpdateMoss();
            _updateMossTimer -= Time.deltaTime;
        }
    }

    private void HandleLightingUpdate(object sender, System.EventArgs e)
    {
        _updateMossTimer = mossFadeSpeed;
    }

    private void InitMoss()
    {
        //L: Initialize all moss tiles
        if (LightManager.instance != null)
        {
            ForEachMossTileIn(mossMap, (pos) =>
            {
                mossMap.SetTileFlags(pos, TileFlags.None);
                recededMossMap.SetTileFlags(pos, TileFlags.None);

                bool posIsLit = LightManager.instance.GetLightMaskAt(mossMap, pos);
                SetMossState(pos, posIsLit);
            });
        }

        MossUpdated?.Invoke(this, new MossUpdatedArgs { stile = stile });
    }

    private void SetMossState(Vector3Int pos, bool posIsLit)
    {
        Color invisibleWhite = new Color(1.0f, 1.0f, 1.0f, 0.0f);

        mossMap.SetColor(pos, posIsLit ? invisibleWhite : Color.white);
        recededMossMap.SetColor(pos, posIsLit ? Color.white : invisibleWhite);
        mossCollidersMap.SetColliderType(pos, posIsLit ? Tile.ColliderType.None : Tile.ColliderType.Grid);
    }

    private void UpdateMoss()
    {
        if (SGrid.Current != null && SGrid.Current as CaveGrid != null)
        {
            if (LightManager.instance != null)
            {
                ForEachMossTileIn(mossMap, (pos) =>
                {
                    UpdateMossTile(pos);
                });
            }
        }

        if (_playerMoveOffMoss.isActiveAndEnabled)
        {
            _playerMoveOffMoss.CheckPlayerCollidingWithMoss();
        }

        MossUpdated?.Invoke(this, new MossUpdatedArgs { stile = stile });
    }

    private void UpdateMossTile(Vector3Int pos)
    {
        bool posIsLit = LightManager.instance.GetLightMaskAt(mossMap, pos);

        Color darkAlpha = mossMap.GetColor(pos);
        Color litAlpha = recededMossMap.GetColor(pos);
        darkAlpha.a = Mathf.MoveTowards(darkAlpha.a, posIsLit ? 0f : 1f, mossFadeSpeed * Time.deltaTime);
        litAlpha.a = Mathf.MoveTowards(litAlpha.a, posIsLit ? 1f : 0f, mossFadeSpeed * Time.deltaTime);
        mossMap.SetColor(pos, darkAlpha);
        recededMossMap.SetColor(pos, litAlpha);

        if (litAlpha.a > colliderForgiveness)
        {
            mossCollidersMap.SetColliderType(pos, Tile.ColliderType.None);
        } else
        {
            mossCollidersMap.SetColliderType(pos, Tile.ColliderType.Grid);
        }
    }

    private void ForEachMossTileIn(Tilemap tm, Action<Vector3Int> func)
    {
        foreach (Vector3Int pos in mossMap.cellBounds.allPositionsWithin)
        {
            MossTile tile = mossMap.GetTile(pos) as MossTile;
            if (tile != null)
            {
                func(pos);
            }
        }
    }
}