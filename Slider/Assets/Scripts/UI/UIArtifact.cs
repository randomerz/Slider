using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIArtifact : MonoBehaviour
{
    public ArtifactButton[] buttons;
    private ArtifactButton currentButton;
    private List<ArtifactButton> adjacentButtons = new List<ArtifactButton>();

    private static UIArtifact _instance;
    
    private void Awake()
    {
        _instance = this;
    }

    public static UIArtifact GetInstance()
    {
        return _instance;
    }

    public void DeselectCurrentButton()
    {
        if (currentButton == null)
            return;

        currentButton.SetPushedDown(false);
        foreach (ArtifactButton b in adjacentButtons)
        {
            b.SetHighlighted(false);
        }
        currentButton = null;
        adjacentButtons.Clear();
    }
    
    public void SelectButton(ArtifactButton button)
    {
        if (!EightPuzzle.GetCanSlide())
        {
            return;
        }

        if (currentButton == button)
        {
            DeselectCurrentButton();
        }
        else if (adjacentButtons.Contains(button)) // compare by id?
        {
            Swap(currentButton, button);
            DeselectCurrentButton();
        }
        else
        {
            DeselectCurrentButton();

            if (button.isEmpty)
            {
                return;
            }

            adjacentButtons = GetAdjacent(button);
            if (adjacentButtons.Count == 0)
            {
                return;
            }
            else
            {
                Debug.Log("Selected button " + button.islandId);
                currentButton = button;
                button.SetPushedDown(true);
                foreach (ArtifactButton b in adjacentButtons)
                {
                    b.SetHighlighted(true);
                }
            }
        }
        //else
        //{
        //    // default deselect
        //    DeselectCurrentButton();
        //}
    }

    // replaces adjacentButtons
    private List<ArtifactButton> GetAdjacent(ArtifactButton button)
    {
        adjacentButtons.Clear();

        Vector2 buttPos = new Vector2(button.xPos, button.yPos);
        foreach (ArtifactButton b in buttons)
        {
            if (b.isEmpty && (buttPos - new Vector2(b.xPos, b.yPos)).magnitude == 1)
            {
                adjacentButtons.Add(b);
            }
        }

        return adjacentButtons;
    }

    private void Swap(ArtifactButton buttonCurrent, ArtifactButton buttonEmpty)
    {
        int x = buttonCurrent.xPos;
        int y = buttonCurrent.yPos;
        Direction dir = DirectionUtil.V2D(new Vector2(buttonEmpty.xPos, buttonEmpty.yPos) - new Vector2(x, y));
        EightPuzzle.MoveSlider(x, y, dir);

        buttonCurrent.SetPosition(buttonEmpty.xPos, buttonEmpty.yPos);
        buttonEmpty.SetPosition(x, y);
    }

    public static void AddButton(int islandId)
    {
        foreach (ArtifactButton b in _instance.buttons)
        {
            if (b.islandId == islandId)
            {
                b.SetEmpty(false);
                return;
            }
        }
    }
}
