using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UIMenu : MonoBehaviour
{
    [SerializeField] private UnityEvent onOpen;
    [SerializeField] private UnityEvent onClose;
    [SerializeField] private UnityEvent onMoveToParentMenu;
    [SerializeField] private UIMenu parentMenu;

    /// <summary>
    /// First, this method deactivates the previous menu by calling <see cref="Close"/> on it unless 
    /// that menu is a parent menu of this one. Then it sets the GameObject for this UIMenu active
    /// and does any necessary initialization of the menu.
    /// </summary>
    /// <param name="previousMenu"></param>
    public void Open(UIMenu previousMenu = null)
    {
        if (previousMenu != null && !IsChildMenuOf(previousMenu))
        {
            previousMenu.Close();
        }
        OpenAllParentMenusNotAlreadyOpen();

        gameObject.SetActive(true);
        UINavigationManager.CurrentMenu = gameObject;
        onOpen?.Invoke();
    }

    /// <summary>
    /// Sets the GameObject for this UIMenu inactive and does any necessary cleanup for closing the menu.
    /// Typically, this is called by another menu's <see cref="Open(UIMenu)"/> method to close the previous
    /// menu if it is not a parent of the newly opened menu.
    /// </summary>
    public void Close()
    {
        gameObject.SetActive(false);
        UINavigationManager.CurrentMenu = null;
        onClose?.Invoke();
    }

    public void MoveToMenu(UIMenu menu)
    {
        menu.Open(previousMenu: this);
    }

    /// <summary>
    /// Moves to the immediate parent menu of this UI Menu. If the parent menu is null,
    /// this instead closes the current menu. If the parent menu is the menu itself, the menu cannot be closed.
    /// </summary>
    public void MoveToParentMenu()
    {
        // Setting the parent to the menu itself indicates a menu which cannot be closed by pressing Back/Cancel
        if (parentMenu != this)
        {
            if (parentMenu != null)
            {
                MoveToMenu(parentMenu);
            }
            else
            {
                Close();
            }
        }
        onMoveToParentMenu?.Invoke();
    }

    /// <summary>
    /// Whether this menu is a child of the passed in menu. This checks for multiple levels of parenthood.
    /// <para>
    /// Example: For A -> B -> C (A is the parent of B, which is the parent of C), C.IsChildMenuOf(A) == true
    /// </para>
    /// </summary>
    /// <param name="menu"></param>
    /// <returns></returns>
    public bool IsChildMenuOf(UIMenu menu)
    {
        UIMenu currentParent = parentMenu;
        while (currentParent != null)
        {
            if (currentParent == menu)
            {
                return true;
            }
            // A menu which cannot be closed using Back/Cancel has itself as a parent, so we
            // need to break out to avoid an infinite loop
            if (currentParent.parentMenu == currentParent)
            {
                break;
            }
            currentParent = currentParent.parentMenu;
        }
        return false;
    }

    /// <summary>
    /// Closes the menu, then immediately re-opens it. 
    /// This can be used to trigger events such as onOpen, onClose, onEnable on menu elements, etc.
    /// </summary>
    public void Reopen()
    {
        Close();
        Open();
    }

    private void OpenAllParentMenusNotAlreadyOpen()
    {
        // We want to open the parents starting with the one highest in the hierarchy
        // and working our way down to the immediate parent of this menu
        Stack<UIMenu> parentsInOrderOfHierarchy = new();
        UIMenu currentParent = parentMenu;
        while (currentParent != null && currentParent.parentMenu != currentParent)
        {
            parentsInOrderOfHierarchy.Push(currentParent);
            currentParent = currentParent.parentMenu;
        }

        while (parentsInOrderOfHierarchy.Count > 0)
        {
            UIMenu parent = parentsInOrderOfHierarchy.Pop();
            if (!parent.gameObject.activeSelf)
            {
                parent.Open();
            }
        }
    }
}
