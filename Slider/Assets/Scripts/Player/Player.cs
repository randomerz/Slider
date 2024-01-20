using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class Player : Singleton<Player>, ISavable, ISTileLocatable
{
    public static event Action<string> OnControlSchemeChanged;

    public class HousingChangeArgs : System.EventArgs
    {
        public bool newIsInHouse;
    }

    public static System.EventHandler<HousingChangeArgs> OnHousingChanged;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5;
    [SerializeField] private bool isOnWater = false;

    [Header("References")]
    // [SerializeField] private Sprite trackerSprite;
    [SerializeField] private PlayerAction playerAction;
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private PlayerInput playerInput;
    
    [SerializeField] private SpriteRenderer playerSpriteRenderer;
    [SerializeField] private Animator playerAnimator;

    [SerializeField] private Collider2D colliderPlayerVers;
    [SerializeField] private Collider2D colliderBoatVers;
    [SerializeField] private GameObject boatGameObject;
    [SerializeField] private Transform boatGetSTileUnderneathTransform;
    [SerializeField] private Transform feetTransform;
    
    // [SerializeField] private Rigidbody2D rb;
    [SerializeField] private List<Material> ppMaterials;
    [SerializeField] private GameObject lightningEffect;


    private float moveSpeedMultiplier = 1;
    private Vector2 directionalMoveSpeedMultiplier = Vector2.one;
    private bool canMove = true;
    private bool canAnimateMovement = true;
    private bool noClipEnabled = false;
    private bool dontUpdateStileUnderneath;

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
        InitSingleton();

        Controls.RegisterBindingBehavior(this, Controls.Bindings.Player.Move, context => _instance.UpdateMove(context.ReadValue<Vector2>()));
        UpdatePlayerSpeed();
    }

    private void Start() 
    {
        SetTracker(true);
    }

    private void OnDisable() 
    {
        foreach (Material m in ppMaterials)
        {
            m.SetVector("_PlayerPosition", new Vector4(-1000, -1000, 0, 0));
        }    
    }
    
    void Update()
    {
        if (canAnimateMovement)
        {
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
        }

    }

    private void FixedUpdate()
    {
        if (canMove)
        {
            // we offset where the raycast starts because when you're in the boat, the collider is at the boat not the player
            Vector3 basePosition = GetPlayerTransformRaycastPosition();
            Vector3 raycastOffset = transform.position - basePosition;
            Vector3 target = basePosition + moveSpeed * moveSpeedMultiplier * Time.deltaTime * Vector3.Scale(directionalMoveSpeedMultiplier, inputDir.normalized);
            if (noClipEnabled)
            {
                transform.position = target + raycastOffset;
            }
            else
            {
                Physics2D.queriesHitTriggers = false;
                RaycastHit2D raycasthit = Physics2D.Raycast(basePosition, Vector3.Scale(directionalMoveSpeedMultiplier, inputDir.normalized), moveSpeed * moveSpeedMultiplier * Time.deltaTime, LayerMask.GetMask("Default"));

                if (raycasthit.collider == null || raycasthit.collider.Equals(this.GetComponent<BoxCollider2D>()))
                {
                    transform.position = target + raycastOffset;
                }
                else
                {
                    Vector3 testMoveDir = new Vector3(directionalMoveSpeedMultiplier.x * inputDir.x, 0f).normalized;
                    target = basePosition + moveSpeed * moveSpeedMultiplier * Time.deltaTime * testMoveDir;
                    RaycastHit2D raycasthitX = Physics2D.Raycast(basePosition, testMoveDir.normalized, moveSpeed * moveSpeedMultiplier * Time.deltaTime, LayerMask.GetMask("Default"));
                    if (raycasthitX.collider == null)
                    {
                        transform.position = target + raycastOffset;
                    }
                    else
                    {
                        testMoveDir = new Vector3(0f, directionalMoveSpeedMultiplier.y * inputDir.y).normalized;
                        target = basePosition + moveSpeed * moveSpeedMultiplier * Time.deltaTime * testMoveDir;
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
        if(!dontUpdateStileUnderneath)
            currentStileUnderneath = GetSTileUnderneath();
        // Debug.Log("Currently on: " + currentStileUnderneath);

        if (currentStileUnderneath != null && !dontUpdateStileUnderneath)
        {
            transform.SetParent(currentStileUnderneath.transform);
        }
        else
        {
            transform.SetParent(null);
        }
    }

    private void LateUpdate()
    {
        foreach(Material m in ppMaterials)
        {
            m.SetVector("_PlayerPosition", new Vector4(transform.position.x, transform.position.y, 0, 0));
        }
    }
    //Jroo: Either says "Keyboard Mouse" or "Controller" based on last input
    public string GetCurrentControlScheme()
    {
        return playerInput.currentControlScheme;
    }

    /// <summary>
    /// Called when control scheme changes (between "Controller" or "Keyboard Mouse")
    /// </summary>
    public void OnControlsChanged()
    {
        string newControlScheme = GetCurrentControlScheme();
        //Debug.Log("Control Scheme changed to: " + newControlScheme);
        OnControlSchemeChanged?.Invoke(newControlScheme);
        Controls.CurrentControlScheme = newControlScheme;
    }

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
        if (!didInit)
            Init();

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
        if (SettingsManager.DevConsole && DebugUIManager.justDidSetScene)
        {
            // skip setting position if just did SetScene()
            DebugUIManager.justDidSetScene = false;

            SetIsOnWater(SGrid.Current.MyArea == Area.Ocean);
            SetIsInHouse(SGrid.Current.MyArea == Area.Village);
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

        playerInventory.Init();

        // Other init functions
        UpdatePlayerSpeed();

        // If haven't logged on in a while + correct scene, spawn with anchor
        if (profile.GetBool("playerSpawnWithAnchorEquipped"))
        {
            profile.SetBool("playerSpawnWithAnchorEquipped", false);
            PlayerInventory.NextItem();
        }
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

    public static void SetCanMove(bool canMove, bool canAnimateMovement=true) 
    {
        _instance.canMove = canMove;
        _instance.canAnimateMovement = canAnimateMovement;

        if (!canAnimateMovement)
        {
            _instance.playerAnimator.SetBool("isRunning", false);
        }
    }

    public static bool GetCanMove()
    {
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

    public Transform GetPlayerFeetTransform()
    {
        return feetTransform;
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

    public static void SetDirectionalMoveSpeedMultiplier(Vector2 vec)
    {
        _instance.directionalMoveSpeedMultiplier = vec;
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

    public void SetTracker(bool value)
    {
        if (value)
        {
            if (SettingsManager.MiniPlayerIcon)
            {
                UITrackerManager.AddNewTracker(
                    gameObject,
                    UITrackerManager.DefaultSprites.playerBlackCircle,
                    UITrackerManager.DefaultSprites.playerBlackCircleEmpty,
                    UITrackerManager.DefaultSprites.playerWhiteCircle,
                    UITrackerManager.DefaultSprites.playerWhiteCircleEmpty,
                    blinkTime: 3f,
                    timeUntilBlinkRepeat: 10f
                );
            }
            else
            {
                UITrackerManager.AddNewTracker(
                    gameObject,
                    UITrackerManager.DefaultSprites.playerFullBlackCircle,
                    UITrackerManager.DefaultSprites.playerFullBlackCircleEmpty,
                    UITrackerManager.DefaultSprites.playerFullWhiteCircle,
                    UITrackerManager.DefaultSprites.playerFullWhiteCircleEmpty,
                    blinkTime: 3f,
                    timeUntilBlinkRepeat: 10f
                );
            }
        }
        else
        {
            UITrackerManager.RemoveTracker(gameObject);
        }
    }

    // This is a dangerous operation! You should probably only use it when the grid is locked.
    public void SetDontUpdateSTileUnderneath(bool value)
    {
        if (SGrid.Current.GetTotalNumTiles() != SGrid.Current.GetNumTilesCollected())
        {
            Debug.LogWarning("Updating 'dontUpdateSTileUnderneath' when the grid isn't full. Be warned!");
        }
        dontUpdateStileUnderneath = value;
    }

    public static bool GetIsInHouse()
    {
        return _instance.isInHouse;
    }

    public static void SetIsInHouse(bool isInHouse)
    {
        AudioManager.SetListenerIsIndoor(isInHouse);
        _instance.isInHouse = isInHouse;

        OnHousingChanged?.Invoke(_instance, new HousingChangeArgs { newIsInHouse = isInHouse });
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

    public void ToggleLightning(bool val)
    {
        lightningEffect.SetActive(val);
    }

    Tilemap ISTileLocatable.GetCurrentMaterialTilemap()
    {
        if (currentStileUnderneath == null)
        {
            var fallback = SGrid.Current.GetWorldGridTilemaps();
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
