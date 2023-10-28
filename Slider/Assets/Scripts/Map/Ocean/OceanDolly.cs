using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;

public class OceanDolly : CameraDolly
{
    public NPC intern1;
    public NPC intern2;

    public float fadeDuration;
    protected override IEnumerator Rollercoaster(bool DontReturnToPlayerOnEnd = false)
    {
        UIEffects.FadeFromBlack();
        PauseManager.AddPauseRestriction(owner: gameObject);
        Player.SetCanMove(false);

        float t = 0;
        
        // fade out at end
        while (t < duration - fadeDuration)
        {
            float x = (t / duration);

            dolly.m_PathPosition = pathMovementCurve.Evaluate(x) * (numWaypoints - 1);

            yield return null;
            t += Time.deltaTime;
        }

        intern1.TypeCurrentDialogue();
        yield return new WaitForSeconds(5.5f);
        intern1.AdvanceDialogueChain();
        intern2.TypeCurrentDialogue();
        yield return new WaitForSeconds(5f);
        intern2.AdvanceDialogueChain();

        UIEffects.FadeToBlack(
            () => { 
                EndTrack();
                intern1.gameObject.SetActive(false);
                intern2.gameObject.SetActive(false);
            }
        );
        
    }

}
