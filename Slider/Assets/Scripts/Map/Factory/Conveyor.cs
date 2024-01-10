using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Conveyor : ElectricalNode
{
    [SerializeField] private Vector2Int start;

    [SerializeField] private Vector2Int dir;

    [SerializeField] private int length;

    [SerializeField] private FactoryArtifact artifact;
    [SerializeField] private ConveyorOverrideHandler conveyorOverride;

    [SerializeField] private List<Animator> animators;

    public Vector2Int StartPos => start;
    public Vector2Int Dir => dir;

    private bool waitingToQueueAndFinishMove = false;

    private new void Awake()
    {
        base.Awake();
        nodeType = NodeType.OUTPUT;
    }

    private void Start()
    {
        animators.ForEach((a) => {
            a.SetFloat("speed", ConveyorIsPoweredAndActive() ? 2 : 0);
        });

        if (artifact == null)
        {
            artifact = UIArtifact.GetInstance() as FactoryArtifact;
        }
    }

    private new void OnEnable()
    {
        base.OnEnable();
        SGridAnimator.OnSTileMoveEndEarly += OnSTileMoveEndEarly;
        SGrid.OnSTileEnabled += OnSTileEnabled;
    }

    private new void OnDisable()
    {
        base.OnDisable();
        SGridAnimator.OnSTileMoveEndEarly -= OnSTileMoveEndEarly;
        SGrid.OnSTileEnabled -= OnSTileEnabled;
    }

    private new void Update()
    {
        base.Update();
        UpdateConveyorStatus();
    }

    public override void OnPoweredHandler(OnPoweredArgs e)
    {
        base.OnPoweredHandler(e);
    }

    public bool ConveyorIsPoweredAndActive()
    {
        return Powered && !PowerCrystal.Blackout && !FactoryGrid.PlayerInPast && !DebugUIManager.disableConveyers && !OverrideActive();
    }

    private void UpdateConveyorStatus()
    {
        animators.ForEach((a) => {
            a.SetFloat("speed", ConveyorIsPoweredAndActive() ? 2 : 0);
        });

        TryQueueConveyorMove();
    }

    public bool OverrideActive()
    {
        return conveyorOverride.IsOn;
    }

    private void OnSTileEnabled(object sender, SGrid.OnSTileEnabledArgs e)
    {
        TryQueueConveyorMove();
    }

    private void OnSTileMoveEndEarly(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        TryQueueConveyorMove();
    }

    private void TryQueueConveyorMove()
    {
        if (ConveyorIsPoweredAndActive() && !waitingToQueueAndFinishMove)
        {
            SMoveConveyor move = ConstructMove();
            if (move != null)
            {
                waitingToQueueAndFinishMove = true;
                StartCoroutine(WaitUntilCanQueueMoveSafely(move, QueueConveyorMove));
            }
        }
    }

    public IEnumerator WaitUntilCanQueueMoveSafely(SMove move, System.Action callback = null)
    {
        if (FactoryArtifact.DequeueLocked)
        {
            yield return new WaitUntil(() => !FactoryArtifact.DequeueLocked);   //Mutex locks, woo hoo!
        }
        FactoryArtifact.DequeueLocked = true;

        List<SMove> activeMovesWhenCalled = new List<SMove>(UIArtifact.GetActiveMoves());
        foreach (SMove activeMove in activeMovesWhenCalled)
        {
            if (activeMove.Overlaps(move))
            {
                while (UIArtifact.GetActiveMoves().Contains(activeMove))
                {
                    yield return null;
                }
                break;
            }
        }

        if (callback != null)
        {
            callback();
        }
    }

    private void QueueConveyorMove()
    {
        //Construct a brand new move because it might have changed! (ex: tile moving in front of conveyor, so have to push both tiles).
        SMoveConveyor newMove = ConstructMove();
        if (ConveyorIsPoweredAndActive() && newMove != null)
        {
            artifact.QueueMoveToFront(newMove);
        }
        waitingToQueueAndFinishMove = false;
        FactoryArtifact.DequeueLocked = false;  //Make sure the key is released, even if we didn't do the move, or else we will spinlock.
        UIArtifact.GetInstance().ProcessQueue();
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
        for (int i = 0; i < moveLength; i++)
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