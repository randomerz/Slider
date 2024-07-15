using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class JungleArtifact : UIArtifact
{
    // private static STile prevLinkTile = null;

    // Taking some inspo from the JungleRecipeBook class
    private BindingBehavior directionalBindingBehavior;
    private BindingBehavior quitBindingBehaviorEsc;
    private BindingBehavior quitBindingBehaviorAction;
    private Vector2 lastDirectionalInput;

    public override bool TryQueueMoveFromButtonPair(ArtifactTileButton buttonCurrent, ArtifactTileButton buttonEmpty)
    {
        if (buttonCurrent.LinkButton == null)
        {
            //L: Just a normal move
            return base.TryQueueMoveFromButtonPair(buttonCurrent, buttonEmpty);
        }
        else
        {
            //L: Below is to handle the case for if you have linked tiles.
            int x = buttonCurrent.x;
            int y = buttonCurrent.y;

            //L: Make sure that all checks/swaps are with respect to the UI and NOT the grid (bc the grid can be behind due to queuing)
            int linkx = buttonCurrent.LinkButton.x;
            int linky = buttonCurrent.LinkButton.y;
            int dx = buttonEmpty.x - x;
            int dy = buttonEmpty.y - y;

            SMove linkedSwap = new SMoveLinkedSwap(x, y, buttonEmpty.x, buttonEmpty.y, linkx, linky,
                                                    buttonCurrent.islandId, buttonCurrent.LinkButton.islandId);

            Movement movecoords = new Movement(linkx, linky, linkx + dx, linky + dy, buttonCurrent.islandId);


            // if we do a diagonal click, make sure to swap the correct tiles
            if (dx != 0 && dy != 0)
            {
                linkedSwap = new SMoveLinkedSwap(x, y, x, buttonEmpty.y, linkx, linky,
                                                buttonCurrent.islandId, buttonCurrent.LinkButton.islandId);
            }
            else
            {
                // horizontal idk why
                if (Math.Abs(dx) > 1)
                {
                    return TryQueueMoveFromButtonPair(buttonCurrent.LinkButton, buttonEmpty);
                }
            }

            if (SGrid.Current.CanMove(linkedSwap) && 
                (OpenPath(movecoords) || GetButton(linkx + dx, linky + dy) == buttonCurrent) && 
                moveQueue.Count < maxMoveQueueSize)
            {

                QueueAdd(linkedSwap);

                if (dx != 0 && dy != 0)
                {
                    // diagonal case
                    SwapButtons(buttonCurrent, GetButton(x, y + dy));
                    SwapButtons(buttonCurrent.LinkButton, GetButton(linkx, linky + dy));
                }
                else
                {
                    // vert/horiz case
                    SwapButtons(buttonCurrent, buttonEmpty);
                    SwapButtons(buttonCurrent.LinkButton, GetButton(linkx + dx, linky + dy));
                }
                
                ProcessQueue();
                UpdateMoveOptions();

                return true;
            }
            else
            {
                LogMoveFailure();
                AudioManager.Play("Artifact Error");
                return false;
            }
        }
    }

    //protected override void HandleControllerCheck()
    //{
    //    base.HandleControllerCheck();
    //}

    //protected override void OnEnable()
    //{
    //    UIArtifactMenus.OnArtifactOpened += CheckUsingController;
    //    base.OnEnable();
    //}

    //protected override void OnDisable()
    //{
    //    UIArtifactMenus.OnArtifactOpened -= CheckUsingController;
    //    base.OnDisable();
    //}



    //public void CheckUsingController(object sender, System.EventArgs e)
    //{
    //    if (Controls.CurrentControlScheme.Equals(Controls.CONTROL_SCHEME_CONTROLLER))
    //    {
    //        Debug.Log($"IS USING CONTROLLER: {Controls.CurrentControlScheme}");
    //        directionalBindingBehavior = Controls.RegisterBindingBehavior(this, Controls.Bindings.UI.Navigate,
    //            context => {
    //                HandleDirectionalInput(context.ReadValue<Vector2>());
    //                //Debug.Log($"CONTEXT ACTION: {context.control}");
    //            }
    //        );
    //        quitBindingBehaviorAction = Controls.RegisterBindingBehavior(this, Controls.Bindings.Player.Action,
    //            context =>
    //            {
    //                Controls.UnregisterBindingBehavior(directionalBindingBehavior);
    //                Controls.UnregisterBindingBehavior(quitBindingBehaviorAction);
    //                Controls.UnregisterBindingBehavior(quitBindingBehaviorEsc);
    //            }
    //        );
    //        quitBindingBehaviorEsc = Controls.RegisterBindingBehavior(this, Controls.Bindings.UI.Cancel,
    //            context =>
    //            {
    //                Controls.UnregisterBindingBehavior(directionalBindingBehavior);
    //                Controls.UnregisterBindingBehavior(quitBindingBehaviorAction);
    //                Controls.UnregisterBindingBehavior(quitBindingBehaviorEsc);
    //            }

    //        );

    //    }
    //    else
    //    {
    //        Controls.UnregisterBindingBehavior(directionalBindingBehavior);

    //    }
    //}

    //Checks if the move can happen on the grid.
    //L: This should maybe be checked with GetMoveOptions?
    private bool OpenPath(Movement move)
    {
        List<Vector2Int> checkedCoords = new List<Vector2Int>();
        int dx = move.endLoc.x - move.startLoc.x;
        int dy = move.endLoc.y - move.startLoc.y;
        int toCheck = Math.Max(Math.Abs(dx), Math.Abs(dy));

        if (dx == 0)
        {
            int dir = dy / Math.Abs(dy);
            for (int i = 1; i <= toCheck; i++)
            {
                if (GetButton(move.startLoc.x, move.startLoc.y + i * dir).TileIsActive)
                {
                    return false;
                }
            }
        }
        else if (dy == 0)
        {
            int dir = dx / Math.Abs(dx);
            for (int i = 1; i <= toCheck; i++)
            {
                if (GetButton(move.startLoc.x + i * dir, move.startLoc.y).TileIsActive)
                {
                    return false;
                }
            }
        }
        return true;
    }

    
    protected override List<ArtifactTileButton> GetMoveOptions(ArtifactTileButton button)
    {
        if (button.LinkButton == null)
        {
            return base.GetMoveOptions(button);
        }

        List<ArtifactTileButton> options = new List<ArtifactTileButton>();

        Vector2Int[] dirsVert = {
            Vector2Int.up,
            Vector2Int.down
        };

        Vector2Int[] dirsHoriz =
        {
            Vector2Int.left,
            Vector2Int.right
        };

        // Up and Down
        foreach (Vector2Int dir in dirsVert)
        {
            ArtifactTileButton b = GetButton(button.x + dir.x, button.y + dir.y);
            ArtifactTileButton c = GetButton(button.LinkButton.x + dir.x, button.LinkButton.y + dir.y);
            int i = 2;
            while (b != null && !b.TileIsActive && c != null && !c.TileIsActive)
            {
                options.Add(b);
                options.Add(c);
                b = GetButton(button.x + dir.x * i, button.y + dir.y * i);
                c = GetButton(button.LinkButton.x + dir.x * i, button.LinkButton.y + dir.y * i);
                i++;
            }
        }

        // Left and Right
        foreach (Vector2Int dir in dirsHoriz)
        {
            if (button.islandId == 2)
            {
                ArtifactTileButton b = GetButton(button.x + dir.x, button.y + dir.y);
                if (b != null && dir == Vector2Int.right)
                {
                    b = GetButton(button.x + dir.x * 2, button.y + dir.y);
                    if (b != null && !b.TileIsActive)
                    {
                        options.Add(b);
                    }
                }
                else if (dir == Vector2Int.left)
                {
                    if (b != null && !b.TileIsActive)
                    {
                        options.Add(b);
                    }
                }
            }
            else if (button.islandId == 3)
            {
                ArtifactTileButton b = GetButton(button.x + dir.x, button.y + dir.y);
                if (dir == Vector2Int.right)
                {
                    if (b != null && !b.TileIsActive)
                    {
                        options.Add(b);
                    }
                }
                else if (b != null && dir == Vector2Int.left)
                {
                    b = GetButton(button.x + dir.x * 2, button.y + dir.y);
                    if (b != null && !b.TileIsActive)
                    {
                        options.Add(b);
                    }
                }
            }
        }

        return options;
    }

    /// <summary>
    /// Leaving this here in case we decide to try to reprogram the long
    /// tile to be treated as one button on controller support.  Until then,
    /// this method doesn't really do anything.
    /// </summary>
    /// <param name="input">Input direction vector</param>
    //private void HandleDirectionalInput(Vector2 input)
    //{
    //    if (input.magnitude < 0.5f)
    //    {
    //        lastDirectionalInput = Vector2.zero;
    //        return;
    //    }

    //    float angle = Mathf.Atan2(input.y, input.x) * Mathf.Rad2Deg;

    //    // Up
    //    if (45 <= angle && angle < 135)
    //    {
    //        if (lastDirectionalInput == Vector2.up)
    //            return;

    //        lastDirectionalInput = Vector2.up;
    //        //jungleRecipeBookUI.IncrementCurrentShape();
    //        // TODO: handle the directional input stuff for the UI Artifact for Jungle
    //    }
    //    // Left
    //    else if (135 <= angle && angle < 225)
    //    {
    //        if (lastDirectionalInput == Vector2.left)
    //            return;

    //        lastDirectionalInput = Vector2.left;
    //        //jungleRecipeBookUI.DecrementCurrentShape();
    //    }
    //    // Down
    //    else if (225 <= angle && angle < 315)
    //    {
    //        if (lastDirectionalInput == Vector2.down)
    //            return;

    //        lastDirectionalInput = Vector2.down;
    //        //jungleRecipeBookUI.DecrementRecipeDisplay();
    //    }
    //    // Right
    //    else
    //    {
    //        if (lastDirectionalInput == Vector2.right)
    //            return;

    //        lastDirectionalInput = Vector2.right;
    //        //jungleRecipeBookUI.IncrementCurrentShape();
    //    }
        
    //}

    public override void SelectButton(ArtifactTileButton button, bool isDragged = false)
    {
        if (button.LinkButton == null)
        {
            base.SelectButton(button, isDragged);
        }
        else
        {
            ArtifactTileButton linkButton = button.LinkButton;
            if (buttonSelected != null && buttonSelected.Equals(linkButton))
            {

                DeselectSelectedButton();
            } else
            {
                base.SelectButton(button, isDragged);
            }
        }

    }


}

