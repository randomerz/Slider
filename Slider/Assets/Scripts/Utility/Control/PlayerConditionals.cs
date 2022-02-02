using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerConditionals : MonoBehaviour
{

    public UnityEvent onSuccess;
    
    public bool addToOnAction;


    //      im just gonna brute force check them
    // public class PlayerCondition {
    //     public abstract bool Evaluate() {
    //         return false;
    //     }
    // }

    public bool isCarryingItem;


    // private void OnDisable() {               Maybe this is needed?
    //     PlayerAction.OnAction -= OnActionListener;
    // }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            // Debug.Log("Adding listener!");
            if (addToOnAction) {
                PlayerAction.OnAction += OnActionListener;
                Player.GetPlayerAction().IncrementActionsAvailable();
            }
            else {
                CheckCondition(); // might need to be Player.OnUpdate or something
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            // Debug.Log("Removing listener!");
            if (addToOnAction) {
                PlayerAction.OnAction -= OnActionListener;
                Player.GetPlayerAction().DecrementActionsAvailable();
            }
        }
    }

    public bool CheckCondition() 
    {
        if (isCarryingItem) {
            if (!Player.GetPlayerAction().HasItem()) {
                return false;
            }
        }

        onSuccess?.Invoke();
        return true;
    }

    private void OnActionListener(object sender, System.EventArgs e) {
        CheckCondition();
    }
    
}
