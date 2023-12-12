using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.Experimental.Rendering.Universal;

public class TrailerCutScene3 : MonoBehaviour
{
    public CinemachineVirtualCamera cmWorldCam;
    public UIArtifact uiArtifact;
    public PixelPerfectCamera mainCameraPixelPerfect;
    

    // public GameObject[] tiles;

    // public AnimationCurve shrinkCurve;
    // public float shrinkEndRotation;
    // public float shrinkDuration;

    void Start()
    {
        StartCoroutine(StartIntroCutscene());
    }
    

    private IEnumerator StartIntroCutscene()
    {
        yield return new WaitForSeconds(3);

        Debug.Log("Starting...");

        (SGrid.Current as VillageGrid).ForceEnableCompletionsForTrailer();
        (SGrid.Current as VillageGrid).RemoveSlider2Tracker();


        // Swap 7/9
        uiArtifact.TryQueueMoveFromButtonPair(uiArtifact.GetButton(7), uiArtifact.GetButton(9));

        yield return new WaitForSeconds(1.01f);
        
        // Swap 2/9
        uiArtifact.TryQueueMoveFromButtonPair(uiArtifact.GetButton(2), uiArtifact.GetButton(9));

        yield return new WaitForSeconds(1.01f);
        
        // Swap 3/8
        uiArtifact.TryQueueMoveFromButtonPair(uiArtifact.GetButton(3), uiArtifact.GetButton(8));

        yield return new WaitForSeconds(1.01f);
        
        // Swap 1/9
        uiArtifact.TryQueueMoveFromButtonPair(uiArtifact.GetButton(1), uiArtifact.GetButton(9));
        cmWorldCam.Priority = 0;

        yield return new WaitForSeconds(1.01f);
        
        // Swap 6/8
        uiArtifact.TryQueueMoveFromButtonPair(uiArtifact.GetButton(6), uiArtifact.GetButton(8));

        yield return new WaitForSeconds(2);

        mainCameraPixelPerfect.enabled = true;
    }
}
