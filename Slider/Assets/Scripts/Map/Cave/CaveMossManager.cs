using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CaveMossManager : MonoBehaviour
{
    public Tilemap mossMap;
    public Tilemap recededMossMap;
    public Tilemap mossCollidersMap;


    public float mossFadeSpeed;
    public float playerBoopSpeed;

    public Transform player;
    public Transform playerRespawn;

    private BoundsInt mossBounds;

    private HashSet<Vector3Int> tilesAnimating;
    private bool movingPlayer;

    private void Start()
    {
        mossBounds = mossMap.cellBounds;
        tilesAnimating = new HashSet<Vector3Int>();
        player = GameObject.FindWithTag("Player").GetComponent<Transform>();
        movingPlayer = false;

        //L: Set moss transparencies initially
        if (LightManager.instance != null)
        {
            for (int x = mossBounds.x; x < mossBounds.xMax; x++)
            {
                for (int y = mossBounds.y; y < mossBounds.yMax; y++)
                {
                    Vector3Int pos = new Vector3Int(x, y, mossBounds.z);
                    MossTile tile = mossMap.GetTile(pos) as MossTile;
                    if (tile != null)
                    {
                        mossMap.SetTileFlags(pos, TileFlags.None);
                        recededMossMap.SetTileFlags(pos, TileFlags.None);

                        Vector3Int posInWorld = new Vector3Int(x + (int)transform.position.x, y + (int)transform.position.y, pos.z);
                        bool posIsLit = LightManager.instance.GetLightMaskAt(posInWorld.x, posInWorld.y);

                        if (posIsLit)
                        {
                            mossMap.SetColor(pos, new Color(1.0f, 1.0f, 1.0f, 0.0f));
                            recededMossMap.SetColor(pos, Color.white);
                            mossCollidersMap.SetColliderType(pos, Tile.ColliderType.None);
                        } else
                        {
                            mossMap.SetColor(pos, Color.white);
                            recededMossMap.SetColor(pos, new Color(1.0f, 1.0f, 1.0f, 0.0f));
                            mossCollidersMap.SetColliderType(pos, Tile.ColliderType.Grid);
                        }
                    }
                }
            }
        }
    }

    private void Update()
    {
        if (LightManager.instance != null)
        {
            for (int x = mossBounds.x; x < mossBounds.xMax; x++)
            {
                for (int y = mossBounds.y; y < mossBounds.yMax; y++)
                {
                    Vector3Int pos = new Vector3Int(x, y, mossBounds.z);
                    MossTile tile = mossMap.GetTile(pos) as MossTile;
                    if (tile != null)
                    {

                        Vector3Int posInWorld = new Vector3Int(x + (int) transform.position.x, y + (int) transform.position.y, pos.z);
                        bool posIsLit = LightManager.instance.GetLightMaskAt(posInWorld.x, posInWorld.y);
                        bool needsToAnimate = posIsLit ? mossMap.GetColor(pos).a > 0.5f : mossMap.GetColor(pos).a < 0.5f;

                        //L: Fade moss away.
                        if (!tilesAnimating.Contains(pos) && needsToAnimate)
                        {
                            StartCoroutine(GrowMoss(pos, posIsLit));
                        }
                    }
                }
            }
        }
    }

    public IEnumerator GrowMoss(Vector3Int pos, bool lit)
    {
        tilesAnimating.Add(pos);

        if (lit)
        {
            while (mossMap.GetColor(pos).a > 0.0f)
            {
                Color c = mossMap.GetColor(pos);
                mossMap.SetColor(pos, new Color(c.r, c.g, c.b, c.a - mossFadeSpeed));
                recededMossMap.SetColor(pos, new Color(c.r, c.g, c.b, 1 - (c.a - mossFadeSpeed)));
                Debug.Log(recededMossMap.GetColor(pos));
                yield return new WaitForSeconds(0.1f);
            }

            //L: Disable the moss collider
            mossCollidersMap.SetColliderType(pos, Tile.ColliderType.None);
        }
        else
        {
            Vector3Int posInWorld = new Vector3Int(pos.x + (int)transform.position.x, pos.y + (int)transform.position.y, pos.z);
            bool movePlayerOffMoss = (int)player.position.x == posInWorld.x && (int)player.position.y == posInWorld.y;

            if (movePlayerOffMoss && !movingPlayer)
            {
                StartCoroutine(MovePlayerOffMoss());
            }

            while (mossMap.GetColor(pos).a < 1.0f)
            {
                Color c = mossMap.GetColor(pos);
                mossMap.SetColor(pos, new Color(c.r, c.g, c.b, c.a + mossFadeSpeed));
                recededMossMap.SetColor(pos, new Color(c.r, c.g, c.b, 1 - (c.a + mossFadeSpeed)));

                yield return new WaitForSeconds(0.1f);
            }

            //L: Enable the moss collider
            mossCollidersMap.SetColliderType(pos, Tile.ColliderType.Grid);
        }

        tilesAnimating.Remove(pos);
    }

    public IEnumerator MovePlayerOffMoss()
    {
        movingPlayer = true;

        //Move player off the moss
        float t = 0.0f;
        while (t < 1.0f)
        {
            t += playerBoopSpeed;
            player.transform.position = Vector3.Lerp(player.transform.position, playerRespawn.position, t);
            yield return new WaitForSeconds(0.1f);
        }

        Debug.Log(t);

        Debug.Log(player.transform.position);
        Debug.Log(playerRespawn.position);
        movingPlayer = false;
    }
}