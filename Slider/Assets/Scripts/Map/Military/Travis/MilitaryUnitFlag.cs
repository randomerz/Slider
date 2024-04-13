using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MilitaryUnitFlag : Item
{
    [SerializeField] private MilitaryUnit attachedUnit;

    Transform locationPriorToLastPickup;

    public override void Awake()
    {
        base.Awake();
        attachedUnit.OnDeath.AddListener(() => gameObject.SetActive(false));
    }

    public override void PickUpItem(Transform pickLocation, Action callback = null)
    {
        base.PickUpItem(pickLocation, callback);
        locationPriorToLastPickup = pickLocation;
    }

    public override STile DropItem(Vector3 dropLocation, Action callback = null)
    {
        StartCoroutine(AnimateDrop(dropLocation, () => AfterDropComplete(callback)));
        return null;
    }

    private void AfterDropComplete(Action callback = null)
    {
        STile hitStile = SGrid.GetSTileUnderneath(gameObject);

        if (!IsOrthogonallyAdjacentToUnitCurrentTile(hitStile))
        {
            AudioManager.Play("Hurt");
            transform.position = attachedUnit.FlagReturnPosition;
        }
        
        STile finalSTileLocation = SGrid.GetSTileUnderneath(gameObject);
        if (finalSTileLocation == null)
        {
            gameObject.transform.SetParent(null);
        }
        else
        {
            gameObject.transform.SetParent(finalSTileLocation.transform);
        }

        Debug.LogError(transform.position);
        Debug.LogError(MilitaryUnit.WorldPositionToGridPosition(transform.position));

        attachedUnit.GridPosition = MilitaryUnit.WorldPositionToGridPosition(transform.position);

        callback?.Invoke();

        MilitaryTurnManager.EndPlayerTurn();
    }

    private bool IsOrthogonallyAdjacentToUnitCurrentTile(STile hitStile)
    {
        return attachedUnit != null && Math.Abs(hitStile.x - attachedUnit.GridPosition.x) + Math.Abs(hitStile.y - attachedUnit.GridPosition.y) <= 1;
    }
}
