using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RatAI : MonoBehaviour
{
    [Header("Player")]
    public float playerAggroRange;
    public float playerDeaggroRange;

    [Header("Pathfinding")]
    [SerializeField]
    internal WorldNavigation nav;
    [SerializeField]
    internal WorldNavAgent navAgent;

    //Misc AI control weights
    //Note: In terms of the AI, darkness is treated the same as walls.
    /*
    [Header("Weights")]
    [SerializeField]
    internal float minDistToWall;   //Determines at what distance the AI is allowed to be near a wall before moving in a different direction (scaled by moveSpeed)
    [SerializeField]
    internal float idealDistFromWall; //The ideal distance the AI wants to keep from walls (this is essentially the rat's "vision")
    [SerializeField]
    [Range(0f, 1f)]
    internal float avoidWallsWeight;    //The tendency for the rat to avoid walls (0 to 1) (Note: This should be small, or else you get weird behaviour on the edge cases.
    [SerializeField]
    [Range(-1f, 1f)]
    internal float decisiveness;    //The tendency for the rat to keep moving in the direction it's facing
    */

    [Header("Internal References")]
    [SerializeField]
    private Animator anim;
    [SerializeField]
    public Rigidbody2D rb;
    [SerializeField]
    private Transform mouth;

    [Header("External References")]
    [SerializeField]
    internal GameObject objectToSteal;
    [SerializeField]
    internal Transform player;


    [Header("Cost Map Calculations")]
    [SerializeField]
    internal int tileMaxPenalty = 100;
    //These values are REALLY important for optimization, so don't make them too high!
    //Max dist vision optimally should be about 2 times player aggro range
    [SerializeField]
    internal float maxDistVision = 2f;
    [SerializeField]
    internal float maxDistCost = 2f;
    [SerializeField]
    internal float updateTimer = 0.25f;

    internal bool holdingObject;
    private STile currentStileUnderneath;
    private BehaviourTreeNode behaviourTree;

    private Vector2 _dirFacing;
    public Vector2 DirectionFacing
    {
        get
        {
            return _dirFacing;
        }
    }

    //Costs for running away from player
    public Dictionary<Vector2Int, int> CostMap
    {
        get
        {
            return _costMap;
        }
    }
    private Dictionary<Vector2Int, int> _costMap = null;

    [HideInInspector]
    internal HashSet<Vector2Int> visited = null;    //For debugging

    private void Awake()
    {

        if (objectToSteal == null)
        {
            Debug.LogWarning("Rat does not have reference to slider piece");
        }

        if (player == null)
        {
            Debug.LogError("Rat does not have reference to player.");
        }

        nav = GetComponentInParent<WorldNavigation>();
        if (nav == null)
        {
            Debug.LogError("Rat requires a WorldNavigation component in the root of the scene.");
        }

        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
        navAgent = GetComponent<WorldNavAgent>();
    }

    private void Start()
    {
        ConstructBehaviourTree();
        StealPiece();
    }

    private void Update()
    {
        behaviourTree.Evaluate();

        GenerateCostMap();

        if (behaviourTree.State == BehaviourTreeNode.NodeState.FAILURE)
        {
            navAgent.StopPath();
        }
    }

    private void FixedUpdate()
    {
        // updating childing
        UpdateStileUnderneath();
        // Debug.Log("Currently on: " + currentStileUnderneath);

        if (currentStileUnderneath != null)
        {
            transform.SetParent(currentStileUnderneath.transform);
        }
        else
        {
            transform.SetParent(GameObject.Find("World Grid").transform);
        }

        anim.SetFloat("speed", rb.velocity.magnitude);
    }

    private void OnEnable()
    {
        CaveMossManager.MossIsGrowing += DieOnMoss;
    }

    private void OnDisable()
    {
        CaveMossManager.MossIsGrowing -= DieOnMoss;
    }

    public void SetDirection(Vector2 dir)
    {
        _dirFacing = dir.normalized;
        transform.up = new Vector3(_dirFacing.x, _dirFacing.y, 0);
    }

    private void DieOnMoss(object sender, CaveMossManager.MossIsGrowingArgs e)
    {
        Vector2Int posAsInt = TileUtil.WorldToTileCoords(transform.position);
        if (posAsInt.Equals((Vector2Int) e.pos))
        {
            Die();
        }
    }

    public void Die()
    {
        //Play Death Animation

        if (holdingObject && objectToSteal != null)
        {
            objectToSteal.transform.parent = transform.parent;  //"Unparent" The Rat from the object so the Rat "Drops" it
        }
        Destroy(gameObject);
    }

    private void StealPiece()
    {
        //L: Reparent Slider piece to be child of Rat
        holdingObject = true;
        objectToSteal.transform.parent = transform;
        objectToSteal.transform.localPosition = mouth.localPosition;
    }

    private void ConstructBehaviourTree()
    {
        var setDestToObjectNode = new SetDestToObjectNode(this);

        var playerAggroNode = new AggroAtProximityNode(transform, player, playerAggroRange, playerDeaggroRange);
        var setDestToAvoidPlayerNode = new SetDestToAvoidPlayerNode(this);

        var setDestToLightTileNode = new SetDestToLightTileNode(this);

        var moveTowardsSetDestNode = new MoveTowardsSetPosNode(this, updateTimer);

        var stayInPlaceNode = new DoNothingNode(this);


        //L: IMPORTANT NOTE: The ordering of the nodes in the tree matters
        var stealSequence = new SequenceNode(new List<BehaviourTreeNode>() { setDestToObjectNode, moveTowardsSetDestNode });
        var runFromPlayerSequence = new SequenceNode(new List<BehaviourTreeNode> { playerAggroNode, setDestToAvoidPlayerNode, moveTowardsSetDestNode});
        var runToLightSequence = new SequenceNode(new List<BehaviourTreeNode> { setDestToLightTileNode, moveTowardsSetDestNode });

        behaviourTree = new SelectorNode(new List<BehaviourTreeNode> { stealSequence, runFromPlayerSequence, runToLightSequence, stayInPlaceNode });
    }

    //Efficiency: (2*maxDistVision+1)^2 * (2*maxDistCostmap+1)^2 (This is the most costly operation in the AI)
    private void GenerateCostMap()
    {
        if (LightManager.instance != null)
        {
            _costMap = new Dictionary<Vector2Int, int>();
            Vector2Int posAsInt = TileUtil.WorldToTileCoords(transform.position);
            //Square that includes Rat vision (which itself is a circle)
            for (int x = (int)-maxDistVision; x <= (int)maxDistVision; x++)
            {
                for (int y = (int)-maxDistVision; y <= (int)maxDistVision; y++)
                {
                    Vector2Int pos = posAsInt + new Vector2Int(x, y);
                    if (nav.IsValidPt(pos) && LightManager.instance.GetLightMaskAt(pos.x, pos.y))
                    {
                        _costMap.Add(pos, CostToThreat(GetDistToNearestBadTile(pos), false));
                    }
                }
            }
        }

        Debug.Assert(_costMap != null, "Tried to initialize Cost Map before LightManager. This might be a problem.");
    }

    internal int CostToThreat(float distToThreat, bool threatIsPlayer)
    {
        float penaltyDivider = threatIsPlayer ? playerAggroRange : maxDistCost;
        int cost = (distToThreat == float.MaxValue) ? 0 : Mathf.Clamp(tileMaxPenalty - (int)(tileMaxPenalty / penaltyDivider * (distToThreat - 1f)), 0, tileMaxPenalty);

        cost *= threatIsPlayer ? 1000 : 1; //Basically make the player as unappealing as possible (because the Rat loses if it touches the player)

        //Debug.Log("Distance: " + distToThreat);
        //Debug.Log("Cost: " + cost);
        return cost;
    }

    //This algorithm essentially checks the given pos, it's neighbors, the neighbors' neighbors, and so on moving outwards from the original pos.
    //Efficiency: (2*maxDistCost+1)^2
    private float GetDistToNearestBadTile(Vector2Int posAsInt)
    {
        float distToNearestObstacle = float.MaxValue;
        Vector2Int[] neighborDirs = { Vector2Int.up, Vector2Int.left, Vector2Int.down, Vector2Int.right,
                                      new Vector2Int(1, 1), new Vector2Int(1, -1), new Vector2Int(-1, 1), new Vector2Int(-1, -1) };

        float dist = 0f;
        var queue = new Queue<Vector2Int>();
        var visited = new HashSet<Vector2Int>();
        visited.Add(posAsInt);
        queue.Enqueue(posAsInt);
        while (queue.Count > 0 && dist < maxDistCost)
        {
            Vector2Int currPos = queue.Dequeue();

            foreach (var dir in neighborDirs)
            {
                Vector2Int posToCheck = currPos + dir;
                if (!visited.Contains(posToCheck))
                {
                    float distToPoint = Vector2Int.Distance(posAsInt, currPos);
                    dist = Mathf.Max(dist, distToPoint);
                    visited.Add(posToCheck);
                    queue.Enqueue(posToCheck);

                    //Check wall, darkness, or player occupation
                    if (!nav.IsValidPt(posToCheck) || !LightManager.instance.GetLightMaskAt(posToCheck.x, posToCheck.y) && distToPoint < distToNearestObstacle)
                    {
                        distToNearestObstacle = distToPoint;
                    }
                }

            }
        }

        return distToNearestObstacle;
    }

    // DC: a better way of calculating which stile the player is on, accounting for overlapping stiles
    private void UpdateStileUnderneath()
    {
        // this doesnt work when you queue a move and stand at the edge. for some reason, on the moment of impact hits does not overlap with anything??
        // Collider2D[] hits = Physics2D.OverlapPointAll(_instance.transform.position, LayerMask.GetMask("Slider"));
        // Debug.Log("Hit " + hits.Length + " at " + _instance.transform.position);

        // STile stileUnderneath = null;
        // for (int i = 0; i < hits.Length; i++)
        // {
        //     STile s = hits[i].GetComponent<STile>();
        //     if (s != null && s.isTileActive)
        //     {
        //         if (currentStileUnderneath != null && s.islandId == currentStileUnderneath.islandId)
        //         {
        //             // we are still on top of the same one
        //             return;
        //         }
        //         if (stileUnderneath == null)
        //         {
        //             // otherwise we only care about the first hit
        //             stileUnderneath = s;
        //         }
        //     }
        // }
        // currentStileUnderneath = stileUnderneath;

        STile[,] grid = SGrid.current.GetGrid();
        float offset = grid[0, 0].STILE_WIDTH / 2f;
        float housingOffset = -150;

        STile stileUnderneath = null;
        foreach (STile s in grid)
        {
            if (s.isTileActive && IsInSTileBounds(s.transform.position, offset, housingOffset))
            {
                if (currentStileUnderneath != null && s.islandId == currentStileUnderneath.islandId)
                {
                    // we are still on top of the same one
                    return;
                }

                if (stileUnderneath == null || s.islandId < stileUnderneath.islandId)
                {
                    // in case where multiple overlap and none are picked, take the lowest number?
                    stileUnderneath = s;
                }
            }
        }

        currentStileUnderneath = stileUnderneath;
    }

    private bool IsInSTileBounds(Vector3 stilePos, float offset, float housingOffset)
    {
        Vector3 pos = transform.position;
        if (stilePos.x - offset < pos.x && pos.x < stilePos.x + offset &&
           (stilePos.y - offset < pos.y && pos.y < stilePos.y + offset ||
            stilePos.y - offset + housingOffset < pos.y && pos.y < stilePos.y + offset + housingOffset))
        {
            return true;
        }
        return false;
    }

    private void OnDrawGizmosSelected()
    {
        /*
        if (visited != null)
        {
            foreach (Vector2Int pt in visited)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(new Vector3(pt.x, pt.y, 0), 0.2f);
            }
        }
        */
        
        if (CostMap != null)
        {
            foreach (Vector2Int pt in CostMap.Keys)
            {
                if (CostMap[pt] == int.MaxValue)
                {
                    Gizmos.color = Color.red;
                } else
                {
                    //Debug.Log($"CostMap in OnDrawGizmosSelected: {CostMap[pt]}");
                    Gizmos.color = Color.Lerp(Color.green, Color.red, (float) CostMap[pt] / tileMaxPenalty);
                }

                Gizmos.DrawSphere(new Vector3(pt.x, pt.y, 0), 0.2f);
            }
        }
        
    }
}