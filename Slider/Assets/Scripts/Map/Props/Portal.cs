using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Portal : MonoBehaviour
{
    public static event System.EventHandler<OnTimeChangeArgs> OnTimeChange;
    // [SerializeField] private bool isInPast;

    // [SerializeField] private bool useSpecialEventInstead;
    // public UnityEvent SpecialPortalEvent;

    public enum PortalEnum
    {
        MAGITECH_PRESENT,
        MAGITECH_PAST,
        MAGITECH_TO_DESERT,
        DESERT_TO_MAGITECH,
    }

    public PortalEnum portalEnum;
    public SceneChanger sceneChanger;
    public Portal otherPortal;
    public Transform spawnPoint;
    public static bool playerInPortal;
    public static Portal recentPortal;
    private bool isTeleporting;

    public class OnTimeChangeArgs : System.EventArgs
    {
        public bool fromPast;
    }

    // private void Start()
    // {
    //     isInPast = MagiTechGrid.IsInPast(transform);
    // }

    public void OnPlayerEnter()
    {
        if(playerInPortal || isTeleporting) return;

        isTeleporting = true;
        if(portalEnum is PortalEnum.MAGITECH_PRESENT || portalEnum is PortalEnum.MAGITECH_PAST)
        {
            UIEffects.FadeToBlack(callback: InitTeleport, speed: 2, alpha:0.5f, disableAtEnd: false);
            playerInPortal = true;
            recentPortal = this;
        }
        else
        {
            AudioManager.Play("Portal");
            sceneChanger.ChangeScenes();
        }
        
    }

    private void InitTeleport()
    {
        UIEffects.FadeFromScreenshot(Teleport);
    }

    private void Teleport()
    {
        AudioManager.Play("Portal");
        //screenshot effect
        Player.SetPosition(otherPortal.spawnPoint.position);
        OnTimeChange?.Invoke(this, new OnTimeChangeArgs { fromPast = portalEnum is PortalEnum.MAGITECH_PAST });
        isTeleporting = false;
    }
    

    public void OnPlayerExit()
    {
        if(isTeleporting || recentPortal == this) return;
        UIEffects.FadeFromBlack(alpha:0.5f);
        playerInPortal = false;
        recentPortal = null;
    }
}
