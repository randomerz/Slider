using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RatAI : MonoBehaviour
{
    public float playerAggroRange;
    public float playerDeaggroRange;
    public float moveSpeed;
    public float pathfindingTolerance;

    [SerializeField]
    internal STileNavAgent navAgent;


    //Misc AI control weights
    //Note: In terms of the AI, darkness is treated the same as walls.

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


    [SerializeField]
    private Animator anim;
    [SerializeField]
    public Rigidbody2D rb;
    [SerializeField]
    private Transform mouth;

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
        ConstructBehaviourTree();
    }

    private void Update()
    {
        behaviourTree.Evaluate();
        if (behaviourTree.State == BehaviourTreeNode.NodeState.FAILURE)
        {
            Stay();
        }
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
            //L: Reparent Slider piece to be child of Rat
            holdingObject = true; 
            objectToSteal.transform.parent = transform;
            objectToSteal.transform.localPosition = mouth.localPosition;
        }
    }

    private void ConstructBehaviourTree()
    {
        var setDestToObjectNode = new SetDestToObjectNode(this);
        var moveTowardsSetDestNode = new MoveTowardsSetPosNode(this);

        var playerAggroNode = new AggroAtProximityNode(transform, player, playerAggroRange, playerDeaggroRange);
        var moveAwayFromPlayerNode = new MoveAwayFromPlayerNode(this, player);

        var setDestToLightTileNode = new SetDestToLightTileNode(this);

        var stayInPlaceNode = new StayInPlaceNode(this);


        //L: IMPORTANT NOTE: The ordering of the nodes in the tree matters
        var stealSequence = new SequenceNode(new List<BehaviourTreeNode>() { setDestToObjectNode, moveTowardsSetDestNode });
        var runFromPlayerSequence = new SequenceNode(new List<BehaviourTreeNode> { playerAggroNode, moveAwayFromPlayerNode });
        var runToLightSequence = new SequenceNode(new List<BehaviourTreeNode> { setDestToLightTileNode, moveTowardsSetDestNode });

        behaviourTree = new SelectorNode(new List<BehaviourTreeNode> { stealSequence, runFromPlayerSequence, runToLightSequence, stayInPlaceNode });
    }
}
