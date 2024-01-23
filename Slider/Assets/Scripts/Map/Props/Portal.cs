using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Portal : MonoBehaviour
{
    public static event System.EventHandler<OnTimeChangeArgs> OnTimeChange;
    
    public enum PortalEnum
    {
        NONE,
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
    public static PortalEnum recentPortal;
    public static Portal recentPortalObj;
    private bool isTeleporting;
    public Transform desyncItemFallbackSpawn;

    public class OnTimeChangeArgs : System.EventArgs
    {
        public bool fromPast;
    }

    [System.Serializable]
    public class DesertPortalUI
    {
        public GameObject UIPortalIcon;
        public ArtifactTBPluginLaser laserPlugin;
    }
    public DesertPortalUI desertPortalUI;
    public UILaserManager uILaserManager;

    public void OnPlayerEnter()
    {
        if(playerInPortal || isTeleporting) return;
        
        playerInPortal = true;
        recentPortal = portalEnum;
        recentPortalObj = this;
        if(portalEnum is PortalEnum.MAGITECH_PRESENT || portalEnum is PortalEnum.MAGITECH_PAST)
        {
            UIEffects.FadeToBlack(callback: InitTeleport, speed: 2, alpha:0.5f, disableAtEnd: false);
            isTeleporting = true;
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
        Player.SetPosition(otherPortal.spawnPoint.position);
        OnTimeChange?.Invoke(this, new OnTimeChangeArgs { fromPast = portalEnum is PortalEnum.MAGITECH_PAST });
        isTeleporting = false;
        UIEffects.FadeFromBlack(alpha:0.5f);
    }
    

    public void OnPlayerExit()
    {
        if(isTeleporting || recentPortal == portalEnum) return;
        playerInPortal = false;
        recentPortal = PortalEnum.NONE;
        recentPortalObj = null;
    }

    public void OnPlayerNear(bool enter)
    {
        if(playerInPortal) return;
        Player.GetInstance().ToggleLightning(enter);
    }

    public void EnableDesertUIPortal()
    {
        desertPortalUI.UIPortalIcon.SetActive(true);
        uILaserManager.AddSource(desertPortalUI.laserPlugin.laserUIData);
        uILaserManager.UpdateSpritesFromSource();
    }
}
