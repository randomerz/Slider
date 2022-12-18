using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UIArtifactBorder : MonoBehaviour 
{
    [SerializeField] private SpriteSwapper artifactBorderSwapper;
    private Coroutine artifactBorderFlickerCoroutine;

    private bool shouldFlickerOnEnable;

    private void OnEnable() 
    {
        bool checkingCompletion = SGrid.Current.CheckCompletion;
        bool areaCompleted = SaveSystem.Current.GetSGridData(SGrid.Current.MyArea).completionColor == ArtifactWorldMapArea.AreaStatus.color;
        shouldFlickerOnEnable = checkingCompletion && !areaCompleted;
        CheckArtifactBorder();
    }

    public void SetFlickerOnEnable(bool value)
    {
        shouldFlickerOnEnable = value;
    }

    public void CheckArtifactBorder()
    {
        if (shouldFlickerOnEnable)
            FlickerArtifactBorder(4);
        else
            artifactBorderSwapper.TurnOn();
    }

    public void FlickerArtifactBorder(int n)
    {
        if (artifactBorderFlickerCoroutine != null)
            StopCoroutine(artifactBorderFlickerCoroutine);
        artifactBorderFlickerCoroutine = StartCoroutine(ArtifactBorderFlicker(n));
    }

    private IEnumerator ArtifactBorderFlicker(int numFlickers) 
    {
        for (int i = 0; i < numFlickers; i++) 
        {
            artifactBorderSwapper.TurnOff(); // Off = highlight
            yield return new WaitForSeconds(.5f);
            artifactBorderSwapper.TurnOn(); // On = default
            yield return new WaitForSeconds(.5f);
        }

        artifactBorderFlickerCoroutine = null;
    }    
}