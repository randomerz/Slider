using UnityEngine;

public class FactoryPrinterTracker : MonoBehaviour
{
    public PoweredDoor outerDoor;
    public PoweredDoor innerDoor;

    private Coroutine fezziwigCoroutine;
    public NPC fezziwigNPC;
    public Transform fezziwigOffMap;
    public Transform fezziwigMiddle;
    public Transform fezziwigInside;
    public Transform playerOutside;
    
    private const string PRINTER_SOFTLOCK_FLAG = "FactoryIsInPrinterSoftlock";

    public enum PrinterPosition
    {
        Outside,
        Middle,
        Inside,
    }

    private PrinterPosition playerPosition;

    private void Start()
    {
        OnOuterDoorUpdate();
        OnInnerDoorUpdate();
    }

    public void SetPlayerOutside() 
    {
        playerPosition = PrinterPosition.Outside;

        if (fezziwigCoroutine != null)
        {
            StopCoroutine(fezziwigCoroutine);
            fezziwigCoroutine = null;
        }
    }

    public void SetPlayerMiddle() => playerPosition = PrinterPosition.Middle;
    public void SetPlayerInside() => playerPosition = PrinterPosition.Inside;

    public void OnOuterDoorUpdate()
    {
        if (!outerDoor.Powered && playerPosition == PrinterPosition.Middle || playerPosition == PrinterPosition.Inside)
        {
            SetFezziwigPositionDelayed(PrinterPosition.Middle);
        }
    }

    public void OnInnerDoorUpdate()
    {
        if (!innerDoor.Powered && playerPosition == PrinterPosition.Inside)
        {
            SetFezziwigPositionDelayed(PrinterPosition.Inside);
        }
    }

    public void TeleportPlayerOut()
    {
        Player.SetPosition(playerOutside.position);
        SetFezziwigPosition(PrinterPosition.Outside);
    }

    private void SetFezziwigPositionDelayed(PrinterPosition position)
    {
        if (fezziwigCoroutine != null)
        {
            return;
        }

        fezziwigCoroutine = CoroutineUtils.ExecuteAfterDelay(() => SetFezziwigPosition(position), this, 5);
    }

    private void SetFezziwigPosition(PrinterPosition position)
    {
        fezziwigCoroutine = null;
        switch (position)
        {
            case PrinterPosition.Outside:
                SaveSystem.Current.SetBool(PRINTER_SOFTLOCK_FLAG, false);
                fezziwigNPC.Teleport(fezziwigOffMap);
                break;

            case PrinterPosition.Middle:
                SaveSystem.Current.SetBool(PRINTER_SOFTLOCK_FLAG, true);
                fezziwigNPC.Teleport(fezziwigMiddle);
                break;

            case PrinterPosition.Inside:
                SaveSystem.Current.SetBool(PRINTER_SOFTLOCK_FLAG, true);
                fezziwigNPC.Teleport(fezziwigInside);
                break;
        }
    }
}