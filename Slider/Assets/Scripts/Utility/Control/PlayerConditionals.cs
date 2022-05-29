using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerConditionals : MonoBehaviour
{

    public UnityEvent onSuccess;

    public bool addToOnAction;

    private bool isEnabled = true;


    public bool isCarryingItem;
    public string itemNameCheck;

    // caves
    public bool triggerIsLit;
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
                if (isEnabled)
                {
                    PlayerAction.OnAction += OnActionListener;
                    Player.GetPlayerAction().IncrementActionsAvailable();
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
                if (isEnabled)
                {
                    PlayerAction.OnAction -= OnActionListener;
                    Player.GetPlayerAction().DecrementActionsAvailable();
                }
            }
        }
    }

    public void CheckConditionSpec(Conditionals.Condition c)
    {
        c.SetSpec(CheckCondition(false));
    }

    public bool CheckCondition(bool invoke=true)
    {
        if (!isEnabled)
        {
            return false;
        }

        // caves
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

    private void OnActionListener(object sender, System.EventArgs e)
    {
        CheckCondition();
    }

    public void DisableConditionals()
    {
        PlayerAction.OnAction -= OnActionListener;
        Player.GetPlayerAction().DecrementActionsAvailable();
        addToOnAction = false;
        isEnabled = false;
    }

}
