using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIArtifact : MonoBehaviour
{
    public ArtifactButton[] buttons;
    private ArtifactButton currentButton;
    private List<ArtifactButton> adjacentButtons;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DeselectCurrentButton()
    {
        currentButton = null;
    }
    
    public void SelectButton(ArtifactButton button)
    {
        if (currentButton == button)
        {
            DeselectCurrentButton();
        }
        else if (adjacentButtons.Contains(button)) // compare by id?
        {
            // swap
        }
        else if (currentButton == null)
        {
            adjacentButtons = GetAdjacent(button);
            if (adjacentButtons == null)
            {
                DeselectCurrentButton();
            }
            else
            {
                currentButton = button;
                foreach (ArtifactButton b in adjacentButtons)
                {
                    b.SetHighlighted(true);
                }
            }
        }
        else
        {
            // default deselect
            DeselectCurrentButton();
        }
    }

    private List<ArtifactButton> GetAdjacent(ArtifactButton button)
    {
        return null;
    }

    public static void AddButton(int num)
    {

    }
}
