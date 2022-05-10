using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Conveyor : ElectricalNode
{
    //Probably want to do an animation later instead of sprite swapping

    [SerializeField]
    private GameObject off;
    [SerializeField]
    private GameObject on;

    [SerializeField]
    private Vector2Int start;

    [SerializeField]
    private Vector2Int dir;

    [SerializeField]
    private int length; //This is probably going to be 1

    [SerializeField]
    private UIArtifact artifact;

    private new void Awake()
    {
        base.Awake();
        nodeType = NodeType.OUTPUT;

        //Just initializing to make sure they're not both active at the same time
        off.SetActive(!Powered);
        on.SetActive(Powered);

        artifact = UIArtifact.GetInstance();
    }

    private void OnEnable()
    {
        SGridAnimator.OnSTileMoveEnd += OnTileMove;
    }

    private void OnDisable()
    {
        SGridAnimator.OnSTileMoveEnd -= OnTileMove;
    }

    public override void OnPoweredHandler(OnPoweredArgs e)
    {
        off.SetActive(!e.powered);
        on.SetActive(e.powered);

        if (e.powered)
        {
            StartCoroutine(WaitForCurrentAndCheckForMove());
        }
    }

    private void OnTileMove(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        if (Powered)
        {
            StartCoroutine(WaitForCurrentAndCheckForMove());
        }
    }

    //This might cause race conditions for multiple conveyor belts affecting the same tiles, might need to revisit. (static ref. counter for Conveyors)
    private IEnumerator WaitForCurrentAndCheckForMove()
    {
        SMoveConveyor move = ConstructMove();
        if (move != null)
        {
            //A Move is going to happen on the conveyor belt.
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

            //Queue the move, then immediately unqueue it so that it becomes the next active move.
            artifact.QueueCheckAndAdd(move);
            artifact.QueueCheckAfterMove(this, null);

            //We need to update the UI since the move was initiated by the conveyor belt instead of the player clicking buttons.
            artifact.SetArtifactToGrid();
            artifact.UpdateMoveOptions();

            //Now that the UI has updated to reflect the conveyor move, we can reenable queueing for the player
            artifact.PlayerCanQueue = true;
        }

    }

    //This could be put in SMove.cs. Idk. I thought it made more sense here.
    private SMoveConveyor ConstructMove()
    {
        List<Movement> moves = new List<Movement>();
        STile[,] stiles = SGrid.current.GetGrid();


        //Loop until reaching the first non-active tile.
        int count = 0;
        Vector2Int curr = start;
        while (!stiles[curr.x, curr.y].isTileActive)
        {
            curr += dir;
            count++;

            //There's no tile on the conveyor belt.
            if (count >= length)
            {
                return null;
            }
        }

        Vector2Int moveStart = curr;

        List<Vector2Int> movingTiles = new List<Vector2Int>();
        List<Vector2Int> emptyTiles = new List<Vector2Int>();
        int moveLength = 0;
        bool passedFirstEmpty = false;
        while (moveLength < length && curr.x < SGrid.current.width && curr.y < SGrid.current.height)
        {
            if (stiles[curr.x, curr.y].isTileActive && !passedFirstEmpty)
            {
                //
                movingTiles.Add(curr);
            }
            else if (!stiles[curr.x, curr.y].isTileActive)
            {
                //There's space at the end of the conveyor for a move
                emptyTiles.Add(curr);
                moveLength++;
                passedFirstEmpty = true;
            } else
            {
                //Active tile after passing the first empty tile
                //NOTE: There is an edge case where if length is more than one, it could push into an empty square and then push into another chain, but our grids are small enough that this case doesn't occur.
                break;
            }

            curr += dir;
            if (!passedFirstEmpty && (curr.x >= SGrid.current.width || curr.y >= SGrid.current.height))
            {
                //Belt is locked, so this move isn't possible.
                return null;
            }
        }

        foreach (Vector2Int pos in movingTiles)
        {
            moves.Add(new Movement(pos, pos + dir * moveLength, stiles[pos.x, pos.y].islandId));
        }

        for (int i=0; i<moveLength; i++)
        {
            moves.Add(new Movement(emptyTiles[i], moveStart + dir * i, stiles[emptyTiles[i].x, emptyTiles[i].y].islandId));
        }

        return new SMoveConveyor(moves);
    }
}