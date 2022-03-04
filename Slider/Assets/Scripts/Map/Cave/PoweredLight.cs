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
        success = new UnityAction(() => { 
            SetLightOn(true, playSound:true); 
        });
        failure = new UnityAction(() => { 
            SetLightOn(false, playSound:true); 
        });
    }

    private void OnEnable()
    {
        powerConditionals.onSuccess?.AddListener(success);
        powerConditionals.onFail?.AddListener(failure);
    }
    private void OnDisable()
    {
        powerConditionals.onSuccess?.RemoveListener(success);
        powerConditionals.onFail?.RemoveListener(failure);

        SGrid.OnSTileEnabled -= CheckConditions;
        SGridAnimator.OnSTileMoveStart -= CheckConditions;
        SGridAnimator.OnSTileMoveEnd -= CheckConditions;
    }

    private void Start()
    {
        SGrid.OnSTileEnabled += CheckConditions;
        SGridAnimator.OnSTileMoveStart += CheckConditions;
        SGridAnimator.OnSTileMoveEnd += CheckConditions;
    }

    private void CheckConditions(object sender, SGrid.OnSTileEnabledArgs e)
    {
        powerConditionals.CheckConditions();
    }

    private void CheckConditions(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        powerConditionals.CheckConditions();
    }
}

