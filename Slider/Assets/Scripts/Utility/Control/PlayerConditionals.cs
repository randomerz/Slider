using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerConditionals : MonoBehaviour
{

    public UnityEvent onSuccess;

    public bool addToOnAction;

    public bool isCarryingItem;
    public string itemNameCheck;

    public bool triggerIsLit;

    private bool actionAdded;
    private bool onActionEnabled = true;
    private LightManager lm;

    private void Awake()
    {
        lm = GameObject.Find("LightManager")?.GetComponent<LightManager>();
    }

    // private void OnDisable() {               Maybe this is needed?
    //     PlayerAction.OnAction -= OnActionListener;
    // }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Debug.Log("Adding listener!");
            if (addToOnAction)
            {
                if (onActionEnabled)
                {
                    AddAction();
                }
            }
            else
            {
                CheckCondition(); // might need to be Player.OnUpdate or something
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Debug.Log("Removing listener!");
            if (addToOnAction)
            {
                if (onActionEnabled)
                {
                    RemoveAction();
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

        // Caves power station door
        if (triggerIsLit)
        {
            if (lm != null)
            {
                if (!lm.GetLightMaskAt((int)transform.position.x, (int)transform.position.y))
                {
                    return false;
                }
            }
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

    public void DisableConditionals()
    {
        if (addToOnAction && onActionEnabled)
        {
            RemoveAction();
            onActionEnabled = false;
        }
    }

    private void OnActionListener(object sender, System.EventArgs e)
    {
        CheckCondition();
    }

    private void AddAction()
    {
        if (!actionAdded)
        {
            actionAdded = true;
            PlayerAction.OnAction += OnActionListener;
            Player.GetPlayerAction().IncrementActionsAvailable();
        }
    }

    private void RemoveAction()
    {
        if (actionAdded)
        {
            actionAdded = false;
            PlayerAction.OnAction -= OnActionListener;
            Player.GetPlayerAction().DecrementActionsAvailable();
        }
    }
}
