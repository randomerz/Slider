using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RatAI : MonoBehaviour
{
    public float playerMinRange;
    public float moveSpeed;
    [SerializeField]
    internal float raycastDistFactor;   //Determines how quickly the AI will move in a different direction if it encounters an obstacle.

    [SerializeField]
    private Animator anim;
    [SerializeField]
    private Rigidbody2D rb;

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
        var stealSequence = new SequenceNode(new List<BehaviourTreeNode>() { isPlayerCloseNode, moveTowardsObjectNode });
        var runSequence = new SequenceNode(new List<BehaviourTreeNode> { isPlayerCloseNode, findDirToRunNode, moveNode });


        behaviourTree = new SelectorNode(new List<BehaviourTreeNode> { stealSequence, runSequence, stayInPlaceNode });
    }
}
