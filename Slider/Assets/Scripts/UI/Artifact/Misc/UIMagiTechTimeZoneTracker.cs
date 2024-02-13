using UnityEngine;
using UnityEngine.UI;

public class UIMagiTechTimeZoneTracker : MonoBehaviour
{
    [SerializeField] private MagiTechArtifact magiTechArtifact;
    [SerializeField] private Image sprite;

    void LateUpdate()
    {
        bool shouldShow = magiTechArtifact != null && magiTechArtifact.IsDisplayingPast();
        sprite.enabled = shouldShow;
    }
}
