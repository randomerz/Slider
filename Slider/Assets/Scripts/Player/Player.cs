using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    //public Item currentHeldItem; // for rope puzzle

    // Movement
    public float moveSpeed = 5;
    public LayerMask knotMask;
    public bool picked = false;
    public static bool canMove = true;

    GameObject knotNode;
    private Vector3 inputDir;

    // References
    public SpriteRenderer playerSpriteRenderer;
    public Animator playerAnimator;

    private InputSettings controls;
    private static Player _instance;

    void Awake()
    {
        _instance = this;

        controls = new InputSettings();
        controls.Player.Action.performed += context => Action();
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
            if (picked)
            {
                knotNode.transform.position = transform.position;
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

    private void Action() 
    {
        PickUpNode();
    }

    public void PickUpNode()
    {
        Collider2D[] nodes = Physics2D.OverlapCircleAll(new Vector2(transform.position.x, transform.position.y), 0.5f, knotMask);
        if (nodes.Length > 0  && !picked)
        {
            knotNode = nodes[0].gameObject;
            picked = true;
        } else if (picked)
        {
            picked = false;
        }
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
