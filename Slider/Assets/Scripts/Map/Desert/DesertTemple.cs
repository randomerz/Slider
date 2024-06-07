using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DesertTemple : MonoBehaviour, ISavable
{
    [SerializeField] private ArtifactHousingButtonsManager artifactHousingButtonsManager;
    [SerializeField] private GameObject templeTrapBlockingRoom;
    [SerializeField] private GameObject templeTrapBlockingRoomCollider;
    [SerializeField] private GameObject templeEntranceCollapseRocks;
    [SerializeField] private DesertTempleMusic templeMusic;
    private Coroutine shuffleBuildUpCoroutine;
    

    private bool isInTemple;
    

    public void Save() {}

    public void Load(SaveProfile profile)
    {
        isInTemple = profile.GetBool("desertIsInTemple");
        if (isInTemple)
        {
            SetIsInTemple(true);

            if (
                SaveSystem.Current.GetBool("desertTempleEntranceCollapsed") &&
                !PlayerInventory.Contains("Scroll of Realigning")
            )
            {
                templeEntranceCollapseRocks.SetActive(true);
            }
        }
    }

    private void OnEnable() 
    {
        if (SaveSystem.Current.GetBool("desertTempleActivatedTrap") &&
            !SaveSystem.Current.GetBool("desertTempleTrapCleared"))
        {
            ArtifactTabManager.AfterScrollRearrage += OnScrollRearrage;
        }
    }

    private void OnDisable() 
    {
        if (SaveSystem.Current.GetBool("desertTempleActivatedTrap"))
        {
            ArtifactTabManager.AfterScrollRearrage -= OnScrollRearrage;
        }
    }
    
    public void SetIsInTemple(bool isInTemple)
    {
        this.isInTemple = isInTemple;
        SaveSystem.Current.SetBool("desertIsInTemple", isInTemple);
        if (isInTemple)
        {
            SaveSystem.Current.SetBool("desertEnteredTemple", true);
        }
        artifactHousingButtonsManager.SetSpritesToHousing(isInTemple);
        templeMusic.SetIsInTemple(isInTemple);
        Player.GetInstance().SetTrackerEnabled(!isInTemple);
        Player.GetInstance().SetDontUpdateSTileUnderneath(isInTemple);
    }

    public void CollapseTempleEntrance()
    {
        if (!SaveSystem.Current.GetBool("desertTempleEntranceCollapsed"))
        {
            StartCoroutine(DoCollapseTempleEntrance());
        }
    }

    private IEnumerator DoCollapseTempleEntrance()
    {
        SaveSystem.Current.SetBool("desertTempleEntranceCollapsed", true);
        templeEntranceCollapseRocks.SetActive(true);

        DoCollapseParticles();
        DoCollapseParticles();
        CameraShake.Shake(1, 0.75f);
        AudioManager.PlayWithVolume("Slide Rumble", 0.5f);
        AudioManager.PlayWithVolume("Hat Click", 0.3f);

        yield return new WaitForSeconds(0.75f);

        DoCollapseParticles();
        CameraShake.Shake(0.25f, 0.5f);
        AudioManager.PlayWithVolume("Slide Explosion", 0.25f);
        AudioManager.PlayWithVolume("Hat Click", 0.3f);

        yield return new WaitForSeconds(0.375f);

        DoCollapseParticles();
        CameraShake.Shake(0.25f, 0.25f);
        AudioManager.PlayWithVolume("Slide Explosion", 0.25f);
        AudioManager.PlayWithVolume("Hat Click", 0.3f);
    }

    private void DoCollapseParticles()
    {
        for (int i = 0; i < 6; i++)
        {
            Vector3 r = Random.insideUnitCircle;
            r = templeEntranceCollapseRocks.transform.position + 3 * r;
            ParticleManager.SpawnParticle(ParticleType.SmokePoof, r, templeEntranceCollapseRocks.transform);
        }
    }

    public void ActivateTrap()
    {
        AchievementManager.SetAchievementStat("collectedScroll", 1);
        SaveSystem.Current.SetBool("desertTempleTrapActivated", true);
        if (shuffleBuildUpCoroutine == null)
        {
            shuffleBuildUpCoroutine = StartCoroutine(ActivateTrapBuildUp());
        }
    }

    private IEnumerator ActivateTrapBuildUp()
    {
        templeTrapBlockingRoomCollider.SetActive(true);

        CameraShake.Shake(0.25f, 0.25f);
        AudioManager.Play("Slide Rumble");

        yield return new WaitForSeconds(1f);

        CameraShake.Shake(0.25f, 0.25f);
        AudioManager.Play("Slide Rumble");

        yield return new WaitForSeconds(1f);

        CameraShake.Shake(0.75f, 0.5f);
        AudioManager.Play("Slide Rumble");

        yield return new WaitForSeconds(1f);

        CameraShake.Shake(1.5f, 2.5f);
        AudioManager.PlayWithVolume("Slide Explosion", 0.2f);
        AudioManager.Play("TFT Bell");

        yield return new WaitForSeconds(0.25f);

        UIEffects.FlashWhite();
        SGrid.Current.SetGrid(SGrid.GridStringToSetGridFormat("815493672"));
        templeTrapBlockingRoom.SetActive(true);
        SaveSystem.Current.SetBool("desertTempleActivatedTrap", true);

        ArtifactTabManager.AfterScrollRearrage += OnScrollRearrage;

        yield return new WaitForSeconds(0.75f);

        CameraShake.Shake(2, 0.9f);
        shuffleBuildUpCoroutine = null;
    }

    private void OnScrollRearrage(object sender, System.EventArgs e)
    {
        templeTrapBlockingRoom.SetActive(false);
        templeTrapBlockingRoomCollider.SetActive(false);
        SaveSystem.Current.SetBool("desertTempleTrapCleared", true);
        ArtifactTabManager.AfterScrollRearrage -= OnScrollRearrage;
        AchievementManager.SetAchievementStat("completedDesert", 1);
    }
}
