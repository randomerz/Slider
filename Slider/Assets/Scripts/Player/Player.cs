using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

// ** THIS CLASS HAS BEEN UPDATED TO USE THE NEW SINGLETON BASE CLASS. PLEASE REPORT NEW ISSUES YOU SUSPECT ARE RELATED TO THIS CHANGE TO TRAVIS AND/OR DANIEL! **
//L: I moved the STile underneath stuff to static method in STile since it's used in other places.
public class Player : Singleton<Player>, ISavable, ISTileLocatable
{

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5;
    [SerializeField] private bool isOnWater = false;

    [Header("References")]
    // [SerializeField] private Sprite trackerSprite;
    [SerializeField] private PlayerAction playerAction;
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private SpriteRenderer playerSpriteRenderer;
    [SerializeField] private Collider2D colliderPlayerVers;
    [SerializeField] private Collider2D colliderBoatVers;
    [SerializeField] private GameObject boatGameObject;
    [SerializeField] private Transform boatGetSTileUnderneathTransform;
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private bool debugDontUpdateStileUnderneath;

    private PlayerInput playerInput;
    

    private float moveSpeedMultiplier = 1;
    private bool canMove = true;
    private bool noClipEnabled = false;

    private bool isInHouse = false;

    private STile currentStileUnderneath;

    private Vector3 lastMoveDir;
    private Vector3 inputDir;

    private bool didInit;

    private static float houseYThreshold = -75; // below this y value the player must be in a house

    protected void Awake()
    {
        if (!didInit)
            Init();
    }

    public void InitSingleton()
    {
        InitializeSingleton(overrideExistingInstanceWith: this);
    }

    public void Init()
    {
        didInit = true;
        InitializeSingleton(overrideExistingInstanceWith: this);

        Controls.RegisterBindingBehavior(this, Controls.Bindings.Player.Move, context => _instance.UpdateMove(context.ReadValue<Vector2>()));
        playerInventory.Init();
        UpdatePlayerSpeed();

        playerInput= GetComponent<PlayerInput>();
    }

    private void Start() 
    {
        UITrackerManager.AddNewTracker(
            gameObject, 
            UITrackerManager.DefaultSprites.playerBlackCircle, 
            UITrackerManager.DefaultSprites.playerBlackCircleEmpty, 
            UITrackerManager.DefaultSprites.playerWhiteCircle, 
            UITrackerManager.DefaultSprites.playerWhiteCircleEmpty, 
            3f
        );
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
            // we offset where the raycast starts because when you're in the boat, the collider is at the boat not the player
            Vector3 basePosition = GetPlayerTransformRaycastPosition();
            Vector3 raycastOffset = transform.position - basePosition;
            Vector3 target = basePosition + moveSpeed * moveSpeedMultiplier * inputDir.normalized * Time.deltaTime;
            if (noClipEnabled)
            {
                transform.position = target + raycastOffset;
            }
            else
            {
                Physics2D.queriesHitTriggers = false;
                RaycastHit2D raycasthit = Physics2D.Raycast(basePosition, inputDir.normalized, moveSpeed * moveSpeedMultiplier * Time.deltaTime, LayerMask.GetMask("Default"));

                if (raycasthit.collider == null || raycasthit.collider.Equals(this.GetComponent<BoxCollider2D>()))
                {
                    transform.position = target + raycastOffset;
                }
                else
                {
                    Vector3 testMoveDir = new Vector3(inputDir.x, 0f).normalized;
                    target = basePosition + moveSpeed * moveSpeedMultiplier * testMoveDir * Time.deltaTime;
                    RaycastHit2D raycasthitX = Physics2D.Raycast(basePosition, testMoveDir.normalized, moveSpeed * moveSpeedMultiplier * Time.deltaTime, LayerMask.GetMask("Default"));
                    if (raycasthitX.collider == null)
                    {
                        transform.position = target + raycastOffset;
                    }
                    else
                    {
                        testMoveDir = new Vector3(0f, inputDir.y).normalized;
                        target = basePosition + moveSpeed * moveSpeedMultiplier * testMoveDir * Time.deltaTime;
                        RaycastHit2D raycasthitY = Physics2D.Raycast(basePosition, testMoveDir.normalized, moveSpeed * moveSpeedMultiplier * Time.deltaTime, LayerMask.GetMask("Default"));
                        if (raycasthitY.collider == null)
                        {
                            transform.position = target + raycastOffset;
                        }
                    }
                }
                Physics2D.queriesHitTriggers = true;
            }
        }

        // updating childing
        currentStileUnderneath = GetSTileUnderneath();
        // Debug.Log("Currently on: " + currentStileUnderneath);

        if (currentStileUnderneath != null && !debugDontUpdateStileUnderneath)
        {
            transform.SetParent(currentStileUnderneath.transform);
        }
        else
        {
            transform.SetParent(null);
        }
    }
    //Jroo: Either says "Keyboard Mouse" or "Controller" based on last input
    public string GetCurrentControlScheme()
    {
        return playerInput.currentControlScheme;
    }
    public event Action<string> ControlSchemeChanged;
    /// <summary>
    /// Called when control scheme changes (between "Controller" or "Keyboard Mouse")
    /// </summary>
    public void OnControlsChanged()
    {
        //string newControlScheme = GetCurrentControlScheme();
        string newControlScheme = playerInput.currentControlScheme;
        Debug.Log("Control Scheme changed to: " + newControlScheme);
        ControlSchemeChanged?.Invoke(newControlScheme);
    }
    /*
    public void OnAltViewHold()
    {
        Debug.Log("Alt View Hold");
    }*/

    // Here is where we pay for all our Singleton Sins
    public void ResetInventory()
    {
        playerInventory.Reset();
    }

    public static Player GetInstance()
    {
        return _instance;
    }

    public static PlayerAction GetPlayerAction()
    {
        return _instance.playerAction;
    }

    public static PlayerInventory GetPlayerInventory()
    {
        return _instance.playerInventory;
    }

    public static SpriteRenderer GetSpriteRenderer()
    {
        return _instance.playerSpriteRenderer;
    }

    public void Save()
    {
        SerializablePlayer sp = new SerializablePlayer();

        // Player
        sp.position = new float[3];
        Vector3 pos = GetSavePosition();
        sp.position[0] = pos.x;
        sp.position[1] = pos.y;
        sp.position[2] = pos.z;
        sp.isOnWater = isOnWater;
        sp.isInHouse = isInHouse;

        // PlayerInventory
        sp.collectibles = new List<Collectible.CollectibleData>(GetPlayerInventory().GetCollectiblesList());
        sp.hasCollectedAnchor = GetPlayerInventory().GetHasCollectedAnchor();

        SaveSystem.Current.SetSerializeablePlayer(sp);

        // Debug.Log("Saved player position to: " + pos);
    }

    private Vector3 GetSavePosition()
    {
        // We need this in case an STile is moving while the player is on it!

        // Player positions
        Vector3 pos = transform.position;
        Vector3 localPos = transform.localPosition;

        // STile postitions
        STile stile = GetSTileUnderneath();
        if (stile == null)
        {
            return pos;
        }
        else
        {
            Vector2Int stileEndCoords = GetEndStileLocation(stile.islandId);
            Vector3 stilePos = stile.calculatePosition(stileEndCoords.x, stileEndCoords.y);

            return stilePos + localPos;
        }
    }

    private Vector2Int GetEndStileLocation(int myStileId)
    {
        STile[,] grid = SGrid.Current.GetGrid();
        for (int x = 0; x < SGrid.Current.Width; x++)
        {
            for (int y = 0; y < SGrid.Current.Height; y++)
            {
                if (grid[x, y].islandId == myStileId)
                {
                    return new Vector2Int(x, y);
                }
            }
        }
        Debug.LogError("Could not find STile of id " + myStileId);
        return Vector2Int.zero;
    }

    public void Load(SaveProfile profile)
    {
        InitializeSingleton(overrideExistingInstanceWith: this);

        if (profile == null || profile.GetSerializablePlayer() == null)
        {
            playerInventory.Reset();
            SetIsInHouse(transform.position.y <= houseYThreshold);
            return;
        }

        SerializablePlayer sp = profile.GetSerializablePlayer();

        // Player

        SetIsOnWater(sp.isOnWater);
        SetIsInHouse(sp.isInHouse);

        // Update position
        if (GameManager.instance.debugModeActive && DebugUIManager.justDidSetScene)
        {
            // skip setting position if just did SetScene()
            DebugUIManager.justDidSetScene = false;

            SetIsOnWater(SGrid.Current.MyArea == Area.Ocean);
        }
        else
        {
            transform.SetParent(null);
            transform.position = new Vector3(sp.position[0], sp.position[1], sp.position[2]);
            STile stileUnderneath = GetSTileUnderneath();
            transform.SetParent(stileUnderneath != null ? stileUnderneath.transform : null);
        }

        // PlayerInventory
        playerInventory.SetCollectiblesList(sp.collectibles);
        playerInventory.SetHasCollectedAnchor(sp.hasCollectedAnchor);

        // Other init functions
        UpdatePlayerSpeed();
    }

    private void UpdateMove(Vector2 moveDir) 
    {
        inputDir = new Vector3(moveDir.x, moveDir.y);
        if (moveDir.magnitude != 0) 
        {
            lastMoveDir = inputDir;
        }
    }

    public static Vector3 GetLastMoveDirection()
    {
        return _instance.lastMoveDir;
    }

    public static void SetCanMove(bool value) {
        _instance.canMove = value;
    }

    public static bool GetCanMove(){
        return _instance.canMove;
    }

    public void toggleCollision()
    {
        _instance.noClipEnabled = !_instance.noClipEnabled;

        if (_instance.noClipEnabled)
        {
            colliderPlayerVers.enabled = false;
            colliderBoatVers.enabled = false;
        }
        else
        {
            SetIsOnWater(isOnWater);
        }
    }

    public void TPX(string val)
    {
        int n = int.Parse(val);
        transform.position += new Vector3(n, 0, 0);
    }

    public void TPY(string val)
    {
        int n = int.Parse(val);
        transform.position += new Vector3(0, n, 0);
    }


    public static void SetPosition(Vector3 pos)
    {
        _instance.transform.position = pos;
    }

    public static void SetParent(Transform parent)
    {
        _instance.transform.SetParent(parent);
    }

    public static Vector3 GetPosition()
    {
        if (!_instance)
            return Vector3.zero;
        return _instance.transform.position;
    }


    public Vector3 GetPlayerTransformRaycastPosition()
    {
        if (!isOnWater)
        {
            return transform.position;
        }
        else
        {
            return boatGetSTileUnderneathTransform.transform.position;
        }
    }

    public STile GetSTileUnderneath()
    {
        Transform transformToUse = isOnWater ? boatGetSTileUnderneathTransform : transform;
        currentStileUnderneath = SGrid.GetSTileUnderneath(transformToUse, currentStileUnderneath);
        return currentStileUnderneath;
    }

    public static void SetMoveSpeedMultiplier(float x)
    {
        _instance.moveSpeedMultiplier = x;
    }

    public void UpdatePlayerSpeed()
    {
        moveSpeed = 5;

        if (PlayerInventory.Contains("Boots"))
        {
            moveSpeed += 2;
        }

        if (isOnWater)
        {
            moveSpeed += 1;
        }
    }

    public static bool GetIsInHouse()
    {
        return _instance.isInHouse;
    }

    public static void SetIsInHouse(bool isInHouse)
    {
        if (isInHouse)
            AudioManager.EnqueueModifier(AudioModifier.ModifierType.IndoorMusic3Eq);
        else
            AudioManager.DequeueModifier(AudioModifier.ModifierType.IndoorMusic3Eq);
        _instance.isInHouse = isInHouse;
    }

    public bool GetIsOnWater()
    {
        return isOnWater;
    }

    public void GetIsOnWater(Condition c)
    {
        c.SetSpec(isOnWater);
    }

    public void SetIsOnWater(bool isOnWater)
    {
        this.isOnWater = isOnWater;

        colliderPlayerVers.enabled = !isOnWater;
        colliderBoatVers.enabled = isOnWater;
        boatGameObject.SetActive(isOnWater);

        UpdatePlayerSpeed();
    }

    Tilemap ISTileLocatable.GetCurrentMaterialTilemap()
    {
        if (currentStileUnderneath == null)
        {
            var fallback = SGrid.GetWorldGridTilemaps();
            if (fallback == null) 
                return null;
            else 
                return fallback.materials;
        }
        else if (isInHouse)
        {
            if (currentStileUnderneath.houseTilemaps == null) 
                return null;
            else 
                return currentStileUnderneath.houseTilemaps.materials;
        }
        else
        {
            if (currentStileUnderneath.stileTilemaps == null)
                return null;
            else
                return currentStileUnderneath.stileTilemaps.materials;
        }
    }
}
