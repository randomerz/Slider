using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Deprecated!
public class LostGuyMovement : MonoBehaviour
{
    private bool isBeaching;
    public bool hasBeached;
    public Transform BeachLocation;
    public SpriteRenderer raftSprite;
    public Sprite trackerSprite;
    public SGridAnimator animator;
    public GameObject fog6;
    public GameObject fog7;

    // Start is called before the first frame update
    void Start()
    {
        UITrackerManager.AddNewTracker(gameObject, trackerSprite);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnEnable()
    {
        SGrid.OnGridMove += UpdateMovement;
    }

    private void OnDisable()
    {
        SGrid.OnGridMove -= UpdateMovement;
    }

    public void UpdateMovement(object sender, SGrid.OnGridMoveArgs e)
    {
        if (hasBeached)
        {
            return;
        }

        STile curr = gameObject.GetComponentInParent<STile>();
       

        STile newSTile;
   
        newSTile = CanMove(e.grid, Vector2Int.down, curr);

        if (newSTile != null)
        {
            Move(newSTile, Vector2Int.down);
            return;
        }

        newSTile = CanMove(e.grid, Vector2Int.left, curr);

        if (newSTile != null)
        {
            Move(newSTile, Vector2Int.left);
            return;
        }

        newSTile = CanMove(e.grid, Vector2Int.up, curr);

        if (newSTile != null)
        {
            Move(newSTile, Vector2Int.up);
            return;
        }

        newSTile = CanMove(e.grid, Vector2Int.right, curr);

        if (newSTile != null)
        {
            Move(newSTile, Vector2Int.right);
            return;
        }


        
    }

    private Vector2Int GetGridPosition(STile[,] grid, STile curr)
    {
        for(int x = 0; x < SGrid.Current.Width; x++)
        {
            for(int y = 0; y < SGrid.Current.Height; y++)
            {
                if (grid[x,y].islandId == curr.islandId)
                {
                    return new Vector2Int(x, y);
                }
            }
        }

        return new Vector2Int(curr.x, curr.y);
    }

    private STile CanMove(STile[,] e, Vector2Int dir, STile original)
    {

        STile prev = original;
   
        int x = GetGridPosition(e, original).x;
        int y = GetGridPosition(e, original).y;

       

        x += dir.x;
        y += dir.y;

        

        if ((x < 0 || x >= SGrid.Current.Width))
        {
            return null;
        }

        if (y < 0 || y >= SGrid.Current.Height)
        {
            return null;
        }

        if (!e[x,y].isTileActive)
        {
            return null;
        }
        
        if (e[x, y].islandId == 9 || e[x, y].islandId == 2)         //lost guy cannot move onto tiles volcano and taver tile
        {
            return null;
        }

        if(e[x, y].islandId == 6 && fog6.activeSelf == false)         //lost guy cannot move onto the foggy island
        {
            return null;
        }

        if(e[x, y].islandId == 7 && fog7.activeSelf == false)         //lost guy cannot move onto the foggy island
        {
            return null;
        }

        if (e[x, y].islandId == 1 && dir == Vector2Int.left)        //lost guy cannot move to right starting beach tile if they are moving left
        {
            return null;
        }

        if (e[x, y].islandId == 8 && dir == Vector2Int.down)        //lost guy cannot move to top beach tile if they are moving down
        {
            return null;
        }

        if (original.islandId == 8 && dir == Vector2Int.up)         //lost guy cannot move up if they are starting on top beach tile
        {
            return null;
        }

        if (original.islandId == 1 && dir == Vector2Int.right)      //if lost guy is on the starting beach tile and is moving right they will be beached
        {
            isBeaching = true;
        }

        return e[x, y];
    }

    private void Move(STile end, Vector2 dir)
    {
        if (!isBeaching)
        {
            StartCoroutine(StartLostGuyAnimation(end, dir, animator.GetMovementDuration()));
        }

        else
        {
            gameObject.transform.position = BeachLocation.position;
            gameObject.transform.SetParent(BeachLocation);
            hasBeached = true;
            raftSprite.enabled = false;
        }
    }

    private IEnumerator StartLostGuyAnimation(STile end, Vector2 dir, float duration)
    {
        float t = 0;

        Vector3 firstTileEdge = (dir * end.STILE_WIDTH) / 2;

        yield return null;

        while (t < duration / 2) //movement from start to halfway
        {
            float percent = t / (duration / 2);
            Vector3 newPos = Vector3.Lerp(Vector3.zero, firstTileEdge, percent);
            gameObject.transform.localPosition = newPos;
            yield return null;
            t += Time.deltaTime;
        }

        Vector3 lastTileEdge = -firstTileEdge;
        gameObject.transform.SetParent(end.transform);

        while (t < duration) // movement from halfway to end
        {
            float percent = (t - (duration / 2)) / (duration / 2);
            Vector3 newPos = Vector3.Lerp(lastTileEdge, Vector3.zero, percent);
            gameObject.transform.localPosition = newPos;
            yield return null;
            t += Time.deltaTime;
        }

    }
}
