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

    private ArtifactTileButton button;

    private bool isGateEnabled;
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

        isDiodeEnabled |= myBattery.IsDiodeEnabled;
        
        poweredImage.enabled = isGateEnabled && !isGatePowered && !nodeOnDisabledButton;
        poweredImage.sprite = isDiodeEnabled ? spriteOn : spriteOff;
    }
}
