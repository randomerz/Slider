using UnityEngine;

public class Player : MonoBehaviour
{
    private static Player _instance;

    // Movement
    [SerializeField] private float moveSpeed = 5;
    private float moveSpeedMultiplier = 1;
    private bool canMove = true;
    [SerializeField] private bool isOnWater = false;
    private bool isInHouse = false;

    private STile currentStileUnderneath;

    private InputSettings controls;
    private Vector3 lastMoveDir;
    private Vector3 inputDir;
    
    [Header("References")]
    [SerializeField] private Sprite trackerSprite;
    [SerializeField] private PlayerAction playerAction;
    [SerializeField] private SpriteRenderer playerSpriteRenderer;
    [SerializeField] private SpriteRenderer boatSpriteRenderer;
    [SerializeField] private Animator playerAnimator;

    void Awake()
    {
        _instance = this;

        controls = new InputSettings();
        controls.Player.Move.performed += context => UpdateMove(context.ReadValue<Vector2>());
        if (PlayerInventory.Contains("Boots"))
        {
            BootsSpeedUp();
        }
        UITrackerManager.AddNewTracker(this.gameObject, trackerSprite);
    }

    private void OnEnable() {
        controls.Enable();
    }

    private void OnDisable() {
        controls.Disable();
    }
    
    void Update()
    {

        // inputDir = new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if (inputDir.x < 0)
        {
            playerSpriteRenderer.flipX = true;
        }
        else if (inputDir.x > 0)
        {
            playerSpriteRenderer.flipX = false;
        }

        playerAnimator.SetBool("isRunning", inputDir.magnitude != 0);
        playerAnimator.SetBool("isOnWater", isOnWater);
        // playerAnimator.SetBool("hasSunglasses", hasSunglasses);
    }

    private void FixedUpdate()
    {
        if (canMove)
        {
            transform.position += moveSpeed * moveSpeedMultiplier * inputDir.normalized * Time.deltaTime;
        }

        // updating childing
        UpdateStileUnderneath();
        // Debug.Log("Currently on: " + currentStileUnderneath);

        if (currentStileUnderneath != null)
        {
            transform.SetParent(currentStileUnderneath.transform);
        }
        else
        {
            transform.SetParent(null);
        }
    }


    public static Player GetInstance()
    {
        return _instance;
    }

    public static PlayerAction GetPlayerAction() 
    {
        return _instance.playerAction;
    }

    public static SpriteRenderer GetSpriteRenderer()
    {
        return _instance.playerSpriteRenderer;
    }



    private void UpdateMove(Vector2 moveDir) 
    {
        inputDir = new Vector3(moveDir.x, moveDir.y);
        if (moveDir.magnitude != 0) 
        {
            lastMoveDir = inputDir;
        }
    }

    public static Vector3 GetLastMoveDir() 
    {
        return _instance.lastMoveDir;
    }

    public static void SetCanMove(bool value) {
        _instance.canMove = value;
    }



    public static void SetPosition(Vector3 pos)
    {
        _instance.transform.position = pos;
    }

    public static Vector3 GetPosition()
    {
        return _instance.transform.position;
    }



    public static bool IsSafe()
    {
        // DC: this was needed for game jam, but probably not really anymore
        // Collider2D hit = Physics2D.OverlapPoint(_instance.transform.position, LayerMask.GetMask("SlideableArea"));
        // return hit != null;
        return true;
    }

    public static STile GetStileUnderneath()
    {
        _instance.UpdateStileUnderneath();
        return _instance.currentStileUnderneath;
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
            if (s.isTileActive && IsPlayerInSTileBounds(s.transform.position, offset, housingOffset))
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

    private bool IsPlayerInSTileBounds(Vector3 stilePos, float offset, float housingOffset)
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

    public static void SetMoveSpeedMultiplier(float x)
    {
        _instance.moveSpeedMultiplier = x;
    }

    public void BootsSpeedUp()
    {
        if (moveSpeed==5)
        {   // tested, does effectively change the player's speed whenever boots are picked up
            // _instance.moveSpeed+=20;
            _instance.moveSpeed+=2;
            // Debug.Log(_instance.moveSpeed);

            // lol you'll have to pick up a ton of these boots if you want the speed to be noticeable
        }
    }

    public static bool GetIsInHouse()
    {
        return _instance.isInHouse;
    }

    public static void SetIsInHouse(bool isInHouse)
    {
        _instance.isInHouse = isInHouse;
    }

    public bool GetIsOnWater()
    {
        return isOnWater;
    }

    public void GetIsOnWater(Conditionals.Condition c)
    {
        c.SetSpec(isOnWater);
    }

    public void SetIsOnWater(bool isOnWater)
    {
        this.isOnWater = isOnWater;
        boatSpriteRenderer.enabled = isOnWater;
    }
}
