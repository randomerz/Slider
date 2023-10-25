using UnityEngine;
using UnityEngine.InputSystem;

public class MagiTechTabManager : ArtifactTabManager
{
    [Header("Preview Tab")]
    [SerializeField] private ArtifactTab previewTab;
    [SerializeField] private Animator previewTabAnimator;

    private BindingHeldBehavior bindingHeldBehavior;
    private bool tabEnabled;

    protected override void Awake()
    {
        base.Awake();

        InitBindingHeldBehavior();
    }

    private void OnEnable()
    {
        Controls.RegisterBindingBehavior(bindingHeldBehavior);
    }

    private void OnDisable()
    {
        Controls.UnregisterBindingBehavior(bindingHeldBehavior);
    }

    private void InitBindingHeldBehavior()
    {
        if (bindingHeldBehavior != null)
            return;

        bindingHeldBehavior = new BindingHeldBehavior(
            Controls.Bindings.Player.AltViewHold,
            float.MaxValue,
            onHoldStarted: (context) => { OnAltViewHoldStarted(context); },
            onHoldCompleted: (context) => { OnAltViewHoldCanceled(context); },
            onButtonReleasedEarly: (context) => { OnAltViewHoldCanceled(new InputAction.CallbackContext()); } // this wont cause anything bad to happen
        );
    }


    private void OnAltViewHoldStarted(InputAction.CallbackContext callbackContext)
    {
        PreviewOnHoverEnter();
    }

    private void OnAltViewHoldCanceled(InputAction.CallbackContext callbackContext)
    {
        PreviewOnHoverExit();
    }

    public override void SetCurrentScreen(int screenIndex)
    {
        base.SetCurrentScreen(screenIndex);
        previewTab.SetIsVisible(screenIndex == previewTab.homeScreen && tabEnabled);
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

    public void EnableTab()
    {
        tabEnabled = true;
    }
}
