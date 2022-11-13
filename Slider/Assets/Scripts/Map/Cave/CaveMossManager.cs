using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CaveMossManager : MonoBehaviour
{
    public enum MossState { 
        GROWN,
        RECEDED,
    }

    [SerializeField] private Tilemap mossMap;
    [SerializeField] private Tilemap recededMossMap;
    public Tilemap mossCollidersMap;
    [SerializeField] private float mossFadeSpeed;
    [SerializeField] private STile stile;

    //L: Keep track of the tiles that are animating so that we are not calling the coroutine twice. 
    private Dictionary<Vector3Int, Coroutine> mossCoroutines = new Dictionary<Vector3Int, Coroutine>();
    private Dictionary<Vector3Int, MossState> mossStates = new Dictionary<Vector3Int, MossState>(); 
    private bool updatingMoss;

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
    }

    private void Start()
    {
        InitMoss();
    }

    private void OnEnable()
    {
        //Conditions under which the moss updates.
        SGridAnimator.OnSTileMoveEnd += UpdateMoss;
        SGrid.OnSTileEnabled += UpdateMoss;
        CaveLight.OnLightSwitched += UpdateMoss;
        LightManager.OnLightMaskChanged += UpdateMoss;
    }

    private void OnDisable()
    {
        //Conditions under which the moss updates.
        SGridAnimator.OnSTileMoveEnd -= UpdateMoss;
        SGrid.OnSTileEnabled -= UpdateMoss;
        CaveLight.OnLightSwitched -= UpdateMoss;
        LightManager.OnLightMaskChanged -= UpdateMoss;
    }

    private void Update()
    {
        if (updatingMoss && mossCoroutines.Count == 0)
        {
            MossUpdated?.Invoke(this, new MossUpdatedArgs { stile = stile });
            updatingMoss = false;
        }
    }

    private MossState GetStateFromLit(bool lit)
    {
        return lit ? MossState.RECEDED : MossState.GROWN;
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
    }

    private void SetMossState(Vector3Int pos, bool posIsLit)
    {
        Color invisibleWhite = new Color(1.0f, 1.0f, 1.0f, 0.0f);

        if (!mossStates.ContainsKey(pos))
        {
            mossStates.Add(pos, GetStateFromLit(posIsLit));
        } else
        {
            mossStates[pos] = GetStateFromLit(posIsLit);
        }

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
    }

    private void UpdateMossTile(Vector3Int pos)
    {
        bool posIsLit = LightManager.instance.GetLightMaskAt(mossMap, pos);

        if (!mossStates.ContainsKey(pos))
        {
            SetMossState(pos, posIsLit);
        }

        if (mossStates[pos] != GetStateFromLit(posIsLit))
        {
            mossStates[pos] = GetStateFromLit(posIsLit);

            if (mossCoroutines.ContainsKey(pos))
            {
                //L: The tile is animating the wrong way, stop the animation.
                if (mossCoroutines[pos] != null)
                {
                    StopCoroutine(mossCoroutines[pos]);
                }
                mossCoroutines.Remove(pos);
            }

            //L: The tile needs to animate and is not already animating in that direction.
            mossCoroutines.Add(pos, StartCoroutine(mossStates[pos] == MossState.GROWN ? GrowMoss(pos) : RecedeMoss(pos)));
            updatingMoss = true;
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

    private void UpdateMoss(object sender, System.EventArgs e)
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
        while (mossMap.GetColor(pos).a < 1.0f)
        {

            Color c = mossMap.GetColor(pos);
            mossMap.SetColor(pos, new Color(c.r, c.g, c.b, c.a + mossFadeSpeed));
            recededMossMap.SetColor(pos, new Color(c.r, c.g, c.b, 1 - (c.a + mossFadeSpeed)));
            yield return new WaitForSeconds(0.1f);
        }
        mossCoroutines.Remove(pos);
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
        mossCoroutines.Remove(pos);
    }
}