using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CaveMossManager : MonoBehaviour
{
    internal class MossAnimData
    {
        public Coroutine Animation { get; }
        public bool IsGrowing { get; }

        public MossAnimData(Coroutine animation, bool isGrowing)
        {
            this.Animation = animation;
            this.IsGrowing = isGrowing;
        }
    }

    //L: For now, the moss manager handles 3 tilemaps: The moss map, the receded moss map, and the corresponding colliders.
    //L: Alternatively, could have a list of triplets if we only wanted to have one manager in a scene.
    public Tilemap mossMap;
    public Tilemap recededMossMap;
    public Tilemap mossCollidersMap;

    public PlayerMoveOffMoss playerTP;


    public float mossFadeSpeed;

    private BoundsInt mossBounds;

    //L: Keep track of the tiles that are animating so that we are not calling the coroutine twice. 
    private Dictionary<Vector3Int, MossAnimData> tilesAnimating;

    [SerializeField]
    private STile stile;

    [SerializeField]
    private STile debugTile;    //L: In case you need to debug a specific tile

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
    public static event System.EventHandler<MossIsGrowingArgs> MossIsGrowing;

    public static event System.EventHandler<MossUpdatedArgs> MossUpdated;

    private bool updating;

    private void Awake()
    {
        if (stile == null)
        {
            stile = GetComponentInParent<CaveSTile>();
        }
    }

    private void Start()
    {
        mossBounds = mossMap.cellBounds;
        tilesAnimating = new Dictionary<Vector3Int, MossAnimData>();

        //L: Initialize all moss tiles
        if (LightManager.instance != null)
        {
            ForEachMossTileIn(mossMap, (pos) =>
            {
                mossMap.SetTileFlags(pos, TileFlags.None);
                recededMossMap.SetTileFlags(pos, TileFlags.None);

                if (GetComponentInParent<STile>() == debugTile)
                {
                    //Debug.Log("Position of Tile: " + pos);
                    //Debug.Log("Position of Tile in World: " + (mossMap.CellToWorld(pos) + new Vector3(0.5f, 0.5f, 0)));
                }

                bool posIsLit = LightManager.instance.GetLightMaskAt(mossMap, pos);

                if (posIsLit)
                {
                    //L: mossMap -> Off, recededMossMap -> On, Colliders -> Off
                    mossMap.SetColor(pos, new Color(1.0f, 1.0f, 1.0f, 0.0f));
                    recededMossMap.SetColor(pos, Color.white);
                    mossCollidersMap.SetColliderType(pos, Tile.ColliderType.None);
                    MossIsGrowing?.Invoke(this, new MossIsGrowingArgs { stile = stile, mossMap = mossCollidersMap, cellPos = pos, isGrowing = false });
                }
                else
                {
                    //L: mossMap -> On, recededMossMap -> Off, Colliders -> On
                    mossMap.SetColor(pos, Color.white);
                    recededMossMap.SetColor(pos, new Color(1.0f, 1.0f, 1.0f, 0.0f));
                    mossCollidersMap.SetColliderType(pos, Tile.ColliderType.Grid);
                    MossIsGrowing?.Invoke(this, new MossIsGrowingArgs { stile = stile, mossMap = mossCollidersMap, cellPos = pos, isGrowing = true });
                }
            });
        }
    }

    private void OnEnable()
    {
        //Conditions under which the moss updates.
        SGridAnimator.OnSTileMoveEnd += UpdateMoss;
        SGrid.OnSTileEnabled += UpdateMoss;
        CaveLight.OnLightSwitched += UpdateMoss;
    }

    private void OnDisable()
    {
        //Conditions under which the moss updates.
        SGridAnimator.OnSTileMoveEnd -= UpdateMoss;
        SGrid.OnSTileEnabled -= UpdateMoss;
        CaveLight.OnLightSwitched -= UpdateMoss;
    }

    private void Update()
    {
        if (updating && tilesAnimating.Count == 0)
        {
            MossUpdated?.Invoke(this, new MossUpdatedArgs { stile = stile });
            updating = false;
        }
    }

    private void UpdateMoss()
    {
        if (SGrid.current != null && SGrid.current as CaveGrid != null)
        {
            if (LightManager.instance != null)
            {
                ForEachMossTileIn(mossMap, (pos) =>
                {

                    bool posIsLit = LightManager.instance.GetLightMaskAt(mossMap, pos);
                    bool needsToAnimate = posIsLit ? mossMap.GetColor(pos).a > 0.5f : mossMap.GetColor(pos).a < 0.5f;

                    if (needsToAnimate)
                    {
                        /*
                        if (tilesAnimating.ContainsKey(pos) && tilesAnimating[pos].IsGrowing == posIsLit)
                        {
                            //L: The tile is animating the wrong way, stop the animation.
                            StopCoroutine(tilesAnimating[pos].Animation);
                            tilesAnimating.Remove(pos);
                        }
                        */

                        if (!tilesAnimating.ContainsKey(pos) && needsToAnimate)
                        {
                            //L: The tile needs to animate and is not already animating in that direction.
                            tilesAnimating.Add(pos, new MossAnimData(StartCoroutine(posIsLit ? RecedeMoss(pos) : GrowMoss(pos)), !posIsLit));
                            updating = true;
                        }
                    }
                });
            }
        }
    }

    private void UpdateMoss(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        UpdateMoss();
    }

    private void UpdateMoss(object sender, SGrid.OnSTileEnabledArgs e)
    {
        UpdateMoss();
    }

    private void UpdateMoss(object sender, CaveLight.OnLightSwitchedArgs e)
    {
        UpdateMoss();
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

    public IEnumerator GrowMoss(Vector3Int pos)
    {
        //L: Enable the moss collider
        mossCollidersMap.SetColliderType(pos, Tile.ColliderType.Grid);
        MossIsGrowing?.Invoke(this, new MossIsGrowingArgs { stile = stile, mossMap = mossCollidersMap, cellPos = pos, isGrowing = true });

        while (mossMap.GetColor(pos).a < 1.0f)
        {

            Color c = mossMap.GetColor(pos);
            mossMap.SetColor(pos, new Color(c.r, c.g, c.b, c.a + mossFadeSpeed));
            recededMossMap.SetColor(pos, new Color(c.r, c.g, c.b, 1 - (c.a + mossFadeSpeed)));
            yield return new WaitForSeconds(0.1f);
        }
        tilesAnimating.Remove(pos);
    }

    public IEnumerator RecedeMoss(Vector3Int pos)
    {
        while (mossMap.GetColor(pos).a > 0.0f)
        {
            Color c = mossMap.GetColor(pos);
            mossMap.SetColor(pos, new Color(c.r, c.g, c.b, c.a - mossFadeSpeed));
            recededMossMap.SetColor(pos, new Color(c.r, c.g, c.b, 1 - (c.a - mossFadeSpeed)));
            yield return new WaitForSeconds(0.1f);
        }

        //L: Disable the moss collider
        mossCollidersMap.SetColliderType(pos, Tile.ColliderType.None);
        tilesAnimating.Remove(pos);

        MossIsGrowing?.Invoke(this, new MossIsGrowingArgs { stile = stile, mossMap = mossCollidersMap, cellPos = pos, isGrowing = false });
    }
}