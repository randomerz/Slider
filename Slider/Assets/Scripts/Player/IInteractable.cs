using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
    /// <returns>Whether the action was successful or not</returns>
    public bool Interact();

    public int InteractionPriority { get => 0; }

    public bool DisplayInteractionPrompt { get => true; }
}
