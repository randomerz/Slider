using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RatAI : MonoBehaviour
{
    public float playerMinRange;
    public float moveSpeed;


    //Misc AI control weights
    //Note: In terms of the AI, darkness is treated the same as walls.

    [SerializeField]
    internal float minDistToWallFactor;   //Determines at what distance the AI is allowed to be near a wall before moving in a different direction (scaled by moveSpeed)
    [SerializeField]
    internal float idealDistFromWall; //The ideal distance the AI wants to keep from walls (this is essentially the rat's "vision")
    [SerializeField]
    [Range(0f, 1f)]
    internal float avoidWallsWeight;    //The tendency for the rat to avoid walls (0 to 1) (Note: This should be small, or else you get weird behaviour on the edge cases.


    [SerializeField]
    private Animator anim;
    [SerializeField]
    private Rigidbody2D rb;
    [SerializeField]
    private Transform mouth;

    public GameObject objectToSteal;
    public Transform player;

    internal bool holdingObject;

    private BehaviourTreeNode behaviourTree;

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

        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
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
        transform.up = new Vector3(dir.x, dir.y, 0);
    }

    public void Move()
    {
        rb.velocity = transform.up * moveSpeed;
        anim.SetBool("isRunning", true);
    }

    public void Stay()
    {
        rb.velocity = Vector2.zero;
        anim.SetBool("isRunning", false);
    }

    public void StealObject()
    {
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
        var isPlayerCloseNode = new IsTargetCloseNode(transform, player, playerMinRange);
        var findDirToRunNode = new FindDirToRunNode(this, player);
        var moveNode = new MoveNode(this);
        var stayInPlaceNode = new StayInPlaceNode(this);
        var moveTowardsObjectNode = new MoveTowardsObjectNode(this);

        //L: IMPORTANT NOTE: The ordering of the nodes in the tree matters
        var stealSequence = new SequenceNode(new List<BehaviourTreeNode>() { moveTowardsObjectNode });
        var runSequence = new SequenceNode(new List<BehaviourTreeNode> { isPlayerCloseNode, findDirToRunNode, moveNode });


        behaviourTree = new SelectorNode(new List<BehaviourTreeNode> { stealSequence, runSequence, stayInPlaceNode });
    }
}
