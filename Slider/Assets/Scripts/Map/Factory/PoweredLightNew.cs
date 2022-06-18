using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoweredLightNew : ElectricalNode
{
    [SerializeField] protected SpriteSwapper swapper;

    private new void Awake()
    {
        base.Awake();
        nodeType = NodeType.OUTPUT;

        if (swapper == null)
        {
            swapper = GetComponent<SpriteSwapper>();
        }
    }

    /*
    protected override void PropagateSignal(bool value, ElectricalNode prev, HashSet<ElectricalNode> recStack, int numRefs = 1)
    {
        //L: I was gonna do other stuff here, but I didn't ...
        base.PropagateSignal(value, prev, recStack, numRefs);
    }
    */

    public override void OnPoweredHandler(OnPoweredArgs e)
    {
        base.OnPoweredHandler(e);
        if (e.powered)
        {
            swapper.TurnOn();
        } else
        {
            swapper.TurnOff();
        }
    }

    /*
    public void SetLightOn(bool value, bool playSound = true)
    {
        spriteRenderer.sprite = value ? onSprite : offSprite;

        if (playSound)
        {
            AudioManager.Play(value ? "Power On" : "Power Off");
        }
    }
    */
}
