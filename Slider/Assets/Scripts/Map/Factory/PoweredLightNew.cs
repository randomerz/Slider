using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoweredLightNew : ElectricalNode
{

    [SerializeField] private SpriteRenderer spriteRenderer;

    [SerializeField] private Sprite onSprite;
    [SerializeField] private Sprite offSprite;

    private void Awake()
    {
        nodeType = NodeType.OUTPUT;
    }

    public override void PropagateSignal(bool value, List<ElectricalNode> recStack, int numSignals = 1)
    {
        //L: I was gonna do other stuff here, but I didn't ...
        base.PropagateSignal(value, recStack, numSignals);
    }

    public override void OnPoweredHandler(bool value, bool valueChanged)
    {
        SetLightOn(value, valueChanged);
    }

    public void SetLightOn(bool value, bool playSound = false)
    {
        spriteRenderer.sprite = value ? onSprite : offSprite;

        if (playSound)
        {
            AudioManager.Play(value ? "Power On" : "Power Off");
        }
    }
}
