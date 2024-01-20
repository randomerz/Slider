using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MomentumGizmo : MonoBehaviour
{
    [Tooltip("This will be set automatically in Awake() if needed")]
    public STile myStile;
    public Animator animator; // this is only based on Tree animator controller rn

    private void Awake() 
    {
        if (myStile == null)
        {
            FindSTile();
        }
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    private void OnEnable() 
    {
        if (myStile != null)
            myStile.onChangeMove += OnStileChangeDir;
        animator.SetFloat("random", Random.Range(0f, 1f));
    }

    private void OnDisable() 
    {
        if (myStile != null)
            myStile.onChangeMove -= OnStileChangeDir;
    }

    public void OnStileChangeDir(object sender, STile.STileMoveArgs e)
    {
        animator.SetInteger("dx", -(int)e.moveDir.x);
        animator.SetInteger("dy", -(int)e.moveDir.y);
        if (e.moveDir == Vector2.zero)
        {
            animator.SetTrigger("finishedMoving");
        }
        // Debug.Log("Updated!");
    }

    

    private void FindSTile()
    {
        Transform curr = transform;
        int i = 0;
        while (curr.parent != null && i < 100)
        {
            if (curr.GetComponent<STile>() != null)
            {
                myStile = curr.GetComponent<STile>();
                return;
            }

            // Debug.Log(curr.name);
            curr = curr.parent;
            i += 1;
        }

        if (i == 100)
            Debug.LogWarning("something went wrong in finding stile!");
    }
}
