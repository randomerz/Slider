using UnityEngine;
using UnityEngine.Events;

using System.Collections.Generic;

public class PoweredLight : CaveLight
{
    [SerializeField]
    private Conditionals powerConditionals;

    private UnityAction success;
    private UnityAction failure;

    private void Awake()
    {
        success = new UnityAction(() => { SetLightOn(true); });
        failure = new UnityAction(() => { SetLightOn(false); });
    }

    private void OnEnable()
    {
        powerConditionals.onSuccess?.AddListener(success);
        powerConditionals.onFail?.AddListener(failure);

        SGridAnimator.OnSTileMoveStart += (sender, e) => { powerConditionals.CheckConditions(); };
        SGridAnimator.OnSTileMoveEnd += (sender, e) => { powerConditionals.CheckConditions(); };
    }
    private void OnDisable()
    {
        powerConditionals.onSuccess?.RemoveListener(success);
        powerConditionals.onFail?.RemoveListener(failure);

        SGridAnimator.OnSTileMoveStart -= (sender, e) => { powerConditionals.CheckConditions(); };
        SGridAnimator.OnSTileMoveEnd -= (sender, e) => { powerConditionals.CheckConditions(); };
    }

    private void Start()
    {
        powerConditionals.CheckConditions();
    }
}

