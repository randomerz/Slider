using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoweredLightNew : ConductiveElectricalNode
{

    [SerializeField] private SpriteRenderer spriteRenderer;

    [SerializeField] private Sprite onSprite;
    [SerializeField] private Sprite offSprite;

    private new void Awake()
    {
        base.Awake();
        nodeType = NodeType.OUTPUT;
    }

    public override void PropagateSignal(bool value, ElectricalNode prev, HashSet<ElectricalNode> recStack, int numRefs = 1)
    {
        //L: I was gonna do other stuff here, but I didn't ...
        base.PropagateSignal(value, prev, recStack, numRefs);
    }

    public override void OnPoweredHandler(bool value, bool valueChanged)
    {
        SetLightOn(value, valueChanged);
    }

    public void SetLightOn(bool value, bool playSound = true)
    {
        spriteRenderer.sprite = value ? onSprite : offSprite;

        if (playSound)
        {
            AudioManager.Play(value ? "Power On" : "Power Off");
        }
    }
}
