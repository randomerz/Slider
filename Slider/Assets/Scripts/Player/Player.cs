using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private static Player _instance;

    void Awake()
    {
        _instance = this;
    }
    
    void Update()
    {

        inputDir = new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        playerAnimator.SetBool("isRunning", inputDir.magnitude != 0);
        PickUpNode();
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

    private void FixedUpdate()
    {
        if (!canMove)
            return;

        transform.position += moveSpeed * inputDir.normalized * Time.deltaTime;

    }

    public void PickUpNode()
    {
        Collider2D[] nodes = Physics2D.OverlapCircleAll(new Vector2(transform.position.x, transform.position.y), 0.8f, knotMask);
        if (nodes.Length > 0  && Input.GetKey(KeyCode.E) && !picked)
        {
            knotNode = nodes[0].gameObject;
            Debug.Log(knotNode.name);
            picked = true;
            knotNode.GetComponent<Collider2D>().enabled = false;
        } else if (picked && Input.GetKeyUp(KeyCode.E))
        {
            picked = false;
            knotNode.GetComponent<Collider2D>().enabled = true;

        }
    }

    public static bool IsSafe()
    {
        Collider2D hit = Physics2D.OverlapPoint(_instance.transform.position, LayerMask.GetMask("SlideableArea"));
        return hit != null;
    }

    public static int GetSliderUnderneath()
    {
        Collider2D hit = Physics2D.OverlapPoint(_instance.transform.position, LayerMask.GetMask("Slider"));
        if (hit == null || hit.GetComponent<TileSlider>() == null)
        {
            Debug.LogWarning("Player isn't on top of a slider!");
            return -1;
        }
        return hit.GetComponent<TileSlider>().islandId;
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
