using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoweredFactoryPlug : ElectricalNode
{
    //Probably want to do an animation later instead of sprite swapping

    [SerializeField] private Animator animator;

    private new void Awake()
    {
        base.Awake();
        nodeType = NodeType.OUTPUT;

        animator ??= GetComponent<Animator>();
    }

    protected override void OnEnable() 
    {
        base.OnEnable();

        SGridAnimator.OnSTileMoveEnd += OnMoveHandler;
    }

    protected override void OnDisable() 
    {
        base.OnDisable();

        SGridAnimator.OnSTileMoveEnd -= OnMoveHandler;
    }

    public override void OnPoweredHandler(OnPoweredArgs e)
    {
        //Debug.Log($"We Powered? {e.powered}");
        base.OnPoweredHandler(e);
        UpdateAnimator();
    }

    public void OnMoveHandler(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        UpdateAnimator();
    }

    public void UpdateAnimator()
    {
        animator.SetBool("Powered", ShouldExtend());
    }

    private bool ShouldExtend()
    {
        bool isInvalidSpot = 
            CheckGrid.contains(SGrid.GetGridString(), "..._2.._...") ||
            CheckGrid.contains(SGrid.GetGridString(), "12") ||
            CheckGrid.contains(SGrid.GetGridString(), "32");

        return !isInvalidSpot && Powered;
    }
}
