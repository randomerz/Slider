using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Conveyor : ElectricalNode
{

    [SerializeField] private Vector2Int start;

    [SerializeField] private Vector2Int dir;

    [SerializeField] private int length;

    [SerializeField] private FactoryArtifact artifact;

    [SerializeField] private Animator animator;

    private bool _conveyorEnabled = true;

    public Vector2Int StartPos => start;
    public Vector2Int Dir => dir;

    public bool ConveyorEnabled
    {
        get {
            return _conveyorEnabled;
        }

        set
        {
            _conveyorEnabled = value;
            HandleConveyorPoweredStatus();
        }
    }

    private bool ConveyorPowered => ConveyorEnabled && Powered;
    private bool waitingToDoMove = false;

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

        animator.SetFloat("speed", ConveyorPowered ? 2 : 0);

        if (artifact == null)
        {
            artifact = UIArtifact.GetInstance() as FactoryArtifact;
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

    public override void OnPoweredHandler(OnPoweredArgs e)
    {
        base.OnPoweredHandler(e);
        HandleConveyorPoweredStatus();
    }

    private void HandleConveyorPoweredStatus()
    {
        animator.SetFloat("speed", ConveyorPowered ? 2 : 0);

        TryQueueConveyorMove();
    }

    private void OnTileEnabled(object sender, SGrid.OnSTileEnabledArgs e)
    {
        TryQueueConveyorMove();
    }

    private void OnTileMove(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        TryQueueConveyorMove();
    }

    private void TryQueueConveyorMove()
    {
        if (ConveyorPowered && !waitingToDoMove)
        {
            SMoveConveyor move = ConstructMove();
            if (move != null)
            {
                StartCoroutine(WaitOutOverlappingActiveMoves(move, QueueConveyorMove));
            }
        }
    }

    //This might cause race conditions for multiple conveyor belts affecting the same tiles, might need to revisit. (static ref. counter for Conveyors)
    private IEnumerator WaitOutOverlappingActiveMoves(SMove move, System.Action<SMoveConveyor> callback = null)
    {
        waitingToDoMove = true;

        //This is kinda hacky, but basically we're waiting a bit in case the conveyor is turned off right after a move (Indiana Jones puzzle)
        yield return new WaitForSeconds(0.2f);

        List<SMove> currActiveMoves = UIArtifact.GetActiveMoves();
        foreach (SMove activeMove in currActiveMoves)
        {
            if (activeMove.Overlaps(move))
            {
                while (currActiveMoves.Contains(activeMove))
                {
                    yield return null;
                }
                break;
            }
        }
        waitingToDoMove = false;

        if (callback != null)
        {
            callback(ConstructMove());  //Construct a brand new move because it might have changed! (ex: tile moving in front of conveyor, so have to push both tiles).
        }
    }

    private void QueueConveyorMove(SMoveConveyor move)
    {
        if (ConveyorPowered && move != null)
        {
            artifact.QueueMoveToFront(move);
        }
    }              

    //This method covers conveyor belts that stretch over multiple tiles as well as arbitrary grid size, which is a lot more than we needed lol.
    private SMoveConveyor ConstructMove()
    {
        List<Movement> moves = new List<Movement>();
        STile[,] stiles = SGrid.Current.GetGrid();
        int count = 0;
        Vector2Int curr = start;


        //Get to the start of the move
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
        while (moveLength < length && PosInSGridBounds(curr))
        {
            if (stiles[curr.x, curr.y].isTileActive && !passedFirstEmpty)
            {
                //Add all tiles on the conveyor belt and tiles pushed by those on the conveyor belt.
                movingTiles.Add(curr);
            }
            else if (!stiles[curr.x, curr.y].isTileActive)  //Check that there's space at the end of the conveyor belt.
            {
                
                emptyTiles.Add(curr);
                moveLength++;   
                passedFirstEmpty = true;
            } else
            {
                //Tile is active, and we've already passed at least one empty tile
                break;
            }

            curr += dir;
        }

        if (!passedFirstEmpty)
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

    private bool PosInSGridBounds(Vector2Int pos)
    {
        return pos.x >= 0 && pos.y >= 0 && pos.x < SGrid.Current.Width && pos.y < SGrid.Current.Height;
    }
}