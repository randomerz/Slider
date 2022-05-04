using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoweredLightNew : ElectricalOutput
{
    public bool Powered
    {
        get;
        private set;
    }

    [SerializeField] private SpriteRenderer spriteRenderer;

    [SerializeField] private Sprite onSprite;
    [SerializeField] private Sprite offSprite;

    public class OnLightSwitchedArgs
    {
        public bool lightOn;
    }
    public static event System.EventHandler<OnLightSwitchedArgs> OnLightSwitched;


    public override void PropagateSignal(bool value, List<ElectricalNode> recStack, int numSignals = 1)
    {
        SetLightOn(value, true);
    }


    public void SetLightOn(bool value, bool playSound = false)
    {
        spriteRenderer.sprite = value ? onSprite : offSprite;
        if (Powered != value)
        {
            Powered = value;

            if (playSound)
            {
                if (value)
                    AudioManager.Play("Power On");
                else
                    AudioManager.Play("Power Off");
            }

            OnLightSwitched?.Invoke(this, new OnLightSwitchedArgs { lightOn = value });
        }
    }
}
