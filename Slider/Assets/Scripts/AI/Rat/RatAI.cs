using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RatAI : MonoBehaviour
{
    public float playerMinRange;
    public float obstacleMinRange;

    public Animator anim;
    public GameObject objectToSteal;

    private bool holdingObject;

    private Node behaviourTree;

    private void Awake()
    {
        ConstructBehaviourTree();

        if (objectToSteal == null)
        {
            Debug.LogWarning("Rat does not have reference to slider piece");
        }
    }

    public void SetRunning(bool value)
    {
        anim.SetBool("isRunning", value);
    }
    
    private void ConstructBehaviourTree()
    {


        behaviourTree = new SelectorNode();
    }
}
