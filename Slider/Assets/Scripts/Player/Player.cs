using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    private static Player _instance;

    // Movement
    [SerializeField] private float moveSpeed = 5;
    private float moveSpeedMultiplier = 1;
    private bool canMove = true;
    [SerializeField] private bool isOnWater = false;

    private InputSettings controls;
    private Vector3 lastMoveDir;
    private Vector3 inputDir;
    
    // References
    [SerializeField] private PlayerAction playerAction;
    [SerializeField] private SpriteRenderer playerSpriteRenderer;
    [SerializeField] private Animator playerAnimator;

    void Awake()
    {
        _instance = this;

        controls = new InputSettings();
        controls.Player.Move.performed += context => Move(context.ReadValue<Vector2>());
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

        // if (!canMove)    // its fun to spam left/right in the cutscene :)
        // {
        //     playerAnimator.SetBool("isRunning", false);
        // }
        // else 
        // {
            playerAnimator.SetBool("isRunning", inputDir.magnitude != 0);
            if (inputDir.x < 0)
            {
                playerSpriteRenderer.flipX = true;
            }
            else if (inputDir.x > 0)
            {
                playerSpriteRenderer.flipX = false;
            }
        // }

        playerAnimator.SetBool("isOnWater", isOnWater);
        // playerAnimator.SetBool("hasSunglasses", hasSunglasses);
    }

    private void FixedUpdate()
    {
        if (canMove)
        {
            transform.position += moveSpeed * moveSpeedMultiplier * inputDir.normalized * Time.deltaTime;
        }
    }



    public static PlayerAction GetPlayerAction() 
    {
        return _instance.playerAction;
    }

    public static SpriteRenderer GetSpriteRenderer()
    {
        return _instance.playerSpriteRenderer;
    }



    private void Move(Vector2 moveDir) 
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
        Collider2D hit = Physics2D.OverlapPoint(_instance.transform.position, LayerMask.GetMask("SlideableArea"));
        return hit != null;
    }

    public static int GetStileUnderneath()
    {
        Collider2D hit = Physics2D.OverlapPoint(_instance.transform.position, LayerMask.GetMask("Slider"));
        if (hit == null || hit.GetComponent<STile>() == null)
        {
            //Debug.LogWarning("Player isn't on top of a slider!");
            return -1;
        }
        return hit.GetComponent<STile>().islandId;
    }

    public static void setMoveSpeedMultiplier(float x)
    {
        _instance.moveSpeedMultiplier = x;
    }
}
