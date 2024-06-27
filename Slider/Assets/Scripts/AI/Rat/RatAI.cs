using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

public class RatAI : MonoBehaviour, ISavable
{
    [Header("Player")]
    public float playerAggroRange;
    public float playerDeaggroRange;
    public float minSpeed;
    public float maxSpeed;

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
    internal Collectible collectibleToSteal;
    [SerializeField]
    internal Transform player;
    [SerializeField]
    internal PaintedCostMap paintedCostMap;


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

    [Header("Other")]
    [SerializeField]
    internal bool avoidsDark = false;
    [SerializeField] private float squeakMinTime = 4;
    [SerializeField] private float squeakMaxTime = 7;


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

    private HashSet<int> visitedTiles = new HashSet<int>();
    private bool hasAchievement = false;
    private float squeakTime;
    private float squeakTimeCutoff;


    [HideInInspector]
    internal HashSet<Vector2Int> visited = null;    //For debugging

    private void Awake()
    {

        if (collectibleToSteal == null)
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
        if (collectibleToSteal == null)
            Debug.LogWarning("collectibleToSteal is null!");
        else
            StealPiece();

        squeakTimeCutoff = Random.Range(squeakMinTime, squeakMaxTime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        CheckDeathByMoss(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        CheckDeathByMoss(collision);
    }

    private void Update()
    {
        behaviourTree.Evaluate();
        float distToPlayer = Vector3.Distance(transform.position, Player.GetPosition());
        float speed = Mathf.Lerp(maxSpeed, minSpeed, (distToPlayer - 1) / playerDeaggroRange);
        navAgent.speed = speed;

        if (behaviourTree.State == BehaviourTreeNode.NodeState.FAILURE)
        {
            navAgent.StopPath();
        }

        if (rb.velocity.magnitude > 0.1f)
        {
            transform.up = rb.velocity.normalized;
        }

        if(squeakTime < squeakTimeCutoff)
            squeakTime += Time.deltaTime;
        else {
            squeakTime = 0;
            squeakTimeCutoff = Random.Range(squeakMinTime, squeakMaxTime);
            AudioManager.Play("Rat Squeak", transform);
        }
    }

    private void FixedUpdate()
    {
        // updating childing
        currentStileUnderneath = SGrid.GetSTileUnderneath(transform, currentStileUnderneath);

        if (currentStileUnderneath != null)
        {
            transform.SetParent(currentStileUnderneath.transform);
            visitedTiles.Add(currentStileUnderneath.islandId);
            if(!hasAchievement && visitedTiles.Count >= Mathf.Max(7, SGrid.Current.GetNumTilesCollected())) {
                AchievementManager.SetAchievementStat("completedRatRace", 1);
                hasAchievement = true;
            }
        }

        anim.SetFloat("speed", rb.velocity.magnitude);
    }

    public void SetDirection(Vector2 dir)
    {
        _dirFacing = dir.normalized;
        transform.up = new Vector3(_dirFacing.x, _dirFacing.y, 0);
    }

    private void CheckDeathByMoss(Collision2D collision)
    {
        if (collision.collider.CompareTag("Moss"))
        {
            Tilemap mossColliders = collision.collider.GetComponent<Tilemap>();
            Vector3Int ratCellPos = mossColliders.WorldToCell(transform.position);
            Tile.ColliderType colliderType = mossColliders.GetColliderType(ratCellPos);
            if (colliderType == Tile.ColliderType.Grid)
            {
                Die();
            }
        }
    }

    public void Die()
    {
        //Play Death Animation
        anim.SetBool("dead", true);
        AudioManager.Play("Hurt");

        if (holdingObject && collectibleToSteal != null)
        {
            collectibleToSteal.transform.parent = transform.parent;  //"Unparent" The Rat from the object so the Rat "Drops" it
            BoxCollider2D boxCollider2D = collectibleToSteal.GetComponent<BoxCollider2D>();
            if (boxCollider2D != null)
            {
                boxCollider2D.size = new Vector2(1, 1);
            }
            else
            {
                Debug.LogError("Couldn't get collider on rat's collectible");
            }
        }
        
        visitedTiles.Clear();
        Save();

        Destroy(gameObject);
    }

    private void StealPiece()
    {
        //L: Reparent Slider piece to be child of Rat
        holdingObject = true;
        collectibleToSteal.transform.parent = transform;
        collectibleToSteal.transform.localPosition = mouth.localPosition;
    }

    private void ConstructBehaviourTree()
    {
        var setDestToObjectNode = new SetDestToObjectNode(this);

        var playerAggroNode = new AggroAtProximityNode(transform, player, playerAggroRange, playerDeaggroRange);
        var setDestToAvoidPlayerNode = new SetDestToAvoidPlayerNode(this);

        var setDestToNearestValidPtNode = new SetDestToNearestValidPtNode(this);

        var moveTowardsSetDestNode = new MoveTowardsSetPosNode(this, updateTimer);

        var stayInPlaceNode = new DoNothingNode(this);


        //L: IMPORTANT NOTE: The ordering of the nodes in the tree matters
        var stealSequence = new SequenceNode(new List<BehaviourTreeNode>() { setDestToObjectNode, moveTowardsSetDestNode });
        var runFromPlayerSequence = new SequenceNode(new List<BehaviourTreeNode> { playerAggroNode, setDestToAvoidPlayerNode, moveTowardsSetDestNode });
        var runToValidPtSequence = new SequenceNode(new List<BehaviourTreeNode> { setDestToNearestValidPtNode, moveTowardsSetDestNode });

        behaviourTree = new SelectorNode(new List<BehaviourTreeNode> { stealSequence, runFromPlayerSequence, runToValidPtSequence, stayInPlaceNode }); 

    }


    internal int CostToThreat(float distToThreat, bool threatIsPlayer)
    {

        float cost = 4 * tileMaxPenalty * Mathf.Pow(0.25f, distToThreat) +
                    (0.25f * tileMaxPenalty * (playerAggroRange - distToThreat + 3));

        cost = Mathf.Clamp(cost, 0, 100 * tileMaxPenalty);
        return (int)cost;
    }

    private string SetToString(HashSet<int> set) {
        string output = "";
        foreach(int num in set){
            output += num.ToString();
        }
        return output;
    }

    private HashSet<int> StringToSet(string nums)
    {
        HashSet<int> set = new HashSet<int>();
        for(int i = 0; i < nums.Length; i++) {
            set.Add(nums[i] - '0');
        }
        return set;
    }

    public void Save()
    {
        SaveSystem.Current.SetString("cavesRatTiles", SetToString(visitedTiles));
    }

    public void Load(SaveProfile profile)
    {
        visitedTiles = StringToSet(profile.GetString("cavesRatTiles"));
    }
}