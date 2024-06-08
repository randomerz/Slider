using UnityEngine;
using UnityEngine.UI;

public class UIRockTracker : FlashWhiteImage
{
    [SerializeField] private Image rockedImage;
    [SerializeField] private ExplodableRock myRock;

    [SerializeField] private MagiTechArtifact magiTechArtifact;
    [SerializeField] private bool magitechPastOnly;

    private ArtifactTileButton button;

    public bool isExploded => myRock.isExploded;

    protected override void Awake()
    {
        base.Awake();

        if (myRock == null)
        {
            Debug.LogError("UIRockTracker requires rock to track");
        }
        button = GetComponentInParent<ArtifactTileButton>();
    }

    private void Update()
    {
        UpdateRockImageEnabled();
    }

    private void UpdateRockImageEnabled()
    {
        bool nodeOnDisabledButton = button != null && !button.TileIsActive;
        bool pastOK = true;
        if (magiTechArtifact != null && magitechPastOnly)
        {
            pastOK = magiTechArtifact.IsDisplayingPast();
        }
        rockedImage.enabled = !myRock.isExploded && !nodeOnDisabledButton && pastOK;
    }
}
