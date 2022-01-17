using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    private static Player _instance;

    // Movement
    public float moveSpeed = 5;
    private bool canMove = true;


    private InputSettings controls;
    private Vector3 inputDir;
    
    // References
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

        if (!canMove) 
        {
            playerAnimator.SetBool("isRunning", false);
        }
        else 
        {
            playerAnimator.SetBool("isRunning", inputDir.magnitude != 0);
            if (inputDir.x < 0)
            {
                playerSpriteRenderer.flipX = false;
            }
            else if (inputDir.x > 0)
            {
                playerSpriteRenderer.flipX = true;
            }
        }
    }

    private void FixedUpdate()
    {
        if (canMove)
        {
            transform.position += moveSpeed * inputDir.normalized * Time.deltaTime;
        }
    }

    private void Move(Vector2 moveDir) 
    {
        inputDir = new Vector3(moveDir.x, moveDir.y);
    }

    public static void SetCanMove(bool value) {
        _instance.canMove = value;
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

    public static void SetPosition(Vector3 pos)
    {
        _instance.transform.position = pos;
    }

    public static Vector3 GetPosition()
    {
        return _instance.transform.position;
    }
}
