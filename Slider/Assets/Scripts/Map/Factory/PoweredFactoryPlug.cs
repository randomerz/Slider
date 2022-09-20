using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoweredFactoryPlug : ElectricalNode
{
    public override bool Powered => ShouldExtend(); // you can do this??

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
        // base.OnPoweredHandler(e); // We only power on when extended... >:) this is for map icon purposes

        UpdateAnimator(shouldInvokeOnPowered: false);
    }

    public void OnMoveHandler(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        UpdateAnimator();
    }

    public void UpdateAnimator(bool shouldInvokeOnPowered=true)
    {
        if (shouldInvokeOnPowered)
        {
            OnPowered?.Invoke(new OnPoweredArgs{
                powered = ShouldExtend()
            });
        }

        if (ShouldExtend())
        {
            OnPoweredOn?.Invoke();
        } else
        {
            OnPoweredOff?.Invoke();
        }

        animator.SetBool("Powered", ShouldExtend());
    }

    private bool ShouldExtend()
    {
        bool isInvalidSpot = 
            CheckGrid.contains(SGrid.GetGridString(), "..._2.._...") ||
            CheckGrid.contains(SGrid.GetGridString(), "12") ||
            CheckGrid.contains(SGrid.GetGridString(), "32");

        return !isInvalidSpot && base.Powered;
    }
}
