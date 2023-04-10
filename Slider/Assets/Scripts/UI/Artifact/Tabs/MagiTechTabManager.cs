using UnityEngine;
using UnityEngine.InputSystem;

public class MagiTechTabManager : ArtifactTabManager
{
    [Header("Preview Tab")]
    [SerializeField] private ArtifactTab previewTab;
    [SerializeField] private Animator previewTabAnimator;

    public override void SetCurrentScreen(int screenIndex)
    {
        base.SetCurrentScreen(screenIndex);
        previewTab.SetIsVisible(screenIndex == previewTab.homeScreen);
        MagiTechArtifact artifact = (MagiTechArtifact) uiArtifactMenus.uiArtifact;
        previewTabAnimator.SetFloat("speed", artifact.PlayerIsInPast ? -1 : 1);
    }

    public void PreviewOnHoverEnter()
    {
        MagiTechArtifact artifact = (MagiTechArtifact)uiArtifactMenus.uiArtifact;
        artifact.SetPreview(true);
        previewTabAnimator.SetBool("isHovered", true);
        previewTabAnimator.SetFloat("speed", previewTabAnimator.GetFloat("speed") * -1);
        artifact.DeselectSelectedButton();
    }

    public void PreviewOnHoverExit()
    {
        MagiTechArtifact artifact = (MagiTechArtifact)uiArtifactMenus.uiArtifact;
        artifact.SetPreview(false);
        previewTabAnimator.SetBool("isHovered", false);
        previewTabAnimator.SetFloat("speed", previewTabAnimator.GetFloat("speed") * -1);
    }

    private InputAction altViewHoldAction;

    private void OnEnable()
    {
        altViewHoldAction = Player.GetInstance().GetComponent<PlayerInput>().actions.FindAction("AltViewHold");

        altViewHoldAction.started += OnAltViewHoldStarted;
        altViewHoldAction.canceled += OnAltViewHoldCanceled;
    }

    private void OnDisable()
    {
        altViewHoldAction.started -= OnAltViewHoldStarted;
        altViewHoldAction.canceled -= OnAltViewHoldCanceled;
    }

    private void OnAltViewHoldStarted(InputAction.CallbackContext callbackContext)
    {
        Debug.Log("Alt View Hold started!");
        PreviewOnHoverEnter();
    }
    private void OnAltViewHoldCanceled(InputAction.CallbackContext callbackContext)
    {
        Debug.Log("Alt View Hold Canceled!");
        PreviewOnHoverExit();
    }
}
