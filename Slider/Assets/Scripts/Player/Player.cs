using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour, ISavable
{
    private static Player _instance;
    private bool didInit;

    // Movement
    [SerializeField] private float moveSpeed = 5;
    private float moveSpeedMultiplier = 1;
    private bool canMove = true;
    private bool collision = true;
    [SerializeField] private bool isOnWater = false;
    private bool isInHouse = false;

    private STile currentStileUnderneath;

    private InputSettings controls;
    private Vector3 lastMoveDir;
    private Vector3 inputDir;
    
    [Header("References")]
    // [SerializeField] private Sprite trackerSprite;
    [SerializeField] private PlayerAction playerAction;
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private SpriteRenderer playerSpriteRenderer;
    [SerializeField] private SpriteRenderer boatSpriteRenderer;
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private Rigidbody2D rb;

    void Awake()
    {
        _instance = this;
        _instance.controls = new InputSettings();
        LoadBindings();

        if (!didInit)
            Init();
    }

    public void SetSingleton()
    {
        _instance = this;
    }

    public void Init()
    {
        didInit = true;

        playerInventory.Init();

        UpdatePlayerSpeed();
    }
    
    private void Start() 
    {
        UITrackerManager.AddNewTracker(gameObject, UITrackerManager.DefaultSprites.circle1, UITrackerManager.DefaultSprites.circleEmpty, 3f);
    }

    public static void LoadBindings()
    {
        if (_instance == null)
        {
            return;
        }

        var rebinds = PlayerPrefs.GetString("rebinds");
        if (!string.IsNullOrEmpty(rebinds))
        {
            _instance.controls.LoadBindingOverridesFromJson(rebinds);
        }
        _instance.controls.Player.Move.performed += context => _instance.UpdateMove(context.ReadValue<Vector2>());
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
            //currently checks everything in the default layer, also does not hit triggers
            Vector3 target = transform.position + moveSpeed * moveSpeedMultiplier * inputDir.normalized * Time.deltaTime;
            if (!collision)
            {
                transform.position = target;
            }
            else
            {
                Physics2D.queriesHitTriggers = false;
                RaycastHit2D raycasthit = Physics2D.Raycast(transform.position, inputDir.normalized, moveSpeed * moveSpeedMultiplier * Time.deltaTime, LayerMask.GetMask("Default"));

                if (raycasthit.collider == null || raycasthit.collider.Equals(this.GetComponent<BoxCollider2D>()))
                {
                    transform.position = target;
                }
                else
                {
                    Vector3 testMoveDir = new Vector3(inputDir.x, 0f).normalized;
                    target = transform.position + moveSpeed * moveSpeedMultiplier * testMoveDir * Time.deltaTime;
                    RaycastHit2D raycasthitX = Physics2D.Raycast(transform.position, testMoveDir.normalized, moveSpeed * moveSpeedMultiplier * Time.deltaTime, LayerMask.GetMask("Default"));
                    if (raycasthitX.collider == null)
                    {
                        transform.position = target;
                    }
                    else
                    {
                        testMoveDir = new Vector3(0f, inputDir.y).normalized;
                        target = transform.position + moveSpeed * moveSpeedMultiplier * testMoveDir * Time.deltaTime;
                        RaycastHit2D raycasthitY = Physics2D.Raycast(transform.position, testMoveDir.normalized, moveSpeed * moveSpeedMultiplier * Time.deltaTime, LayerMask.GetMask("Default"));
                        if (raycasthitY.collider == null)
                        {
                            transform.position = target;
                        }
                    }
                }
                Physics2D.queriesHitTriggers = true;
            }
        }

        // updating childing
        currentStileUnderneath = STile.GetSTileUnderneath(transform, currentStileUnderneath);
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

    //L: I moved the STile underneath stuff to static method in STile since it's used in other places.


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
        Vector3 pos = GetPosition();
        sp.position[0] = pos.x;
        sp.position[1] = pos.y;
        sp.position[2] = pos.z;
        sp.isOnWater = isOnWater;
        sp.isInHouse = isInHouse;

        // PlayerInventory
        sp.collectibles = GetPlayerInventory().GetCollectiblesList();
        sp.hasCollectedAnchor = GetPlayerInventory().GetHasCollectedAnchor();

        SaveSystem.Current.SetSerializeablePlayer(sp);
    }

    public void Load(SaveProfile profile)
    {
        if (profile == null || profile.GetSerializablePlayer() == null)
            return;

        SerializablePlayer sp = profile.GetSerializablePlayer();

        // Player

        // Update position
        transform.SetParent(null);
        transform.position = new Vector3(sp.position[0], sp.position[1], sp.position[2]);
        STile stileUnderneath = STile.GetSTileUnderneath(transform, null);
        transform.SetParent(stileUnderneath != null ? stileUnderneath.transform : null);
        //Debug.Log("setting position to: " + new Vector3(sp.position[0], sp.position[1], sp.position[2]));

        isOnWater = sp.isOnWater;
        isInHouse = sp.isInHouse;

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

    public static Vector3 GetLastMoveDir() 
    {
        return _instance.lastMoveDir;
    }

    public static void SetCanMove(bool value) {
        _instance.canMove = value;
    }

    public void toggleCollision()
    {
        _instance.collision = !_instance.collision;
        Collider2D collider = GetComponent<Collider2D>();
        collider.enabled = collision;
    }



    public static void SetPosition(Vector3 pos)
    {
        _instance.transform.position = pos;
    }

    public static Vector3 GetPosition()
    {
        if (!_instance)
            return Vector3.zero;
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
        _instance.currentStileUnderneath = STile.GetSTileUnderneath(_instance.transform, _instance.currentStileUnderneath);
        return _instance.currentStileUnderneath;
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

        UpdatePlayerSpeed();
    }
}
