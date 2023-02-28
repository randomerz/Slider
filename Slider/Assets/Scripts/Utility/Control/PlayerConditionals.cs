using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerConditionals : MonoBehaviour, IInteractable
{

    public UnityEvent onSuccess;

    public bool addToOnAction;

    public bool isCarryingItem;
    public string itemNameCheck;

    private bool onActionEnabled = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (addToOnAction)
            {
                if (onActionEnabled)
                {
                    Player.GetPlayerAction().AddInteractable(this);
                }
            }
            else
            {
                CheckCondition();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (addToOnAction)
            {
                if (onActionEnabled)
                {
                    Player.GetPlayerAction().RemoveInteractable(this);
                }
            }
        }
    }

    public void CheckConditionSpec(Condition c)
    {
        c.SetSpec(CheckCondition(false));
    }

    public bool CheckCondition(bool invoke=true)
    {
        if (!onActionEnabled)
        {
            return false;
        }

        if (isCarryingItem)
        {
            if (!Player.GetPlayerAction().HasItem())
            {
                return false;
            }
            if (!Player.GetPlayerAction().pickedItem.itemName.Equals(itemNameCheck))
            {
                return false;
            }
        }

        if (invoke)
        {
            InvokeSuccess();
        }
        return true;
    }

    public void InvokeSuccess()
    {
        onSuccess?.Invoke();
    }

    public void EnableConditionals()
    {
        if (addToOnAction)
        {
            onActionEnabled = true;
        }
    }

    public void DisableConditionals()
    {
        if (addToOnAction && onActionEnabled)
        {
            Player.GetPlayerAction().RemoveInteractable(this);
            onActionEnabled = false;
        }
    }

    public bool Interact()
    {
        return CheckCondition();
    }
}
