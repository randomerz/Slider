using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using static UnityEngine.UI.GridLayoutGroup;

public class PauseManager
{
    /// <summary>
    /// Called when the game is paused on unpaused. 
    /// Passes true if the game is now paused and false if the game is unpaused.
    /// </summary>
    public static Action<bool> PauseStateChanged;

    private static readonly List<PauseRestriction> pauseRestrictions = new();

    public static bool IsPaused { get; private set; } = false;

    /// <summary>
    /// Prevents the player from opening the pause menu. This is setup to support multiple locations
    /// allowing and disallowing pausing independently, so you don't need to worry about overwriting
    /// the existing state by calling this method. This is achieved by keeping a list of the current
    /// restrictions in place. When you want to allow pausing again, call <see cref="RemovePauseRestriction(PauseRestriction)"/>
    /// with the <see cref="PauseRestriction"/> returned by this method to remove it.
    /// </summary>
    public static PauseRestriction AddPauseRestriction(GameObject owner)
    {
        PauseRestriction restriction = new(owner);
        pauseRestrictions.Add(restriction);
        return restriction;
    }

    public static void RemovePauseRestriction(PauseRestriction restrictionToRemove)
    {
        Controls.StartCoroutineOnInstance(IRemovePauseRestrictions(restrictionToRemove));
    }

    public static void RemovePauseRestriction(GameObject owner)
    {
        List<PauseRestriction> restrictionsWithMatchingOwner
            = pauseRestrictions.Where(restriction => restriction.owner == owner).ToList();
        
        Controls.StartCoroutineOnInstance(IRemovePauseRestrictions(restrictionsWithMatchingOwner.ToArray()));
    }

    private static IEnumerator IRemovePauseRestrictions(params PauseRestriction[] restrictions)
    {
        yield return new WaitForEndOfFrame();
        restrictions.ToList().ForEach(restriction => pauseRestrictions.Remove(restriction));
    }

    public static void RemoveAllPauseRestrictions()
    {
        pauseRestrictions.Clear();
    }

    public static void SetPauseState(bool paused)
    {
        bool newPauseState = CanPause() && paused;
        if (newPauseState == IsPaused)
        {
            return;
        }
        Controls.StartCoroutineOnInstance(IUpdateIsPausedAfterEndOfFrame(paused));
    }

    // We want to wait until the end the frame to unpause so we avoid double behavior
    // from inputs (like Escape unpausing and immediately re-pausing)
    private static IEnumerator IUpdateIsPausedAfterEndOfFrame(bool newValue)
    {
        yield return new WaitForEndOfFrame();

        IsPaused = newValue;
        Time.timeScale = IsPaused ? 0 : 1;
        PauseStateChanged?.Invoke(newValue);
    }

    /// <summary>
    /// The player cannot pause the game in some circumstances, such as if they are
    /// on the main menu or in a cutscene.
    /// </summary>
    public static bool CanPause()
    {
        return !GameUI.instance.isMenuScene && pauseRestrictions.Count == 0;
    }

    /// <summary>
    /// A flag used to prevent the player from pausing the game. The owner of the 
    /// PauseRestriction can be used to easily add and remove them without needing to store
    /// a reference to the PauseRestriction returned by <see cref="AddPauseRestriction"/>.
    /// You can instead call <see cref=""/>
    /// </summary>
    public struct PauseRestriction
    {
        public GameObject owner;

        public PauseRestriction(GameObject owner)
        {
            this.owner = owner;
        }
    }
}
