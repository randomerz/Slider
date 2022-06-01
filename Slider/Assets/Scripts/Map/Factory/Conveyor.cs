using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Conveyor : ElectricalNode
{
    //Probably want to do an animation later instead of sprite swapping

    [SerializeField] private Vector2Int start;

    [SerializeField] private Vector2Int dir;

    [SerializeField] private int length; //This is probably going to be 1 for all of them

    [SerializeField] private UIArtifact artifact;   //We need reference to the artifact for the queue and to update the UI

    [SerializeField] private Animator animator;

    private Coroutine gettingMoveCoroutine; //Only should have one coroutine running at a time.

    #region Unity Events
    private new void Awake()
    {
        base.Awake();
        nodeType = NodeType.OUTPUT;

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    private void Start()
    {

        animator.SetFloat("speed", Powered ? 2 : 0);

        if (artifact == null)
        {
            artifact = UIArtifact.GetInstance();
        }
    }

    private new void OnEnable()
    {
        base.OnEnable();
        SGridAnimator.OnSTileMoveEnd += OnTileMove;
        SGrid.OnSTileEnabled += OnTileEnabled;
    }

    private new void OnDisable()
    {
        base.OnDisable();
        SGridAnimator.OnSTileMoveEnd -= OnTileMove;
        SGrid.OnSTileEnabled -= OnTileEnabled;
    }
    #endregion

    public override void OnPoweredHandler(OnPoweredArgs e)
    {
        animator.SetFloat("speed", e.powered ? 2 : 0);

        if (e.powered && gettingMoveCoroutine == null)
        {
            gettingMoveCoroutine = StartCoroutine(WaitForCurrentAndCheckForMove());
        }
    }

    private void OnTileEnabled(object sender, SGrid.OnSTileEnabledArgs e)
    {
        if (Powered && gettingMoveCoroutine == null)
        {
            gettingMoveCoroutine = StartCoroutine(WaitForCurrentAndCheckForMove());
        }
    }

    private void OnTileMove(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        if (Powered && gettingMoveCoroutine == null)
        {
            gettingMoveCoroutine = StartCoroutine(WaitForCurrentAndCheckForMove());
        }
    }

    //This might cause race conditions for multiple conveyor belts affecting the same tiles, might need to revisit. (static ref. counter for Conveyors)
    private IEnumerator WaitForCurrentAndCheckForMove()
    {
        SMoveConveyor move = ConstructMove();
        if (move != null)
        {
            //Do not allow the player to queue any moves at this point!
            artifact.PlayerCanQueue = false;
            UIArtifact.ClearQueues();

            foreach (SMove activeMove in UIArtifact.GetActiveMoves())
            {
                if (activeMove.Overlaps(move))
                {
                    //We need to make sure that none of the active moves are interfering with the conveyor belt moves.
                    //Wait out the active moves
                    while (UIArtifact.ActiveMovesExist())
                    {
                        yield return null;
                    }
                    break;
                }
            }

            //This is kinda hacky, but basically we're waiting a bit in case the conveyor is turned off right after a move (Indiana Jones)
            yield return new WaitForSeconds(0.2f);
            if (Powered)
            {
                //Queue the move, then immediately unqueue it so that it becomes the next active move.
                artifact.QueueCheckAndAdd(move);
                artifact.QueueCheckAfterMove(this, null);

                //We need to update the UI since the move was initiated by the conveyor belt instead of the player clicking buttons.
                artifact.SetArtifactToGrid();
                artifact.UpdateMoveOptions();


            }
            //Now that the UI has updated to reflect the conveyor move, we can reenable queueing for the player
            artifact.PlayerCanQueue = true;
        }

        gettingMoveCoroutine = null;
    }

    //This could be put in SMove.cs. Idk. I thought it made more sense here.
    //This method covers conveyor belts that stretch over multiple tiles as well as arbitrary grid size, which is a lot more than we needed lol.
    private SMoveConveyor ConstructMove()
    {
        List<Movement> moves = new List<Movement>();
        STile[,] stiles = SGrid.current.GetGrid();


        //Check if a tile is on the conveyor belt
        int count = 0;
        Vector2Int curr = start;
        while (!stiles[curr.x, curr.y].isTileActive)
        {
            curr += dir;
            count++;


            if (count >= length)
            {
                //There's no tile on the conveyor belt.
                return null;
            }
        }

        Vector2Int moveStart = curr;

        List<Vector2Int> movingTiles = new List<Vector2Int>();
        List<Vector2Int> emptyTiles = new List<Vector2Int>();
        int moveLength = 0;
        bool passedFirstEmpty = false;
        while (moveLength < length && curr.x >= 0 && curr.y >= 0 && curr.x < SGrid.current.width && curr.y < SGrid.current.height)
        {
            if (stiles[curr.x, curr.y].isTileActive && !passedFirstEmpty)
            {
                //Add all tiles on the conveyor belt and tiles pushed by those on the conveyor belt.
                movingTiles.Add(curr);
            }
            else if (!stiles[curr.x, curr.y].isTileActive)
            {
                //Check that there's space at the end of the conveyor belt.
                //This only happens once for length 1 conveyors (i.e. Factory)
                emptyTiles.Add(curr);
                moveLength++;   
                passedFirstEmpty = true;
            } else
            {
                //Tile is active, and we've already passed at least one empty tile
                //NOTE: There is an edge case where if length is more than one, it could push into an empty square and then push into another chain, but our grids are small enough that this case doesn't occur.
                break;
            }

            curr += dir;
        }

        if (!passedFirstEmpty) //&& (curr.x >= SGrid.current.width || curr.y >= SGrid.current.height || curr.x < 0 || curr.y < 0))
        {
            //Belt is locked, so this move isn't possible.
            return null;
        }

        foreach (Vector2Int pos in movingTiles)
        {
            moves.Add(new Movement(pos, pos + dir * moveLength, stiles[pos.x, pos.y].islandId));
        }

        //The empty tiles are put at the start of the conveyor belt. (again, for Factory, there's only 1 possible empty tile.)
        for (int i=0; i<moveLength; i++)
        {
            moves.Add(new Movement(emptyTiles[i], moveStart + dir * i, stiles[emptyTiles[i].x, emptyTiles[i].y].islandId));
        }

        return new SMoveConveyor(moves);
    }
}