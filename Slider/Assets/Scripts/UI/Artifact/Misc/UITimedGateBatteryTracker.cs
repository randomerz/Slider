using UnityEngine;
using UnityEngine.UI;

public class UITimedGateBatteryTracker : FlashWhiteImage
{
    [SerializeField] private Image poweredImage;
    [SerializeField] private BatteryProp myBattery;
    [SerializeField] private TimedGate myGate;
    [SerializeField] private Sprite spriteOn;
    [SerializeField] private Sprite spriteOff;
    [SerializeField] private bool flashOnEnable;
    [SerializeField] private bool stayOnUntilGateIsPowered;

    private ArtifactTileButton button;

    private bool isGateEnabled;
    private bool hasGateBeenEnabledOnce;
    private bool isDiodeEnabled;
    private bool isGatePowered;

    protected override void Awake()
    {
        base.Awake();

        if (myBattery == null)
        {
            Debug.LogError("UIPowerTracker requires battery to track");
        }
        button = GetComponentInParent<ArtifactTileButton>();
    }

    private void OnEnable()
    {
        isGateEnabled = myBattery.IsGateEnabled;
        hasGateBeenEnabledOnce |= isGateEnabled;
        isDiodeEnabled = myBattery.IsDiodeEnabled;
        isGatePowered = myGate.Powered;
        
        if (flashOnEnable)
        {
            Flash(2);
        }
    }

    private void Update()
    {
        UpdatePoweredImageEnabled();
    }

    private void UpdatePoweredImageEnabled()
    {
        bool nodeOnDisabledButton = button != null && !button.TileIsActive;

        bool shouldBeEnabled = !nodeOnDisabledButton;
        if (stayOnUntilGateIsPowered)
        {
            // Fail condition once gate is powered
            shouldBeEnabled &= hasGateBeenEnabledOnce && !isGatePowered;
        }
        else
        {
            // Fail condition when gate is enabled and not yet powered 
            shouldBeEnabled &= isGateEnabled && !isGatePowered;
        }

        isDiodeEnabled |= myBattery.IsDiodeEnabled;
        
        poweredImage.enabled = shouldBeEnabled;
        poweredImage.sprite = isDiodeEnabled ? spriteOn : spriteOff;
    }
}
