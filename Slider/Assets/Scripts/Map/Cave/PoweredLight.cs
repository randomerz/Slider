using UnityEngine;
using UnityEngine.Events;

using System.Collections.Generic;

public class PoweredLight : CaveLight
{
    [SerializeField]
    private Conditionals powerConditionals;

    private void Start()
    {
        powerConditionals.onSuccess?.AddListener(new UnityAction(() => { SetLightOn(true); }));
        powerConditionals.onFail?.AddListener(new UnityAction(() => { SetLightOn(false); }));

        SGridAnimator.OnSTileMoveStart += (sender, e) => { powerConditionals.CheckConditions(); };
        SGridAnimator.OnSTileMoveEnd += (sender, e) => { powerConditionals.CheckConditions(); };
    }
}

