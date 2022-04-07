using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RatAI : MonoBehaviour
{
    [Header("Player")]
    public float playerAggroRange;
    public float playerDeaggroRange;

    [Header("Pathfinding")]
    public float moveSpeed;
    public float pathfindingTolerance;
    [SerializeField]
    internal STileNavAgent navAgent;

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
    public GameObject objectToSteal;
    public Transform player;

    internal bool holdingObject;

    private BehaviourTreeNode behaviourTree;

    private Vector2 _dirFacing;
    public Vector2 DirectionFacing
    {
        get
        {
            return _dirFacing;
        }
    }

    [HideInInspector]
    internal HashSet<Vector2Int> visited = null;    //For debugging


    //Costs for running away from player
    public Dictionary<Vector2Int, int> CostMap
    {
        get
        {
            if (_costMap == null)
            {
                GenerateCostMap();
            }
            return _costMap;
        }
    }
    private Dictionary<Vector2Int, int> _costMap = null;

    internal const int tileMaxPenalty = 100;
    internal const float maxDistCostmap = 3f;
    internal const float maxDistVision = 2f;

    private void Awake()
    {

        if (objectToSteal == null)
        {
            Debug.LogError("Rat does not have reference to slider piece");
        }

        if (player == null)
        {
            Debug.LogError("Rat does not have reference to player.");
        }

        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
        navAgent = GetComponent<STileNavAgent>();

        navAgent.speed = moveSpeed;
        navAgent.tolerance = pathfindingTolerance;

    }

    private void Start()
    {
        GenerateCostMap();
        ConstructBehaviourTree();
        StealPiece();
    }

    private void Update()
    {
        behaviourTree.Evaluate();
        if (behaviourTree.State == BehaviourTreeNode.NodeState.FAILURE)
        {
            Stay();
        }
    }

    private void OnEnable()
    {
        WorldNavigation.OnValidPtsChanged += CostMapEventHandler;
        LightManager.OnLightMaskChanged += CostMapEventHandler;
    }

    private void OnDisable()
    {
        WorldNavigation.OnValidPtsChanged -= CostMapEventHandler;
        LightManager.OnLightMaskChanged -= CostMapEventHandler;
    }

    private void CostMapEventHandler(object sender, System.EventArgs e)
    {
        GenerateCostMap();
    }

    public void SetDirection(Vector2 dir)
    {
        _dirFacing = dir.normalized;
        transform.up = new Vector3(_dirFacing.x, _dirFacing.y, 0);
    }

    public void Move()
    {
        rb.velocity = transform.up * moveSpeed;
        anim.SetFloat("speed", moveSpeed);
    }

    public void Stay()
    {
        rb.velocity = Vector2.zero;
        anim.SetFloat("speed", 0f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject == objectToSteal)
        {
        }
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
        var moveTowardsSetDestNode = new MoveTowardsSetPosNode(this);

        var playerAggroNode = new AggroAtProximityNode(transform, player, playerAggroRange, playerDeaggroRange);
        //var moveAwayFromPlayerNode = new MoveAwayFromPlayerNode(this, player);

        var setDestToAvoidPlayerNode = new SetDestToAvoidPlayerNode(this);
        var setDestToLightTileNode = new SetDestToLightTileNode(this);

        var stayInPlaceNode = new StayInPlaceNode(this);


        //L: IMPORTANT NOTE: The ordering of the nodes in the tree matters
        var stealSequence = new SequenceNode(new List<BehaviourTreeNode>() { setDestToObjectNode, moveTowardsSetDestNode });
        var runFromPlayerSequence = new SequenceNode(new List<BehaviourTreeNode> { playerAggroNode, setDestToAvoidPlayerNode, moveTowardsSetDestNode});
        var runToLightSequence = new SequenceNode(new List<BehaviourTreeNode> { setDestToLightTileNode, moveTowardsSetDestNode });

        behaviourTree = new SelectorNode(new List<BehaviourTreeNode> { stealSequence, runFromPlayerSequence, runToLightSequence, stayInPlaceNode });
    }

    private Dictionary<Vector2Int, int> GenerateCostMap()
    {
        var nav = GetComponentInParent<WorldNavigation>();
        HashSet<Vector2Int> validPts = nav.ValidPts;

        _costMap = new Dictionary<Vector2Int, int>();
        foreach (var pt in validPts)
        {
            if (LightManager.instance != null && LightManager.instance.GetLightMaskAt(pt.x, pt.y))
            {
                _costMap.Add(pt, CostToThreat(GetDistToNearestBadTile(pt)));
            }
        }
        return _costMap;
    }

    internal static int CostToThreat(float distToThreat)
    {
        int cost = Mathf.Clamp(tileMaxPenalty - (int)(tileMaxPenalty / maxDistCostmap * (distToThreat - 1f)), 0, tileMaxPenalty);
        //Debug.Log("Distance: " + distToThreat);
        //Debug.Log("Cost: " + cost);
        return cost;
    }

    //This algorithm essentially checks the given pos, it's neighbors, the neighbors' neighbors, and so on moving outwards from the original pos.
    private float GetDistToNearestBadTile(Vector2Int posAsInt)
    {
        WorldNavigation nav = GetComponentInParent<WorldNavigation>();

        float dist = 0f;
        const float maxDistCheck = tileMaxPenalty / 10f + 1f;

        var queue = new Queue<Vector2Int>();
        var visited = new HashSet<Vector2Int>();
        visited.Add(posAsInt);
        queue.Enqueue(posAsInt);
        while (queue.Count > 0 && dist < maxDistCheck)   //worst case scenario it's in the corner and has to check up to the opposite corner
        {
            Vector2Int currPos = queue.Dequeue();

            Vector2Int[] neighborDirs = { Vector2Int.up, Vector2Int.left, Vector2Int.down, Vector2Int.right,
                                      new Vector2Int(1, 1), new Vector2Int(1, -1), new Vector2Int(-1, 1), new Vector2Int(-1, -1) };

            foreach (var dir in neighborDirs)
            {
                Vector2Int posToCheck = currPos + dir;
                dist = Vector2Int.Distance(posAsInt, posToCheck);
                if (!visited.Contains(posToCheck))
                {
                    visited.Add(posToCheck);
                    queue.Enqueue(posToCheck);

                    //Check wall, darkness, or player occupation
                    if (!nav.ValidPts.Contains(posToCheck) || !LightManager.instance.GetLightMaskAt(posToCheck.x, posToCheck.y))
                    {
                        return dist;
                    }
                }

            }
        }

        return int.MaxValue;    //Obstacles are ("infinitely far") from the ai (far enough that the ai doesn't need to care)
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
